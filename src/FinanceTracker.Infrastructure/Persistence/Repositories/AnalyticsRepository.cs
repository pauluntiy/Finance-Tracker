using Dapper;
using FinanceTracker.Application.DTOs;
using FinanceTracker.Application.Interfaces.Persistence;
using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Infrastructure.Persistence.Repositories;

public class AnalyticsRepository(ISqlConnectionFactory connectionFactory) : IAnalyticsRepository
{
    public async Task<SummaryModel> GetSummaryAsync(
        Guid userId, 
        DateTime? fromDate = null, DateTime? toDate = null,
        CancellationToken ct = default)
    {
        using var connection = connectionFactory.CreateConnection();

        const string sql = """
                           SELECT
                                COALESCE(SUM(CASE WHEN c."Type" = 1 THEN t."Amount" ELSE 0 END), 0) AS TotalIncome,
                                COALESCE(SUM(CASE WHEN c."Type" = 2 THEN t."Amount" ELSE 0 END), 0) AS TotalExpense
                           FROM "Transactions" t
                           INNER JOIN "Categories" c ON t."CategoryId" = c."Id"
                           WHERE t."UserId" = @UserId
                            AND (@StartDate IS NULL OR t."DateUtc" >= @StartDate)
                            AND (@EndDate IS NULL OR t."DateUtc" <= @EndDate);
                           """;

        var command = new CommandDefinition(sql, new { UserId = userId, StartDate = (DateTime?)fromDate, EndDate = (DateTime?)toDate }, cancellationToken: ct);
        
        return await connection.QueryFirstOrDefaultAsync<SummaryModel>(command)
            ?? new SummaryModel(0, 0);
    }

    public async Task<IReadOnlyList<CategorySummaryModel>> GetCategoryBreakdownAsync(
        Guid userId, 
        DateTime? fromDate = null, DateTime? toDate = null, 
        CategoryType? categoryType = null,
        CancellationToken ct = default)
    {
        using var connection = connectionFactory.CreateConnection();

        const string sql = """
                           SELECT
                                c."Id" AS CategoryId,
                                c."Name" AS CategoryName, 
                                c."ColorHex" AS ColorHex,
                                c."Type" AS CategoryType,
                                SUM(t."Amount") AS TotalAmount,
                                COUNT(t."Id")::int AS TransactionCount
                           FROM "Categories" c
                           INNER JOIN "Transactions" t ON c."Id" = t."CategoryId"
                           WHERE t."UserId" = @UserId
                            AND (@StartDate IS NULL OR t."DateUtc" >= @StartDate)
                            AND (@EndDate IS NULL OR t."DateUtc" <= @EndDate)
                            AND (@CategoryType IS NULL OR c."Type" = @CategoryType)
                           GROUP BY c."Id", c."Name", c."ColorHex", c."Type"
                           ORDER BY TotalAmount DESC;
                           """;
        
        var command = new CommandDefinition(
            sql, 
            new { UserId = userId, StartDate = (DateTime?)fromDate, EndDate = (DateTime?)toDate, CategoryType = (int?)categoryType }, 
            cancellationToken: ct );

        var result = await connection.QueryAsync<CategorySummaryModel>(command);
        return result.AsList();
    }

    public async Task<IReadOnlyList<TrendPointModel>> GetDailyTrendAsync(
        Guid userId, 
        DateTime fromDate, DateTime toDate,
        CancellationToken ct = default)
    {
        using var connection = connectionFactory.CreateConnection();

        const string sql = """
                           SELECT
                                t."DateUtc"::date AS "Date", 
                                COALESCE(SUM(CASE WHEN c."Type" = 1 THEN t."Amount" ELSE 0 END), 0) AS TotalIncome,
                                COALESCE(SUM(CASE WHEN c."Type" = 2 THEN t."Amount" ELSE 0 END), 0) AS TotalExpense
                           FROM "Transactions" t
                           INNER JOIN "Categories" c ON t."CategoryId" = c."Id"
                           WHERE t."UserId" = @UserId
                            AND t."DateUtc" >= @StartDate
                            AND t."DateUtc" <= @EndDate
                           GROUP BY t."DateUtc"::date
                           ORDER BY "Date" ASC;
                           """;
        
        var command = new CommandDefinition(
            sql,
            new { UserId = userId, StartDate = fromDate, EndDate = toDate },
            cancellationToken: ct);
        
        var result = await connection.QueryAsync<TrendPointModel>(command);
        return result.AsList();
    }

    public async Task<IReadOnlyList<TopExpenseModel>> GetTopExpensesAsync(
        Guid userId, 
        int topCount,
        DateTime? fromDate = null, DateTime? toDate = null,
        CancellationToken ct = default)
    {
        using var connection = connectionFactory.CreateConnection();

        const string sql = """
                           SELECT
                                t."Id" as Id,
                                t."Amount" as Amount,
                                t."Description" as Description,
                                t."DateUtc" as DateUtc,
                                t."CategoryId" as CategoryId,
                                c."Name" as CategoryName,
                                c."ColorHex" as ColorHex
                           FROM "Transactions" t
                           INNER JOIN "Categories" c ON t."CategoryId" = c."Id"
                           WHERE t."UserId" = @UserId
                            AND (@StartDate IS NULL OR t."DateUtc" >= @StartDate)
                            AND (@EndDate IS NULL OR t."DateUtc" <= @EndDate)
                            AND c."Type" = 2
                           ORDER BY t."Amount" DESC
                           LIMIT @TopCount;
                           """;

        var command = new CommandDefinition(
            sql,
            new { UserId = userId, StartDate = (DateTime?)fromDate, EndDate = (DateTime?)toDate, TopCount = topCount },
            cancellationToken: ct);
        
        var result = await connection.QueryAsync<TopExpenseModel>(command);
        return result.AsList();
    }
}