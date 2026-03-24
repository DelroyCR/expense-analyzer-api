using ExpenseAnalyzer.Application.Interfaces;
using ExpenseAnalyzer.Domain.Entities;
using ExpenseAnalyzer.Infrastructure.Persistence;
using ExpenseAnalyzer.Application.DTOs;
using Microsoft.EntityFrameworkCore;

namespace ExpenseAnalyzer.Infrastructure.Services;

public class TransactionRepository : ITransactionRepository
{
    private readonly ExpenseAnalyzerDbContext _dbContext;

    public TransactionRepository(ExpenseAnalyzerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddRangeAsync(IEnumerable<Transaction> transactions)
    {
        await _dbContext.Transactions.AddRangeAsync(transactions);
    }

    public async Task<IReadOnlyList<Transaction>> GetByUserIdAsync(Guid userId)
    {
        return await _dbContext.Transactions
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.Date)
            .ThenByDescending(x => x.CreatedAtUtc)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Transaction>> GetByUserIdAsync(Guid userId, TransactionFilterDto filter)
    {
        var query = BuildFilteredQuery(userId, filter);
        query = ApplySorting(query, filter);

        return await query.ToListAsync();
    }

    public async Task<(IReadOnlyList<Transaction> Items, int TotalCount)> GetPagedByUserIdAsync(Guid userId, TransactionFilterDto filter)
    {
        var query = BuildFilteredQuery(userId, filter);

        var totalCount = await query.CountAsync();

        query = ApplySorting(query, filter);

        var items = await query
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<Transaction?> GetByIdAsync(Guid transactionId, Guid userId)
    {
        return await _dbContext.Transactions
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == transactionId && x.UserId == userId);
    }

    public async Task<IReadOnlyList<Transaction>> GetByImportJobIdAsync(Guid importJobId, Guid userId)
    {
        return await _dbContext.Transactions
            .AsNoTracking()
            .Where(x => x.ImportJobId == importJobId && x.UserId == userId)
            .OrderByDescending(x => x.Date)
            .ThenByDescending(x => x.CreatedAtUtc)
            .ToListAsync();
    }

    private IQueryable<Transaction> BuildFilteredQuery(Guid userId, TransactionFilterDto filter)
    {
        var query = _dbContext.Transactions
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .AsQueryable();

        if (filter.From.HasValue)
        {
            query = query.Where(x => x.Date >= filter.From.Value);
        }

        if (filter.To.HasValue)
        {
            query = query.Where(x => x.Date <= filter.To.Value);
        }

        if (filter.MinAmount.HasValue)
        {
            query = query.Where(x => x.Amount >= filter.MinAmount.Value);
        }

        if (filter.MaxAmount.HasValue)
        {
            query = query.Where(x => x.Amount <= filter.MaxAmount.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Description))
        {
            var description = filter.Description.Trim();
            query = query.Where(x => EF.Functions.ILike(x.Description, $"%{description}%"));
        }

        if (filter.ImportJobId.HasValue)
        {
            query = query.Where(x => x.ImportJobId == filter.ImportJobId.Value);
        }

        return query;
    }

    private static IQueryable<Transaction> ApplySorting(
        IQueryable<Transaction> query,
        TransactionFilterDto filter)
    {
        var sortBy = (filter.SortBy ?? "date").Trim().ToLower();
        var sortDirection = (filter.SortDirection ?? "desc").Trim().ToLower();

        return (sortBy, sortDirection) switch
        {
            ("date", "asc") => query
                .OrderBy(x => x.Date)
                .ThenBy(x => x.CreatedAtUtc),

            ("date", "desc") => query
                .OrderByDescending(x => x.Date)
                .ThenByDescending(x => x.CreatedAtUtc),

            ("amount", "asc") => query
                .OrderBy(x => x.Amount)
                .ThenByDescending(x => x.CreatedAtUtc),

            ("amount", "desc") => query
                .OrderByDescending(x => x.Amount)
                .ThenByDescending(x => x.CreatedAtUtc),

            _ => query
                .OrderByDescending(x => x.Date)
                .ThenByDescending(x => x.CreatedAtUtc)
        };
    }
}