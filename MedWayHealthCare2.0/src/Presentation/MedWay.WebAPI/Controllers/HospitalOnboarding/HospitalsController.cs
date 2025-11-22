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
    /// Get all hospitals (System Admin only)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "SuperAdmin,SystemAdmin")]
    [ProducesResponseType(typeof(List<HospitalDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllHospitals()
    {
        // TODO: Implement GetAllHospitalsQuery with pagination
        return Ok(new List<HospitalDto>());
    }

    /// <summary>
    /// Approve hospital registration (System Admin only)
    /// </summary>
    [HttpPost("{id:guid}/approve")]
    [Authorize(Roles = "SuperAdmin,SystemAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ApproveHospital(Guid id)
    {
        // TODO: Implement ApproveHospitalCommand
        return Ok(new { message = "Hospital approved successfully" });
    }

    /// <summary>
    /// Reject hospital registration (System Admin only)
    /// </summary>
    [HttpPost("{id:guid}/reject")]
    [Authorize(Roles = "SuperAdmin,SystemAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RejectHospital(Guid id, [FromBody] RejectHospitalRequest request)
    {
        // TODO: Implement RejectHospitalCommand
        return Ok(new { message = "Hospital rejected" });
    }

    /// <summary>
    /// Suspend hospital (System Admin only)
    /// </summary>
    [HttpPost("{id:guid}/suspend")]
    [Authorize(Roles = "SuperAdmin,SystemAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SuspendHospital(Guid id, [FromBody] SuspendHospitalRequest request)
    {
        // TODO: Implement SuspendHospitalCommand
        return Ok(new { message = "Hospital suspended" });
    }

    /// <summary>
    /// Activate subscription for hospital
    /// </summary>
    [HttpPost("{id:guid}/subscribe")]
    [Authorize(Roles = "HospitalAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ActivateSubscription(Guid id, [FromBody] ActivateSubscriptionRequest request)
    {
        // TODO: Implement ActivateSubscriptionCommand
        return Ok(new { message = "Subscription activated" });
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

public record RejectHospitalRequest(string Reason);
public record SuspendHospitalRequest(string Reason);
public record ActivateSubscriptionRequest(Guid SubscriptionPlanId, int DurationMonths);
