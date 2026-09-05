using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Domain.Entities;

public class Category
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string ColorHex { get; private set; } = "#808080";
    public TransactionType TransactionType { get; private set; }
    
    // navigation properties
    public Guid? UserId { get; private set; }
    public User? User { get; private set; }
    
    public ICollection<Transaction> Transactions { get; private set; } = new List<Transaction>();
    public ICollection<Subscription> Subscriptions { get; private set; } = new List<Subscription>();
}