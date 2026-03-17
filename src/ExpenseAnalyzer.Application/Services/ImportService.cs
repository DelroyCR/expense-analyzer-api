using System.Globalization;
using ExpenseAnalyzer.Application.Common.Exceptions;
using ExpenseAnalyzer.Application.DTOs;
using ExpenseAnalyzer.Application.Interfaces;
using ExpenseAnalyzer.Domain.Entities;

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

        if (fileStream is null || fileStream.Length == 0)
        {
            throw new ValidationException("A non-empty CSV file is required.");
        }

        if (!Path.GetExtension(fileName).Equals(".csv", StringComparison.OrdinalIgnoreCase))
        {
            throw new ValidationException("Only .csv files are allowed.");
        }

        var userId = _currentUserService.UserId.Value;
        var importedAtUtc = DateTime.UtcNow;

        var importJob = new ImportJob
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            FileName = fileName,
            ImportedAtUtc = importedAtUtc
        };

        var transactions = new List<Transaction>();
        var skippedCount = 0;

        using var reader = new StreamReader(fileStream);

        var headerLine = await reader.ReadLineAsync();

        if (string.IsNullOrWhiteSpace(headerLine))
        {
            throw new ValidationException("The CSV file is empty or missing a header row.");
        }

        const string expectedHeader = "Date,Description,Amount";

        if (!headerLine.Equals(expectedHeader, StringComparison.OrdinalIgnoreCase))
        {
            throw new ValidationException("The CSV header must be exactly: Date,Description,Amount");
        }

        string? line;
        while ((line = await reader.ReadLineAsync()) is not null)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                skippedCount++;
                continue;
            }

            var columns = line.Split(',');

            if (columns.Length != 3)
            {
                skippedCount++;
                continue;
            }

            var dateText = columns[0].Trim();
            var description = columns[1].Trim();
            var amountText = columns[2].Trim();

            var isDateValid = DateTime.TryParse(
                dateText,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var parsedDate);

            var isAmountValid = decimal.TryParse(
                amountText,
                NumberStyles.Number | NumberStyles.AllowLeadingSign,
                CultureInfo.InvariantCulture,
                out var amount);

            if (!isDateValid || string.IsNullOrWhiteSpace(description) || !isAmountValid)
            {
                skippedCount++;
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

        await _importJobRepository.AddAsync(importJob);

        if (transactions.Count > 0)
        {
            await _transactionRepository.AddRangeAsync(transactions);
        }

        await _unitOfWork.SaveChangesAsync();

        return new ImportCsvResponseDto
        {
            ImportJobId = importJob.Id,
            FileName = importJob.FileName,
            ImportedCount = transactions.Count,
            SkippedCount = skippedCount,
            ImportedAtUtc = importJob.ImportedAtUtc
        };
    }
}