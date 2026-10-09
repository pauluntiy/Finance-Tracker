using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Application.DTOs;

public record CategoryDto(
        Guid Id,
        string Name,
        CategoryType Type,
        string ColorHex,
        bool IsSystem,
        Guid? UserId);
        
        
public record CreateCategoryRequest(
        string Name,
        string? ColorHex,
        CategoryType Type);

public record UpdateCategoryRequest(
        string Name);