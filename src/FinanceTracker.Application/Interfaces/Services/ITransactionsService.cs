using CSharpFunctionalExtensions;
using FinanceTracker.Application.DTOs;

namespace FinanceTracker.Application.Interfaces.Services;

public interface ITransactionsService
{
    public Task<Result<PagedResult<TransactionDto>>> GetPagedAsync(TransactionFilterParams filterParams, 
        CancellationToken ct =  default);
    public Task<Result<TransactionDto>> GetByIdAsync(Guid id,
        CancellationToken ct = default);
    public Task<Result<TransactionDto>> CreateAsync(CreateTransactionRequest request,
        CancellationToken ct = default);
    public Task<Result> UpdateAsync(Guid id, UpdateTransactionRequest request,
        CancellationToken ct = default);
    public Task<Result> DeleteAsync(Guid id,
        CancellationToken ct = default);
}