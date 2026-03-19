using ExpenseAnalyzer.Domain.Entities;

namespace ExpenseAnalyzer.Application.Interfaces;

public interface IImportJobRepository
{
    Task AddAsync(ImportJob importJob);
    Task<IReadOnlyList<ImportJob>> GetByUserIdAsync(Guid userId);
    Task<ImportJob?> GetByIdAsync(Guid importJobId, Guid userId);
}