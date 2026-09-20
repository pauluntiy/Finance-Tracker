using FinanceTracker.Application.DTOs;
using FinanceTracker.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceTracker.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoriesService _categoriesService;

    public CategoriesController(ICategoriesService categoriesService)
    {
        _categoriesService = categoriesService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _categoriesService.GetAllAsync(ct);
        
        return result.IsSuccess 
            ? Ok(result.Value)
            : BadRequest(new { error = result.Error});
    }
    
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var result = await _categoriesService.GetByIdAsync(id, ct);
        
        return result.IsSuccess 
            ? Ok(result.Value)
            : NotFound(new { error = result.Error});
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request, 
        CancellationToken ct)
    {
        var result = await _categoriesService.CreateAsync(request, ct);
        
        return result.IsSuccess 
            ? Ok(result.Value)
            : BadRequest(new { error = result.Error});
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCategoryRequest request,
        CancellationToken ct)
    {
        var result = await _categoriesService.UpdateAsync(id, request, ct);
        
        return result.IsSuccess 
            ? NoContent()
            : NotFound(new { error = result.Error});
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await _categoriesService.DeleteAsync(id, ct);
        
        return result.IsSuccess 
            ? NoContent()
            : NotFound(new { error = result.Error});
    }
}