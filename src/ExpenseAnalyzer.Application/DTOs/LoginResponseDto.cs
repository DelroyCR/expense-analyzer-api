namespace ExpenseAnalyzer.Application.DTOs;

public class LoginResponseDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}