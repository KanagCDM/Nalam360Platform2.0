using Microsoft.AspNetCore.Mvc;
using ProductAdminPortal.Models.Domain;
using ProductAdminPortal.Services;

namespace ProductAdminPortal.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuditController : ControllerBase
{
    private readonly IAuditService _auditService;
    private readonly ILogger<AuditController> _logger;

    public AuditController(IAuditService auditService, ILogger<AuditController> logger)
    {
        _auditService = auditService;
        _logger = logger;
    }

    /// <summary>
    /// Get audit logs with optional filtering
    /// </summary>
    [HttpGet("logs")]
    [ProducesResponseType(typeof(List<AuditLog>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AuditLog>>> GetAuditLogs(
        [FromHeader(Name = "X-Tenant-Id")] Guid tenantId,
        [FromQuery] string? entityType,
        [FromQuery] Guid? entityId,
        [FromQuery] Guid? userId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var logs = await _auditService.GetAuditLogsAsync(
                tenantId,
                entityType,
                entityId,
                userId,
                startDate,
                endDate,
                pageNumber,
                pageSize,
                cancellationToken);

            return Ok(logs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting audit logs for tenant {TenantId}", tenantId);
            return StatusCode(500, new { error = "An error occurred while retrieving audit logs" });
        }
    }

    /// <summary>
    /// Get configuration versions for an entity
    /// </summary>
    [HttpGet("versions")]
    [ProducesResponseType(typeof(List<ConfigurationVersion>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ConfigurationVersion>>> GetConfigurationVersions(
        [FromHeader(Name = "X-Tenant-Id")] Guid tenantId,
        [FromQuery] string entityType,
        [FromQuery] Guid entityId,
        CancellationToken cancellationToken)
    {
        try
        {
            var versions = await _auditService.GetConfigurationVersionsAsync(
                entityType,
                entityId,
                tenantId,
                cancellationToken);

            return Ok(versions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting versions for {EntityType} {EntityId}", entityType, entityId);
            return StatusCode(500, new { error = "An error occurred while retrieving configuration versions" });
        }
    }

    /// <summary>
    /// Get a specific configuration version
    /// </summary>
    [HttpGet("versions/{versionId}")]
    [ProducesResponseType(typeof(ConfigurationVersion), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConfigurationVersion>> GetConfigurationVersion(
        Guid versionId,
        [FromHeader(Name = "X-Tenant-Id")] Guid tenantId,
        CancellationToken cancellationToken)
    {
        try
        {
            var version = await _auditService.GetConfigurationVersionByIdAsync(versionId, tenantId, cancellationToken);
            if (version == null)
                return NotFound(new { error = $"Configuration version {versionId} not found" });

            return Ok(version);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting configuration version {VersionId}", versionId);
            return StatusCode(500, new { error = "An error occurred while retrieving the configuration version" });
        }
    }

    /// <summary>
    /// Create a configuration version snapshot
    /// </summary>
    [HttpPost("versions")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<ActionResult<Guid>> CreateConfigurationVersion(
        [FromBody] CreateConfigurationVersionRequest request,
        [FromHeader(Name = "X-Tenant-Id")] Guid tenantId,
        [FromHeader(Name = "X-User-Id")] Guid userId,
        CancellationToken cancellationToken)
    {
        try
        {
            var versionId = await _auditService.CreateConfigurationVersionAsync(
                request.EntityType,
                request.EntityId,
                request.ConfigurationSnapshot,
                request.ChangeSummary,
                tenantId,
                userId,
                cancellationToken);

            return CreatedAtAction(nameof(GetConfigurationVersion), new { versionId }, versionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating configuration version for {EntityType} {EntityId}", 
                request.EntityType, request.EntityId);
            return StatusCode(500, new { error = "An error occurred while creating the configuration version" });
        }
    }

    /// <summary>
    /// Rollback to a specific configuration version
    /// </summary>
    [HttpPost("versions/{versionId}/rollback")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RollbackToVersion(
        Guid versionId,
        [FromHeader(Name = "X-Tenant-Id")] Guid tenantId,
        [FromHeader(Name = "X-User-Id")] Guid userId,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _auditService.RollbackToVersionAsync(versionId, tenantId, userId, cancellationToken);
            return Ok(new { message = "Rollback completed successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rolling back to version {VersionId}", versionId);
            return StatusCode(500, new { error = "An error occurred while performing the rollback" });
        }
    }
}

public record CreateConfigurationVersionRequest(
    string EntityType,
    Guid EntityId,
    string ConfigurationSnapshot,
    string? ChangeSummary
);
