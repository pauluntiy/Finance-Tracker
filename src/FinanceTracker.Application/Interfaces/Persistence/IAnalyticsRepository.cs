using FinanceTracker.Application.DTOs;
using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Application.Interfaces.Persistence;

public interface IAnalyticsRepository
{
    Task<SummaryModel> GetSummaryAsync(Guid userId,
        DateTime? fromDate = null, DateTime? toDate = null,
        CancellationToken ct = default);

    Task<IReadOnlyList<CategorySummaryModel>> GetCategoryBreakdownAsync(Guid userId,
        DateTime? fromDate = null, DateTime? toDate = null,
        CategoryType? categoryType = null,
        CancellationToken ct = default);

    Task<IReadOnlyList<TrendPointModel>> GetDailyTrendAsync(Guid userId,
        DateTime fromDate, DateTime toDate,
        CancellationToken ct = default);

    Task<IReadOnlyList<TopExpenseModel>> GetTopExpensesAsync(Guid userId,
        int topCount,
        DateTime? fromDate = null, DateTime? toDate = null,
        CancellationToken ct = default);
}