namespace ExpenseAnalyzer.Application.DTOs;

public sealed class ImportJobTransactionDto
{
    public Guid TransactionId { get; set; }
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}