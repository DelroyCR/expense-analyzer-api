namespace ExpenseAnalyzer.Application.DTOs;

public sealed class ImportJobDetailDto
{
    public Guid ImportJobId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public int TotalRows { get; set; }
    public int ImportedRows { get; set; }
    public int SkippedRows { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime ImportedAtUtc { get; set; }

    public IReadOnlyList<ImportJobTransactionDto> Transactions { get; set; }
        = new List<ImportJobTransactionDto>();
}