using Microsoft.EntityFrameworkCore;
using SimpleMedWayDemo.Data;
using SimpleMedWayDemo.Models;

namespace SimpleMedWayDemo.Services;

/// <summary>
/// Service for managing hospital subscriptions and access control
/// </summary>
public interface ISubscriptionService
{
    // Subscription Management
    Task<HospitalSubscription?> GetActiveSubscriptionAsync(Guid hospitalId);
    Task<List<HospitalSubscription>> GetSubscriptionHistoryAsync(Guid hospitalId);
    Task<HospitalSubscription> CreateSubscriptionAsync(Guid hospitalId, int planId, bool isYearly, bool isTrial = false);
    Task<bool> UpgradeSubscriptionAsync(Guid hospitalId, int newPlanId, string reason, string changedBy);
    Task<bool> DowngradeSubscriptionAsync(Guid hospitalId, int newPlanId, string reason, string changedBy);
    Task<bool> CancelSubscriptionAsync(Guid hospitalId, string reason);
    Task<bool> RenewSubscriptionAsync(Guid hospitalId);
    
    // Access Control
    Task<bool> HasAccessToModuleAsync(Guid hospitalId, string moduleCode);
    Task<bool> HasAccessToEntityAsync(Guid hospitalId, string entityCode);
    Task<List<string>> GetAccessibleModulesAsync(Guid hospitalId);
    Task<List<string>> GetAccessibleEntitiesAsync(Guid hospitalId, string moduleCode);
    
    // Plan Information
    Task<List<SubscriptionPlan>> GetAllPlansAsync();
    Task<SubscriptionPlan?> GetPlanAsync(int planId);
    Task<SubscriptionPlan?> GetPlanByTierAsync(SubscriptionTier tier);
    
    // Usage Tracking
    Task<bool> UpdateUsageAsync(Guid hospitalId, int userCount, int patientCount, decimal storageGB);
    Task<bool> CheckUsageLimitsAsync(Guid hospitalId);
    Task<UsageSummary> GetUsageSummaryAsync(Guid hospitalId);
    
    // Subscription Validation
    Task ProcessExpiredSubscriptionsAsync();
    Task<List<HospitalSubscription>> GetExpiringSubscriptionsAsync(int daysBeforeExpiry);
}

public class SubscriptionService : ISubscriptionService
{
    private readonly ApplicationDbContext _context;

    public SubscriptionService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<HospitalSubscription?> GetActiveSubscriptionAsync(Guid hospitalId)
    {
        return await _context.HospitalSubscriptions
            .Include(s => s.SubscriptionPlan)
                .ThenInclude(p => p!.IncludedEntities)
                    .ThenInclude(e => e.ModuleEntity)
            .Where(s => s.HospitalId == hospitalId && s.IsActive && s.Status == SubscriptionStatus.Active)
            .OrderByDescending(s => s.StartDate)
            .FirstOrDefaultAsync();
    }

    public async Task<List<HospitalSubscription>> GetSubscriptionHistoryAsync(Guid hospitalId)
    {
        return await _context.HospitalSubscriptions
            .Include(s => s.SubscriptionPlan)
            .Include(s => s.UpgradeHistory)
            .Where(s => s.HospitalId == hospitalId)
            .OrderByDescending(s => s.StartDate)
            .ToListAsync();
    }

