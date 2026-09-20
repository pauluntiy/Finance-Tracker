using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Application.DTOs;

public record TransactionDto(
    Guid Id,
    decimal Amount,
    string? Description,
    DateTime DateUtc,
    Guid CategoryId,
    string CategoryName,
    CategoryType Type,
    Guid UserId
);

public record CreateTransactionRequest(
    decimal Amount,
    string? Description,
    DateTime DateUtc,
    Guid CategoryId
);
    
public record UpdateTransactionRequest(
    decimal Amount,
    string? Description,
    DateTime DateUtc,
    Guid CategoryId
);

public record TransactionFilterParams(
    DateTime? FromDate,
    DateTime? ToDate,
    Guid? CategoryId,
    CategoryType? Type,
    int Page = 1,
    int PageSize = 20);

public record PagedResult<T>(
    IReadOnlyCollection<T> Items,
    int Page,
    int PageSize,
    int TotalCount)
{
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}