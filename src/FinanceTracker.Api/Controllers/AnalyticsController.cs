using FinanceTracker.Api.Extensions;
using FinanceTracker.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceTracker.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AnalyticsController(IAnalyticsService analyticsService) : ControllerBase
{
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate,
        CancellationToken ct)
    {
        var result = await analyticsService.GetCompleteSummaryAsync(fromDate, toDate, ct);
        return result.ToActionResult();
    }

    [HttpGet("trend")]
    public async Task<IActionResult> GetTrend(
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate,
        CancellationToken ct)
    {
        var result = await analyticsService.GetTrendPointsAsync(fromDate, toDate, ct);
        return result.ToActionResult();
    }

    [HttpGet("topexpenses")]
    public async Task<IActionResult> GetTopExpenses(
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate,
        [FromQuery] int topCount,
        CancellationToken ct)
    {
        var result = await analyticsService.GetTopExpensesAsync(fromDate, toDate, topCount, ct);
        return result.ToActionResult();
    }
}
