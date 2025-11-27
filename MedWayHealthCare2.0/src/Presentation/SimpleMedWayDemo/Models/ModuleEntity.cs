namespace SimpleMedWayDemo.Models;

/// <summary>
/// Represents an entity/feature within a module
/// </summary>
public class ModuleEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string EntityCode { get; set; } = string.Empty; // Unique identifier for the entity
    public string ModuleName { get; set; } = string.Empty; // Dashboard, Patients, Appointments, etc.
    public string Description { get; set; } = string.Empty;
    public bool IsCore { get; set; } // Core entities available in all plans
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Module names enum for consistency
/// </summary>
public enum ModuleName
{
    Dashboard,
    Patients,
    Appointments,
    Billing,
    Inventory,
    Reports,
    Settings,
    Users
}

/// <summary>
/// Predefined entities for each module
/// </summary>
public static class PredefinedEntities
{
    public static List<ModuleEntity> GetAllEntities()
    {
        return new List<ModuleEntity>
        {
            // Dashboard Module Entities
            new ModuleEntity { EntityCode = "DASH_OVERVIEW", Name = "Dashboard Overview", ModuleName = "DASHBOARD", Description = "Basic dashboard with key metrics", IsCore = true },
            new ModuleEntity { EntityCode = "DASH_ANALYTICS", Name = "Advanced Analytics", ModuleName = "DASHBOARD", Description = "Advanced analytics and charts", IsCore = false },
            new ModuleEntity { EntityCode = "DASH_REALTIME", Name = "Real-time Monitoring", ModuleName = "DASHBOARD", Description = "Real-time data monitoring", IsCore = false },
            new ModuleEntity { EntityCode = "DASH_CUSTOM", Name = "Custom Dashboards", ModuleName = "DASHBOARD", Description = "Create custom dashboard layouts", IsCore = false },

            // Patients Module Entities
            new ModuleEntity { EntityCode = "PAT_REGISTER", Name = "Patient Registration", ModuleName = "PATIENTS", Description = "Register new patients", IsCore = true },
            new ModuleEntity { EntityCode = "PAT_RECORDS", Name = "Medical Records", ModuleName = "PATIENTS", Description = "View and manage medical records", IsCore = true },
            new ModuleEntity { EntityCode = "PAT_HISTORY", Name = "Patient History", ModuleName = "PATIENTS", Description = "Complete patient history tracking", IsCore = false },
            new ModuleEntity { EntityCode = "PAT_DOCUMENTS", Name = "Document Management", ModuleName = "PATIENTS", Description = "Upload and manage patient documents", IsCore = false },
            new ModuleEntity { EntityCode = "PAT_INSURANCE", Name = "Insurance Management", ModuleName = "PATIENTS", Description = "Manage patient insurance details", IsCore = false },
            new ModuleEntity { EntityCode = "PAT_FAMILY", Name = "Family Tree", ModuleName = "PATIENTS", Description = "Patient family medical history", IsCore = false },

            // Appointments Module Entities
            new ModuleEntity { EntityCode = "APPT_SCHEDULE", Name = "Appointment Scheduling", ModuleName = "APPOINTMENTS", Description = "Schedule patient appointments", IsCore = true },
            new ModuleEntity { EntityCode = "APPT_CALENDAR", Name = "Calendar View", ModuleName = "APPOINTMENTS", Description = "Calendar-based appointment view", IsCore = true },
            new ModuleEntity { EntityCode = "APPT_ONLINE", Name = "Online Booking", ModuleName = "APPOINTMENTS", Description = "Patient self-service booking", IsCore = false },
            new ModuleEntity { EntityCode = "APPT_REMINDER", Name = "SMS/Email Reminders", ModuleName = "APPOINTMENTS", Description = "Automated appointment reminders", IsCore = false },
            new ModuleEntity { EntityCode = "APPT_WAITLIST", Name = "Waiting List Management", ModuleName = "APPOINTMENTS", Description = "Manage appointment waiting lists", IsCore = false },
            new ModuleEntity { EntityCode = "APPT_VIDEO", Name = "Video Consultation", ModuleName = "APPOINTMENTS", Description = "Telemedicine video appointments", IsCore = false },

            // Billing Module Entities
            new ModuleEntity { EntityCode = "BILL_INVOICE", Name = "Invoice Generation", ModuleName = "BILLING", Description = "Generate patient invoices", IsCore = true },
            new ModuleEntity { EntityCode = "BILL_PAYMENT", Name = "Payment Processing", ModuleName = "BILLING", Description = "Process patient payments", IsCore = true },
            new ModuleEntity { EntityCode = "BILL_INSURANCE", Name = "Insurance Claims", ModuleName = "BILLING", Description = "Process insurance claims", IsCore = false },
            new ModuleEntity { EntityCode = "BILL_REPORTS", Name = "Financial Reports", ModuleName = "BILLING", Description = "Detailed financial reporting", IsCore = false },
            new ModuleEntity { EntityCode = "BILL_ONLINE", Name = "Online Payment Gateway", ModuleName = "BILLING", Description = "Accept online payments", IsCore = false },
            new ModuleEntity { EntityCode = "BILL_INSTALLMENT", Name = "Installment Plans", ModuleName = "BILLING", Description = "Manage payment installments", IsCore = false },

            // Inventory Module Entities
            new ModuleEntity { EntityCode = "INV_STOCK", Name = "Stock Management", ModuleName = "INVENTORY", Description = "Basic inventory tracking", IsCore = true },
            new ModuleEntity { EntityCode = "INV_PURCHASE", Name = "Purchase Orders", ModuleName = "INVENTORY", Description = "Manage purchase orders", IsCore = false },
            new ModuleEntity { EntityCode = "INV_SUPPLIER", Name = "Supplier Management", ModuleName = "INVENTORY", Description = "Manage supplier information", IsCore = false },
            new ModuleEntity { EntityCode = "INV_EXPIRY", Name = "Expiry Tracking", ModuleName = "INVENTORY", Description = "Track expiring items", IsCore = false },
            new ModuleEntity { EntityCode = "INV_BARCODE", Name = "Barcode System", ModuleName = "INVENTORY", Description = "Barcode-based inventory", IsCore = false },
            new ModuleEntity { EntityCode = "INV_ANALYTICS", Name = "Inventory Analytics", ModuleName = "INVENTORY", Description = "Advanced inventory insights", IsCore = false },

            // Reports Module Entities
            new ModuleEntity { EntityCode = "REP_BASIC", Name = "Basic Reports", ModuleName = "REPORTS", Description = "Standard predefined reports", IsCore = true },
            new ModuleEntity { EntityCode = "REP_CUSTOM", Name = "Custom Reports", ModuleName = "REPORTS", Description = "Create custom reports", IsCore = false },
            new ModuleEntity { EntityCode = "REP_SCHEDULED", Name = "Scheduled Reports", ModuleName = "REPORTS", Description = "Automated report generation", IsCore = false },
            new ModuleEntity { EntityCode = "REP_EXPORT", Name = "Advanced Export", ModuleName = "REPORTS", Description = "Export to Excel, PDF, CSV", IsCore = false },
            new ModuleEntity { EntityCode = "REP_DASHBOARD", Name = "Report Dashboards", ModuleName = "REPORTS", Description = "Interactive report dashboards", IsCore = false },

            // Settings Module Entities
            new ModuleEntity { EntityCode = "SET_PROFILE", Name = "Hospital Profile", ModuleName = "SETTINGS", Description = "Manage hospital profile", IsCore = true },
            new ModuleEntity { EntityCode = "SET_DEPARTMENTS", Name = "Department Setup", ModuleName = "SETTINGS", Description = "Configure departments", IsCore = true },
            new ModuleEntity { EntityCode = "SET_ROLES", Name = "Role Management", ModuleName = "SETTINGS", Description = "Configure user roles", IsCore = false },
            new ModuleEntity { EntityCode = "SET_WORKFLOW", Name = "Workflow Customization", ModuleName = "SETTINGS", Description = "Customize workflows", IsCore = false },
            new ModuleEntity { EntityCode = "SET_INTEGRATION", Name = "Third-party Integration", ModuleName = "SETTINGS", Description = "Integration settings", IsCore = false },
            new ModuleEntity { EntityCode = "SET_BACKUP", Name = "Backup & Restore", ModuleName = "SETTINGS", Description = "Data backup configuration", IsCore = false },

            // Users Module Entities
            new ModuleEntity { EntityCode = "USER_MANAGE", Name = "User Management", ModuleName = "USERS", Description = "Create and manage users", IsCore = true },
            new ModuleEntity { EntityCode = "USER_PERMISSIONS", Name = "Permission Control", ModuleName = "USERS", Description = "Manage user permissions", IsCore = true },
            new ModuleEntity { EntityCode = "USER_AUDIT", Name = "Audit Logs", ModuleName = "USERS", Description = "Track user activities", IsCore = false },
            new ModuleEntity { EntityCode = "USER_SSO", Name = "Single Sign-On", ModuleName = "USERS", Description = "SSO integration", IsCore = false },
            new ModuleEntity { EntityCode = "USER_2FA", Name = "Two-Factor Auth", ModuleName = "USERS", Description = "Enhanced security with 2FA", IsCore = false },
        };
    }
}
