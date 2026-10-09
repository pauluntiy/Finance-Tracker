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
    public async Task RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        bool userExists = await dbContext.Users
            .AnyAsync(u => u.Email == request.Email, ct);
        if (userExists)
            throw new InvalidOperationException("User with the same email already exists.");
        
        var hashedPassword = passwordHasher.Generate(request.Password);
        
        var user = User.Create(
            Guid.NewGuid(), request.Email, hashedPassword, request.UserName).Value;
        
        await dbContext.Users.AddAsync(user, ct);
        await dbContext.SaveChangesAsync(ct);
    }

    public async Task<string> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        // get user from db
        var user = await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == request.Email, ct);

        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }
        
        // verify password
        var verificationResult = passwordHasher.Verify(user.PasswordHash, request.Password);
        if (!verificationResult)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        // return jwt token
        return jwtProvider.GenerateToken(user);
    }
}