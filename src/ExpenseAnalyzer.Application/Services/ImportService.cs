using System.Globalization;
using CsvHelper;
using AppValidationException = ExpenseAnalyzer.Application.Common.Exceptions.ValidationException;
using ExpenseAnalyzer.Application.Common.Exceptions;
using ExpenseAnalyzer.Application.DTOs;
using ExpenseAnalyzer.Application.Interfaces;
using ExpenseAnalyzer.Domain.Entities;
using ExpenseAnalyzer.Domain.Enums;

namespace ExpenseAnalyzer.Application.Services;

public class ImportService : IImportService
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IImportJobRepository _importJobRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ImportService(
        ICurrentUserService currentUserService,
        IImportJobRepository importJobRepository,
        ITransactionRepository transactionRepository,
        IUnitOfWork unitOfWork)
    {
        _currentUserService = currentUserService;
        _importJobRepository = importJobRepository;
        _transactionRepository = transactionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ImportCsvResponseDto> ImportCsvAsync(Stream fileStream, string fileName)
    {
        if (!_currentUserService.IsAuthenticated || _currentUserService.UserId is null)
        {
            throw new UnauthorizedException("User is not authenticated.");
        }

        if (fileStream is null)
        {
            throw new AppValidationException("A non-empty CSV file is required.");
        }

        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new AppValidationException("A valid file name is required.");
        }

        if (!Path.GetExtension(fileName).Equals(".csv", StringComparison.OrdinalIgnoreCase))
        {
            throw new AppValidationException("Only .csv files are allowed.");
        }

        var userId = _currentUserService.UserId.Value;
        var importedAtUtc = DateTime.UtcNow;

        var importJob = new ImportJob
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            FileName = fileName,
            ImportedAtUtc = importedAtUtc,
            TotalRows = 0,
            ImportedRows = 0,
            SkippedRows = 0,
            Status = ImportJobStatus.Pending,
            CreatedAtUtc = importedAtUtc
        };

        await _importJobRepository.AddAsync(importJob);
        await _unitOfWork.SaveChangesAsync();

        var transactions = new List<Transaction>();
        var errors = new List<ImportCsvRowErrorDto>();
        var totalRows = 0;

        try
        {
            using var reader = new StreamReader(fileStream);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            csv.Context.RegisterClassMap<ImportCsvRowDtoMap>();

            if (!await csv.ReadAsync())
            {
                throw new AppValidationException("The CSV file is empty or missing a header row.");
            }

            csv.ReadHeader();

            var headerRecord = csv.HeaderRecord;

            if (headerRecord is null)
            {
                throw new AppValidationException("The CSV file is empty or missing a header row.");
            }

            if (headerRecord.Length != 3 ||
                !string.Equals(headerRecord[0], "Date", StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(headerRecord[1], "Description", StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(headerRecord[2], "Amount", StringComparison.OrdinalIgnoreCase))
            {
                throw new AppValidationException("The CSV header must be exactly: Date,Description,Amount.");
            }

            while (await csv.ReadAsync())
            {
                var parser = csv.Context.Parser;

                if (parser is null)
                {
                    throw new AppValidationException("The CSV parser could not read the current row.");
                }

                var rowNumber = parser.Row;
                var rawLine = parser.RawRecord?.TrimEnd('\r', '\n') ?? string.Empty;

                totalRows++;

                ImportCsvRowDto? row;

                try
                {
                    row = csv.GetRecord<ImportCsvRowDto>();
                }
                catch
                {
                    errors.Add(new ImportCsvRowErrorDto
                    {
                        RowNumber = rowNumber,
                        RawLine = rawLine,
                        Message = "The row could not be parsed."
                    });

                    continue;
                }

                if (row is null)
                {
                    errors.Add(new ImportCsvRowErrorDto
                    {
                        RowNumber = rowNumber,
                        RawLine = rawLine,
                        Message = "The row could not be parsed."
                    });

                    continue;
                }

                if (string.IsNullOrWhiteSpace(rawLine))
                {
                    errors.Add(new ImportCsvRowErrorDto
                    {
                        RowNumber = rowNumber,
                        RawLine = rawLine,
                        Message = "The row is empty."
                    });

                    continue;
                }

                var dateText = row.Date?.Trim() ?? string.Empty;
                var description = row.Description?.Trim() ?? string.Empty;
                var amountText = row.Amount?.Trim() ?? string.Empty;

                var isDateValid = DateTime.TryParse(
                    dateText,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var parsedDate);

                if (!isDateValid)
                {
                    errors.Add(new ImportCsvRowErrorDto
                    {
                        RowNumber = rowNumber,
                        RawLine = rawLine,
                        Message = "Date is invalid."
                    });

                    continue;
                }

                if (string.IsNullOrWhiteSpace(description))
                {
                    errors.Add(new ImportCsvRowErrorDto
                    {
                        RowNumber = rowNumber,
                        RawLine = rawLine,
                        Message = "Description is required."
                    });

                    continue;
                }

                var isAmountValid = decimal.TryParse(
                    amountText,
                    NumberStyles.Number | NumberStyles.AllowLeadingSign,
                    CultureInfo.InvariantCulture,
                    out var amount);

                if (!isAmountValid)
                {
                    errors.Add(new ImportCsvRowErrorDto
                    {
                        RowNumber = rowNumber,
                        RawLine = rawLine,
                        Message = "Amount is invalid."
                    });

                    continue;
                }

                var date = DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc);

                transactions.Add(new Transaction
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    ImportJobId = importJob.Id,
                    Date = date,
                    Description = description,
                    Amount = amount,
                    CreatedAtUtc = importedAtUtc
                });
            }

            if (transactions.Count > 0)
            {
                await _transactionRepository.AddRangeAsync(transactions);
            }

            importJob.TotalRows = totalRows;
            importJob.ImportedRows = transactions.Count;
            importJob.SkippedRows = errors.Count;
            importJob.Status = errors.Count == 0
                ? ImportJobStatus.Completed
                : ImportJobStatus.CompletedWithErrors;

            await _unitOfWork.SaveChangesAsync();

            return new ImportCsvResponseDto
            {
                ImportJobId = importJob.Id,
                FileName = importJob.FileName,
                ImportedCount = transactions.Count,
                SkippedCount = errors.Count,
                ImportedAtUtc = importJob.ImportedAtUtc,
                Errors = errors
            };
        }
        catch
        {
            importJob.Status = ImportJobStatus.Failed;
            await _unitOfWork.SaveChangesAsync();
            throw;
        }
    }

    public async Task<IReadOnlyList<ImportJobSummaryDto>> GetImportHistoryAsync()
    {
        if (!_currentUserService.IsAuthenticated || _currentUserService.UserId is null)
        {
            throw new UnauthorizedException("User is not authenticated.");
        }

        var userId = _currentUserService.UserId.Value;

        var importJobs = await _importJobRepository.GetByUserIdAsync(userId);

        var result = importJobs
            .Select(x => new ImportJobSummaryDto
            {
                ImportJobId = x.Id,
                FileName = x.FileName,
                TotalRows = x.TotalRows,
                ImportedRows = x.ImportedRows,
                SkippedRows = x.SkippedRows,
                Status = x.Status.ToString(),
                ImportedAtUtc = x.ImportedAtUtc
            })
            .ToList();

        return result;
    }

    public async Task<ImportJobDetailDto> GetImportByIdAsync(Guid importJobId)
    {
        var userId = _currentUserService.UserId;

        if (!userId.HasValue)
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        var importJob = await _importJobRepository.GetByIdAsync(importJobId, userId.Value);

        if (importJob is null)
        {
            throw new KeyNotFoundException("Import job not found.");
        }

        var transactions = await _transactionRepository.GetByImportJobIdAsync(importJobId, userId.Value);

        return new ImportJobDetailDto
        {
            ImportJobId = importJob.Id,
            FileName = importJob.FileName,
            TotalRows = importJob.TotalRows,
            ImportedRows = importJob.ImportedRows,
            SkippedRows = importJob.SkippedRows,
            Status = importJob.Status.ToString(),
            ImportedAtUtc = importJob.ImportedAtUtc,
            Transactions = transactions
                .Select(x => new ImportJobTransactionDto
                {
                    TransactionId = x.Id,
                    Date = x.Date,
                    Description = x.Description,
                    Amount = x.Amount,
                    CreatedAtUtc = x.CreatedAtUtc
                })
                .ToList()
        };
    }
}