using CSharpFunctionalExtensions;
using FinanceTracker.Application.DTOs;

namespace FinanceTracker.Application.Interfaces.Services;

public interface IUsersService
{
    Task<Result> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    Task<Result<string>> LoginAsync(LoginRequest request, CancellationToken ct = default);
}