    public async Task<HospitalSubscription> CreateSubscriptionAsync(
        Guid hospitalId, 
        int planId, 
        bool isYearly, 
        bool isTrial = false)
    {
        var plan = await _context.SubscriptionPlans.FindAsync(planId);
        if (plan == null)
            throw new ArgumentException($"Subscription plan {planId} not found");

        var hospital = await _context.Hospitals.FindAsync(hospitalId);
        if (hospital == null)
            throw new ArgumentException($"Hospital {hospitalId} not found");

        // Deactivate any existing active subscriptions
        var existingSubscriptions = await _context.HospitalSubscriptions
            .Where(s => s.HospitalId == hospitalId && s.IsActive)
            .ToListAsync();

        foreach (var sub in existingSubscriptions)
        {
            sub.IsActive = false;
            sub.UpdatedAt = DateTime.UtcNow;
        }

        var subscription = new HospitalSubscription
        {
            HospitalId = hospitalId,
            SubscriptionPlanId = planId,
            StartDate = DateTime.UtcNow,
            EndDate = isYearly ? DateTime.UtcNow.AddYears(1) : DateTime.UtcNow.AddMonths(1),
            IsYearlyBilling = isYearly,
            IsActive = true,
            Status = SubscriptionStatus.Active,
            IsTrial = isTrial,
            AutoRenew = !isTrial
        };

        if (isTrial)
        {
            subscription.TrialStartDate = DateTime.UtcNow;
            subscription.TrialEndDate = DateTime.UtcNow.AddDays(30);
            subscription.EndDate = subscription.TrialEndDate.Value;
        }

        subscription.NextBillingDate = subscription.EndDate;

        _context.HospitalSubscriptions.Add(subscription);
        await _context.SaveChangesAsync();

        return subscription;
    }

