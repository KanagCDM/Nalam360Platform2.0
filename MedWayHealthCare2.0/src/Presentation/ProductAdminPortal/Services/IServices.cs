using ProductAdminPortal.DTOs;
using ProductAdminPortal.Models.Domain;

namespace ProductAdminPortal.Services;

/// <summary>
/// Service interface for product management
/// </summary>
public interface IProductService
{
    Task<ProductResponse> GetProductByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default);
    Task<List<ProductResponse>> GetAllProductsAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<ProductResponse> CreateProductAsync(CreateProductRequest request, Guid tenantId, Guid userId, CancellationToken cancellationToken = default);
    Task<ProductResponse> UpdateProductAsync(Guid id, UpdateProductRequest request, Guid tenantId, Guid userId, CancellationToken cancellationToken = default);
    Task DeleteProductAsync(Guid id, Guid tenantId, Guid userId, CancellationToken cancellationToken = default);
    
    Task<ModuleResponse> AddModuleToProductAsync(Guid productId, CreateModuleRequest request, Guid tenantId, Guid userId, CancellationToken cancellationToken = default);
    Task<List<ModuleResponse>> GetProductModulesAsync(Guid productId, Guid tenantId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Service interface for pricing calculations
/// </summary>
public interface IPricingService
{
    Task<PricingCalculationResponse> CalculatePricingAsync(PricingCalculationRequest request, CancellationToken cancellationToken = default);
    Task<PricingSimulationResponse> SimulatePricingAsync(PricingSimulationRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
/// Service interface for billing operations
/// </summary>
public interface IBillingService
{
    Task<Guid> GenerateInvoiceAsync(Guid customerSubscriptionId, DateTime billingPeriodStart, DateTime billingPeriodEnd, Guid tenantId, Guid userId, CancellationToken cancellationToken = default);
    Task<Invoice?> GetInvoiceByIdAsync(Guid invoiceId, CancellationToken cancellationToken = default);
    Task<List<Invoice>> GetCustomerInvoicesAsync(Guid customerId, Guid tenantId, CancellationToken cancellationToken = default);
    Task<bool> MarkInvoiceAsPaidAsync(Guid invoiceId, Guid tenantId, Guid userId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Service interface for usage tracking
/// </summary>
public interface IUsageService
{
    Task<Guid> RecordUsageAsync(Guid customerSubscriptionId, Guid entityId, int units, string? complexity, Dictionary<string, object>? metadata, Guid tenantId, Guid userId, CancellationToken cancellationToken = default);
    Task<List<UsageRecord>> GetUsageRecordsAsync(Guid customerSubscriptionId, DateTime? startDate, DateTime? endDate, string? billingStatus, CancellationToken cancellationToken = default);
    Task<Dictionary<Guid, int>> GetUsageSummaryAsync(Guid customerSubscriptionId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task CheckUsageLimitsAsync(Guid customerSubscriptionId, Guid entityId, Guid tenantId, Guid userId, CancellationToken cancellationToken = default);
    Task<List<UsageAlert>> GetActiveAlertsAsync(Guid customerSubscriptionId, CancellationToken cancellationToken = default);
    Task<bool> ResolveAlertAsync(Guid alertId, Guid tenantId, Guid userId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Service interface for audit logging
/// </summary>
public interface IAuditService
{
    Task LogActionAsync(string action, string entityType, Guid entityId, string? ipAddress, Dictionary<string, object>? changes, Guid tenantId, Guid userId, CancellationToken cancellationToken = default);
    Task<List<AuditLog>> GetAuditLogsAsync(Guid tenantId, string? entityType, Guid? entityId, Guid? userId, DateTime? startDate, DateTime? endDate, int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default);
    
    Task<Guid> CreateConfigurationVersionAsync(string entityType, Guid entityId, string configurationSnapshot, string? changeSummary, Guid tenantId, Guid userId, CancellationToken cancellationToken = default);
    Task<List<ConfigurationVersion>> GetConfigurationVersionsAsync(string entityType, Guid entityId, Guid tenantId, CancellationToken cancellationToken = default);
    Task<ConfigurationVersion?> GetConfigurationVersionByIdAsync(Guid versionId, Guid tenantId, CancellationToken cancellationToken = default);
    Task<bool> RollbackToVersionAsync(Guid versionId, Guid tenantId, Guid userId, CancellationToken cancellationToken = default);
}
