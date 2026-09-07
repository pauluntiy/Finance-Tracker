using FinanceTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Application.Interfaces.Persistence;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Transaction> Transactions { get; }
    DbSet<Category> Categories { get; }
    DbSet<Subscription> Subscriptions { get; }
    
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}