using Microsoft.AspNetCore.Mvc;
using ProductAdminPortal.DTOs;
using ProductAdminPortal.Services;

namespace ProductAdminPortal.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class PricingController : ControllerBase
{
    private readonly IPricingService _pricingService;
    private readonly ILogger<PricingController> _logger;

    public PricingController(IPricingService pricingService, ILogger<PricingController> logger)
    {
        _pricingService = pricingService;
        _logger = logger;
    }

    /// <summary>
    /// Calculate actual pricing for a customer subscription
    /// </summary>
    [HttpPost("calculate")]
    [ProducesResponseType(typeof(PricingCalculationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PricingCalculationResponse>> CalculatePricing(
        [FromBody] PricingCalculationRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _pricingService.CalculatePricingAsync(request, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating pricing for subscription {SubscriptionId}", request.CustomerSubscriptionId);
            return StatusCode(500, new { error = "An error occurred while calculating pricing" });
        }
    }

    /// <summary>
    /// Simulate pricing for a subscription plan with hypothetical usage
    /// </summary>
    [HttpPost("simulate")]
    [ProducesResponseType(typeof(PricingSimulationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PricingSimulationResponse>> SimulatePricing(
        [FromBody] PricingSimulationRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _pricingService.SimulatePricingAsync(request, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error simulating pricing for plan {PlanId}", request.SubscriptionPlanId);
            return StatusCode(500, new { error = "An error occurred while simulating pricing" });
        }
    }
}
