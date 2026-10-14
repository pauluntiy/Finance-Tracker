using CSharpFunctionalExtensions;
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
    public async Task<Result<PagedResult<TransactionDto>>> GetPagedAsync(TransactionFilterParams filterParams, 
        CancellationToken ct = default)
    {
        var userId = currentUserIdAccessor.UserId;
        
        var query = dbContext.Transactions
            .AsNoTracking()
            .Where(t => t.UserId == userId);
        
        if (filterParams.FromDate.HasValue)
            query = query.Where(t => t.DateUtc >= filterParams.FromDate.Value.ToUniversalTime());
        
        if (filterParams.ToDate.HasValue)
            query = query.Where(t => t.DateUtc <= filterParams.ToDate.Value.ToUniversalTime());
        
        if (filterParams.Type.HasValue)
            query = query.Where(t => t.Category.Type == filterParams.Type.Value);
        
        if (filterParams.CategoryId.HasValue)
            query = query.Where(t => t.CategoryId == filterParams.CategoryId.Value);
        
        var items = await query
            .OrderByDescending(t => t.DateUtc)
            .Skip((filterParams.Page - 1) * filterParams.PageSize)
            .Take(filterParams.PageSize)
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
        
        var result = new PagedResult<TransactionDto>(items, filterParams.Page, filterParams.PageSize, items.Count);
        return Result.Success(result);
    }

    public async Task<Result<TransactionDto>> GetByIdAsync(Guid id,
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
            return Result.Failure<TransactionDto>("Transaction is not found");
        
        return Result.Success(transaction);
    }

    public async Task<Result<TransactionDto>> CreateAsync(CreateTransactionRequest request,
        CancellationToken ct = default)
    {
        var userId = currentUserIdAccessor.UserId;
        
        var category = await dbContext.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.CategoryId && 
                                      (c.UserId == userId || c.UserId == null), ct);
        
        if (category == null)
            return Result.Failure<TransactionDto>("Category is not found or access is denied");

        var transactionResult = Transaction.Create(
            Guid.NewGuid(),
            request.Amount,
            request.Description,
            request.DateUtc,
            request.CategoryId,
            userId);
        
        if (transactionResult.IsFailure)
            return Result.Failure<TransactionDto>(transactionResult.Error);
        
        var transaction = transactionResult.Value;
        await dbContext.Transactions.AddAsync(transaction, ct);
        await dbContext.SaveChangesAsync(ct);

        return Result.Success(new TransactionDto(
            transaction.Id,
            transaction.Amount,
            transaction.Description,
            transaction.DateUtc,
            transaction.CategoryId,
            category.Name,
            category.Type,
            userId));
    }

    public async Task<Result> UpdateAsync(Guid id, UpdateTransactionRequest request,
        CancellationToken ct = default)
    {
        var userId = currentUserIdAccessor.UserId;
        
        if (request.Amount < 0)
            return Result.Failure("Amount cannot be negative.");
        
        var dateUtc = request.DateUtc.ToUniversalTime();

        var categoryExists = await dbContext.Categories
            .AnyAsync(c => c.Id == request.CategoryId && (c.UserId == null || c.UserId == userId), ct);
        if (!categoryExists)
            return Result.Failure("Selected category does not exist or access is denied.");
        
        var rowsAffected = await dbContext.Transactions
            .Where(t => t.Id == id && t.UserId == userId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(t => t.Amount, request.Amount)
                .SetProperty(t => t.Description, request.Description)
                .SetProperty(t => t.DateUtc, dateUtc)
                .SetProperty(t => t.CategoryId, request.CategoryId), ct);
        
        if (rowsAffected == 0)
            return Result.Failure("Transaction not found or access denied.");
        
        return Result.Success();
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var userId = currentUserIdAccessor.UserId;

        int deletedRows = await dbContext.Transactions
            .Where(t => t.Id == id && t.UserId == userId)
            .ExecuteDeleteAsync(ct);

        if (deletedRows == 0)
            return Result.Failure("Transaction not found or access denied.");

        return Result.Success();
    }
}