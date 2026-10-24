using CSharpFunctionalExtensions;
using FinanceTracker.Application.Common;
using FinanceTracker.Application.DTOs;

namespace FinanceTracker.Application.Interfaces.Services;

public interface IUsersService
{
    Task<Result<UserResponse, AppError>> RegisterAsync(
        RegisterRequest request,
        CancellationToken ct = default);

    Task<Result<LoginResult, AppError>> LoginAsync(
        LoginRequest request,
        CancellationToken ct = default);
}
