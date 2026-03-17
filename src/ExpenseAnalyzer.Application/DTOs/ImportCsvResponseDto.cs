namespace ExpenseAnalyzer.Application.DTOs;

public class ImportCsvResponseDto
{
    public Guid ImportJobId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public int ImportedCount { get; set; }
    public int SkippedCount { get; set; }
    public DateTime ImportedAtUtc { get; set; }
    public IReadOnlyList<ImportCsvRowErrorDto> Errors { get; init; } = [];
}