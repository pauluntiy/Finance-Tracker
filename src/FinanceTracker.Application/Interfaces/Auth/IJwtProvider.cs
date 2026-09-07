using FinanceTracker.Domain.Entities;

namespace FinanceTracker.Application.Interfaces.Auth;

public interface IJwtProvider
{
    string GenerateToken(User user);
}