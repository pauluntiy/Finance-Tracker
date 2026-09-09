namespace FinanceTracker.Application.Interfaces.Auth;

public interface ICurrentUserIdAccessor
{
    Guid UserId { get; }
    bool IsAuthenticated { get; }
}