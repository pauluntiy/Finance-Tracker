using CSharpFunctionalExtensions;
using FinanceTracker.Application.DTOs.Auth;
using FinanceTracker.Application.Interfaces.Auth;
using FinanceTracker.Application.Interfaces.Persistence;
using FinanceTracker.Application.Interfaces.Services;
using FinanceTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Application.Services;

public class UsersService : IUsersService
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtProvider _jwtProvider;

    public UsersService(IApplicationDbContext dbContext, IPasswordHasher passwordHasher, 
        IJwtProvider jwtProvider)
    {
        _passwordHasher = passwordHasher;
        _dbContext = dbContext;
        _jwtProvider = jwtProvider;
    }

    public async Task RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        bool userExists = await _dbContext.Users
            .AnyAsync(u => u.Email == request.Email, ct);
        if (userExists)
            throw new InvalidOperationException("User with the same email already exists.");
        
        var hashedPassword = _passwordHasher.Generate(request.Password);
        
        var user = User.Create(
            Guid.NewGuid(), request.Email, hashedPassword, request.UserName).Value;
        
        await _dbContext.Users.AddAsync(user, ct);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task<string> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        // get user from db
        var user = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == request.Email, ct);

        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }
        
        // verify password
        var verificationResult = _passwordHasher.Verify(user.PasswordHash, request.Password);
        if (!verificationResult)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        // return jwt token
        return _jwtProvider.GenerateToken(user);
    }
}