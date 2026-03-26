using ExpenseAnalyzer.Domain.Common;
using ExpenseAnalyzer.Domain.Enums;

namespace ExpenseAnalyzer.Domain.Entities;

public class ImportJob : BaseEntity
{
    public Guid UserId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public DateTime ImportedAtUtc { get; set; }

    public int TotalRows { get; set; }
    public int ImportedRows { get; set; }
    public int SkippedRows { get; set; }

    public ImportJobStatus Status { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public User User { get; set; } = null!;
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}