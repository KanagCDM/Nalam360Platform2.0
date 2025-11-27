namespace SimpleMedWayDemo.Services;

public class Module
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
}

public class ModuleService
{
    private readonly List<Module> _availableModules = new()
    {
        new Module { Id = "dashboard", Name = "Dashboard", Description = "Overview and analytics", Icon = "📊", Color = "#4CAF50" },
        new Module { Id = "patients", Name = "Patient Management", Description = "Manage patient records", Icon = "👥", Color = "#2196F3" },
        new Module { Id = "appointments", Name = "Appointments", Description = "Schedule and manage appointments", Icon = "📅", Color = "#FF9800" },
        new Module { Id = "billing", Name = "Billing", Description = "Invoice and payment management", Icon = "💰", Color = "#9C27B0" },
        new Module { Id = "inventory", Name = "Inventory", Description = "Medical supplies and equipment", Icon = "📦", Color = "#F44336" },
        new Module { Id = "reports", Name = "Reports", Description = "Analytics and reporting", Icon = "📈", Color = "#00BCD4" },
        new Module { Id = "settings", Name = "Settings", Description = "System configuration", Icon = "⚙️", Color = "#607D8B" },
        new Module { Id = "users", Name = "User Management", Description = "Manage users and roles", Icon = "👤", Color = "#795548" }
    };

    public Task<List<Module>> GetAvailableModulesAsync()
    {
        return Task.FromResult(_availableModules);
    }

    public Task<Module?> GetModuleByIdAsync(string id)
    {
        var module = _availableModules.FirstOrDefault(m => m.Id == id);
        return Task.FromResult(module);
    }
}
