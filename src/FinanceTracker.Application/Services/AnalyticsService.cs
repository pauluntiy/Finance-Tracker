using CSharpFunctionalExtensions;
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
    
    public async Task<Result<AnalyticsSummaryDto>> GetCompleteSummaryAsync(
        DateTime fromDate, DateTime toDate,
        CancellationToken ct = default)
    {
        if (fromDate > toDate)
            return Result.Failure<AnalyticsSummaryDto>("Start date cannot be greater than end date.");
        
        var userId = currentUserIdAccessor.UserId;
        if (userId == Guid.Empty)
            return Result.Failure<AnalyticsSummaryDto>("User is not authenticated.");
        
        var summary = await repository.GetSummaryAsync(userId, fromDate, toDate, ct);
        
        var incomeByCategory = 
            await repository.GetCategoryBreakdownAsync(userId, fromDate, toDate, CategoryType.Income, ct);
        
        var expenseByCategory =
            await repository.GetCategoryBreakdownAsync(userId, fromDate, toDate, CategoryType.Expense, ct);
        
        var result = new AnalyticsSummaryDto(
            summary.TotalIncome,
            summary.TotalExpense,
            summary.TotalIncome - summary.TotalExpense,
            incomeByCategory,
            expenseByCategory);
        
        return Result.Success(result);
    }

    public async Task<Result<IReadOnlyList<TrendPointModel>>> GetTrendPointsAsync(
        DateTime fromDate, DateTime toDate,
        CancellationToken ct = default)
    {
        if (fromDate > toDate)
            return Result.Failure<IReadOnlyList<TrendPointModel>>("Start date cannot be greater than end date.");
        
        if (toDate - fromDate > MaxTrendPeriod)
            return Result.Failure<IReadOnlyList<TrendPointModel>>("Daily trend interval cannot exceed 1 year.");
        
        var userId = currentUserIdAccessor.UserId;
        if (userId == Guid.Empty)
            return Result.Failure<IReadOnlyList<TrendPointModel>>("User is not authenticated.");
        
        var trendPoints = 
            await repository.GetDailyTrendAsync(userId, fromDate, toDate, ct);

        return Result.Success(trendPoints);
    }

    public async Task<Result<IReadOnlyList<TopExpenseModel>>> GetTopExpensesAsync(
        DateTime fromDate, DateTime toDate,
        int topCount, 
        CancellationToken ct = default)
    {
        if (fromDate > toDate)
            return Result.Failure<IReadOnlyList<TopExpenseModel>>("Start date cannot be greater than end date.");
        
        var userId = currentUserIdAccessor.UserId;
        if (userId == Guid.Empty)
            return Result.Failure<IReadOnlyList<TopExpenseModel>>("User is not authenticated.");
        
        var topExpenses = 
            await repository.GetTopExpensesAsync(userId, topCount, fromDate, toDate, ct);

        return Result.Success(topExpenses);
    }
}

