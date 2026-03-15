using System.Security.Claims;
using ExpenseAnalyzer.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace ExpenseAnalyzer.Api.Services;

public class CurrentUserService : ICurrentUserService 
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId{
        get{
            var userIdValue = _httpContextAccessor
            .HttpContext?
            .User?
            .FindFirstValue(ClaimTypes.NameIdentifier);

            if (Guid.TryParse(userIdValue, out var userId))
            {
                return userId;
            }

            return null;
        }
    }

    public string? Email =>
        _httpContextAccessor
        .HttpContext?
        .User?
        .FindFirstValue(ClaimTypes.Email);

    public bool IsAuthenticated =>
        _httpContextAccessor
        .HttpContext?
        .User?
        .Identity?
        .IsAuthenticated ?? false;
}