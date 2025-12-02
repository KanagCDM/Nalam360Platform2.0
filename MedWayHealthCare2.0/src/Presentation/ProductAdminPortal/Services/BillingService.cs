using Microsoft.EntityFrameworkCore;
using ProductAdminPortal.Data;
using ProductAdminPortal.Models.Domain;
using System.Text.Json;

namespace ProductAdminPortal.Services;

public class BillingService : IBillingService
{
    private readonly ProductAdminDbContext _context;
    private readonly IPricingService _pricingService;
    private readonly IAuditService _auditService;

    public BillingService(
        ProductAdminDbContext context,
        IPricingService pricingService,
        IAuditService auditService)
    {
        _context = context;
        _pricingService = pricingService;
        _auditService = auditService;
    }

    public async Task<Guid> GenerateInvoiceAsync(
        Guid customerSubscriptionId,
        DateTime billingPeriodStart,
        DateTime billingPeriodEnd,
        Guid tenantId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var subscription = await _context.CustomerSubscriptions
            .Include(cs => cs.Customer)
            .Include(cs => cs.SubscriptionPlan)
            .FirstOrDefaultAsync(cs => cs.Id == customerSubscriptionId, cancellationToken);

        if (subscription == null)
            throw new KeyNotFoundException($"Customer subscription {customerSubscriptionId} not found");

        // Get usage records for the billing period
        var usageRecords = await _context.UsageRecords
            .Where(ur => ur.CustomerSubscriptionId == customerSubscriptionId &&
                        ur.RecordedAt >= billingPeriodStart &&
                        ur.RecordedAt < billingPeriodEnd &&
                        ur.BillingStatus == "unbilled")
            .ToListAsync(cancellationToken);

        // Calculate pricing using PricingService
        var pricingRequest = new DTOs.PricingCalculationRequest(
            customerSubscriptionId,
            billingPeriodStart,
            billingPeriodEnd,
            usageRecords.Select(ur => new DTOs.UsageRecordInput(
                ur.EntityId,
                ur.Units,
                ur.Complexity
            )).ToList()
        );

        var pricingResult = await _pricingService.CalculatePricingAsync(pricingRequest, cancellationToken);

        // Generate invoice number
        var invoiceCount = await _context.Invoices.CountAsync(cancellationToken);
        var invoiceNumber = $"INV-{DateTime.UtcNow:yyyyMM}-{(invoiceCount + 1):D5}";

        // Create invoice
        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            CustomerSubscriptionId = customerSubscriptionId,
            InvoiceNumber = invoiceNumber,
            BillingPeriodStart = billingPeriodStart,
            BillingPeriodEnd = billingPeriodEnd,
            Subtotal = pricingResult.Subtotal,
            TaxAmount = pricingResult.TaxAmount,
            DiscountAmount = pricingResult.DiscountAmount,
            TotalAmount = pricingResult.TotalAmount,
            Status = "draft",
            DueDate = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Invoices.Add(invoice);

        // Create invoice line items
        foreach (var lineItem in pricingResult.LineItems)
        {
            var invoiceLineItem = new InvoiceLineItem
            {
                Id = Guid.NewGuid(),
                InvoiceId = invoice.Id,
                Description = lineItem.Description,
                Quantity = lineItem.Quantity,
                UnitPrice = lineItem.UnitPrice,
                Amount = lineItem.Amount,
                Metadata = lineItem.Metadata != null ? JsonSerializer.Serialize(lineItem.Metadata) : null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.InvoiceLineItems.Add(invoiceLineItem);
        }

        // Update usage records billing status
        foreach (var usageRecord in usageRecords)
        {
            usageRecord.BillingStatus = "invoiced";
            usageRecord.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Log audit entry
        await _auditService.LogActionAsync(
            "create",
            "Invoice",
            invoice.Id,
            null,
            new Dictionary<string, object>
            {
                { "invoice_number", invoiceNumber },
                { "customer_subscription_id", customerSubscriptionId },
                { "total_amount", pricingResult.TotalAmount },
                { "line_items_count", pricingResult.LineItems.Count }
            },
            tenantId,
            userId,
            cancellationToken);

        return invoice.Id;
    }

    public async Task<Invoice?> GetInvoiceByIdAsync(Guid invoiceId, CancellationToken cancellationToken = default)
    {
        return await _context.Invoices
            .Include(i => i.InvoiceLineItems)
            .Include(i => i.CustomerSubscription)
                .ThenInclude(cs => cs.Customer)
            .Include(i => i.CustomerSubscription)
                .ThenInclude(cs => cs.SubscriptionPlan)
            .FirstOrDefaultAsync(i => i.Id == invoiceId, cancellationToken);
    }

    public async Task<List<Invoice>> GetCustomerInvoicesAsync(
        Guid customerId,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Invoices
            .Include(i => i.InvoiceLineItems)
            .Include(i => i.CustomerSubscription)
            .Where(i => i.TenantId == tenantId && 
                       i.CustomerSubscription.CustomerId == customerId)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> MarkInvoiceAsPaidAsync(
        Guid invoiceId,
        Guid tenantId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var invoice = await _context.Invoices
            .FirstOrDefaultAsync(i => i.Id == invoiceId && i.TenantId == tenantId, cancellationToken);

        if (invoice == null)
            throw new KeyNotFoundException($"Invoice {invoiceId} not found");

        if (invoice.Status == "paid")
            return false; // Already paid

        var previousStatus = invoice.Status;
        invoice.Status = "paid";
        invoice.PaidAt = DateTime.UtcNow;
        invoice.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        // Log audit entry
        await _auditService.LogActionAsync(
            "update",
            "Invoice",
            invoiceId,
            null,
            new Dictionary<string, object>
            {
                { "previous_status", previousStatus },
                { "new_status", "paid" },
                { "paid_at", invoice.PaidAt.Value }
            },
            tenantId,
            userId,
            cancellationToken);

        return true;
    }
}
