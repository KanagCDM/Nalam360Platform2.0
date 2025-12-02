using Microsoft.EntityFrameworkCore;
using ProductAdminPortal.Data;
using ProductAdminPortal.Models.Domain;
using System.Text.Json;

namespace ProductAdminPortal.Services;

public class AuditService : IAuditService
{
    private readonly ProductAdminDbContext _context;

    public AuditService(ProductAdminDbContext context)
    {
        _context = context;
    }

    public async Task LogActionAsync(
        string action,
        string entityType,
        Guid entityId,
        string? ipAddress,
        Dictionary<string, object>? changes,
        Guid tenantId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = userId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Changes = changes != null ? JsonSerializer.Serialize(changes) : null,
            IpAddress = ipAddress,
            UserAgent = null, // Can be populated from HttpContext
            CreatedAt = DateTime.UtcNow
        };

        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<AuditLog>> GetAuditLogsAsync(
        Guid tenantId,
        string? entityType,
        Guid? entityId,
        Guid? userId,
        DateTime? startDate,
        DateTime? endDate,
        int pageNumber = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var query = _context.AuditLogs
            .Include(al => al.User)
            .Where(al => al.TenantId == tenantId);

        if (!string.IsNullOrEmpty(entityType))
            query = query.Where(al => al.EntityType == entityType);

        if (entityId.HasValue)
            query = query.Where(al => al.EntityId == entityId.Value);

        if (userId.HasValue)
            query = query.Where(al => al.UserId == userId.Value);

        if (startDate.HasValue)
            query = query.Where(al => al.CreatedAt >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(al => al.CreatedAt < endDate.Value);

        return await query
            .OrderByDescending(al => al.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<Guid> CreateConfigurationVersionAsync(
        string entityType,
        Guid entityId,
        string configurationSnapshot,
        string? changeSummary,
        Guid tenantId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        // Get the latest version number for this entity
        var latestVersion = await _context.ConfigurationVersions
            .Where(cv => cv.TenantId == tenantId && 
                        cv.EntityType == entityType && 
                        cv.EntityId == entityId)
            .OrderByDescending(cv => cv.VersionNumber)
            .Select(cv => cv.VersionNumber)
            .FirstOrDefaultAsync(cancellationToken);

        var configVersion = new ConfigurationVersion
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            EntityType = entityType,
            EntityId = entityId,
            VersionNumber = latestVersion + 1,
            ConfigurationSnapshot = configurationSnapshot,
            ChangeSummary = changeSummary,
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.ConfigurationVersions.Add(configVersion);
        await _context.SaveChangesAsync(cancellationToken);

        // Log audit entry
        await LogActionAsync(
            "create",
            "ConfigurationVersion",
            configVersion.Id,
            null,
            new Dictionary<string, object>
            {
                { "entity_type", entityType },
                { "entity_id", entityId },
                { "version_number", configVersion.VersionNumber }
            },
            tenantId,
            userId,
            cancellationToken);

        return configVersion.Id;
    }

    public async Task<List<ConfigurationVersion>> GetConfigurationVersionsAsync(
        string entityType,
        Guid entityId,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return await _context.ConfigurationVersions
            .Include(cv => cv.CreatedByUser)
            .Where(cv => cv.TenantId == tenantId && 
                        cv.EntityType == entityType && 
                        cv.EntityId == entityId)
            .OrderByDescending(cv => cv.VersionNumber)
            .ToListAsync(cancellationToken);
    }

    public async Task<ConfigurationVersion?> GetConfigurationVersionByIdAsync(
        Guid versionId,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return await _context.ConfigurationVersions
            .Include(cv => cv.CreatedByUser)
            .FirstOrDefaultAsync(cv => cv.Id == versionId && cv.TenantId == tenantId, cancellationToken);
    }

    public async Task<bool> RollbackToVersionAsync(
        Guid versionId,
        Guid tenantId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var version = await _context.ConfigurationVersions
            .FirstOrDefaultAsync(cv => cv.Id == versionId && cv.TenantId == tenantId, cancellationToken);

        if (version == null)
            throw new KeyNotFoundException($"Configuration version {versionId} not found");

        // Deserialize the configuration snapshot
        var snapshot = JsonSerializer.Deserialize<Dictionary<string, object>>(version.ConfigurationSnapshot);
        
        if (snapshot == null)
            throw new InvalidOperationException("Configuration snapshot is invalid");

        // Apply rollback based on entity type
        switch (version.EntityType)
        {
            case "Product":
                await RollbackProductAsync(version.EntityId, snapshot, cancellationToken);
                break;
            case "Module":
                await RollbackModuleAsync(version.EntityId, snapshot, cancellationToken);
                break;
            case "Entity":
                await RollbackEntityAsync(version.EntityId, snapshot, cancellationToken);
                break;
            case "SubscriptionPlan":
                await RollbackSubscriptionPlanAsync(version.EntityId, snapshot, cancellationToken);
                break;
            case "PricingRule":
                await RollbackPricingRuleAsync(version.EntityId, snapshot, cancellationToken);
                break;
            default:
                throw new InvalidOperationException($"Rollback not supported for entity type: {version.EntityType}");
        }

        // Create a new version to record the rollback
        await CreateConfigurationVersionAsync(
            version.EntityType,
            version.EntityId,
            version.ConfigurationSnapshot,
            $"Rolled back to version {version.VersionNumber}",
            tenantId,
            userId,
            cancellationToken);

        // Log audit entry
        await LogActionAsync(
            "rollback",
            version.EntityType,
            version.EntityId,
            null,
            new Dictionary<string, object>
            {
                { "rolled_back_to_version", version.VersionNumber },
                { "version_id", versionId }
            },
            tenantId,
            userId,
            cancellationToken);

        return true;
    }

    private async Task RollbackProductAsync(Guid productId, Dictionary<string, object> snapshot, CancellationToken cancellationToken)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);
        if (product == null) return;

        product.Name = snapshot.GetValueOrDefault("Name")?.ToString() ?? product.Name;
        product.Code = snapshot.GetValueOrDefault("Code")?.ToString() ?? product.Code;
        product.Description = snapshot.GetValueOrDefault("Description")?.ToString();
        product.Industry = snapshot.GetValueOrDefault("Industry")?.ToString();
        product.Version = snapshot.GetValueOrDefault("Version")?.ToString() ?? product.Version;
        product.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task RollbackModuleAsync(Guid moduleId, Dictionary<string, object> snapshot, CancellationToken cancellationToken)
    {
        var module = await _context.Modules.FirstOrDefaultAsync(m => m.Id == moduleId, cancellationToken);
        if (module == null) return;

        module.Name = snapshot.GetValueOrDefault("Name")?.ToString() ?? module.Name;
        module.Code = snapshot.GetValueOrDefault("Code")?.ToString() ?? module.Code;
        module.Description = snapshot.GetValueOrDefault("Description")?.ToString();
        module.DisplayOrder = Convert.ToInt32(snapshot.GetValueOrDefault("DisplayOrder") ?? module.DisplayOrder);
        module.IsRequired = Convert.ToBoolean(snapshot.GetValueOrDefault("IsRequired") ?? module.IsRequired);
        module.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task RollbackEntityAsync(Guid entityId, Dictionary<string, object> snapshot, CancellationToken cancellationToken)
    {
        var entity = await _context.Entities.FirstOrDefaultAsync(e => e.Id == entityId, cancellationToken);
        if (entity == null) return;

        entity.Name = snapshot.GetValueOrDefault("Name")?.ToString() ?? entity.Name;
        entity.Code = snapshot.GetValueOrDefault("Code")?.ToString() ?? entity.Code;
        entity.Type = snapshot.GetValueOrDefault("Type")?.ToString() ?? entity.Type;
        entity.BasePricing = snapshot.GetValueOrDefault("BasePricing")?.ToString();
        entity.ComplexityLevels = snapshot.GetValueOrDefault("ComplexityLevels")?.ToString();
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task RollbackSubscriptionPlanAsync(Guid planId, Dictionary<string, object> snapshot, CancellationToken cancellationToken)
    {
        var plan = await _context.SubscriptionPlans.FirstOrDefaultAsync(sp => sp.Id == planId, cancellationToken);
        if (plan == null) return;

        plan.Name = snapshot.GetValueOrDefault("Name")?.ToString() ?? plan.Name;
        plan.Code = snapshot.GetValueOrDefault("Code")?.ToString() ?? plan.Code;
        plan.Price = Convert.ToDecimal(snapshot.GetValueOrDefault("Price") ?? plan.Price);
        plan.BillingCycle = snapshot.GetValueOrDefault("BillingCycle")?.ToString() ?? plan.BillingCycle;
        plan.TrialDays = Convert.ToInt32(snapshot.GetValueOrDefault("TrialDays") ?? plan.TrialDays);
        plan.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task RollbackPricingRuleAsync(Guid ruleId, Dictionary<string, object> snapshot, CancellationToken cancellationToken)
    {
        var rule = await _context.PricingRules.FirstOrDefaultAsync(pr => pr.Id == ruleId, cancellationToken);
        if (rule == null) return;

        rule.Name = snapshot.GetValueOrDefault("Name")?.ToString() ?? rule.Name;
        rule.RuleType = snapshot.GetValueOrDefault("RuleType")?.ToString() ?? rule.RuleType;
        rule.Configuration = snapshot.GetValueOrDefault("Configuration")?.ToString();
        rule.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
