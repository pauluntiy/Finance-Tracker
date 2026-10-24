using CSharpFunctionalExtensions;
using FinanceTracker.Application.Common;
using FinanceTracker.Application.DTOs;
using FinanceTracker.Application.Interfaces.Auth;
using FinanceTracker.Application.Interfaces.Persistence;
using FinanceTracker.Application.Interfaces.Services;
using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Application.Services;

public class AnalyticsService(
    IAnalyticsRepository repository,
    ICurrentUserIdAccessor currentUserIdAccessor) : IAnalyticsService
{
    private static readonly TimeSpan MaxTrendPeriod = TimeSpan.FromDays(366);
    private const int MaxTopCount = 100;

    public async Task<Result<AnalyticsSummaryDto, AppError>> GetCompleteSummaryAsync(
        DateTime fromDate, DateTime toDate,
        CancellationToken ct = default)
    {
        if (fromDate > toDate)
            return Result.Failure<AnalyticsSummaryDto, AppError>(
                AppError.Validation("Start date cannot be greater than end date."));

        var userId = currentUserIdAccessor.UserId;
        if (userId == Guid.Empty)
            return Result.Failure<AnalyticsSummaryDto, AppError>(
                AppError.Unauthorized("User is not authenticated."));

        var startUtc = fromDate.AsUtc();
        var endUtc = toDate.AsUtc();

        var summary = await repository.GetSummaryAsync(userId, startUtc, endUtc, ct);

        var incomeByCategory =
            await repository.GetCategoryBreakdownAsync(userId, startUtc, endUtc, CategoryType.Income, ct);

        var expenseByCategory =
            await repository.GetCategoryBreakdownAsync(userId, startUtc, endUtc, CategoryType.Expense, ct);

        var result = new AnalyticsSummaryDto(
            summary.TotalIncome,
            summary.TotalExpense,
            summary.TotalIncome - summary.TotalExpense,
            incomeByCategory,
            expenseByCategory);

        return Result.Success<AnalyticsSummaryDto, AppError>(result);
    }

    public async Task<Result<IReadOnlyList<TrendPointModel>, AppError>> GetTrendPointsAsync(
        DateTime fromDate, DateTime toDate,
        CancellationToken ct = default)
    {
        if (fromDate > toDate)
            return Result.Failure<IReadOnlyList<TrendPointModel>, AppError>(
                AppError.Validation("Start date cannot be greater than end date."));

        var startUtc = fromDate.AsUtc();
        var endUtc = toDate.AsUtc();

        if (endUtc - startUtc > MaxTrendPeriod)
            return Result.Failure<IReadOnlyList<TrendPointModel>, AppError>(
                AppError.Validation("Daily trend interval cannot exceed 1 year."));

        var userId = currentUserIdAccessor.UserId;
        if (userId == Guid.Empty)
            return Result.Failure<IReadOnlyList<TrendPointModel>, AppError>(
                AppError.Unauthorized("User is not authenticated."));

        var trendPoints =
            await repository.GetDailyTrendAsync(userId, startUtc, endUtc, ct);

        return Result.Success<IReadOnlyList<TrendPointModel>, AppError>(trendPoints);
    }

    public async Task<Result<IReadOnlyList<TopExpenseModel>, AppError>> GetTopExpensesAsync(
        DateTime fromDate, DateTime toDate,
        int topCount,
        CancellationToken ct = default)
    {
        if (fromDate > toDate)
            return Result.Failure<IReadOnlyList<TopExpenseModel>, AppError>(
                AppError.Validation("Start date cannot be greater than end date."));

        if (topCount < 1)
            return Result.Failure<IReadOnlyList<TopExpenseModel>, AppError>(
                AppError.Validation("Top count must be greater than zero."));

        var clampedTopCount = Math.Min(topCount, MaxTopCount);

        var userId = currentUserIdAccessor.UserId;
        if (userId == Guid.Empty)
            return Result.Failure<IReadOnlyList<TopExpenseModel>, AppError>(
                AppError.Unauthorized("User is not authenticated."));

        var topExpenses =
            await repository.GetTopExpensesAsync(
                userId, clampedTopCount, fromDate.AsUtc(), toDate.AsUtc(), ct);

        return Result.Success<IReadOnlyList<TopExpenseModel>, AppError>(topExpenses);
    }
}
