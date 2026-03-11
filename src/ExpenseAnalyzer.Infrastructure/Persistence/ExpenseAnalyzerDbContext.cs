using ExpenseAnalyzer.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExpenseAnalyzer.Infrastructure.Persistence;

public class ExpenseAnalyzerDbContext : DbContext
{
    public ExpenseAnalyzerDbContext(DbContextOptions<ExpenseAnalyzerDbContext> options)
        : base(options)
        {
        }

        public DbSet<User> Users => Set<User>(); 
}