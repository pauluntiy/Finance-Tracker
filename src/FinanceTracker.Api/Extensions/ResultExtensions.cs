using CSharpFunctionalExtensions;
using FinanceTracker.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace FinanceTracker.Api.Extensions;

public static class ResultExtensions
{
    /// <summary>
    /// Status on custom action
    /// </summary>
    /// <param name="onSuccess"> custom action </param>
    public static IActionResult ToActionResult<T>(
        this Result<T, AppError> result,
        Func<T, IActionResult> onSuccess)
        => result.IsSuccess
            ? onSuccess(result.Value)
            : result.Error.ToActionResult();

    /// <summary>
    /// OK -- 200
    /// </summary>
    public static IActionResult ToActionResult<T>(this Result<T, AppError> result)
        => result.IsSuccess
            ? new OkObjectResult(result.Value)
            : result.Error.ToActionResult();

    /// <summary>
    /// NoContentResult -- 204
    /// </summary>
    public static IActionResult ToActionResult(this UnitResult<AppError> result)
        => result.IsSuccess
            ? new NoContentResult()
            : result.Error.ToActionResult();

    public static IActionResult ToActionResult(this AppError error) => error.Type switch
    {
        ErrorType.Validation => new BadRequestObjectResult(new { error = error.Message }),
        ErrorType.Unauthorized => new UnauthorizedObjectResult(new { error = error.Message }),
        ErrorType.NotFound => new NotFoundObjectResult(new { error = error.Message }),
        ErrorType.Conflict => new ConflictObjectResult(new { error = error.Message }),
        _ => new ObjectResult(new { error = error.Message }) { StatusCode = 500 }
    };
}
