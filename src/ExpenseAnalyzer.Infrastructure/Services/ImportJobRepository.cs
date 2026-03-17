using ExpenseAnalyzer.Application.Interfaces;
using ExpenseAnalyzer.Domain.Entities;
using ExpenseAnalyzer.Infrastructure.Persistence;

namespace ExpenseAnalyzer.Infrastructure.Services;

public class ImportJobRepository : IImportJobRepository
{
    private readonly ExpenseAnalyzerDbContext _dbContext;

    public ImportJobRepository(ExpenseAnalyzerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(ImportJob importJob)
    {
        await _dbContext.ImportJobs.AddAsync(importJob);
    }
}