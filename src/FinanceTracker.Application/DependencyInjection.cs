using FinanceTracker.Application.Interfaces.Services;
using FinanceTracker.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FinanceTracker.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IUsersService, UsersService>();
        return services;
    }
}