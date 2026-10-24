using CSharpFunctionalExtensions;
using FinanceTracker.Application.Common;
using FinanceTracker.Application.DTOs;

namespace FinanceTracker.Application.Interfaces.Services;

public interface ICategoriesService
{
    Task<Result<IReadOnlyCollection<CategoryDto>, AppError>> GetAllAsync(
        CancellationToken ct = default);

    Task<Result<CategoryDto, AppError>> GetByIdAsync(
        Guid id,
        CancellationToken ct = default);

    Task<Result<CategoryDto, AppError>> CreateAsync(
        CreateCategoryRequest request,
        CancellationToken ct = default);

    Task<Result<CategoryDto, AppError>> UpdateAsync(
        Guid id,
        UpdateCategoryRequest request,
        CancellationToken ct = default);

    Task<UnitResult<AppError>> DeleteAsync(
        Guid id,
        CancellationToken ct = default);
}
