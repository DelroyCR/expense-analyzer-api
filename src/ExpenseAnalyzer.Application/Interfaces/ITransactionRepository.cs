using ExpenseAnalyzer.Domain.Entities;

namespace ExpenseAnalyzer.Application.Interfaces;

public interface ITransactionRepository
{
    Task AddRangeAsync(IEnumerable<Transaction> transactions);
}