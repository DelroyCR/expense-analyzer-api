using ExpenseAnalyzer.Application.Common.Exceptions;
using ExpenseAnalyzer.Application.DTOs;
using ExpenseAnalyzer.Application.Interfaces;
using ExpenseAnalyzer.Domain.Entities;

namespace ExpenseAnalyzer.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasherService _passwordHasherService;
    private readonly ITokenService _tokenService;

    public UserService(
        IUserRepository userRepository,
        IPasswordHasherService passwordHasherService,
        ITokenService tokenService
        )
    {
        _userRepository = userRepository;
        _passwordHasherService = passwordHasherService;
        _tokenService = tokenService;
    }

    public async Task<RegisterUserResponseDto> RegisterAsync(RegisterUserRequestDto request)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ValidationException("Email is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ValidationException("Password is required.");
        }

        if (request.Password.Length < 8)
        {
            throw new ValidationException("Password must be at least 8 characters long.");
        }

        var emailAlreadyExists = await _userRepository.EmailExistsAsync(email);

        if (emailAlreadyExists)
        {
            throw new ConflictException("A user with this email already exists.");
        }

        var user = new User
        {
            Email = email,
            PasswordHash = _passwordHasherService.HashPassword(request.Password),
            CreatedAtUtc = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user);

        return new RegisterUserResponseDto
        {
            Id = user.Id,
            Email = user.Email,
            CreatedAtUtc = user.CreatedAtUtc
        };
    }

    public async Task<RegisterUserResponseDto?> GetByIdAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);

        if (user is null)
        {
            return null;
        }

        return new RegisterUserResponseDto
        {
            Id = user.Id,
            Email = user.Email,
            CreatedAtUtc = user.CreatedAtUtc
        };
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ValidationException("Email is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ValidationException("Password is required.");
        }

        var user = await _userRepository.GetByEmailAsync(email);

        if (user is null)
        {
            throw new UnauthorizedException("Invalid email or password.");
        }

        var passwordIsValid = _passwordHasherService.VerifyPassword(
            user.PasswordHash,
            request.Password);

        if (!passwordIsValid)
        {
            throw new UnauthorizedException("Invalid email or password.");
        }

        return new LoginResponseDto
        {
            Id = user.Id,
            Email = user.Email,
            Token = _tokenService.GenerateToken(user),
            Message = "Login successful."
        };
    }
}