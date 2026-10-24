using CSharpFunctionalExtensions;
using FinanceTracker.Application.Common;
using FinanceTracker.Application.DTOs;

namespace FinanceTracker.Application.Interfaces.Services;

public interface IAnalyticsService
{
    Task<Result<AnalyticsSummaryDto, AppError>> GetCompleteSummaryAsync(
        DateTime fromDate, DateTime toDate,
        CancellationToken ct = default);

    Task<Result<IReadOnlyList<TrendPointModel>, AppError>> GetTrendPointsAsync(
        DateTime fromDate, DateTime toDate,
        CancellationToken ct = default);

    Task<Result<IReadOnlyList<TopExpenseModel>, AppError>> GetTopExpensesAsync(
        DateTime fromDate, DateTime toDate,
        int topCount,
        CancellationToken ct = default);
}
