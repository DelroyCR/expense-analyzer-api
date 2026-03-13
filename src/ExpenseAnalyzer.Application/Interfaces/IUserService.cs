using ExpenseAnalyzer.Application.DTOs;

namespace ExpenseAnalyzer.Application.Interfaces;

public interface IUserService{
    Task<RegisterUserResponseDto> RegisterAsync(RegisterUserRequestDto request);
    Task<RegisterUserResponseDto?> GetByIdAsync(Guid id);
}