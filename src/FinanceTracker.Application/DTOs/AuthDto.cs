namespace FinanceTracker.Application.DTOs;

public record RegisterRequest(string Email, string Password, string UserName);
public record LoginRequest(string Email, string Password);
public record UserResponse(Guid Id, string? Email);