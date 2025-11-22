using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MedWay.HospitalOnboarding.Application.Queries;
using MedWay.HospitalOnboarding.Application.DTOs;

namespace MedWay.WebAPI.Controllers.HospitalOnboarding;

/// <summary>
/// Subscription Plans API - View and manage subscription plans
/// </summary>
[ApiController]
[Route("api/v1/subscription-plans")]
[Produces("application/json")]
public class SubscriptionPlansController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<SubscriptionPlansController> _logger;

    public SubscriptionPlansController(IMediator mediator, ILogger<SubscriptionPlansController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all subscription plans (optionally public only for website display)
    /// </summary>
    /// <param name="publicOnly">If true, returns only public plans visible on website</param>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<SubscriptionPlanDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSubscriptionPlans([FromQuery] bool publicOnly = false)
    {
        var query = new GetSubscriptionPlansQuery(publicOnly);
        var result = await _mediator.Send(query);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error.Message });

        return Ok(result.Value);
    }

    /// <summary>
    /// Get subscription plan by ID with detailed information
    /// </summary>
    /// <param name="id">Subscription Plan ID</param>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(SubscriptionPlanDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSubscriptionPlanById(Guid id)
    {
        // TODO: Create GetSubscriptionPlanByIdQuery
        _logger.LogInformation("Getting subscription plan {PlanId}", id);
        return Ok(new SubscriptionPlanDto());
    }

    /// <summary>
    /// Calculate monthly cost for a subscription plan with custom parameters
    /// </summary>
    /// <param name="id">Subscription Plan ID</param>
    /// <param name="request">Cost calculation parameters</param>
    [HttpPost("{id:guid}/calculate-cost")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(CostCalculationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CalculateMonthlyCost(Guid id, [FromBody] CostCalculationRequest request)
    {
        // TODO: Create CalculateSubscriptionCostQuery
        _logger.LogInformation("Calculating cost for plan {PlanId}. Users: {Users}, Branches: {Branches}", 
            id, request.NumberOfUsers, request.NumberOfBranches);

        // Mock calculation
        var response = new CostCalculationResponse
        {
            PlanId = id,
            BaseMonthlyCost = 299m,
            AdditionalUserCost = 0m,
            AdditionalBranchCost = 0m,
            AdditionalFacilitiesCost = 0m,
            TotalMonthlyCost = 299m,
            Breakdown = new CostBreakdown
            {
                BasePrice = 299m,
                ExtraUsers = 0,
                ExtraUserCost = 0m,
                ExtraBranches = 0,
                ExtraBranchCost = 0m,
                AdditionalFacilities = 0,
                FacilityCost = 0m
            }
        };

        return Ok(response);
    }

    /// <summary>
    /// Compare multiple subscription plans side-by-side
    /// </summary>
    /// <param name="request">Plan IDs to compare</param>
    [HttpPost("compare")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PlanComparisonResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ComparePlans([FromBody] ComparePlansRequest request)
    {
        // TODO: Create CompareSubscriptionPlansQuery
        _logger.LogInformation("Comparing {Count} subscription plans", request.PlanIds.Count);
        
        var response = new PlanComparisonResponse
        {
            Plans = new List<SubscriptionPlanDto>(),
            ComparisonMatrix = new Dictionary<string, List<string>>()
        };

        return Ok(response);
    }

    /// <summary>
    /// Create new subscription plan (System Admin only)
    /// </summary>
    /// <param name="request">Subscription plan details</param>
    [HttpPost]
    [Authorize(Roles = "SuperAdmin,SystemAdmin")]
    [ProducesResponseType(typeof(CreatePlanResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateSubscriptionPlan([FromBody] CreateSubscriptionPlanRequest request)
    {
        // TODO: Create CreateSubscriptionPlanCommand
        _logger.LogInformation("Creating new subscription plan: {Name}", request.Name);
        
        var planId = Guid.NewGuid();
        var response = new CreatePlanResponse
        {
            PlanId = planId,
            Message = "Subscription plan created successfully"
        };

        return CreatedAtAction(nameof(GetSubscriptionPlanById), new { id = planId }, response);
    }

    /// <summary>
    /// Update subscription plan (System Admin only)
    /// </summary>
    /// <param name="id">Plan ID</param>
    /// <param name="request">Updated plan details</param>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "SuperAdmin,SystemAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSubscriptionPlan(Guid id, [FromBody] UpdateSubscriptionPlanRequest request)
    {
        // TODO: Create UpdateSubscriptionPlanCommand
        _logger.LogInformation("Updating subscription plan {PlanId}", id);
        return Ok(new { message = "Subscription plan updated successfully" });
    }

    /// <summary>
    /// Deactivate subscription plan (System Admin only)
    /// </summary>
    /// <param name="id">Plan ID</param>
    [HttpPost("{id:guid}/deactivate")]
    [Authorize(Roles = "SuperAdmin,SystemAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivateSubscriptionPlan(Guid id)
    {
        // TODO: Create DeactivateSubscriptionPlanCommand
        _logger.LogInformation("Deactivating subscription plan {PlanId}", id);
        return Ok(new { message = "Subscription plan deactivated successfully" });
    }
}

// DTOs
public record CostCalculationRequest
{
    public int NumberOfUsers { get; init; }
    public int NumberOfBranches { get; init; }
    public List<Guid> AdditionalFacilityIds { get; init; } = new();
}

public record CostCalculationResponse
{
    public Guid PlanId { get; init; }
    public decimal BaseMonthlyCost { get; init; }
    public decimal AdditionalUserCost { get; init; }
    public decimal AdditionalBranchCost { get; init; }
    public decimal AdditionalFacilitiesCost { get; init; }
    public decimal TotalMonthlyCost { get; init; }
    public CostBreakdown Breakdown { get; init; } = null!;
}

public record CostBreakdown
{
    public decimal BasePrice { get; init; }
    public int ExtraUsers { get; init; }
    public decimal ExtraUserCost { get; init; }
    public int ExtraBranches { get; init; }
    public decimal ExtraBranchCost { get; init; }
    public int AdditionalFacilities { get; init; }
    public decimal FacilityCost { get; init; }
}

public record ComparePlansRequest
{
    public List<Guid> PlanIds { get; init; } = new();
}

public record PlanComparisonResponse
{
    public List<SubscriptionPlanDto> Plans { get; init; } = new();
    public Dictionary<string, List<string>> ComparisonMatrix { get; init; } = new();
}

public record CreateSubscriptionPlanRequest
{
    public string Name { get; init; } = null!;
    public string Code { get; init; } = null!;
    public string Description { get; init; } = null!;
    public decimal BaseMonthlyPrice { get; init; }
    public int IncludedUsers { get; init; }
    public int IncludedBranches { get; init; }
    public int? MaxUsers { get; init; }
    public int? MaxBranches { get; init; }
    public decimal PricePerAdditionalUser { get; init; }
    public decimal PricePerAdditionalBranch { get; init; }
    public bool IsPublic { get; init; }
    public List<Guid> IncludedFacilityIds { get; init; } = new();
}

public record UpdateSubscriptionPlanRequest
{
    public string? Name { get; init; }
    public string? Description { get; init; }
    public decimal? BaseMonthlyPrice { get; init; }
    public decimal? PricePerAdditionalUser { get; init; }
    public decimal? PricePerAdditionalBranch { get; init; }
    public bool? IsPublic { get; init; }
}

public record CreatePlanResponse
{
    public Guid PlanId { get; init; }
    public string Message { get; init; } = null!;
}
