namespace ExpenseAnalyzer.Application.DTOs;

public class RegisterUserRequestDto 
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}