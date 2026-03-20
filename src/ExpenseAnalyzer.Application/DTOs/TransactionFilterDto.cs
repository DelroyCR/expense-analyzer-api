namespace ExpenseAnalyzer.Application.DTOs;

public sealed class TransactionFilterDto
{
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public decimal? MinAmount { get; set; }
}