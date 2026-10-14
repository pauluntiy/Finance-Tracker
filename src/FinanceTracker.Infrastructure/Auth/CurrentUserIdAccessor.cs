using System.Security.Claims;
using FinanceTracker.Application.Interfaces.Auth;
using Microsoft.AspNetCore.Http;

namespace FinanceTracker.Infrastructure.Auth;

public class CurrentUserIdAccessor(IHttpContextAccessor httpContextAccessor) : ICurrentUserIdAccessor
{
    public Guid UserId
    {
        get
        {
            var idClaim = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? httpContextAccessor.HttpContext?.User?.FindFirstValue("sub");
            return Guid.TryParse(idClaim, out var id) 
                ? id : throw new UnauthorizedAccessException("User claim is missing or invalid");
        }
    }

    public bool IsAuthenticated 
        => httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}