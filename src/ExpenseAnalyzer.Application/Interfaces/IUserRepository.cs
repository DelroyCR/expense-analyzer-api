using ExpenseAnalyzer.Domain.Entities;

namespace ExpenseAnalyzer.Application.Interfaces;

public interface IUserRepository
{
    Task<bool> EmailExistsAsync(string email);
    Task AddAsync(User user);
    Task<User?> GetByIdAsync(Guid id);
}