    public async Task<bool> UpgradeSubscriptionAsync(
        Guid hospitalId, 
        int newPlanId, 
        string reason, 
        string changedBy)
    {
        var currentSubscription = await GetActiveSubscriptionAsync(hospitalId);
        if (currentSubscription == null)
            return false;

        var newPlan = await _context.SubscriptionPlans.FindAsync(newPlanId);
        if (newPlan == null || newPlan.Tier <= currentSubscription.SubscriptionPlan!.Tier)
            return false;

        // Calculate proration
        var daysRemaining = (currentSubscription.EndDate - DateTime.UtcNow).Days;
        var totalDays = (currentSubscription.EndDate - currentSubscription.StartDate).Days;
        var prorationRatio = (decimal)daysRemaining / totalDays;
        
        var currentPrice = currentSubscription.IsYearlyBilling 
            ? currentSubscription.SubscriptionPlan.YearlyPrice 
            : currentSubscription.SubscriptionPlan.MonthlyPrice;
        
        var newPrice = currentSubscription.IsYearlyBilling 
            ? newPlan.YearlyPrice 
            : newPlan.MonthlyPrice;
        
        var prorationAmount = (newPrice - currentPrice) * prorationRatio;

        // Record upgrade history
        var history = new SubscriptionUpgradeHistory
        {
            HospitalSubscriptionId = currentSubscription.Id,
            FromPlanId = currentSubscription.SubscriptionPlanId,
            ToPlanId = newPlanId,
            Reason = reason,
            ProrationAmount = prorationAmount,
            ChangedBy = changedBy
        };

        _context.SubscriptionUpgradeHistories.Add(history);

        // Update subscription
        currentSubscription.SubscriptionPlanId = newPlanId;
        currentSubscription.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DowngradeSubscriptionAsync(
        Guid hospitalId, 
        int newPlanId, 
        string reason, 
        string changedBy)
    {
        var currentSubscription = await GetActiveSubscriptionAsync(hospitalId);
        if (currentSubscription == null)
            return false;

        var newPlan = await _context.SubscriptionPlans.FindAsync(newPlanId);
        if (newPlan == null || newPlan.Tier >= currentSubscription.SubscriptionPlan!.Tier)
            return false;

        // Schedule downgrade at end of current billing period
        var history = new SubscriptionUpgradeHistory
        {
            HospitalSubscriptionId = currentSubscription.Id,
            FromPlanId = currentSubscription.SubscriptionPlanId,
            ToPlanId = newPlanId,
            Reason = reason,
            ProrationAmount = 0, // No refund for downgrades
            ChangedBy = changedBy
        };

        _context.SubscriptionUpgradeHistories.Add(history);

        // Will be applied at next billing cycle
        currentSubscription.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CancelSubscriptionAsync(Guid hospitalId, string reason)
    {
        var subscription = await GetActiveSubscriptionAsync(hospitalId);
        if (subscription == null)
            return false;

        subscription.Status = SubscriptionStatus.Cancelled;
        subscription.IsActive = false;
        subscription.CancelledAt = DateTime.UtcNow;
        subscription.CancellationReason = reason;
        subscription.AutoRenew = false;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RenewSubscriptionAsync(Guid hospitalId)
    {
        var subscription = await GetActiveSubscriptionAsync(hospitalId);
        if (subscription == null || !subscription.AutoRenew)
            return false;

        subscription.StartDate = subscription.EndDate;
        subscription.EndDate = subscription.IsYearlyBilling 
            ? subscription.StartDate.AddYears(1) 
            : subscription.StartDate.AddMonths(1);
        
        subscription.LastBillingDate = DateTime.UtcNow;
        subscription.NextBillingDate = subscription.EndDate;
        subscription.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> HasAccessToModuleAsync(Guid hospitalId, string moduleCode)
    {
        var subscription = await GetActiveSubscriptionAsync(hospitalId);
        if (subscription == null)
            return false;

        // Get all entities in the module that are included in the subscription
        var hasAccess = await _context.SubscriptionPlanEntities
            .Include(spe => spe.ModuleEntity)
            .AnyAsync(spe => 
                spe.SubscriptionPlanId == subscription.SubscriptionPlanId &&
                spe.IsEnabled &&
                spe.ModuleEntity!.ModuleName == moduleCode);

        return hasAccess;
    }

    public async Task<bool> HasAccessToEntityAsync(Guid hospitalId, string entityCode)
    {
        var subscription = await GetActiveSubscriptionAsync(hospitalId);
        if (subscription == null)
            return false;

        var hasAccess = await _context.SubscriptionPlanEntities
            .Include(spe => spe.ModuleEntity)
            .AnyAsync(spe => 
                spe.SubscriptionPlanId == subscription.SubscriptionPlanId &&
                spe.IsEnabled &&
                spe.ModuleEntity!.EntityCode == entityCode);

        return hasAccess;
    }

    public async Task<List<string>> GetAccessibleModulesAsync(Guid hospitalId)
    {
        var subscription = await GetActiveSubscriptionAsync(hospitalId);
        if (subscription == null)
            return new List<string>();

        var modules = await _context.SubscriptionPlanEntities
            .Include(spe => spe.ModuleEntity)
            .Where(spe => 
                spe.SubscriptionPlanId == subscription.SubscriptionPlanId &&
                spe.IsEnabled)
            .Select(spe => spe.ModuleEntity!.ModuleName)
            .Distinct()
            .ToListAsync();

        return modules;
    }

    public async Task<List<string>> GetAccessibleEntitiesAsync(Guid hospitalId, string moduleCode)
    {
        var subscription = await GetActiveSubscriptionAsync(hospitalId);
        if (subscription == null)
            return new List<string>();

        var entities = await _context.SubscriptionPlanEntities
            .Include(spe => spe.ModuleEntity)
            .Where(spe => 
                spe.SubscriptionPlanId == subscription.SubscriptionPlanId &&
                spe.IsEnabled &&
                spe.ModuleEntity!.ModuleName == moduleCode)
            .Select(spe => spe.ModuleEntity!.EntityCode)
            .ToListAsync();

        return entities;
    }

    public async Task<List<SubscriptionPlan>> GetAllPlansAsync()
    {
        return await _context.SubscriptionPlans
            .Where(p => p.IsActive)
            .OrderBy(p => p.Tier)
            .ToListAsync();
    }

    public async Task<SubscriptionPlan?> GetPlanAsync(int planId)
    {
        return await _context.SubscriptionPlans
            .Include(p => p.IncludedEntities)
                .ThenInclude(e => e.ModuleEntity)
            .FirstOrDefaultAsync(p => p.Id == planId);
    }

    public async Task<SubscriptionPlan?> GetPlanByTierAsync(SubscriptionTier tier)
    {
        return await _context.SubscriptionPlans
            .Include(p => p.IncludedEntities)
                .ThenInclude(e => e.ModuleEntity)
            .FirstOrDefaultAsync(p => p.Tier == tier && p.IsActive);
    }

    public async Task<bool> UpdateUsageAsync(Guid hospitalId, int userCount, int patientCount, decimal storageGB)
    {
        var subscription = await GetActiveSubscriptionAsync(hospitalId);
        if (subscription == null)
            return false;

        subscription.CurrentUserCount = userCount;
        subscription.CurrentPatientCount = patientCount;
        subscription.CurrentStorageUsedGB = storageGB;
        subscription.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CheckUsageLimitsAsync(Guid hospitalId)
    {
        var subscription = await GetActiveSubscriptionAsync(hospitalId);
        if (subscription == null)
            return false;

        var plan = subscription.SubscriptionPlan!;

        // Check if unlimited (-1)
        if (plan.MaxUsers == -1 && plan.MaxPatients == -1)
            return true;

        var userLimitOk = plan.MaxUsers == -1 || subscription.CurrentUserCount <= plan.MaxUsers;
        var patientLimitOk = plan.MaxPatients == -1 || subscription.CurrentPatientCount <= plan.MaxPatients;
        var storageLimitOk = subscription.CurrentStorageUsedGB <= plan.StorageGB;

        return userLimitOk && patientLimitOk && storageLimitOk;
    }

    public async Task<UsageSummary> GetUsageSummaryAsync(Guid hospitalId)
    {
        var subscription = await GetActiveSubscriptionAsync(hospitalId);
        if (subscription == null)
            throw new InvalidOperationException("No active subscription found");

        var plan = subscription.SubscriptionPlan!;

        return new UsageSummary
        {
            CurrentUsers = subscription.CurrentUserCount,
            MaxUsers = plan.MaxUsers,
            CurrentPatients = subscription.CurrentPatientCount,
            MaxPatients = plan.MaxPatients,
            CurrentStorageGB = subscription.CurrentStorageUsedGB,
            MaxStorageGB = plan.StorageGB,
            UserUsagePercentage = plan.MaxUsers == -1 ? 0 : (decimal)subscription.CurrentUserCount / plan.MaxUsers * 100,
            PatientUsagePercentage = plan.MaxPatients == -1 ? 0 : (decimal)subscription.CurrentPatientCount / plan.MaxPatients * 100,
            StorageUsagePercentage = subscription.CurrentStorageUsedGB / plan.StorageGB * 100,
            IsOverLimit = !(await CheckUsageLimitsAsync(hospitalId)),
            DaysUntilRenewal = (subscription.EndDate - DateTime.UtcNow).Days
        };
    }

    public async Task ProcessExpiredSubscriptionsAsync()
    {
        var expiredSubscriptions = await _context.HospitalSubscriptions
            .Where(s => s.IsActive && 
                       s.Status == SubscriptionStatus.Active && 
                       s.EndDate < DateTime.UtcNow)
            .ToListAsync();

        foreach (var subscription in expiredSubscriptions)
        {
            if (subscription.AutoRenew && !subscription.IsTrial)
            {
                // Auto-renew
                await RenewSubscriptionAsync(subscription.HospitalId);
            }
            else
            {
                // Expire the subscription
                subscription.Status = subscription.IsTrial 
                    ? SubscriptionStatus.TrialEnded 
                    : SubscriptionStatus.Expired;
                subscription.IsActive = false;
                subscription.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync();
    }

    public async Task<List<HospitalSubscription>> GetExpiringSubscriptionsAsync(int daysBeforeExpiry)
    {
        var expiryDate = DateTime.UtcNow.AddDays(daysBeforeExpiry);

        return await _context.HospitalSubscriptions
            .Include(s => s.Hospital)
            .Include(s => s.SubscriptionPlan)
            .Where(s => s.IsActive && 
                       s.Status == SubscriptionStatus.Active && 
                       s.EndDate <= expiryDate &&
                       s.EndDate > DateTime.UtcNow)
            .OrderBy(s => s.EndDate)
            .ToListAsync();
    }
}

/// <summary>
/// Usage summary for a hospital subscription
/// </summary>
public class UsageSummary
{
    public int CurrentUsers { get; set; }
    public int MaxUsers { get; set; }
    public int CurrentPatients { get; set; }
    public int MaxPatients { get; set; }
    public decimal CurrentStorageGB { get; set; }
    public decimal MaxStorageGB { get; set; }
    public decimal UserUsagePercentage { get; set; }
    public decimal PatientUsagePercentage { get; set; }
    public decimal StorageUsagePercentage { get; set; }
    public bool IsOverLimit { get; set; }
    public int DaysUntilRenewal { get; set; }
}
