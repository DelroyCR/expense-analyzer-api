using ExpenseAnalyzer.Application.DTOs;
using ExpenseAnalyzer.Domain.Entities;

namespace ExpenseAnalyzer.Application.Interfaces;

public interface ITransactionRepository
{
    Task AddRangeAsync(IEnumerable<Transaction> transactions);
    Task<IReadOnlyList<Transaction>> GetByUserIdAsync(Guid userId);
    Task<IReadOnlyList<Transaction>> GetByUserIdAsync(Guid userId, TransactionFilterDto filter);
    Task<(IReadOnlyList<Transaction> Items, int TotalCount)> GetPagedByUserIdAsync(Guid userId, TransactionFilterDto filter);
    Task<Transaction?> GetByIdAsync(Guid transactionId, Guid userId);
    Task<IReadOnlyList<Transaction>> GetByImportJobIdAsync(Guid importJobId, Guid userId);
}