using CSharpFunctionalExtensions;

namespace FinanceTracker.Domain.Entities;

public class User
{
    private User(Guid id, string email, string passwordHash, string name)
    {
        Id = id;
        Email = email;
        PasswordHash = passwordHash;
        Name = name;
        CreatedAtUtc = DateTime.UtcNow;
    }
    
    public Guid Id { get; }
    public string Email { get; } 
    public string PasswordHash { get; }
    public string Name { get; private set; }
    public DateTime CreatedAtUtc { get; }
    
    public static Result<User> Create(Guid id, string email, string passwordHash, string name)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return Result.Failure<User>("Email cannot be empty");
        }
        
        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            return Result.Failure<User>("Password hash cannot be empty");
        }
        
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<User>("Name cannot be empty");
        }
        
        var user = new User(id, email, passwordHash, name);
        return Result.Success(user);
    }
    
    // navigation properties
    public ICollection<Transaction> Transactions { get; private set; } = new List<Transaction>();
    public ICollection<Category> Categories { get; private set; } = new List<Category>();
    public ICollection<Subscription> Subscriptions { get; private set; } = new List<Subscription>();
}