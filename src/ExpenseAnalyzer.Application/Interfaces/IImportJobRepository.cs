using ExpenseAnalyzer.Domain.Entities;

namespace ExpenseAnalyzer.Application.Interfaces;

public interface IImportJobRepository
{
    Task AddAsync(ImportJob importJob);
}