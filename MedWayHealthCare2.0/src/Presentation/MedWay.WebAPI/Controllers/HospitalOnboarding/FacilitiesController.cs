using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MedWay.HospitalOnboarding.Application.DTOs;

namespace MedWay.WebAPI.Controllers.HospitalOnboarding;

/// <summary>
/// Facilities Management API - Global facilities and hospital/branch facility mappings
/// </summary>
[ApiController]
[Route("api/v1")]
[Produces("application/json")]
public class FacilitiesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<FacilitiesController> _logger;

    public FacilitiesController(IMediator mediator, ILogger<FacilitiesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all global facilities
    /// </summary>
    /// <param name="category">Optional: Filter by category (Clinical, Diagnostic, Support, Administrative)</param>
    [HttpGet("facilities")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<FacilityDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetGlobalFacilities([FromQuery] string? category = null)
    {
        // TODO: Create GetGlobalFacilitiesQuery
        _logger.LogInformation("Getting global facilities. Category: {Category}", category ?? "All");
        
        // Temporary mock data
        var facilities = new List<FacilityDto>
        {
            new() { Id = Guid.NewGuid(), Name = "Emergency", Code = "EMR", Category = "Clinical", BaseMonthlyCost = 500 },
            new() { Id = Guid.NewGuid(), Name = "ICU", Code = "ICU", Category = "Clinical", BaseMonthlyCost = 800 },
            new() { Id = Guid.NewGuid(), Name = "OPD", Code = "OPD", Category = "Clinical", BaseMonthlyCost = 300 },
            new() { Id = Guid.NewGuid(), Name = "Laboratory", Code = "LAB", Category = "Diagnostic", BaseMonthlyCost = 400 },
            new() { Id = Guid.NewGuid(), Name = "Radiology", Code = "RAD", Category = "Diagnostic", BaseMonthlyCost = 600 },
            new() { Id = Guid.NewGuid(), Name = "Pharmacy", Code = "PHM", Category = "Support", BaseMonthlyCost = 250 },
            new() { Id = Guid.NewGuid(), Name = "Billing", Code = "BIL", Category = "Administrative", BaseMonthlyCost = 200 }
        };

        return Ok(facilities);
    }

    /// <summary>
    /// Get facility by ID
    /// </summary>
    /// <param name="facilityId">Facility ID</param>
    [HttpGet("facilities/{facilityId:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(FacilityDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFacilityById(Guid facilityId)
    {
        // TODO: Create GetFacilityByIdQuery
        _logger.LogInformation("Getting facility {FacilityId}", facilityId);
        return Ok(new FacilityDto());
    }

    // ==================== HOSPITAL FACILITIES ====================

    /// <summary>
    /// Get all facilities mapped to a hospital
    /// </summary>
    /// <param name="hospitalId">Hospital ID</param>
    [HttpGet("hospitals/{hospitalId:guid}/facilities")]
    [Authorize(Roles = "HospitalAdmin,BranchAdmin,SystemAdmin")]
    [ProducesResponseType(typeof(List<FacilityDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetHospitalFacilities(Guid hospitalId)
    {
        // TODO: Create GetHospitalFacilitiesQuery
        _logger.LogInformation("Getting facilities for hospital {HospitalId}", hospitalId);
        return Ok(new List<FacilityDto>());
    }

    /// <summary>
    /// Add facility to hospital
    /// </summary>
    /// <param name="hospitalId">Hospital ID</param>
    /// <param name="request">Facility mapping request</param>
    [HttpPost("hospitals/{hospitalId:guid}/facilities")]
    [Authorize(Roles = "HospitalAdmin")]
    [ProducesResponseType(typeof(AddFacilityResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddFacilityToHospital(Guid hospitalId, [FromBody] AddFacilityRequest request)
    {
        // TODO: Create AddHospitalFacilityCommand
        _logger.LogInformation("Adding facility {FacilityId} to hospital {HospitalId}", request.FacilityId, hospitalId);
        
        var response = new AddFacilityResponse
        {
            HospitalId = hospitalId,
            FacilityId = request.FacilityId,
            Message = "Facility added to hospital successfully"
        };

        return CreatedAtAction(nameof(GetHospitalFacilities), new { hospitalId }, response);
    }

    /// <summary>
    /// Remove facility from hospital
    /// </summary>
    /// <param name="hospitalId">Hospital ID</param>
    /// <param name="facilityId">Facility ID</param>
    [HttpDelete("hospitals/{hospitalId:guid}/facilities/{facilityId:guid}")]
    [Authorize(Roles = "HospitalAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveFacilityFromHospital(Guid hospitalId, Guid facilityId)
    {
        // TODO: Create RemoveHospitalFacilityCommand
        _logger.LogInformation("Removing facility {FacilityId} from hospital {HospitalId}", facilityId, hospitalId);
        return Ok(new { message = "Facility removed from hospital successfully" });
    }

    // ==================== BRANCH FACILITIES ====================

    /// <summary>
    /// Get all facilities mapped to a specific branch
    /// </summary>
    /// <param name="hospitalId">Hospital ID</param>
    /// <param name="branchId">Branch ID</param>
    [HttpGet("hospitals/{hospitalId:guid}/branches/{branchId:guid}/facilities")]
    [Authorize(Roles = "HospitalAdmin,BranchAdmin,SystemAdmin")]
    [ProducesResponseType(typeof(List<FacilityDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBranchFacilities(Guid hospitalId, Guid branchId)
    {
        // TODO: Create GetBranchFacilitiesQuery
        _logger.LogInformation("Getting facilities for branch {BranchId} in hospital {HospitalId}", branchId, hospitalId);
        return Ok(new List<FacilityDto>());
    }

    /// <summary>
    /// Map facility to branch (branch-level facility management)
    /// </summary>
    /// <param name="hospitalId">Hospital ID</param>
    /// <param name="branchId">Branch ID</param>
    /// <param name="request">Facility mapping request</param>
    [HttpPost("hospitals/{hospitalId:guid}/branches/{branchId:guid}/facilities")]
    [Authorize(Roles = "HospitalAdmin,BranchAdmin")]
    [ProducesResponseType(typeof(AddFacilityResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddFacilityToBranch(Guid hospitalId, Guid branchId, [FromBody] AddFacilityRequest request)
    {
        // TODO: Create AddBranchFacilityCommand
        _logger.LogInformation("Adding facility {FacilityId} to branch {BranchId} in hospital {HospitalId}", 
            request.FacilityId, branchId, hospitalId);
        
        var response = new AddFacilityResponse
        {
            HospitalId = hospitalId,
            BranchId = branchId,
            FacilityId = request.FacilityId,
            Message = "Facility mapped to branch successfully"
        };

        return CreatedAtAction(nameof(GetBranchFacilities), new { hospitalId, branchId }, response);
    }

    /// <summary>
    /// Remove facility mapping from branch
    /// </summary>
    /// <param name="hospitalId">Hospital ID</param>
    /// <param name="branchId">Branch ID</param>
    /// <param name="facilityId">Facility ID</param>
    [HttpDelete("hospitals/{hospitalId:guid}/branches/{branchId:guid}/facilities/{facilityId:guid}")]
    [Authorize(Roles = "HospitalAdmin,BranchAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveFacilityFromBranch(Guid hospitalId, Guid branchId, Guid facilityId)
    {
        // TODO: Create RemoveBranchFacilityCommand
        _logger.LogInformation("Removing facility {FacilityId} from branch {BranchId} in hospital {HospitalId}", 
            facilityId, branchId, hospitalId);
        return Ok(new { message = "Facility removed from branch successfully" });
    }

    /// <summary>
    /// Bulk map multiple facilities to a branch
    /// </summary>
    /// <param name="hospitalId">Hospital ID</param>
    /// <param name="branchId">Branch ID</param>
    /// <param name="request">Bulk facility mapping request</param>
    [HttpPost("hospitals/{hospitalId:guid}/branches/{branchId:guid}/facilities/bulk")]
    [Authorize(Roles = "HospitalAdmin,BranchAdmin")]
    [ProducesResponseType(typeof(BulkAddFacilitiesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> BulkAddFacilitiesToBranch(Guid hospitalId, Guid branchId, [FromBody] BulkAddFacilitiesRequest request)
    {
        // TODO: Create BulkAddBranchFacilitiesCommand
        _logger.LogInformation("Bulk adding {Count} facilities to branch {BranchId}", request.FacilityIds.Count, branchId);
        
        var response = new BulkAddFacilitiesResponse
        {
            AddedCount = request.FacilityIds.Count,
            FailedCount = 0,
            Message = $"Successfully added {request.FacilityIds.Count} facilities to branch"
        };

        return Ok(response);
    }
}

// DTOs
public record AddFacilityRequest
{
    public Guid FacilityId { get; init; }
}

public record AddFacilityResponse
{
    public Guid HospitalId { get; init; }
    public Guid? BranchId { get; init; }
    public Guid FacilityId { get; init; }
    public string Message { get; init; } = null!;
}

public record BulkAddFacilitiesRequest
{
    public List<Guid> FacilityIds { get; init; } = new();
}

public record BulkAddFacilitiesResponse
{
    public int AddedCount { get; init; }
    public int FailedCount { get; init; }
    public string Message { get; init; } = null!;
    public List<string> Errors { get; init; } = new();
}
