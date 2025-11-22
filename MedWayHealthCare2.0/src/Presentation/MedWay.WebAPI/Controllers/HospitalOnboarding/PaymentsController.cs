using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MedWay.HospitalOnboarding.Application.Commands;
using MedWay.HospitalOnboarding.Application.Queries;
using MedWay.HospitalOnboarding.Application.DTOs;
using MedWay.HospitalOnboarding.Domain.Entities;

namespace MedWay.WebAPI.Controllers.HospitalOnboarding;

/// <summary>
/// Payments API - Process and manage subscription payments
/// </summary>
[ApiController]
[Route("api/v1/payments")]
[Authorize]
[Produces("application/json")]
public class PaymentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(IMediator mediator, ILogger<PaymentsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get payment history for a hospital
    /// </summary>
    /// <param name="hospitalId">Hospital ID</param>
    [HttpGet("hospitals/{hospitalId:guid}")]
    [Authorize(Roles = "HospitalAdmin,SystemAdmin")]
    [ProducesResponseType(typeof(List<PaymentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPaymentHistory(Guid hospitalId)
    {
        var query = new GetPaymentHistoryQuery(hospitalId);
        var result = await _mediator.Send(query);

        if (result.IsFailure)
            return NotFound(new { error = result.Error.Message });

        return Ok(result.Value);
    }

    /// <summary>
    /// Get payment details by ID
    /// </summary>
    /// <param name="paymentId">Payment ID</param>
    [HttpGet("{paymentId:guid}")]
    [Authorize(Roles = "HospitalAdmin,SystemAdmin")]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPaymentById(Guid paymentId)
    {
        // TODO: Create GetPaymentByIdQuery
        _logger.LogInformation("Getting payment {PaymentId}", paymentId);
        return Ok(new PaymentDto());
    }

    /// <summary>
    /// Process a new payment for subscription
    /// </summary>
    /// <param name="request">Payment details</param>
    [HttpPost("process")]
    [Authorize(Roles = "HospitalAdmin")]
    [ProducesResponseType(typeof(ProcessPaymentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ProcessPayment([FromBody] ProcessPaymentRequest request)
    {
        var command = new ProcessPaymentCommand(
            request.HospitalId,
            request.SubscriptionPlanId,
            request.Amount,
            request.Currency,
            request.PaymentMethod,
            request.PaymentGateway,
            request.PaymentMetadata);

        var result = await _mediator.Send(command);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error.Message });

        var response = new ProcessPaymentResponse
        {
            PaymentId = result.Value,
            Status = "Successful",
            Message = "Payment processed successfully",
            TransactionId = $"TXN-{result.Value.ToString().Substring(0, 16).ToUpper()}"
        };

        return CreatedAtAction(nameof(GetPaymentById), new { paymentId = result.Value }, response);
    }

    /// <summary>
    /// Get pending payments for a hospital
    /// </summary>
    /// <param name="hospitalId">Hospital ID</param>
    [HttpGet("hospitals/{hospitalId:guid}/pending")]
    [Authorize(Roles = "HospitalAdmin,SystemAdmin")]
    [ProducesResponseType(typeof(List<PaymentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPendingPayments(Guid hospitalId)
    {
        // TODO: Create GetPendingPaymentsQuery
        _logger.LogInformation("Getting pending payments for hospital {HospitalId}", hospitalId);
        return Ok(new List<PaymentDto>());
    }

    /// <summary>
    /// Get overdue payments (System Admin)
    /// </summary>
    [HttpGet("overdue")]
    [Authorize(Roles = "SuperAdmin,SystemAdmin")]
    [ProducesResponseType(typeof(List<PaymentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOverduePayments()
    {
        // TODO: Create GetOverduePaymentsQuery
        _logger.LogInformation("Getting overdue payments");
        return Ok(new List<PaymentDto>());
    }

    /// <summary>
    /// Retry a failed payment
    /// </summary>
    /// <param name="paymentId">Payment ID</param>
    [HttpPost("{paymentId:guid}/retry")]
    [Authorize(Roles = "HospitalAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RetryPayment(Guid paymentId)
    {
        // TODO: Create RetryPaymentCommand
        _logger.LogInformation("Retrying payment {PaymentId}", paymentId);
        return Ok(new { message = "Payment retry initiated" });
    }

    /// <summary>
    /// Request refund for a payment (System Admin)
    /// </summary>
    /// <param name="paymentId">Payment ID</param>
    /// <param name="request">Refund request details</param>
    [HttpPost("{paymentId:guid}/refund")]
    [Authorize(Roles = "SuperAdmin,SystemAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RefundPayment(Guid paymentId, [FromBody] RefundPaymentRequest request)
    {
        // TODO: Create RefundPaymentCommand
        _logger.LogInformation("Refunding payment {PaymentId}. Reason: {Reason}", paymentId, request.Reason);
        return Ok(new { message = "Payment refunded successfully" });
    }

    /// <summary>
    /// Download payment invoice
    /// </summary>
    /// <param name="paymentId">Payment ID</param>
    [HttpGet("{paymentId:guid}/invoice")]
    [Authorize(Roles = "HospitalAdmin,SystemAdmin")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadInvoice(Guid paymentId)
    {
        // TODO: Create GenerateInvoicePdfQuery
        _logger.LogInformation("Generating invoice for payment {PaymentId}", paymentId);
        
        // Mock PDF generation
        var pdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46 }; // PDF header
        return File(pdfBytes, "application/pdf", $"Invoice-{paymentId}.pdf");
    }

    /// <summary>
    /// Get payment statistics for a hospital
    /// </summary>
    /// <param name="hospitalId">Hospital ID</param>
    /// <param name="startDate">Start date for statistics</param>
    /// <param name="endDate">End date for statistics</param>
    [HttpGet("hospitals/{hospitalId:guid}/statistics")]
    [Authorize(Roles = "HospitalAdmin,SystemAdmin")]
    [ProducesResponseType(typeof(PaymentStatisticsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaymentStatistics(
        Guid hospitalId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        // TODO: Create GetPaymentStatisticsQuery
        _logger.LogInformation("Getting payment statistics for hospital {HospitalId}", hospitalId);
        
        var response = new PaymentStatisticsResponse
        {
            HospitalId = hospitalId,
            TotalPayments = 0,
            SuccessfulPayments = 0,
            FailedPayments = 0,
            PendingPayments = 0,
            TotalAmountPaid = 0m,
            AveragePaymentAmount = 0m
        };

        return Ok(response);
    }
}

// DTOs
public record ProcessPaymentRequest
{
    public Guid HospitalId { get; init; }
    public Guid? SubscriptionPlanId { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "USD";
    public PaymentMethod PaymentMethod { get; init; }
    public string? PaymentGateway { get; init; }
    public Dictionary<string, string>? PaymentMetadata { get; init; }
}

public record ProcessPaymentResponse
{
    public Guid PaymentId { get; init; }
    public string Status { get; init; } = null!;
    public string Message { get; init; } = null!;
    public string? TransactionId { get; init; }
}

public record RefundPaymentRequest
{
    public string Reason { get; init; } = null!;
}

public record PaymentStatisticsResponse
{
    public Guid HospitalId { get; init; }
    public int TotalPayments { get; init; }
    public int SuccessfulPayments { get; init; }
    public int FailedPayments { get; init; }
    public int PendingPayments { get; init; }
    public decimal TotalAmountPaid { get; init; }
    public decimal AveragePaymentAmount { get; init; }
}
