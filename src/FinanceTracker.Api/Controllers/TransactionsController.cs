using FinanceTracker.Application.DTOs;
using FinanceTracker.Application.Interfaces.Services;
using FinanceTracker.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceTracker.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TransactionsController(ITransactionsService transactionsService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] TransactionFilterParams filterParams,
        CancellationToken ct)
    {
        var result = await transactionsService.GetPagedAsync(filterParams, ct);
        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(new { error = result.Error} );
    }
    
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await transactionsService.GetByIdAsync(id, ct);
        return result.IsSuccess 
            ? Ok(result.Value) 
            : NotFound(new { error = result.Error } );
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTransactionRequest request, 
        CancellationToken ct)
    {
        var result = await transactionsService.CreateAsync(request, ct);
        return result.IsSuccess 
            ? CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value) 
            : BadRequest(new { error = result.Error });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTransactionRequest request, 
        CancellationToken ct)
    {
        var result = await transactionsService.UpdateAsync(id, request, ct);
        return result.IsSuccess 
            ? NoContent() 
            : BadRequest(new { error = result.Error });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await transactionsService.DeleteAsync(id, ct);
        return result.IsSuccess 
            ? NoContent() 
            : BadRequest(new { error = result.Error });
    }
}