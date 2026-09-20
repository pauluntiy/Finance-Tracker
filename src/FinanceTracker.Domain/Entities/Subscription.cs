using CSharpFunctionalExtensions;
using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Domain.Entities;

public class Subscription
{
    private Subscription(Guid id, string title, decimal amount, BillingPeriod billingPeriod,
        DateOnly date, bool isActive, Guid categoryId, Guid userId)
    {
        Id = id;
        Title = title;
        Amount = amount;
        Period = billingPeriod;
        NextPaymentDate = date;
        IsActive = isActive;
        CategoryId = categoryId;
        UserId = userId;
    }
    
    public Guid Id { get; private set; }
    public string Title { get; private set; }
    public decimal Amount { get; private set; }
    public BillingPeriod Period { get; private set; }
    public DateOnly NextPaymentDate { get; private set; }
    public bool IsActive { get; private set; }
    public Guid CategoryId { get; private set; }
    public Guid UserId { get; private set; }

    public static Result<Subscription> Create(Guid id, string title, decimal amount, BillingPeriod billingPeriod,
        DateOnly date, bool isActive, Guid categoryId, Guid userId)
    {
        if (id == Guid.Empty)
            return Result.Failure<Subscription>("Id cannot be empty");
        
        if (string.IsNullOrWhiteSpace(title))
            return Result.Failure<Subscription>("Title cannot be empty");
        
        if (amount < 0)
            return Result.Failure<Subscription>("Amount cannot be negative");
        
        if (categoryId == Guid.Empty)
            return Result.Failure<Subscription>("CategoryId cannot be empty");
        
        if (userId == Guid.Empty)
            return Result.Failure<Subscription>("UserId cannot be empty");
        
        var result = new Subscription(id, title, amount, billingPeriod, date, isActive, categoryId, userId);
        return Result.Success(result);
    }
    
    // navigation properties
    public Category Category { get; private set; } = null!;
    public User User { get; private set; } = null!;
}