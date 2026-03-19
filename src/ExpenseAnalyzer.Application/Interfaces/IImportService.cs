using ExpenseAnalyzer.Application.DTOs;

namespace ExpenseAnalyzer.Application.Interfaces;

public interface IImportService
{
    Task<ImportCsvResponseDto> ImportCsvAsync(Stream fileStream, string fileName);
    Task<IReadOnlyList<ImportJobSummaryDto>> GetImportHistoryAsync();
    Task<ImportJobDetailDto> GetImportByIdAsync(Guid importJobId);
}