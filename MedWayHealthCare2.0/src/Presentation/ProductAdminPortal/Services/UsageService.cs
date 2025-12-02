using Microsoft.EntityFrameworkCore;
using ProductAdminPortal.Data;
using ProductAdminPortal.Models.Domain;
using System.Text.Json;

namespace ProductAdminPortal.Services;

public class UsageService : IUsageService
{
    private readonly ProductAdminDbContext _context;
    private readonly IAuditService _auditService;

    public UsageService(ProductAdminDbContext context, IAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    public async Task<Guid> RecordUsageAsync(
        Guid customerSubscriptionId,
        Guid entityId,
        int units,
        string? complexity,
        Dictionary<string, object>? metadata,
        Guid tenantId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        // Validate customer subscription
        var subscription = await _context.CustomerSubscriptions
            .Include(cs => cs.SubscriptionPlan)
            .FirstOrDefaultAsync(cs => cs.Id == customerSubscriptionId, cancellationToken);

        if (subscription == null)
            throw new KeyNotFoundException($"Customer subscription {customerSubscriptionId} not found");

        if (subscription.Status != "active")
            throw new InvalidOperationException($"Subscription is not active: {subscription.Status}");

        // Validate entity exists
        var entity = await _context.Entities
            .FirstOrDefaultAsync(e => e.Id == entityId, cancellationToken);

        if (entity == null)
            throw new KeyNotFoundException($"Entity {entityId} not found");

        // Create usage record
        var usageRecord = new UsageRecord
        {
            Id = Guid.NewGuid(),
            CustomerSubscriptionId = customerSubscriptionId,
            EntityId = entityId,
            Units = units,
            Complexity = complexity ?? "low",
            BillingStatus = "unbilled",
            RecordedAt = DateTime.UtcNow,
            Metadata = metadata != null ? JsonSerializer.Serialize(metadata) : null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.UsageRecords.Add(usageRecord);
        await _context.SaveChangesAsync(cancellationToken);

        // Check usage limits and create alerts if needed
        await CheckUsageLimitsAsync(customerSubscriptionId, entityId, tenantId, userId, cancellationToken);

        // Log audit entry
        await _auditService.LogActionAsync(
            "create",
            "UsageRecord",
            usageRecord.Id,
            null,
            new Dictionary<string, object>
            {
                { "customer_subscription_id", customerSubscriptionId },
                { "entity_id", entityId },
                { "units", units },
                { "complexity", complexity ?? "low" }
            },
            tenantId,
            userId,
            cancellationToken);

        return usageRecord.Id;
    }

    public async Task<List<UsageRecord>> GetUsageRecordsAsync(
        Guid customerSubscriptionId,
        DateTime? startDate,
        DateTime? endDate,
        string? billingStatus,
        CancellationToken cancellationToken = default)
    {
        var query = _context.UsageRecords
            .Include(ur => ur.Entity)
            .Where(ur => ur.CustomerSubscriptionId == customerSubscriptionId);

        if (startDate.HasValue)
            query = query.Where(ur => ur.RecordedAt >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(ur => ur.RecordedAt < endDate.Value);

        if (!string.IsNullOrEmpty(billingStatus))
            query = query.Where(ur => ur.BillingStatus == billingStatus);

        return await query
            .OrderByDescending(ur => ur.RecordedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Dictionary<Guid, int>> GetUsageSummaryAsync(
        Guid customerSubscriptionId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        var usageByEntity = await _context.UsageRecords
            .Where(ur => ur.CustomerSubscriptionId == customerSubscriptionId &&
                        ur.RecordedAt >= startDate &&
                        ur.RecordedAt < endDate)
            .GroupBy(ur => ur.EntityId)
            .Select(g => new { EntityId = g.Key, TotalUnits = g.Sum(ur => ur.Units) })
            .ToListAsync(cancellationToken);

        return usageByEntity.ToDictionary(u => u.EntityId, u => u.TotalUnits);
    }

    public async Task CheckUsageLimitsAsync(
        Guid customerSubscriptionId,
        Guid entityId,
        Guid tenantId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        // Get subscription entity limits
        var subscriptionEntity = await _context.SubscriptionEntities
            .FirstOrDefaultAsync(se => se.EntityId == entityId &&
                _context.CustomerSubscriptions
                    .Where(cs => cs.Id == customerSubscriptionId)
                    .Select(cs => cs.SubscriptionPlanId)
                    .Contains(se.SubscriptionPlanId), cancellationToken);

        if (subscriptionEntity?.UsageLimit == null)
            return; // No limit set

        var usageLimit = subscriptionEntity.UsageLimit.Value;
        var softLimitThreshold = subscriptionEntity.SoftLimitThreshold ?? 0.8m;

        // Calculate current usage for the current month
        var currentMonth = DateTime.UtcNow.Date.AddDays(1 - DateTime.UtcNow.Day);
        var nextMonth = currentMonth.AddMonths(1);

        var currentUsage = await _context.UsageRecords
            .Where(ur => ur.CustomerSubscriptionId == customerSubscriptionId &&
                        ur.EntityId == entityId &&
                        ur.RecordedAt >= currentMonth &&
                        ur.RecordedAt < nextMonth)
            .SumAsync(ur => ur.Units, cancellationToken);

        var usagePercentage = (decimal)currentUsage / usageLimit;

        // Check for soft limit
        if (usagePercentage >= softLimitThreshold && usagePercentage < 1.0m)
        {
            // Check if alert already exists
            var existingAlert = await _context.UsageAlerts
                .AnyAsync(ua => ua.CustomerSubscriptionId == customerSubscriptionId &&
                               ua.EntityId == entityId &&
                               ua.AlertType == "soft_limit" &&
                               !ua.IsResolved &&
                               ua.CreatedAt >= currentMonth, cancellationToken);

            if (!existingAlert)
            {
                var softAlert = new UsageAlert
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    CustomerSubscriptionId = customerSubscriptionId,
                    EntityId = entityId,
                    AlertType = "soft_limit",
                    Threshold = (int)(usageLimit * softLimitThreshold),
                    CurrentUsage = currentUsage,
                    IsResolved = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.UsageAlerts.Add(softAlert);
            }
        }

        // Check for hard limit
        if (usagePercentage >= 1.0m)
        {
            var existingHardAlert = await _context.UsageAlerts
                .AnyAsync(ua => ua.CustomerSubscriptionId == customerSubscriptionId &&
                               ua.EntityId == entityId &&
                               ua.AlertType == "hard_limit" &&
                               !ua.IsResolved &&
                               ua.CreatedAt >= currentMonth, cancellationToken);

            if (!existingHardAlert)
            {
                var hardAlert = new UsageAlert
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    CustomerSubscriptionId = customerSubscriptionId,
                    EntityId = entityId,
                    AlertType = "hard_limit",
                    Threshold = usageLimit,
                    CurrentUsage = currentUsage,
                    IsResolved = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.UsageAlerts.Add(hardAlert);

                // Log audit entry for hard limit breach
                await _auditService.LogActionAsync(
                    "create",
                    "UsageAlert",
                    hardAlert.Id,
                    null,
                    new Dictionary<string, object>
                    {
                        { "alert_type", "hard_limit" },
                        { "entity_id", entityId },
                        { "usage_limit", usageLimit },
                        { "current_usage", currentUsage }
                    },
                    tenantId,
                    userId,
                    cancellationToken);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<UsageAlert>> GetActiveAlertsAsync(
        Guid customerSubscriptionId,
        CancellationToken cancellationToken = default)
    {
        return await _context.UsageAlerts
            .Include(ua => ua.Entity)
            .Where(ua => ua.CustomerSubscriptionId == customerSubscriptionId && !ua.IsResolved)
            .OrderByDescending(ua => ua.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ResolveAlertAsync(
        Guid alertId,
        Guid tenantId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var alert = await _context.UsageAlerts
            .FirstOrDefaultAsync(ua => ua.Id == alertId && ua.TenantId == tenantId, cancellationToken);

        if (alert == null)
            return false;

        alert.IsResolved = true;
        alert.ResolvedAt = DateTime.UtcNow;
        alert.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        await _auditService.LogActionAsync(
            "update",
            "UsageAlert",
            alertId,
            null,
            new Dictionary<string, object> { { "is_resolved", true } },
            tenantId,
            userId,
            cancellationToken);

        return true;
    }
}
