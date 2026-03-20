using System.Linq;
using ExpenseAnalyzer.Application.DTOs;
using ExpenseAnalyzer.Application.Interfaces;

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

    public async Task<IReadOnlyList<TransactionSummaryDto>> GetTransactionsAsync(TransactionFilterDto filter)
    {
        var userId = _currentUserService.UserId;

        if (!userId.HasValue)
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        var transactions = await _transactionRepository.GetByUserIdAsync(userId.Value, filter);

        return transactions
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
}