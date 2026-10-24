using CSharpFunctionalExtensions;
using FinanceTracker.Application.Common;
using FinanceTracker.Application.DTOs;
using FinanceTracker.Application.Interfaces.Auth;
using FinanceTracker.Application.Interfaces.Persistence;
using FinanceTracker.Application.Interfaces.Services;
using FinanceTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Application.Services;

public class TransactionsService(
    IApplicationDbContext dbContext,
    ICurrentUserIdAccessor currentUserIdAccessor) : ITransactionsService
{
    private const int MaxPageSize = 100;

    public async Task<Result<PagedResult<TransactionDto>, AppError>> GetPagedAsync(
        TransactionFilterParams filterParams,
        CancellationToken ct = default)
    {
        var userId = currentUserIdAccessor.UserId;

        var page = Math.Max(1, filterParams.Page);
        var pageSize = Math.Clamp(filterParams.PageSize, 1, MaxPageSize);

        var query = dbContext.Transactions
            .AsNoTracking()
            .Where(t => t.UserId == userId);

        if (filterParams.FromDate.HasValue)
            query = query.Where(t => t.DateUtc >= filterParams.FromDate.Value.AsUtc());

        if (filterParams.ToDate.HasValue)
            query = query.Where(t => t.DateUtc <= filterParams.ToDate.Value.AsUtc());

        if (filterParams.Type.HasValue)
            query = query.Where(t => t.Category.Type == filterParams.Type.Value);

        if (filterParams.CategoryId.HasValue)
            query = query.Where(t => t.CategoryId == filterParams.CategoryId.Value);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(t => t.DateUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new TransactionDto(
                t.Id,
                t.Amount,
                t.Description,
                t.DateUtc,
                t.CategoryId,
                t.Category.Name,
                t.Category.Type,
                t.UserId))
            .ToListAsync(ct);

        var result = new PagedResult<TransactionDto>(items, page, pageSize, totalCount);
        return Result.Success<PagedResult<TransactionDto>, AppError>(result);
    }

    public async Task<Result<TransactionDto, AppError>> GetByIdAsync(
        Guid id,
        CancellationToken ct = default)
    {
        var userId = currentUserIdAccessor.UserId;

        var transaction = await dbContext.Transactions
            .AsNoTracking()
            .Where(t => t.Id == id && t.UserId == userId)
            .Select(t => new TransactionDto(
                t.Id,
                t.Amount,
                t.Description,
                t.DateUtc,
                t.CategoryId,
                t.Category.Name,
                t.Category.Type,
                t.UserId))
            .FirstOrDefaultAsync(ct);

        if (transaction == null)
            return Result.Failure<TransactionDto, AppError>(
                AppError.NotFound("Transaction is not found"));

        return Result.Success<TransactionDto, AppError>(transaction);
    }

    public async Task<Result<TransactionDto, AppError>> CreateAsync(
        CreateTransactionRequest request,
        CancellationToken ct = default)
    {
        var userId = currentUserIdAccessor.UserId;

        var category = await dbContext.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.CategoryId &&
                                      (c.UserId == userId || c.UserId == null), ct);

        if (category == null)
            return Result.Failure<TransactionDto, AppError>(
                AppError.Validation("Category is not found or access is denied"));

        var transactionResult = Transaction.Create(
            Guid.NewGuid(),
            request.Amount,
            request.Description,
            request.DateUtc.AsUtc(),
            request.CategoryId,
            userId);

        if (transactionResult.IsFailure)
            return Result.Failure<TransactionDto, AppError>(
                AppError.Validation(transactionResult.Error));

        var transaction = transactionResult.Value;
        await dbContext.Transactions.AddAsync(transaction, ct);
        await dbContext.SaveChangesAsync(ct);

        return Result.Success<TransactionDto, AppError>(new TransactionDto(
            transaction.Id,
            transaction.Amount,
            transaction.Description,
            transaction.DateUtc,
            transaction.CategoryId,
            category.Name,
            category.Type,
            userId));
    }

    public async Task<Result<TransactionDto, AppError>> UpdateAsync(
        Guid id,
        UpdateTransactionRequest request,
        CancellationToken ct = default)
    {
        var userId = currentUserIdAccessor.UserId;

        if (request.Amount < 0)
            return Result.Failure<TransactionDto, AppError>(
                AppError.Validation("Amount cannot be negative."));

        var dateUtc = request.DateUtc.AsUtc();

        var categoryExists = await dbContext.Categories
            .AnyAsync(c => c.Id == request.CategoryId &&
                           (c.UserId == null || c.UserId == userId), ct);

        if (!categoryExists)
            return Result.Failure<TransactionDto, AppError>(
                AppError.Validation("Selected category does not exist or access is denied."));

        var rowsAffected = await dbContext.Transactions
            .Where(t => t.Id == id && t.UserId == userId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(t => t.Amount, request.Amount)
                .SetProperty(t => t.Description, request.Description)
                .SetProperty(t => t.DateUtc, dateUtc)
                .SetProperty(t => t.CategoryId, request.CategoryId), ct);

        if (rowsAffected == 0)
            return Result.Failure<TransactionDto, AppError>(
                AppError.NotFound("Transaction not found or access denied."));

        var transaction = await dbContext.Transactions
            .AsNoTracking()
            .Where(t => t.Id == id && t.UserId == userId)
            .Select(t => new TransactionDto(
                t.Id,
                t.Amount,
                t.Description,
                t.DateUtc,
                t.CategoryId,
                t.Category.Name,
                t.Category.Type,
                t.UserId))
            .FirstAsync(ct);

        return Result.Success<TransactionDto, AppError>(transaction);
    }

    public async Task<UnitResult<AppError>> DeleteAsync(
        Guid id,
        CancellationToken ct = default)
    {
        var userId = currentUserIdAccessor.UserId;

        var deletedRows = await dbContext.Transactions
            .Where(t => t.Id == id && t.UserId == userId)
            .ExecuteDeleteAsync(ct);

        if (deletedRows == 0)
            return UnitResult.Failure<AppError>(
                AppError.NotFound("Transaction not found or access denied."));

        return UnitResult.Success<AppError>();
    }
}
