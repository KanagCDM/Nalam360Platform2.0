using Nalam360Enterprise.UI.Core.Security;
using Nalam360Enterprise.UI.Core.Theming;
using Nalam360.Platform.Core.DependencyInjection;
using Nalam360Enterprise.UI;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Add Nalam360 Platform services
builder.Services.AddPlatformCore();

// Add Nalam360 Enterprise UI
builder.Services.AddNalam360EnterpriseUI<SamplePermissionService>(theme =>
{
    theme.CurrentTheme = Theme.Light;
    theme.SyncfusionThemeName = "material";
    theme.AutoDetectTheme = false;
});

var app = builder.Build();

// Configure pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();

// Sample Permission Service
public class SamplePermissionService : IPermissionService
{
    public Task<bool> HasPermissionAsync(string permission)
    {
        // In real app, check against user's roles/permissions
        return Task.FromResult(true); // Allow all for demo
    }
    
    public Task<bool> HasAnyPermissionAsync(params string[] permissions)
    {
        return Task.FromResult(true);
    }
    
    public Task<bool> HasAllPermissionsAsync(params string[] permissions)
    {
        return Task.FromResult(true);
    }
    
    public Task<bool> IsInRoleAsync(string role)
    {
        return Task.FromResult(true);
    }
    
    public Task<IEnumerable<string>> GetUserPermissionsAsync()
    {
        return Task.FromResult<IEnumerable<string>>(new List<string>
        {
            "users.view",
            "users.edit",
            "orders.view",
            "orders.create"
        });
    }
}
