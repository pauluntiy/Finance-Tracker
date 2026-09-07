namespace FinanceTracker.Application.Interfaces.Auth;

public interface IPasswordHasher
{
    string Generate(string password);
    bool Verify(string passwordHash, string password);
}