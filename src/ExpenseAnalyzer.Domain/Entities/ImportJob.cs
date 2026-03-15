using ExpenseAnalyzer.Domain.Common;

namespace ExpenseAnalyzer.Domain.Entities;

public class ImportJob : BaseEntity
{
    public Guid UserId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public DateTime ImportedAtUtc { get; set; }

    public User User { get; set; } = null!;
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}