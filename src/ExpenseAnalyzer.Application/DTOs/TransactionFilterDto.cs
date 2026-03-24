namespace ExpenseAnalyzer.Application.DTOs;

public sealed class TransactionFilterDto
{
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public string? Description { get; set; }
    public Guid? ImportJobId { get; set; }

    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;

    public string? SortBy { get; set; } = "date";
    public string? SortDirection { get; set; } = "desc";
}