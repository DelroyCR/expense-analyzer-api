using ExpenseAnalyzer.Domain.Entities;

namespace ExpenseAnalyzer.Application.Interfaces;

public interface ITokenService
{
    string GenerateToken(User user);
}