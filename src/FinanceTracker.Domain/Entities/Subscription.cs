using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Domain.Entities;

public class Subscription
{
    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public decimal Amount { get; private set; }
    public BillingPeriod Period { get; private set; }
    public DateOnly NextPaymentDate { get; private set; }
    public bool IsActive { get; private set; } = true;
    
    // navigation properties
    public Guid CategoryId { get; private set; }
    public Category Category { get; private set; } = null!;

    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;
}