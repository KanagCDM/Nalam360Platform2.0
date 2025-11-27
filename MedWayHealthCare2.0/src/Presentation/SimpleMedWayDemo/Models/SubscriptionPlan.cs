namespace SimpleMedWayDemo.Models;

/// <summary>
/// Subscription tier levels
/// </summary>
public enum SubscriptionTier
{
    Trial = 0,      // Free trial with limited features
    Standard = 1,   // Basic paid plan
    Gold = 2,       // Advanced features
    Platinum = 3    // Premium plan with all features
}

/// <summary>
/// Subscription plan definition with features and pricing
/// </summary>
public class SubscriptionPlan
{
    public int Id { get; set; }
    public SubscriptionTier Tier { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal MonthlyPrice { get; set; }
    public decimal YearlyPrice { get; set; }
    public int MaxUsers { get; set; }
    public int MaxPatients { get; set; }
    public bool HasAdvancedReports { get; set; }
    public bool HasAPIAccess { get; set; }
    public bool Has24x7Support { get; set; }
    public bool HasCustomBranding { get; set; }
    public int StorageGB { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation property
    public virtual ICollection<SubscriptionPlanEntity> IncludedEntities { get; set; } = new List<SubscriptionPlanEntity>();
}

/// <summary>
/// Predefined subscription plans
/// </summary>
public static class PredefinedPlans
{
    public static List<SubscriptionPlan> GetAllPlans()
    {
        return new List<SubscriptionPlan>
        {
            new SubscriptionPlan
            {
                Tier = SubscriptionTier.Trial,
                Name = "Trial Plan",
                Description = "30-day free trial with basic features",
                MonthlyPrice = 0,
                YearlyPrice = 0,
                MaxUsers = 5,
                MaxPatients = 100,
                HasAdvancedReports = false,
                HasAPIAccess = false,
                Has24x7Support = false,
                HasCustomBranding = false,
                StorageGB = 5,
                IsActive = true
            },
            new SubscriptionPlan
            {
                Tier = SubscriptionTier.Standard,
                Name = "Standard Plan",
                Description = "Essential features for small clinics and hospitals",
                MonthlyPrice = 99.99m,
                YearlyPrice = 999.99m, // ~2 months free
                MaxUsers = 20,
                MaxPatients = 1000,
                HasAdvancedReports = false,
                HasAPIAccess = false,
                Has24x7Support = false,
                HasCustomBranding = false,
                StorageGB = 50,
                IsActive = true
            },
            new SubscriptionPlan
            {
                Tier = SubscriptionTier.Gold,
                Name = "Gold Plan",
                Description = "Advanced features for growing healthcare facilities",
                MonthlyPrice = 249.99m,
                YearlyPrice = 2499.99m,
                MaxUsers = 50,
                MaxPatients = 5000,
                HasAdvancedReports = true,
                HasAPIAccess = true,
                Has24x7Support = false,
                HasCustomBranding = true,
                StorageGB = 200,
                IsActive = true
            },
            new SubscriptionPlan
            {
                Tier = SubscriptionTier.Platinum,
                Name = "Platinum Plan",
                Description = "Enterprise-grade solution with all features and priority support",
                MonthlyPrice = 499.99m,
                YearlyPrice = 4999.99m,
                MaxUsers = -1, // Unlimited
                MaxPatients = -1, // Unlimited
                HasAdvancedReports = true,
                HasAPIAccess = true,
                Has24x7Support = true,
                HasCustomBranding = true,
                StorageGB = 1000,
                IsActive = true
            }
        };
    }
}
