using CSharpFunctionalExtensions;
using FinanceTracker.Application.DTOs;

namespace FinanceTracker.Application.Interfaces.Services;

public interface IAnalyticsService
{
    Task<Result<AnalyticsSummaryDto>> GetCompleteSummaryAsync(
        DateTime fromDate, DateTime toDate,
        CancellationToken ct = default);

    Task<Result<IReadOnlyList<TrendPointModel>>> GetTrendPointsAsync(
        DateTime fromDate, DateTime toDate,
        CancellationToken ct = default);
    
    Task<Result<IReadOnlyList<TopExpenseModel>>> GetTopExpensesAsync(
        DateTime fromDate, DateTime toDate,
        int topCount,
        CancellationToken ct = default);
}