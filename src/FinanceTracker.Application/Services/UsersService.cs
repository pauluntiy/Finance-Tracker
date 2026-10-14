using CSharpFunctionalExtensions;
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
    public async Task<Result> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        bool userExists = await dbContext.Users
            .AnyAsync(u => u.Email == request.Email, ct);
        if (userExists)
            return Result.Failure("User with the same email already exists.");
        
        var hashedPassword = passwordHasher.Generate(request.Password);
        
        var user = User.Create(
            Guid.NewGuid(), request.Email, hashedPassword, request.UserName);
        
        if (user.IsFailure)
            return Result.Failure(user.Error);
        
        await dbContext.Users.AddAsync(user.Value, ct);
        await dbContext.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result<string>> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        // get user from db
        var user = await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == request.Email, ct);

        if (user == null)
            return Result.Failure<string>("Invalid email or password.");
        
        // verify password
        var verificationResult = passwordHasher.Verify(user.PasswordHash, request.Password);
        if (!verificationResult)
            return Result.Failure<string>("Invalid email or password.");

        return Result.Success(jwtProvider.GenerateToken(user));
    }
}