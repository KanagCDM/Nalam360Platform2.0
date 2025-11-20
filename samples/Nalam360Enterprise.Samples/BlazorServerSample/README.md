# Nalam360 Enterprise - Blazor Server Sample

This sample application demonstrates the usage of Nalam360 Enterprise Platform components.

## Features Demonstrated

- **Input Components**: TextBox, NumericTextBox, DatePicker, DropDown, CheckBox, Switch, Rating
- **Button Components**: Various button types with different styles
- **Data Components**: Grid with paging, filtering, and sorting
- **Display Components**: Avatar, Divider, Alerts
- **RBAC Integration**: Permission-based component visibility
- **Theming**: Material design theme
- **Audit Logging**: Track user interactions

## Running the Sample

```bash
cd samples/Nalam360Enterprise.Samples/BlazorServerSample
dotnet run
```

Navigate to `https://localhost:5001` in your browser.

## Project Structure

- `Program.cs` - Application startup and service configuration
- `Pages/Index.razor` - Main demo page with component examples
- `Pages/_Host.cshtml` - Host page with Syncfusion scripts
- `Shared/MainLayout.razor` - Application layout with sidebar navigation

## Components Used

### Input Components
- N360TextBox
- N360NumericTextBox
- N360DatePicker
- N360DropDownList
- N360CheckBox
- N360Switch
- N360Rating

### Button Components
- N360Button (Primary, Secondary, Success, Disabled states)

### Data Components
- N360Grid with paging, filtering, and sorting

### Display Components
- N360Avatar
- N360Divider
- N360Alert (Info, Success, Warning types)

## Customization

### Changing Theme

Edit `Program.cs`:

```csharp
builder.Services.AddNalam360EnterpriseUI<SamplePermissionService>(theme =>
{
    theme.CurrentTheme = Theme.Dark; // Change to Dark theme
    theme.SyncfusionThemeName = "bootstrap5-dark";
});
```

### Adding Permissions

Edit the `SamplePermissionService` in `Program.cs`:

```csharp
public Task<IEnumerable<string>> GetUserPermissionsAsync()
{
    return Task.FromResult<IEnumerable<string>>(new List<string>
    {
        "users.view",
        "users.edit",
        "your.custom.permission"
    });
}
```

## Next Steps

1. Explore other component pages (Forms, Data)
2. Implement real authentication and authorization
3. Connect to backend API
4. Add more complex business logic
5. Customize themes and styling

## Documentation

- [Component Inventory](../../../COMPONENT_INVENTORY.md)
- [Quick Start Guide](../../../QUICK_START_CARD.md)
- [Platform Guide](../../../PLATFORM_GUIDE.md)
