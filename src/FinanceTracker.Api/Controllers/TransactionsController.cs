using FinanceTracker.Api.Extensions;
using FinanceTracker.Application.DTOs;
using FinanceTracker.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceTracker.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TransactionsController(ITransactionsService transactionsService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] TransactionFilterParams filterParams,
        CancellationToken ct)
    {
        var result = await transactionsService.GetPagedAsync(filterParams, ct);
        return result.ToActionResult();
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await transactionsService.GetByIdAsync(id, ct);
        return result.ToActionResult();
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateTransactionRequest request,
        CancellationToken ct)
    {
        var result = await transactionsService.CreateAsync(request, ct);
        return result.ToActionResult(dto =>
            CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateTransactionRequest request,
        CancellationToken ct)
    {
        var result = await transactionsService.UpdateAsync(id, request, ct);
        return result.ToActionResult();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await transactionsService.DeleteAsync(id, ct);
        return result.ToActionResult();
    }
}
