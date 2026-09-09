using CSharpFunctionalExtensions;
using FinanceTracker.Application.DTOs;
using FinanceTracker.Application.Interfaces.Auth;
using FinanceTracker.Application.Interfaces.Persistence;
using FinanceTracker.Application.Interfaces.Services;
using FinanceTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Application.Services;

public class CategoriesService(IApplicationDbContext dbContext, 
    ICurrentUserIdAccessor currentUserIdAccessor) : ICategoriesService
{
    public async Task<Result<IReadOnlyCollection<CategoryDto>>> GetAllAsync(CancellationToken ct = default)
    {
        var userId = currentUserIdAccessor.UserId;

        var categories = await dbContext.Categories
            .AsNoTracking()
            .Where(c => c.UserId == null || c.UserId == userId)
            .OrderBy(c => c.UserId != null)
            .ThenBy(c => c.Name)
            .Select(c => new CategoryDto(c.Id, c.Name, c.Type, c.ColorHex, c.UserId == null, c.UserId))
            .ToListAsync(ct);

        return Result.Success<IReadOnlyCollection<CategoryDto>>(categories);
    }

    public async Task<Result<CategoryDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var userId = currentUserIdAccessor.UserId;

        var category = await dbContext.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id 
                                && (c.UserId == null || c.UserId == userId), ct);
        
        if (category == null)
            return Result.Failure<CategoryDto>("Category not found");
        
        var dto = new CategoryDto(
            category.Id, 
            category.Name, 
            category.Type, 
            category.ColorHex, 
            category.UserId == null, 
            category.UserId);

        return Result.Success(dto);
    }

    public async Task<Result<CategoryDto>> CreateAsync(CreateCategoryRequest request, CancellationToken ct = default)
    {
        var userId = currentUserIdAccessor.UserId;
        
        bool isDuplicate = await dbContext.Categories
            .AnyAsync(c => (c.UserId == userId || c.UserId == null) 
                           && c.Name.ToLower() == request.Name.ToLower() 
                           && c.Type == request.Type, ct);
        
        if (isDuplicate)
            return Result.Failure<CategoryDto>("Category already exists");
        
        // implement Result pattern in Category entity
        var categoryResult = Category.Create(
            Guid.NewGuid(),
            request.Name,
            request.ColorHex,
            request.Type);
        
        if (categoryResult.IsFailure)
            return Result.Failure<CategoryDto>(categoryResult.Error);

        var category = categoryResult.Value;
        await dbContext.Categories.AddAsync(category, ct);
        await dbContext.SaveChangesAsync(ct);

        var dto = new CategoryDto(
            category.Id,
            category.Name,
            category.Type,
            category.ColorHex,
            false,
            category.UserId);
        
        return Result.Success(dto);
    }

    public async Task<Result> UpdateAsync(Guid id, UpdateCategoryRequest request, CancellationToken ct = default)
    {
        var userId = currentUserIdAccessor.UserId;

        var rowsAffected = await dbContext.Categories
            .Where(c => c.Id == id && c.UserId == userId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(c => c.Name, request.Name), ct);
        
        if (rowsAffected == 0)
            return Result.Failure("Category not found");
        
        return Result.Success();
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var userId = currentUserIdAccessor.UserId;

        var rowsAffected = await dbContext.Categories
            .Where(c => c.Id == id && c.UserId == userId)
            .ExecuteDeleteAsync(ct);
        
        if (rowsAffected == 0)
            return Result.Failure("Category not found");

        return Result.Success();
    }
}