using CSharpFunctionalExtensions;
using FinanceTracker.Application.Common;
using FinanceTracker.Application.DTOs;
using FinanceTracker.Application.Interfaces.Auth;
using FinanceTracker.Application.Interfaces.Persistence;
using FinanceTracker.Application.Interfaces.Services;
using FinanceTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Application.Services;

public class CategoriesService(
    IApplicationDbContext dbContext,
    ICurrentUserIdAccessor currentUserIdAccessor) : ICategoriesService
{
    public async Task<Result<IReadOnlyCollection<CategoryDto>, AppError>> GetAllAsync(
        CancellationToken ct = default)
    {
        var userId = currentUserIdAccessor.UserId;

        var categories = await dbContext.Categories
            .AsNoTracking()
            .Where(c => c.UserId == null || c.UserId == userId)
            .OrderBy(c => c.UserId != null)
            .ThenBy(c => c.Name)
            .Select(c => new CategoryDto(c.Id, c.Name, c.Type, c.ColorHex, c.UserId == null, c.UserId))
            .ToListAsync(ct);

        return Result.Success<IReadOnlyCollection<CategoryDto>, AppError>(categories);
    }

    public async Task<Result<CategoryDto, AppError>> GetByIdAsync(
        Guid id,
        CancellationToken ct = default)
    {
        var userId = currentUserIdAccessor.UserId;

        var category = await dbContext.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id &&
                                      (c.UserId == null || c.UserId == userId), ct);

        if (category == null)
            return Result.Failure<CategoryDto, AppError>(
                AppError.NotFound("Category not found"));

        var dto = new CategoryDto(
            category.Id,
            category.Name,
            category.Type,
            category.ColorHex,
            category.UserId == null,
            category.UserId);

        return Result.Success<CategoryDto, AppError>(dto);
    }

    public async Task<Result<CategoryDto, AppError>> CreateAsync(
        CreateCategoryRequest request,
        CancellationToken ct = default)
    {
        var userId = currentUserIdAccessor.UserId;

        bool isDuplicate = await dbContext.Categories
            .AnyAsync(c => (c.UserId == userId || c.UserId == null)
                           && c.Name.ToLower() == request.Name.ToLower()
                           && c.Type == request.Type, ct);

        if (isDuplicate)
            return Result.Failure<CategoryDto, AppError>(
                AppError.Conflict("Category already exists"));

        var categoryResult = Category.Create(
            Guid.NewGuid(),
            request.Name,
            request.ColorHex,
            request.Type,
            userId);

        if (categoryResult.IsFailure)
            return Result.Failure<CategoryDto, AppError>(
                AppError.Validation(categoryResult.Error));

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

        return Result.Success<CategoryDto, AppError>(dto);
    }

    public async Task<Result<CategoryDto, AppError>> UpdateAsync(
        Guid id,
        UpdateCategoryRequest request,
        CancellationToken ct = default)
    {
        var userId = currentUserIdAccessor.UserId;

        if (string.IsNullOrWhiteSpace(request.Name))
            return Result.Failure<CategoryDto, AppError>(
                AppError.Validation("Category name cannot be empty"));

        var rowsAffected = await dbContext.Categories
            .Where(c => c.Id == id && c.UserId == userId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(c => c.Name, request.Name), ct);

        if (rowsAffected == 0)
            return Result.Failure<CategoryDto, AppError>(
                AppError.NotFound("Category not found"));

        var category = await dbContext.Categories
            .AsNoTracking()
            .Where(c => c.Id == id && c.UserId == userId)
            .Select(c => new CategoryDto(
                c.Id,
                c.Name,
                c.Type,
                c.ColorHex,
                c.UserId == null,
                c.UserId))
            .FirstAsync(ct);

        return Result.Success<CategoryDto, AppError>(category);
    }

    public async Task<UnitResult<AppError>> DeleteAsync(
        Guid id,
        CancellationToken ct = default)
    {
        var userId = currentUserIdAccessor.UserId;

        var rowsAffected = await dbContext.Categories
            .Where(c => c.Id == id && c.UserId == userId)
            .ExecuteDeleteAsync(ct);

        if (rowsAffected == 0)
            return UnitResult.Failure<AppError>(AppError.NotFound("Category not found"));

        return UnitResult.Success<AppError>();
    }
}
