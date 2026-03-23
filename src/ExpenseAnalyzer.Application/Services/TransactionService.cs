using System.Linq;
using ExpenseAnalyzer.Application.DTOs;
using ExpenseAnalyzer.Application.Interfaces;
using ExpenseAnalyzer.Application.Common.Exceptions;

namespace ExpenseAnalyzer.Application.Services;

public class TransactionService : ITransactionService
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ITransactionRepository _transactionRepository;

    public TransactionService(
        ICurrentUserService currentUserService,
        ITransactionRepository transactionRepository)
    {
        _currentUserService = currentUserService;
        _transactionRepository = transactionRepository;
    }

 public async Task<PagedResultDto<TransactionSummaryDto>> GetTransactionsAsync(TransactionFilterDto filter)
{
    var userId = _currentUserService.UserId;

    if (!userId.HasValue)
    {
        throw new UnauthorizedAccessException("User is not authenticated.");
    }

    ValidateTransactionFilter(filter);
    ValidatePagination(filter);

    var (transactions, totalCount) =
        await _transactionRepository.GetPagedByUserIdAsync(userId.Value, filter);

    var items = transactions
        .Select(x => new TransactionSummaryDto
        {
            TransactionId = x.Id,
            ImportJobId = x.ImportJobId,
            Date = x.Date,
            Description = x.Description,
            Amount = x.Amount,
            CreatedAtUtc = x.CreatedAtUtc
        })
        .ToList();

    var result = new PagedResultDto<TransactionSummaryDto>
    {
        Items = items,
        TotalCount = totalCount,
        PageNumber = filter.PageNumber,
        PageSize = filter.PageSize
    };

    if (result.TotalCount == 0)
    {
        result.Message = "No transactions were found for the provided filters.";
    }
    else if (result.PageNumber > result.TotalPages)
    {
        result.IsPageOutOfRange = true;
        result.Message = $"There are no records on page {result.PageNumber}. With the current filters, only {result.TotalPages} page(s) exist.";
    }

    return result;
}

    public async Task<TransactionDetailDto> GetTransactionByIdAsync(Guid transactionId)
    {
        var userId = _currentUserService.UserId;

        if (!userId.HasValue)
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        var transaction = await _transactionRepository.GetByIdAsync(transactionId, userId.Value);

        if (transaction is null)
        {
            throw new KeyNotFoundException("Transaction not found.");
        }

        return new TransactionDetailDto
        {
            TransactionId = transaction.Id,
            ImportJobId = transaction.ImportJobId,
            Date = transaction.Date,
            Description = transaction.Description,
            Amount = transaction.Amount,
            CreatedAtUtc = transaction.CreatedAtUtc
        };
    }

    public async Task<TransactionSummaryStatsDto> GetTransactionSummaryAsync(TransactionFilterDto filter)
    {
        var userId = _currentUserService.UserId;

        if (!userId.HasValue)
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        ValidateTransactionFilter(filter);

        var transactions = await _transactionRepository.GetByUserIdAsync(userId.Value, filter);

        if (!transactions.Any())
        {
            return new TransactionSummaryStatsDto
            {
                TotalTransactions = 0,
                TotalAmount = 0,
                AverageAmount = 0,
                HighestAmount = 0,
                LowestAmount = 0
            };
        }

        return new TransactionSummaryStatsDto
        {
            TotalTransactions = transactions.Count,
            TotalAmount = transactions.Sum(x => x.Amount),
            AverageAmount = transactions.Average(x => x.Amount),
            HighestAmount = transactions.Max(x => x.Amount),
            LowestAmount = transactions.Min(x => x.Amount)
        };
    }

    private static void ValidateTransactionFilter(TransactionFilterDto filter)
    {
        if (filter.From.HasValue && filter.To.HasValue && filter.From.Value > filter.To.Value)
        {
            throw new ValidationException("The 'from' date cannot be greater than the 'to' date.");
        }

        if (filter.MinAmount.HasValue && filter.MinAmount.Value < 0)
        {
            throw new ValidationException("The minimum amount cannot be negative.");
        }

        if (filter.MaxAmount.HasValue && filter.MaxAmount.Value < 0)
        {
            throw new ValidationException("The maximum amount cannot be negative.");
        }

        if (filter.MinAmount.HasValue && filter.MaxAmount.HasValue &&
            filter.MinAmount.Value > filter.MaxAmount.Value)
        {
            throw new ValidationException("The minimum amount cannot be greater than the maximum amount.");
        }
    }

    private static void ValidatePagination(TransactionFilterDto filter)
    {
        if (filter.PageNumber < 1)
        {
            throw new ValidationException("The page number must be greater than 0.");
        }

        if (filter.PageSize < 1)
        {
            throw new ValidationException("The page size must be greater than 0.");
        }

        if (filter.PageSize > 100)
        {
            throw new ValidationException("The page size cannot be greater than 100.");
        }
    }
}