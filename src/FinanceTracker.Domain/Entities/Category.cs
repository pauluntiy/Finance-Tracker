using CSharpFunctionalExtensions;
using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Domain.Entities;

public class Category
{
    private Category(Guid id, string name, string colorHex, CategoryType type)
    {
        Id = id;
        Name = name;
        ColorHex = colorHex;
        Type = type;
    }
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string ColorHex { get; private set; }
    public CategoryType Type { get; private set; }

    public static Result<Category> Create(Guid id, string name, string? colorHex, CategoryType type)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Category>("Category name cannot be empty");
        
        var result = new Category(id, name, colorHex ?? "#808080",  type);
        
        return Result.Success(result);
    }
    
    // navigation properties
    public Guid? UserId { get; private set; }
    public User? User { get; private set; }
    
    public ICollection<Transaction> Transactions { get; private set; } = new List<Transaction>();
    public ICollection<Subscription> Subscriptions { get; private set; } = new List<Subscription>();
}