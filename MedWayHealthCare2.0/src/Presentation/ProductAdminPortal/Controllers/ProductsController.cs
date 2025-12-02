using Microsoft.AspNetCore.Mvc;
using ProductAdminPortal.DTOs;
using ProductAdminPortal.Services;

namespace ProductAdminPortal.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IProductService productService, ILogger<ProductsController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    /// <summary>
    /// Get all products for the current tenant
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<ProductResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ProductResponse>>> GetProducts(
        [FromHeader(Name = "X-Tenant-Id")] Guid tenantId,
        CancellationToken cancellationToken)
    {
        try
        {
            var products = await _productService.GetAllProductsAsync(tenantId, cancellationToken);
            return Ok(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting products for tenant {TenantId}", tenantId);
            return StatusCode(500, new { error = "An error occurred while retrieving products" });
        }
    }

    /// <summary>
    /// Get a specific product by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponse>> GetProduct(
        Guid id,
        [FromHeader(Name = "X-Tenant-Id")] Guid tenantId,
        CancellationToken cancellationToken)
    {
        try
        {
            var product = await _productService.GetProductByIdAsync(id, tenantId, cancellationToken);
            return Ok(product);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = $"Product {id} not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product {ProductId}", id);
            return StatusCode(500, new { error = "An error occurred while retrieving the product" });
        }
    }

    /// <summary>
    /// Create a new product
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductResponse>> CreateProduct(
        [FromBody] CreateProductRequest request,
        [FromHeader(Name = "X-Tenant-Id")] Guid tenantId,
        [FromHeader(Name = "X-User-Id")] Guid userId,
        CancellationToken cancellationToken)
    {
        try
        {
            var product = await _productService.CreateProductAsync(request, tenantId, userId, cancellationToken);
            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product");
            return StatusCode(500, new { error = "An error occurred while creating the product" });
        }
    }

    /// <summary>
    /// Update an existing product
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponse>> UpdateProduct(
        Guid id,
        [FromBody] UpdateProductRequest request,
        [FromHeader(Name = "X-Tenant-Id")] Guid tenantId,
        [FromHeader(Name = "X-User-Id")] Guid userId,
        CancellationToken cancellationToken)
    {
        try
        {
            var product = await _productService.UpdateProductAsync(id, request, tenantId, userId, cancellationToken);
            return Ok(product);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = $"Product {id} not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product {ProductId}", id);
            return StatusCode(500, new { error = "An error occurred while updating the product" });
        }
    }

    /// <summary>
    /// Delete a product (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProduct(
        Guid id,
        [FromHeader(Name = "X-Tenant-Id")] Guid tenantId,
        [FromHeader(Name = "X-User-Id")] Guid userId,
        CancellationToken cancellationToken)
    {
        try
        {
            await _productService.DeleteProductAsync(id, tenantId, userId, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = $"Product {id} not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product {ProductId}", id);
            return StatusCode(500, new { error = "An error occurred while deleting the product" });
        }
    }

    /// <summary>
    /// Get all modules for a product
    /// </summary>
    [HttpGet("{id}/modules")]
    [ProducesResponseType(typeof(List<ModuleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<ModuleResponse>>> GetProductModules(
        Guid id,
        [FromHeader(Name = "X-Tenant-Id")] Guid tenantId,
        CancellationToken cancellationToken)
    {
        try
        {
            var modules = await _productService.GetProductModulesAsync(id, tenantId, cancellationToken);
            return Ok(modules);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = $"Product {id} not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting modules for product {ProductId}", id);
            return StatusCode(500, new { error = "An error occurred while retrieving modules" });
        }
    }

    /// <summary>
    /// Add a new module to a product
    /// </summary>
    [HttpPost("{id}/modules")]
    [ProducesResponseType(typeof(ModuleResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ModuleResponse>> AddModule(
        Guid id,
        [FromBody] CreateModuleRequest request,
        [FromHeader(Name = "X-Tenant-Id")] Guid tenantId,
        [FromHeader(Name = "X-User-Id")] Guid userId,
        CancellationToken cancellationToken)
    {
        try
        {
            var module = await _productService.AddModuleToProductAsync(id, request, tenantId, userId, cancellationToken);
            return CreatedAtAction(nameof(GetProductModules), new { id }, module);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = $"Product {id} not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding module to product {ProductId}", id);
            return StatusCode(500, new { error = "An error occurred while adding the module" });
        }
    }
}
