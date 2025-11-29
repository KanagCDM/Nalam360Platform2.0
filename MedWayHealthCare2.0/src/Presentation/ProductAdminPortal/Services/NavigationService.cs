namespace ProductAdminPortal.Services;

public class NavigationItem
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public int Order { get; set; }
}

public class NavigationService
{
    private List<NavigationItem> _navigationItems = new();
    public event Action? OnNavigationChanged;

    public Task<List<NavigationItem>> GetNavigationItemsAsync()
    {
        return Task.FromResult(_navigationItems);
    }

    public Task BuildNavigationAsync(List<string> moduleIds)
    {
        _navigationItems = new List<NavigationItem>();
        
        var moduleNavigation = new Dictionary<string, NavigationItem>
        {
            ["dashboard"] = new NavigationItem { Id = "dashboard", Name = "Dashboard", Icon = "📊", Url = "/dashboard", Order = 1 },
            ["patients"] = new NavigationItem { Id = "patients", Name = "Patients", Icon = "👥", Url = "/patients", Order = 2 },
            ["appointments"] = new NavigationItem { Id = "appointments", Name = "Appointments", Icon = "📅", Url = "/appointments", Order = 3 },
            ["billing"] = new NavigationItem { Id = "billing", Name = "Billing", Icon = "💰", Url = "/billing", Order = 4 },
            ["inventory"] = new NavigationItem { Id = "inventory", Name = "Inventory", Icon = "📦", Url = "/inventory", Order = 5 },
            ["reports"] = new NavigationItem { Id = "reports", Name = "Reports", Icon = "📈", Url = "/reports", Order = 6 },
            ["settings"] = new NavigationItem { Id = "settings", Name = "Settings", Icon = "⚙️", Url = "/settings", Order = 7 },
            ["users"] = new NavigationItem { Id = "users", Name = "Users", Icon = "👤", Url = "/users", Order = 8 }
        };

        foreach (var moduleId in moduleIds)
        {
            if (moduleNavigation.TryGetValue(moduleId, out var navItem))
            {
                _navigationItems.Add(navItem);
            }
        }

        _navigationItems = _navigationItems.OrderBy(n => n.Order).ToList();
        OnNavigationChanged?.Invoke();
        
        return Task.CompletedTask;
    }
}
