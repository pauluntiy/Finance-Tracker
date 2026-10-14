using FinanceTracker.Application.Interfaces;
using FinanceTracker.Application.Interfaces.Auth;

namespace FinanceTracker.Infrastructure.Auth;

public class PasswordHasher : IPasswordHasher
{
    public string Generate(string password) =>
        BCrypt.Net.BCrypt.EnhancedHashPassword(password);

    public bool Verify(string passwordHash, string password) =>
        BCrypt.Net.BCrypt.EnhancedVerify( password, passwordHash);
}