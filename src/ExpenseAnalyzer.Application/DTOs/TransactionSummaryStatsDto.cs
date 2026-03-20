namespace ExpenseAnalyzer.Application.DTOs;

public sealed class TransactionSummaryStatsDto
{
    public int TotalTransactions { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal AverageAmount { get; set; }
    public decimal HighestAmount { get; set; }
    public decimal LowestAmount { get; set; }
}