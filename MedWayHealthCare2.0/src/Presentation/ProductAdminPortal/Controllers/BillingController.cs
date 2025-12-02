using Microsoft.AspNetCore.Mvc;
using ProductAdminPortal.Models.Domain;
using ProductAdminPortal.Services;

namespace ProductAdminPortal.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class BillingController : ControllerBase
{
    private readonly IBillingService _billingService;
    private readonly ILogger<BillingController> _logger;

    public BillingController(IBillingService billingService, ILogger<BillingController> logger)
    {
        _billingService = billingService;
        _logger = logger;
    }

    /// <summary>
    /// Generate an invoice for a customer subscription
    /// </summary>
    [HttpPost("invoices")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Guid>> GenerateInvoice(
        [FromBody] GenerateInvoiceRequest request,
        [FromHeader(Name = "X-Tenant-Id")] Guid tenantId,
        [FromHeader(Name = "X-User-Id")] Guid userId,
        CancellationToken cancellationToken)
    {
        try
        {
            var invoiceId = await _billingService.GenerateInvoiceAsync(
                request.CustomerSubscriptionId,
                request.BillingPeriodStart,
                request.BillingPeriodEnd,
                tenantId,
                userId,
                cancellationToken);

            return CreatedAtAction(nameof(GetInvoice), new { id = invoiceId }, invoiceId);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating invoice for subscription {SubscriptionId}", request.CustomerSubscriptionId);
            return StatusCode(500, new { error = "An error occurred while generating the invoice" });
        }
    }

    /// <summary>
    /// Get a specific invoice by ID
    /// </summary>
    [HttpGet("invoices/{id}")]
    [ProducesResponseType(typeof(Invoice), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Invoice>> GetInvoice(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var invoice = await _billingService.GetInvoiceByIdAsync(id, cancellationToken);
            if (invoice == null)
                return NotFound(new { error = $"Invoice {id} not found" });

            return Ok(invoice);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting invoice {InvoiceId}", id);
            return StatusCode(500, new { error = "An error occurred while retrieving the invoice" });
        }
    }

    /// <summary>
    /// Get all invoices for a customer
    /// </summary>
    [HttpGet("customers/{customerId}/invoices")]
    [ProducesResponseType(typeof(List<Invoice>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<Invoice>>> GetCustomerInvoices(
        Guid customerId,
        [FromHeader(Name = "X-Tenant-Id")] Guid tenantId,
        CancellationToken cancellationToken)
    {
        try
        {
            var invoices = await _billingService.GetCustomerInvoicesAsync(customerId, tenantId, cancellationToken);
            return Ok(invoices);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting invoices for customer {CustomerId}", customerId);
            return StatusCode(500, new { error = "An error occurred while retrieving invoices" });
        }
    }

    /// <summary>
    /// Mark an invoice as paid
    /// </summary>
    [HttpPost("invoices/{id}/mark-paid")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkInvoiceAsPaid(
        Guid id,
        [FromHeader(Name = "X-Tenant-Id")] Guid tenantId,
        [FromHeader(Name = "X-User-Id")] Guid userId,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _billingService.MarkInvoiceAsPaidAsync(id, tenantId, userId, cancellationToken);
            if (!result)
                return Ok(new { message = "Invoice was already marked as paid" });

            return Ok(new { message = "Invoice marked as paid successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking invoice {InvoiceId} as paid", id);
            return StatusCode(500, new { error = "An error occurred while updating the invoice" });
        }
    }
}

public record GenerateInvoiceRequest(
    Guid CustomerSubscriptionId,
    DateTime BillingPeriodStart,
    DateTime BillingPeriodEnd
);
