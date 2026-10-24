namespace FinanceTracker.Application.Common;

public enum ErrorType
{
    Validation = 1,
    Unauthorized = 2,
    NotFound = 3,
    Conflict = 4
}

public sealed record AppError(ErrorType Type, string Message)
{
    public static AppError Validation(string message) => new(ErrorType.Validation, message);
    public static AppError Unauthorized(string message) => new(ErrorType.Unauthorized, message);
    public static AppError NotFound(string message) => new(ErrorType.NotFound, message);
    public static AppError Conflict(string message) => new(ErrorType.Conflict, message);
}
