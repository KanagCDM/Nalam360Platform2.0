using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MedWay.HospitalOnboarding.Application.Commands;
using MedWay.HospitalOnboarding.Application.Queries;

namespace MedWay.WebAPI.Controllers.HospitalOnboarding;

/// <summary>
/// Hospital Onboarding API
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class HospitalsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<HospitalsController> _logger;

    public HospitalsController(IMediator mediator, ILogger<HospitalsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Register new hospital (Public endpoint for trial registration)
    /// </summary>
    /// <param name="request">Hospital registration details</param>
    /// <returns>Hospital ID and tenant credentials</returns>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(RegisterHospitalResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RegisterHospital([FromBody] RegisterHospitalRequest request)
    {
        var command = new RegisterHospitalCommand(
            request.Name,
            request.RegistrationNumber,
            request.Address.Street,
            request.Address.City,
            request.Address.State,
            request.Address.PostalCode,
            request.Address.Country,
            request.PrimaryEmail,
            request.PrimaryPhone,
            request.EstablishedDate,
            request.TaxNumber,
            request.TrialDays ?? 30);

        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            return result.Error.Code switch
            {
                "Conflict" => Conflict(new { error = result.Error.Message }),
                "Validation" => BadRequest(new { error = result.Error.Message }),
                _ => BadRequest(new { error = result.Error.Message })
            };
        }

        var response = new RegisterHospitalResponse
        {
            HospitalId = result.Value,
            Message = "Hospital registered successfully. Trial period activated.",
            TrialEndsOn = DateTime.UtcNow.AddDays(request.TrialDays ?? 30)
        };

        return CreatedAtAction(nameof(GetHospitalById), new { id = result.Value }, response);
    }

    /// <summary>
    /// Get hospital details by ID
    /// </summary>
    /// <param name="id">Hospital ID</param>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "SuperAdmin,SystemAdmin,HospitalAdmin")]
    [ProducesResponseType(typeof(HospitalDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetHospitalById(Guid id)
    {
        var query = new GetHospitalByIdQuery(id);
        var result = await _mediator.Send(query);

        if (result.IsFailure)
            return NotFound(new { error = result.Error.Message });

        return Ok(result.Value);
    }

    /// <summary>
    /// Get all hospitals with pagination and optional filtering (System Admin only)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "SuperAdmin,SystemAdmin")]
    [ProducesResponseType(typeof(PagedResult<HospitalSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAllHospitals(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null,
        [FromQuery] string? searchTerm = null)
    {
        var query = new GetAllHospitalsQuery(
            pageNumber,
            pageSize,
            string.IsNullOrWhiteSpace(status) ? null : Enum.Parse<HospitalStatus>(status),
            searchTerm);

        var result = await _mediator.Send(query);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error.Message });

        return Ok(result.Value);
    }

    /// <summary>
    /// Approve hospital registration (System Admin only)
    /// </summary>
    [HttpPost("{id:guid}/approve")]
    [Authorize(Roles = "SuperAdmin,SystemAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ApproveHospital(Guid id, [FromBody] ApproveHospitalRequest request)
    {
        var command = new ApproveHospitalCommand(id, request.ApprovedBy ?? User.Identity?.Name ?? "System");
        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            return result.Error.Code switch
            {
                "NotFound" => NotFound(new { error = result.Error.Message }),
                _ => BadRequest(new { error = result.Error.Message })
            };
        }

        return Ok(new { message = "Hospital approved successfully" });
    }

    /// <summary>
    /// Reject hospital registration with reason (System Admin only)
    /// </summary>
    [HttpPost("{id:guid}/reject")]
    [Authorize(Roles = "SuperAdmin,SystemAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RejectHospital(Guid id, [FromBody] RejectHospitalRequest request)
    {
        var command = new RejectHospitalCommand(
            id, 
            request.Reason, 
            request.RejectedBy ?? User.Identity?.Name ?? "System");

        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            return result.Error.Code switch
            {
                "NotFound" => NotFound(new { error = result.Error.Message }),
                _ => BadRequest(new { error = result.Error.Message })
            };
        }

        return Ok(new { message = "Hospital rejected", reason = request.Reason });
    }

    /// <summary>
    /// Suspend hospital (System Admin only)
    /// </summary>
    [HttpPost("{id:guid}/suspend")]
    [Authorize(Roles = "SuperAdmin,SystemAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SuspendHospital(Guid id, [FromBody] SuspendHospitalRequest request)
    {
        var command = new SuspendHospitalCommand(
            id, 
            request.Reason, 
            request.SuspendedBy ?? User.Identity?.Name ?? "System");

        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            return result.Error.Code switch
            {
                "NotFound" => NotFound(new { error = result.Error.Message }),
                _ => BadRequest(new { error = result.Error.Message })
            };
        }

        return Ok(new { message = "Hospital suspended", reason = request.Reason });
    }

    /// <summary>
    /// Activate subscription for hospital with plan selection
    /// </summary>
    [HttpPost("{id:guid}/subscribe")]
    [Authorize(Roles = "HospitalAdmin")]
    [ProducesResponseType(typeof(ActivateSubscriptionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActivateSubscription(Guid id, [FromBody] ActivateSubscriptionRequest request)
    {
        var command = new ActivateSubscriptionCommand(
            id,
            request.SubscriptionPlanId,
            request.NumberOfUsers,
            request.NumberOfBranches,
            request.AdditionalFacilityIds ?? new List<Guid>());

        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            return result.Error.Code switch
            {
                "NotFound" => NotFound(new { error = result.Error.Message }),
                _ => BadRequest(new { error = result.Error.Message })
            };
        }

        var response = new ActivateSubscriptionResponse
        {
            Message = "Subscription activated successfully",
            MonthlyCost = result.Value,
            SubscriptionPlanId = request.SubscriptionPlanId,
            NumberOfUsers = request.NumberOfUsers,
            NumberOfBranches = request.NumberOfBranches
        };

        return Ok(response);
    }
}

// Request/Response DTOs
public record RegisterHospitalRequest
{
    public string Name { get; init; } = null!;
    public string RegistrationNumber { get; init; } = null!;
    public string? TaxNumber { get; init; }
    public AddressRequest Address { get; init; } = null!;
    public string PrimaryEmail { get; init; } = null!;
    public string PrimaryPhone { get; init; } = null!;
    public DateTime EstablishedDate { get; init; }
    public int? TrialDays { get; init; }
}

public record AddressRequest
{
    public string Street { get; init; } = null!;
    public string City { get; init; } = null!;
    public string State { get; init; } = null!;
    public string PostalCode { get; init; } = null!;
    public string Country { get; init; } = null!;
}

public record RegisterHospitalResponse
{
    public Guid HospitalId { get; init; }
    public string Message { get; init; } = null!;
    public DateTime TrialEndsOn { get; init; }
}

public record ApproveHospitalRequest
{
    public string? ApprovedBy { get; init; }
}

public record RejectHospitalRequest
{
    public string Reason { get; init; } = null!;
    public string? RejectedBy { get; init; }
}

public record SuspendHospitalRequest
{
    public string Reason { get; init; } = null!;
    public string? SuspendedBy { get; init; }
}

public record ActivateSubscriptionRequest
{
    public Guid SubscriptionPlanId { get; init; }
    public int NumberOfUsers { get; init; }
    public int NumberOfBranches { get; init; }
    public List<Guid>? AdditionalFacilityIds { get; init; }
}

public record ActivateSubscriptionResponse
{
    public string Message { get; init; } = null!;
    public decimal MonthlyCost { get; init; }
    public Guid SubscriptionPlanId { get; init; }
    public int NumberOfUsers { get; init; }
    public int NumberOfBranches { get; init; }
}
