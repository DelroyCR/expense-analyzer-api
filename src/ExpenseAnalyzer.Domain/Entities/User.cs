using ExpenseAnalyzer.Domain.Common;

namespace ExpenseAnalyzer.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }

    public ICollection<ImportJob> ImportJobs { get; set; } = new List<ImportJob>();
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}