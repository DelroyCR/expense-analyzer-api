namespace ExpenseAnalyzer.Application.DTOs;

public class ImportJobSummaryDto
{
    public Guid ImportJobId { get; init; }
    public string FileName { get; init; } = string.Empty;
    public int TotalRows { get; init; }
    public int ImportedRows { get; init; }
    public int SkippedRows { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime ImportedAtUtc { get; init; }
}