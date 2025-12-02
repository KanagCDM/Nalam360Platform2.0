using Microsoft.EntityFrameworkCore;
using ProductAdminPortal.Data;
using ProductAdminPortal.DTOs;
using ProductAdminPortal.Models.Domain;
using System.Text.Json;

namespace ProductAdminPortal.Services;

public class ProductService : IProductService
{
    private readonly ProductAdminDbContext _context;
    private readonly IAuditService _auditService;

    public ProductService(ProductAdminDbContext context, IAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    public async Task<ProductResponse> GetProductByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var product = await _context.Products
            .Include(p => p.Modules)
                .ThenInclude(m => m.ModuleEntities)
                .ThenInclude(me => me.Entity)
            .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId, cancellationToken);

        if (product == null)
            throw new KeyNotFoundException($"Product with ID {id} not found");

        return MapToProductResponse(product);
    }

    public async Task<List<ProductResponse>> GetAllProductsAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var products = await _context.Products
            .Include(p => p.Modules)
            .Where(p => p.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        return products.Select(MapToProductResponse).ToList();
    }

    public async Task<ProductResponse> CreateProductAsync(CreateProductRequest request, Guid tenantId, Guid userId, CancellationToken cancellationToken = default)
    {
        // Check for duplicate code
        var exists = await _context.Products
            .AnyAsync(p => p.TenantId == tenantId && p.Code == request.Code, cancellationToken);

        if (exists)
            throw new InvalidOperationException($"Product with code '{request.Code}' already exists");

        var product = new Product
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = request.Name,
            Code = request.Code,
            Description = request.Description,
            Industry = request.Industry,
            Version = request.Version ?? "1.0.0",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync(cancellationToken);

        // Log audit
        await _auditService.LogActionAsync("create", "Product", product.Id, null,
            new Dictionary<string, object> { { "product_code", request.Code }, { "product_name", request.Name } },
            tenantId, userId, cancellationToken);

        return MapToProductResponse(product);
    }

    public async Task<ProductResponse> UpdateProductAsync(Guid id, UpdateProductRequest request, Guid tenantId, Guid userId, CancellationToken cancellationToken = default)
    {
        var product = await _context.Products
            .Include(p => p.Modules)
            .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId, cancellationToken);

        if (product == null)
            throw new KeyNotFoundException($"Product with ID {id} not found");

        var changes = new Dictionary<string, object>();

        if (request.Name != null && product.Name != request.Name)
        {
            changes["name"] = new { old = product.Name, @new = request.Name };
            product.Name = request.Name;
        }
        if (request.Description != null && product.Description != request.Description)
        {
            changes["description"] = new { old = product.Description, @new = request.Description };
            product.Description = request.Description;
        }
        if (request.Industry != null && product.Industry != request.Industry)
        {
            changes["industry"] = new { old = product.Industry, @new = request.Industry };
            product.Industry = request.Industry;
        }
        if (request.Version != null && product.Version != request.Version)
        {
            changes["version"] = new { old = product.Version, @new = request.Version };
            product.Version = request.Version;
        }

        product.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        // Log audit
        if (changes.Any())
        {
            await _auditService.LogActionAsync("update", "Product", product.Id, null, changes,
                tenantId, userId, cancellationToken);
        }

        return MapToProductResponse(product);
    }

    public async Task DeleteProductAsync(Guid id, Guid tenantId, Guid userId, CancellationToken cancellationToken = default)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId, cancellationToken);

        if (product == null)
            throw new KeyNotFoundException($"Product with ID {id} not found");

        // Soft delete
        product.IsActive = false;
        product.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        // Log audit
        await _auditService.LogActionAsync("delete", "Product", product.Id, null,
            new Dictionary<string, object> { { "soft_delete", true } },
            tenantId, userId, cancellationToken);
    }

    public async Task<ModuleResponse> AddModuleToProductAsync(Guid productId, CreateModuleRequest request, Guid tenantId, Guid userId, CancellationToken cancellationToken = default)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == productId && p.TenantId == tenantId, cancellationToken);

        if (product == null)
            throw new KeyNotFoundException($"Product with ID {productId} not found");

        var module = new Models.Domain.Module
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ProductId = productId,
            Name = request.Name,
            Code = request.Code,
            Description = request.Description,
            DisplayOrder = request.DisplayOrder,
            IsRequired = request.IsRequired,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Modules.Add(module);
        await _context.SaveChangesAsync(cancellationToken);

        // Log audit
        await _auditService.LogActionAsync("create", "Module", module.Id, null,
            new Dictionary<string, object> { { "product_id", productId }, { "module_code", request.Code } },
            tenantId, userId, cancellationToken);

        return MapToModuleResponse(module);
    }

    public async Task<List<ModuleResponse>> GetProductModulesAsync(Guid productId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var product = await _context.Products
            .Include(p => p.Modules)
                .ThenInclude(m => m.ModuleEntities)
                .ThenInclude(me => me.Entity)
            .FirstOrDefaultAsync(p => p.Id == productId && p.TenantId == tenantId, cancellationToken);

        if (product == null)
            throw new KeyNotFoundException($"Product with ID {productId} not found");

        return product.Modules.OrderBy(m => m.DisplayOrder).Select(MapToModuleResponse).ToList();
    }

    private ProductResponse MapToProductResponse(Models.Domain.Product product)
    {
        return new ProductResponse(
            product.Id,
            product.TenantId,
            product.Name,
            product.Code,
            product.Description,
            product.Industry,
            product.Version,
            product.IsActive,
            product.CreatedAt,
            product.UpdatedAt,
            product.Modules.Select(MapToModuleResponse).ToList()
        );
    }

    private ModuleResponse MapToModuleResponse(Models.Domain.Module module)
    {
        return new ModuleResponse(
            module.Id,
            module.ProductId,
            module.Name,
            module.Code,
            module.Description,
            module.DisplayOrder,
            module.IsRequired,
            module.CreatedAt,
            module.ModuleEntities.OrderBy(me => me.DisplayOrder).Select(me => MapToEntityResponse(me.Entity)).ToList()
        );
    }

    private EntityResponse MapToEntityResponse(Models.Domain.Entity entity)
    {
        var basePricing = string.IsNullOrEmpty(entity.BasePricing)
            ? null
            : JsonSerializer.Deserialize<Dictionary<string, object>>(entity.BasePricing);

        var complexityLevels = string.IsNullOrEmpty(entity.ComplexityLevels)
            ? null
            : JsonSerializer.Deserialize<string[]>(entity.ComplexityLevels);

        return new EntityResponse(
            entity.Id,
            entity.Name,
            entity.Code,
            entity.Type,
            entity.Description,
            basePricing,
            complexityLevels,
            entity.CreatedAt
        );
    }
}
