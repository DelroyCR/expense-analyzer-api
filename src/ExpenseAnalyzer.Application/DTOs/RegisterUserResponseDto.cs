namespace ExpenseAnalyzer.Application.DTOs;

public class RegisterUserResponseDto {
 public Guid Id { get; set; }
 public string Email { get; set; } = string.Empty;
 public DateTime CreatedAtUtc { get; set; }   
}