using CSharpFunctionalExtensions;
using FinanceTracker.Application.Common;
using FinanceTracker.Application.DTOs;
using FinanceTracker.Application.Interfaces.Auth;
using FinanceTracker.Application.Interfaces.Persistence;
using FinanceTracker.Application.Interfaces.Services;
using FinanceTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Application.Services;

public class UsersService(
    IApplicationDbContext dbContext,
    IPasswordHasher passwordHasher,
    IJwtProvider jwtProvider)
    : IUsersService
{
    public async Task<Result<UserResponse, AppError>> RegisterAsync(
        RegisterRequest request,
        CancellationToken ct = default)
    {
        bool userExists = await dbContext.Users
            .AnyAsync(u => u.Email == request.Email, ct);

        if (userExists)
            return Result.Failure<UserResponse, AppError>(
                AppError.Conflict("User with the same email already exists."));

        var hashedPassword = passwordHasher.Generate(request.Password);

        var user = User.Create(
            Guid.NewGuid(), request.Email, hashedPassword, request.UserName);

        if (user.IsFailure)
            return Result.Failure<UserResponse, AppError>(
                AppError.Validation(user.Error));

        await dbContext.Users.AddAsync(user.Value, ct);
        await dbContext.SaveChangesAsync(ct);

        return Result.Success<UserResponse, AppError>(
            new UserResponse(user.Value.Id, user.Value.Email));
    }

    public async Task<Result<LoginResult, AppError>> LoginAsync(
        LoginRequest request,
        CancellationToken ct = default)
    {
        var user = await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == request.Email, ct);

        if (user == null)
            return Result.Failure<LoginResult, AppError>(
                AppError.Unauthorized("Invalid email or password."));

        var verificationResult = passwordHasher.Verify(user.PasswordHash, request.Password);
        if (!verificationResult)
            return Result.Failure<LoginResult, AppError>(
                AppError.Unauthorized("Invalid email or password."));

        var token = jwtProvider.GenerateToken(user);

        return Result.Success<LoginResult, AppError>(
            new LoginResult(token, new UserResponse(user.Id, user.Email)));
    }
}
