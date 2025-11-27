namespace SimpleMedWayDemo.Models;

/// <summary>
/// Represents a top-level product in the system
/// A product contains multiple modules
/// </summary>
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty; // Unique product identifier (e.g., "MEDWAY", "HMS")
    public string Description { get; set; } = string.Empty;
    public string Version { get; set; } = "1.0.0";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual ICollection<ProductModule> Modules { get; set; } = new List<ProductModule>();
    public virtual ICollection<SubscriptionProduct> SubscriptionProducts { get; set; } = new List<SubscriptionProduct>();
}

/// <summary>
/// Represents a module within a product
/// A module contains multiple entities/features
/// </summary>
public class ProductModule
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty; // Unique module identifier (e.g., "PATIENTS", "BILLING")
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public int DisplayOrder { get; set; } = 0;
    public bool IsCore { get; set; } = true; // Core modules available in all plans
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual Product? Product { get; set; }
    public virtual ICollection<SubscriptionModule> SubscriptionModules { get; set; } = new List<SubscriptionModule>();
}

/// <summary>
/// Maps products to subscription plans
/// Defines which products are available in each subscription tier
/// </summary>
public class SubscriptionProduct
{
    public int Id { get; set; }
    public int SubscriptionPlanId { get; set; }
    public int ProductId { get; set; }
    public bool IsEnabled { get; set; } = true;
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual SubscriptionPlan? SubscriptionPlan { get; set; }
    public virtual Product? Product { get; set; }
}

/// <summary>
/// Maps modules to subscription plans
/// Defines which modules are available in each subscription tier
/// </summary>
public class SubscriptionModule
{
    public int Id { get; set; }
    public int SubscriptionPlanId { get; set; }
    public int ProductModuleId { get; set; }
    public bool IsEnabled { get; set; } = true;
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual SubscriptionPlan? SubscriptionPlan { get; set; }
    public virtual ProductModule? ProductModule { get; set; }
}

/// <summary>
/// Predefined products for the MedWay Healthcare System
/// </summary>
public static class PredefinedProducts
{
    public static List<Product> GetAllProducts()
    {
        return new List<Product>
        {
            new Product
            {
                Name = "MedWay Healthcare Management System",
                Code = "MEDWAY_HMS",
                Description = "Complete hospital management solution with patient care, billing, and operations",
                Version = "2.0.0",
                IsActive = true
            },
            new Product
            {
                Name = "MedWay Clinical Suite",
                Code = "MEDWAY_CLINICAL",
                Description = "Advanced clinical management and electronic health records",
                Version = "1.5.0",
                IsActive = true
            },
            new Product
            {
                Name = "MedWay Analytics & Reporting",
                Code = "MEDWAY_ANALYTICS",
                Description = "Business intelligence and healthcare analytics platform",
                Version = "1.0.0",
                IsActive = true
            }
        };
    }
}

/// <summary>
/// Predefined modules for the MedWay HMS product
/// </summary>
public static class PredefinedModules
{
    public static List<ProductModule> GetHMSModules()
    {
        return new List<ProductModule>
        {
            new ProductModule
            {
                Name = "Dashboard",
                Code = "DASHBOARD",
                Description = "Executive dashboard with key performance indicators",
                Icon = "dashboard",
                DisplayOrder = 1,
                IsCore = true,
                IsActive = true
            },
            new ProductModule
            {
                Name = "Patient Management",
                Code = "PATIENTS",
                Description = "Patient registration, records, and care management",
                Icon = "people",
                DisplayOrder = 2,
                IsCore = true,
                IsActive = true
            },
            new ProductModule
            {
                Name = "Appointments",
                Code = "APPOINTMENTS",
                Description = "Appointment scheduling and calendar management",
                Icon = "calendar",
                DisplayOrder = 3,
                IsCore = true,
                IsActive = true
            },
            new ProductModule
            {
                Name = "Billing & Finance",
                Code = "BILLING",
                Description = "Invoicing, payments, and financial management",
                Icon = "receipt",
                DisplayOrder = 4,
                IsCore = true,
                IsActive = true
            },
            new ProductModule
            {
                Name = "Inventory Management",
                Code = "INVENTORY",
                Description = "Stock management, procurement, and supplies",
                Icon = "inventory",
                DisplayOrder = 5,
                IsCore = false,
                IsActive = true
            },
            new ProductModule
            {
                Name = "Reports & Analytics",
                Code = "REPORTS",
                Description = "Comprehensive reporting and data analysis",
                Icon = "analytics",
                DisplayOrder = 6,
                IsCore = false,
                IsActive = true
            },
            new ProductModule
            {
                Name = "System Settings",
                Code = "SETTINGS",
                Description = "System configuration and preferences",
                Icon = "settings",
                DisplayOrder = 7,
                IsCore = true,
                IsActive = true
            },
            new ProductModule
            {
                Name = "User Management",
                Code = "USERS",
                Description = "User accounts, roles, and permissions",
                Icon = "person",
                DisplayOrder = 8,
                IsCore = true,
                IsActive = true
            }
        };
    }
}
