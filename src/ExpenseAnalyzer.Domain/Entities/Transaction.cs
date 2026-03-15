using ExpenseAnalyzer.Domain.Common;

namespace ExpenseAnalyzer.Domain.Entities;

public class Transaction : BaseEntity{
    public Guid UserId { get; set; }
    public Guid ImportJobId { get; set; }

    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime CreatedAtUtc { get; set; }

    public User User { get; set; } = null!;
    public ImportJob ImportJob { get; set; } = null!;
}