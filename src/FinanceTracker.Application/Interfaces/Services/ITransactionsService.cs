using CSharpFunctionalExtensions;
using FinanceTracker.Application.Common;
using FinanceTracker.Application.DTOs;

namespace FinanceTracker.Application.Interfaces.Services;

public interface ITransactionsService
{
    Task<Result<PagedResult<TransactionDto>, AppError>> GetPagedAsync(
        TransactionFilterParams filterParams,
        CancellationToken ct = default);

    Task<Result<TransactionDto, AppError>> GetByIdAsync(
        Guid id,
        CancellationToken ct = default);

    Task<Result<TransactionDto, AppError>> CreateAsync(
        CreateTransactionRequest request,
        CancellationToken ct = default);

    Task<Result<TransactionDto, AppError>> UpdateAsync(
        Guid id,
        UpdateTransactionRequest request,
        CancellationToken ct = default);

    Task<UnitResult<AppError>> DeleteAsync(
        Guid id,
        CancellationToken ct = default);
}
