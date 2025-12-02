using Microsoft.AspNetCore.Mvc;
using ProductAdminPortal.Models.Domain;
using ProductAdminPortal.Services;

namespace ProductAdminPortal.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class UsageController : ControllerBase
{
    private readonly IUsageService _usageService;
    private readonly ILogger<UsageController> _logger;

    public UsageController(IUsageService usageService, ILogger<UsageController> logger)
    {
        _usageService = usageService;
        _logger = logger;
    }

    /// <summary>
    /// Record usage for a customer subscription
    /// </summary>
    [HttpPost("record")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> RecordUsage(
        [FromBody] RecordUsageRequest request,
        [FromHeader(Name = "X-Tenant-Id")] Guid tenantId,
        [FromHeader(Name = "X-User-Id")] Guid userId,
        CancellationToken cancellationToken)
    {
        try
        {
            var usageId = await _usageService.RecordUsageAsync(
                request.CustomerSubscriptionId,
                request.EntityId,
                request.Units,
                request.Complexity,
                request.Metadata,
                tenantId,
                userId,
                cancellationToken);

            return CreatedAtAction(nameof(GetUsageRecords), new { subscriptionId = request.CustomerSubscriptionId }, usageId);
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
            _logger.LogError(ex, "Error recording usage for subscription {SubscriptionId}", request.CustomerSubscriptionId);
            return StatusCode(500, new { error = "An error occurred while recording usage" });
        }
    }

    /// <summary>
    /// Get usage records for a subscription
    /// </summary>
    [HttpGet("subscriptions/{subscriptionId}/records")]
    [ProducesResponseType(typeof(List<UsageRecord>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<UsageRecord>>> GetUsageRecords(
        Guid subscriptionId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] string? billingStatus,
        CancellationToken cancellationToken)
    {
        try
        {
            var records = await _usageService.GetUsageRecordsAsync(
                subscriptionId,
                startDate,
                endDate,
                billingStatus,
                cancellationToken);

            return Ok(records);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting usage records for subscription {SubscriptionId}", subscriptionId);
            return StatusCode(500, new { error = "An error occurred while retrieving usage records" });
        }
    }

    /// <summary>
    /// Get usage summary for a subscription
    /// </summary>
    [HttpGet("subscriptions/{subscriptionId}/summary")]
    [ProducesResponseType(typeof(Dictionary<Guid, int>), StatusCodes.Status200OK)]
    public async Task<ActionResult<Dictionary<Guid, int>>> GetUsageSummary(
        Guid subscriptionId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        CancellationToken cancellationToken)
    {
        try
        {
            var summary = await _usageService.GetUsageSummaryAsync(
                subscriptionId,
                startDate,
                endDate,
                cancellationToken);

            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting usage summary for subscription {SubscriptionId}", subscriptionId);
            return StatusCode(500, new { error = "An error occurred while retrieving usage summary" });
        }
    }

    /// <summary>
    /// Get active usage alerts for a subscription
    /// </summary>
    [HttpGet("subscriptions/{subscriptionId}/alerts")]
    [ProducesResponseType(typeof(List<UsageAlert>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<UsageAlert>>> GetActiveAlerts(
        Guid subscriptionId,
        CancellationToken cancellationToken)
    {
        try
        {
            var alerts = await _usageService.GetActiveAlertsAsync(subscriptionId, cancellationToken);
            return Ok(alerts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting alerts for subscription {SubscriptionId}", subscriptionId);
            return StatusCode(500, new { error = "An error occurred while retrieving alerts" });
        }
    }

    /// <summary>
    /// Resolve a usage alert
    /// </summary>
    [HttpPost("alerts/{alertId}/resolve")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResolveAlert(
        Guid alertId,
        [FromHeader(Name = "X-Tenant-Id")] Guid tenantId,
        [FromHeader(Name = "X-User-Id")] Guid userId,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _usageService.ResolveAlertAsync(alertId, tenantId, userId, cancellationToken);
            if (!result)
                return NotFound(new { error = $"Alert {alertId} not found" });

            return Ok(new { message = "Alert resolved successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving alert {AlertId}", alertId);
            return StatusCode(500, new { error = "An error occurred while resolving the alert" });
        }
    }
}

public record RecordUsageRequest(
    Guid CustomerSubscriptionId,
    Guid EntityId,
    int Units,
    string? Complexity,
    Dictionary<string, object>? Metadata
);
