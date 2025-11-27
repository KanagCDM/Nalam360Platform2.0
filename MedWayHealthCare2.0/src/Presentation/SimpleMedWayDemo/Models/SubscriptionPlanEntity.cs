namespace SimpleMedWayDemo.Models;

/// <summary>
/// Maps which entities are included in each subscription plan
/// </summary>
public class SubscriptionPlanEntity
{
    public int Id { get; set; }
    public int SubscriptionPlanId { get; set; }
    public int ModuleEntityId { get; set; }
    public bool IsEnabled { get; set; } = true;
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual SubscriptionPlan? SubscriptionPlan { get; set; }
    public virtual ModuleEntity? ModuleEntity { get; set; }
}

/// <summary>
/// Predefined entity mappings for each subscription tier
/// </summary>
public static class PredefinedPlanEntityMappings
{
    /// <summary>
    /// Returns entity codes that should be included in each subscription tier
    /// </summary>
    public static Dictionary<SubscriptionTier, List<string>> GetPlanEntityMappings()
    {
        return new Dictionary<SubscriptionTier, List<string>>
        {
            // Trial Plan - Only core entities across all modules
            {
                SubscriptionTier.Trial,
                new List<string>
                {
                    // Dashboard
                    "DASH_OVERVIEW",
                    // Patients
                    "PAT_REGISTER", "PAT_RECORDS",
                    // Appointments
                    "APPT_SCHEDULE", "APPT_CALENDAR",
                    // Billing
                    "BILL_INVOICE", "BILL_PAYMENT",
                    // Inventory
                    "INV_STOCK",
                    // Reports
                    "REP_BASIC",
                    // Settings
                    "SET_PROFILE", "SET_DEPARTMENTS",
                    // Users
                    "USER_MANAGE", "USER_PERMISSIONS"
                }
            },

            // Standard Plan - Core entities + some advanced features
            {
                SubscriptionTier.Standard,
                new List<string>
                {
                    // All Trial entities
                    "DASH_OVERVIEW",
                    "PAT_REGISTER", "PAT_RECORDS", "PAT_HISTORY", "PAT_DOCUMENTS",
                    "APPT_SCHEDULE", "APPT_CALENDAR", "APPT_ONLINE", "APPT_REMINDER",
                    "BILL_INVOICE", "BILL_PAYMENT", "BILL_REPORTS",
                    "INV_STOCK", "INV_PURCHASE", "INV_EXPIRY",
                    "REP_BASIC", "REP_EXPORT",
                    "SET_PROFILE", "SET_DEPARTMENTS", "SET_ROLES",
                    "USER_MANAGE", "USER_PERMISSIONS", "USER_AUDIT"
                }
            },

            // Gold Plan - All Standard + premium features
            {
                SubscriptionTier.Gold,
                new List<string>
                {
                    // All Standard entities
                    "DASH_OVERVIEW", "DASH_ANALYTICS", "DASH_REALTIME",
                    "PAT_REGISTER", "PAT_RECORDS", "PAT_HISTORY", "PAT_DOCUMENTS", "PAT_INSURANCE", "PAT_FAMILY",
                    "APPT_SCHEDULE", "APPT_CALENDAR", "APPT_ONLINE", "APPT_REMINDER", "APPT_WAITLIST", "APPT_VIDEO",
                    "BILL_INVOICE", "BILL_PAYMENT", "BILL_INSURANCE", "BILL_REPORTS", "BILL_ONLINE",
                    "INV_STOCK", "INV_PURCHASE", "INV_SUPPLIER", "INV_EXPIRY", "INV_BARCODE", "INV_ANALYTICS",
                    "REP_BASIC", "REP_CUSTOM", "REP_SCHEDULED", "REP_EXPORT", "REP_DASHBOARD",
                    "SET_PROFILE", "SET_DEPARTMENTS", "SET_ROLES", "SET_WORKFLOW", "SET_INTEGRATION",
                    "USER_MANAGE", "USER_PERMISSIONS", "USER_AUDIT", "USER_SSO", "USER_2FA"
                }
            },

            // Platinum Plan - All features (all entity codes)
            {
                SubscriptionTier.Platinum,
                new List<string>
                {
                    // All entities available
                    "DASH_OVERVIEW", "DASH_ANALYTICS", "DASH_REALTIME", "DASH_CUSTOM",
                    "PAT_REGISTER", "PAT_RECORDS", "PAT_HISTORY", "PAT_DOCUMENTS", "PAT_INSURANCE", "PAT_FAMILY",
                    "APPT_SCHEDULE", "APPT_CALENDAR", "APPT_ONLINE", "APPT_REMINDER", "APPT_WAITLIST", "APPT_VIDEO",
                    "BILL_INVOICE", "BILL_PAYMENT", "BILL_INSURANCE", "BILL_REPORTS", "BILL_ONLINE", "BILL_INSTALLMENT",
                    "INV_STOCK", "INV_PURCHASE", "INV_SUPPLIER", "INV_EXPIRY", "INV_BARCODE", "INV_ANALYTICS",
                    "REP_BASIC", "REP_CUSTOM", "REP_SCHEDULED", "REP_EXPORT", "REP_DASHBOARD",
                    "SET_PROFILE", "SET_DEPARTMENTS", "SET_ROLES", "SET_WORKFLOW", "SET_INTEGRATION", "SET_BACKUP",
                    "USER_MANAGE", "USER_PERMISSIONS", "USER_AUDIT", "USER_SSO", "USER_2FA"
                }
            }
        };
    }

    /// <summary>
    /// Get entity count summary for each tier
    /// </summary>
    public static Dictionary<SubscriptionTier, int> GetEntityCountByTier()
    {
        var mappings = GetPlanEntityMappings();
        return mappings.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.Count
        );
    }
}
