namespace ProductAdminPortal.DTOs;

/// <summary>
/// DTO for pricing calculation request
/// </summary>
public record PricingCalculationRequest(
    Guid CustomerSubscriptionId,
    DateTime BillingPeriodStart,
    DateTime BillingPeriodEnd,
    ICollection<UsageRecordInput> UsageRecords
);

/// <summary>
/// DTO for usage record input
/// </summary>
public record UsageRecordInput(
    Guid EntityId,
    int Units,
    string? Complexity
);

/// <summary>
/// DTO for pricing calculation response
/// </summary>
public record PricingCalculationResponse(
    Guid CustomerSubscriptionId,
    DateTime BillingPeriodStart,
    DateTime BillingPeriodEnd,
    decimal Subtotal,
    decimal TaxAmount,
    decimal DiscountAmount,
    decimal TotalAmount,
    ICollection<LineItemResponse> LineItems
);

/// <summary>
/// DTO for invoice line item
/// </summary>
public record LineItemResponse(
    string Description,
    int Quantity,
    decimal UnitPrice,
    decimal Amount,
    object? Metadata
);

/// <summary>
/// DTO for pricing simulation request
/// </summary>
public record PricingSimulationRequest(
    Guid SubscriptionPlanId,
    ICollection<UsageRecordInput> UsageRecords
);

/// <summary>
/// DTO for pricing simulation response
/// </summary>
public record PricingSimulationResponse(
    Guid SubscriptionPlanId,
    string SubscriptionPlanName,
    decimal BasePrice,
    decimal EstimatedUsageCharges,
    decimal EstimatedTotal,
    ICollection<LineItemResponse> Breakdown
);
