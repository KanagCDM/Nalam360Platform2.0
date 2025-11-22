using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MedWay.HospitalOnboarding.Application.Commands;
using MedWay.HospitalOnboarding.Application.Queries;
using MedWay.HospitalOnboarding.Application.DTOs;

namespace MedWay.WebAPI.Controllers.HospitalOnboarding;

/// <summary>
/// Branch Management API for hospitals
/// </summary>
[ApiController]
[Route("api/v1/hospitals/{hospitalId:guid}/branches")]
[Authorize(Roles = "HospitalAdmin,BranchAdmin,SystemAdmin")]
[Produces("application/json")]
public class BranchesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<BranchesController> _logger;

    public BranchesController(IMediator mediator, ILogger<BranchesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Add a new branch to hospital
    /// </summary>
    /// <param name="hospitalId">Hospital ID</param>
    /// <param name="request">Branch details</param>
    /// <returns>Created branch ID</returns>
    [HttpPost]
    [Authorize(Roles = "HospitalAdmin")]
    [ProducesResponseType(typeof(AddBranchResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddBranch(Guid hospitalId, [FromBody] AddBranchRequest request)
    {
        var command = new AddBranchCommand(
            hospitalId,
            request.Name,
            request.BranchCode,
            request.Address.AddressLine1,
            request.Address.AddressLine2,
            request.Address.City,
            request.Address.State,
            request.Address.PostalCode,
            request.Address.Country,
            request.Phone,
            request.Email,
            request.OpeningDate,
            request.OperatingHours);

        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            return result.Error.Code switch
            {
                "NotFound" => NotFound(new { error = result.Error.Message }),
                "Conflict" => Conflict(new { error = result.Error.Message }),
                _ => BadRequest(new { error = result.Error.Message })
            };
        }

        var response = new AddBranchResponse
        {
            BranchId = result.Value,
            HospitalId = hospitalId,
            Message = "Branch added successfully"
        };

        return CreatedAtAction(nameof(GetBranchById), new { hospitalId, branchId = result.Value }, response);
    }

    /// <summary>
    /// Get all branches for a hospital
    /// </summary>
    /// <param name="hospitalId">Hospital ID</param>
    [HttpGet]
    [ProducesResponseType(typeof(List<BranchDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBranches(Guid hospitalId)
    {
        // TODO: Create GetBranchesByHospitalQuery
        _logger.LogInformation("Getting branches for hospital {HospitalId}", hospitalId);
        return Ok(new List<BranchDto>());
    }

    /// <summary>
    /// Get specific branch details
    /// </summary>
    /// <param name="hospitalId">Hospital ID</param>
    /// <param name="branchId">Branch ID</param>
    [HttpGet("{branchId:guid}")]
    [ProducesResponseType(typeof(BranchDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBranchById(Guid hospitalId, Guid branchId)
    {
        // TODO: Create GetBranchByIdQuery
        _logger.LogInformation("Getting branch {BranchId} for hospital {HospitalId}", branchId, hospitalId);
        return Ok(new BranchDto());
    }

    /// <summary>
    /// Update branch details
    /// </summary>
    /// <param name="hospitalId">Hospital ID</param>
    /// <param name="branchId">Branch ID</param>
    /// <param name="request">Updated branch details</param>
    [HttpPut("{branchId:guid}")]
    [Authorize(Roles = "HospitalAdmin,BranchAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateBranch(Guid hospitalId, Guid branchId, [FromBody] UpdateBranchRequest request)
    {
        // TODO: Create UpdateBranchCommand
        _logger.LogInformation("Updating branch {BranchId} for hospital {HospitalId}", branchId, hospitalId);
        return Ok(new { message = "Branch updated successfully" });
    }

    /// <summary>
    /// Deactivate a branch
    /// </summary>
    /// <param name="hospitalId">Hospital ID</param>
    /// <param name="branchId">Branch ID</param>
    [HttpPost("{branchId:guid}/deactivate")]
    [Authorize(Roles = "HospitalAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivateBranch(Guid hospitalId, Guid branchId)
    {
        // TODO: Create DeactivateBranchCommand
        _logger.LogInformation("Deactivating branch {BranchId} for hospital {HospitalId}", branchId, hospitalId);
        return Ok(new { message = "Branch deactivated successfully" });
    }

    /// <summary>
    /// Assign manager to branch
    /// </summary>
    /// <param name="hospitalId">Hospital ID</param>
    /// <param name="branchId">Branch ID</param>
    /// <param name="request">Manager details</param>
    [HttpPost("{branchId:guid}/assign-manager")]
    [Authorize(Roles = "HospitalAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignManager(Guid hospitalId, Guid branchId, [FromBody] AssignManagerRequest request)
    {
        // TODO: Create AssignBranchManagerCommand
        _logger.LogInformation("Assigning manager {ManagerId} to branch {BranchId}", request.ManagerUserId, branchId);
        return Ok(new { message = "Manager assigned successfully" });
    }
}

// DTOs
public record AddBranchRequest
{
    public string Name { get; init; } = null!;
    public string BranchCode { get; init; } = null!;
    public BranchAddressRequest Address { get; init; } = null!;
    public string Phone { get; init; } = null!;
    public string Email { get; init; } = null!;
    public DateTime OpeningDate { get; init; }
    public string? OperatingHours { get; init; }
}

public record BranchAddressRequest
{
    public string AddressLine1 { get; init; } = null!;
    public string? AddressLine2 { get; init; }
    public string City { get; init; } = null!;
    public string State { get; init; } = null!;
    public string PostalCode { get; init; } = null!;
    public string Country { get; init; } = null!;
}

public record AddBranchResponse
{
    public Guid BranchId { get; init; }
    public Guid HospitalId { get; init; }
    public string Message { get; init; } = null!;
}

public record UpdateBranchRequest
{
    public string? Name { get; init; }
    public BranchAddressRequest? Address { get; init; }
    public string? Phone { get; init; }
    public string? Email { get; init; }
    public string? OperatingHours { get; init; }
}

public record AssignManagerRequest
{
    public Guid ManagerUserId { get; init; }
}

public record BranchDto
{
    public Guid Id { get; init; }
    public Guid HospitalId { get; init; }
    public string Name { get; init; } = null!;
    public string BranchCode { get; init; } = null!;
    public AddressDto Address { get; init; } = null!;
    public string Phone { get; init; } = null!;
    public string Email { get; init; } = null!;
    public Guid? ManagerUserId { get; init; }
    public bool IsActive { get; init; }
    public DateTime OpeningDate { get; init; }
    public DateTime? ClosingDate { get; init; }
    public string? OperatingHours { get; init; }
    public List<FacilityDto> Facilities { get; init; } = new();
}

public record AddressDto
{
    public string AddressLine1 { get; init; } = null!;
    public string? AddressLine2 { get; init; }
    public string City { get; init; } = null!;
    public string State { get; init; } = null!;
    public string PostalCode { get; init; } = null!;
    public string Country { get; init; } = null!;
}
