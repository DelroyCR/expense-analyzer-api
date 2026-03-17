using ExpenseAnalyzer.Application.Interfaces;
using ExpenseAnalyzer.Domain.Entities;
using ExpenseAnalyzer.Infrastructure.Persistence;

namespace ExpenseAnalyzer.Infrastructure.Services;

public class TransactionRepository : ITransactionRepository
{
    private readonly ExpenseAnalyzerDbContext _dbContext;
    
    public TransactionRepository(ExpenseAnalyzerDbContext dbContext)
    {
        _dbContext = dbContext;
    }


    public async Task AddRangeAsync(IEnumerable<Transaction> transactions)
    {
        await _dbContext.Transactions.AddRangeAsync(transactions);
    }
}