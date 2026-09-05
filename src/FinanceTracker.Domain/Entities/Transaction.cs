using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Domain.Entities;

public class Transaction
{
    public Guid Id { get; private set; }
    public decimal Amount { get; private set; }
    public string? Description { get; private set; }
    public DateTime DateUtc { get; private set; }
    
    // navigation properties
    public Guid CategoryId { get; private set; }
    public Category Category { get; private set; } = null!;
    
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;

}