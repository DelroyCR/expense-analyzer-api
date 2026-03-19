using ExpenseAnalyzer.Application.Interfaces;
using ExpenseAnalyzer.Domain.Entities;
using ExpenseAnalyzer.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

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

    public async Task<IReadOnlyList<ImportJob>> GetByUserIdAsync(Guid userId)
    {
        return await _dbContext.ImportJobs
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.ImportedAtUtc)
            .ToListAsync();
    }

    public async Task<ImportJob?> GetByIdAsync(Guid importJobId, Guid userId)
    {
        return await _dbContext.ImportJobs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == importJobId && x.UserId == userId);
    }
}