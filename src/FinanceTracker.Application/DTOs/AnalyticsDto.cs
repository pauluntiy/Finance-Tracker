using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Application.DTOs;

// api
public record AnalyticsSummaryDto(
    decimal TotalIncome,
    decimal TotalExpense,
    decimal Balance,
    IReadOnlyList<CategorySummaryModel> IncomeByCategory,
    IReadOnlyList<CategorySummaryModel> ExpenseByCategory
);

// db
public record SummaryModel(
    decimal TotalIncome, 
    decimal TotalExpense);

// db
public record CategorySummaryModel(
    Guid CategoryId,
    string CategoryName,
    string ColorHex,
    CategoryType CategoryType,
    decimal TotalAmount,
    int TransactionCount);

// db & api
public record TrendPointModel(
    DateOnly Date,
    decimal TotalIncome,
    decimal TotalExpense);

// db & api
public record TopExpenseModel(
    Guid Id,
    decimal Amount,
    string? Description,
    DateTime DateUtc,
    Guid CategoryId,
    string CategoryName,
    string ColorHex);