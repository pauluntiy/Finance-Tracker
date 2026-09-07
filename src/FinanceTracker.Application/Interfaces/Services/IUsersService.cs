using FinanceTracker.Application.DTOs.Auth;

namespace FinanceTracker.Application.Interfaces.Services;

public interface IUsersService
{
    Task RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    Task<string> LoginAsync(LoginRequest request, CancellationToken ct = default);
}