using CSharpFunctionalExtensions;

namespace FinanceTracker.Domain.Entities;

public class Transaction
{
    private Transaction(Guid id, decimal amount, string? description, DateTime dateUtc, 
        Guid categoryId, Guid userId)
    {
        Id = id;
        Amount = amount;
        Description = description;
        DateUtc = dateUtc;
        CategoryId = categoryId;
        UserId = userId;
    }
    
    public Guid Id { get; private set; }
    public decimal Amount { get; private set; }
    public string? Description { get; private set; }
    public DateTime DateUtc { get; private set; }
    public Guid CategoryId { get; private set; }
    public Guid UserId { get; private set; }

    public static Result<Transaction> Create(Guid id, decimal amount, string? description, DateTime date, 
        Guid categoryId, Guid userId)
    {
        if (id == Guid.Empty)
            return Result.Failure<Transaction>("Id cannot be empty");
        
        if (amount < 0)
            return Result.Failure<Transaction>("Amount cannot be negative");
        
        if (string.IsNullOrWhiteSpace(description))
            return Result.Failure<Transaction>("Description cannot be empty");

        if (userId == Guid.Empty)
            return Result.Failure<Transaction>("User ID cannot be empty");
        
        if (categoryId == Guid.Empty)
            return Result.Failure<Transaction>("CategoryID cannot be empty");
        
        var result = new Transaction(id, amount, description, date, categoryId, userId);
        return Result.Success(result);
    }
    
    // navigation properties
    public Category Category { get; private set; } = null!;
    public User User { get; private set; } = null!;
}