namespace ExpenseAnalyzer.Application.DTOs;

public class LoginResponseDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string message { get; set; } = string.Empty;
}