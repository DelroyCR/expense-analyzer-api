using ExpenseAnalyzer.Domain.Entities;
using ExpenseAnalyzer.Application.DTOs;

namespace ExpenseAnalyzer.Application.Interfaces;

public interface ITransactionRepository
{
    Task AddRangeAsync(IEnumerable<Transaction> transactions);
    Task<IReadOnlyList<Transaction>> GetByUserIdAsync(Guid userId);
    Task<IReadOnlyList<Transaction>> GetByUserIdAsync(Guid userId, TransactionFilterDto filter);
    Task<Transaction?> GetByIdAsync(Guid transactionId, Guid userId);
}