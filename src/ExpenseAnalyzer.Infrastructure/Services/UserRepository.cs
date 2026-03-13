using ExpenseAnalyzer.Application.Interfaces;
using ExpenseAnalyzer.Domain.Entities;
using ExpenseAnalyzer.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ExpenseAnalyzer.Infrastructure.Services;

public class UserRepository : IUserRepository
{
    private readonly ExpenseAnalyzerDbContext _dbContext;

    public UserRepository(ExpenseAnalyzerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _dbContext.Users.AnyAsync(u => u.Email == email);
    }

    public async Task AddAsync(User user)
    {
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id);
    }
}