namespace ProductAdminPortal.DTOs;

/// <summary>
/// DTO for creating a new product
/// </summary>
public record CreateProductRequest(
    string Name,
    string Code,
    string? Description,
    string? Industry,
    string? Version
);

/// <summary>
/// DTO for updating a product
/// </summary>
public record UpdateProductRequest(
    string? Name,
    string? Description,
    string? Industry,
    string? Version,
    bool? IsActive
);

/// <summary>
/// DTO for product response
/// </summary>
public record ProductResponse(
    Guid Id,
    Guid TenantId,
    string Name,
    string Code,
    string? Description,
    string? Industry,
    string Version,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    ICollection<ModuleResponse>? Modules
);

/// <summary>
/// DTO for creating a module
/// </summary>
public record CreateModuleRequest(
    string Name,
    string Code,
    string? Description,
    int DisplayOrder,
    bool IsRequired
);

/// <summary>
/// DTO for module response
/// </summary>
public record ModuleResponse(
    Guid Id,
    Guid ProductId,
    string Name,
    string Code,
    string? Description,
    int DisplayOrder,
    bool IsRequired,
    DateTime CreatedAt,
    ICollection<EntityResponse>? Entities
);

/// <summary>
/// DTO for creating an entity
/// </summary>
public record CreateEntityRequest(
    string Name,
    string Code,
    string Type,
    string? Description,
    object? BasePricing,
    string[]? ComplexityLevels
);

/// <summary>
/// DTO for entity response
/// </summary>
public record EntityResponse(
    Guid Id,
    string Name,
    string Code,
    string Type,
    string? Description,
    object? BasePricing,
    string[]? ComplexityLevels,
    DateTime CreatedAt
);
