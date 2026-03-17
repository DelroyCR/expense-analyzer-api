using ExpenseAnalyzer.Application.Interfaces;
using ExpenseAnalyzer.Infrastructure.Persistence;

namespace ExpenseAnalyzer.Infrastructure.Services;

public class UnitOfWork : IUnitOfWork
{
    private readonly ExpenseAnalyzerDbContext _dbContext;

    public UnitOfWork(ExpenseAnalyzerDbContext dbContext){
        _dbContext = dbContext;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}