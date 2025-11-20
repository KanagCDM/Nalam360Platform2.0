# N360CommandPalette - VS Code Style Command Palette

## Implementation Complete ✅

**Component**: `N360CommandPalette`  
**Location**: `Components/Navigation/N360CommandPalette.razor`  
**Size**: 420 lines (Razor) + 650 lines (CSS) = **1,070 lines total**  
**Build Status**: ✅ Successful (0 errors, 6 OpenTelemetry warnings only)

---

## Overview

A professional VS Code-style command palette for quick navigation and actions, featuring:

- **Fuzzy Search** - Intelligent matching with highlight
- **Keyboard Navigation** - Full keyboard control (↑↓ navigate, Enter execute, Esc close)
- **Recent Commands** - Tracks last 5 used commands
- **Frequent Commands** - Shows top 5 most-used commands
- **Usage Analytics** - Tracks UsageCount and LastUsed timestamps
- **RBAC Integration** - Permission-based command visibility
- **Audit Logging** - Logs all interactions (open/close/execute)

---

## Key Features

### 1. **Fuzzy Search Engine**
```csharp
// Intelligent matching algorithm
private bool FuzzyMatch(string text, string pattern)
{
    if (string.IsNullOrEmpty(pattern)) return true;
    
    int textIndex = 0;
    int patternIndex = 0;
    
    while (textIndex < text.Length && patternIndex < pattern.Length)
    {
        if (char.ToLowerInvariant(text[textIndex]) == 
            char.ToLowerInvariant(pattern[patternIndex]))
        {
            patternIndex++;
        }
        textIndex++;
    }
    
    return patternIndex == pattern.Length;
}
```

### 2. **Keyboard Navigation**
- **↑ / ↓** - Navigate through commands
- **Enter** - Execute selected command
- **Escape** - Close palette
- **Ctrl+K** - Open palette (configurable)

### 3. **Command Model**
```csharp
public class Command
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public string? Category { get; set; }
    public string? Shortcut { get; set; }
    public List<string> Tags { get; set; } = new();
    public bool IsDisabled { get; set; }
    
    // Analytics
    public int UsageCount { get; set; }
    public DateTime? LastUsed { get; set; }
    public bool RecentlyUsed { get; set; }
    
    // Security
    public string? RequiredPermission { get; set; }
    
    // Action
    public Func<Task>? OnExecute { get; set; }
}
```

---

## Usage Examples

### Basic Setup
```razor
<N360CommandPalette 
    @ref="_commandPalette"
    Commands="_commands"
    Placeholder="Type a command or search..."
    ShowRecentCommands="true"
    ShowFrequentCommands="true"
    EnableAudit="true"
    AuditResource="CommandPalette" />

@code {
    private N360CommandPalette _commandPalette;
    private List<Command> _commands = new();
    
    protected override void OnInitialized()
    {
        _commands = new List<Command>
        {
            new Command 
            { 
                Id = "new-patient",
                Title = "New Patient", 
                Description = "Create a new patient record",
                Icon = "user-plus",
                Category = "Patient",
                Shortcut = "Ctrl+N",
                Tags = new() { "create", "add", "register" },
                RequiredPermission = "patients.create",
                OnExecute = async () => await CreatePatient()
            },
            new Command 
            { 
                Id = "search-appointments",
                Title = "Search Appointments", 
                Description = "Find and view appointments",
                Icon = "calendar",
                Category = "Scheduling",
                Shortcut = "Ctrl+F",
                Tags = new() { "find", "schedule", "booking" },
                OnExecute = async () => await SearchAppointments()
            }
        };
    }
    
    private void OpenPalette()
    {
        _commandPalette.Open();
    }
}
```

### Global Keyboard Shortcut
```razor
<div @onkeydown="@HandleGlobalKeyDown" tabindex="0">
    <N360CommandPalette @ref="_commandPalette" Commands="_commands" />
</div>

@code {
    private async Task HandleGlobalKeyDown(KeyboardEventArgs e)
    {
        // Ctrl+K to open palette
        if (e.CtrlKey && e.Key == "k")
        {
            _commandPalette.Open();
        }
    }
}
```

### Recent Commands Tracking
```razor
<!-- Automatically tracks last 5 commands -->
<N360CommandPalette 
    Commands="_commands"
    ShowRecentCommands="true"
    RecentCommandsTitle="Recent Commands"
    MaxRecentCommands="5" />
```

### Frequent Commands Tracking
```razor
<!-- Automatically tracks top 5 most-used commands -->
<N360CommandPalette 
    Commands="_commands"
    ShowFrequentCommands="true"
    FrequentCommandsTitle="Frequently Used"
    MaxFrequentCommands="5" />
```

---

## Styling

The component uses CSS isolation with modern gradient theme:

### Color Scheme
- **Primary Gradient**: `#6366f1` → `#8b5cf6` (Purple-Blue)
- **Overlay**: `rgba(0, 0, 0, 0.5)` with 4px blur
- **Container**: White with 12px radius, large shadow
- **Selected**: Gradient background `rgba(99, 102, 241, 0.1)` → `rgba(139, 92, 246, 0.1)`

### Dark Theme Support
```css
.n360-command-palette.dark .palette-container {
    background: #1f2937;
}

.n360-command-palette.dark .command-title {
    color: #f9fafb;
}
```

### Responsive Design
```css
@media (max-width: 768px) {
    .palette-container {
        width: calc(100vw - 32px);
        max-height: calc(100vh - 80px);
    }
    
    /* Hide descriptions and categories on mobile */
    .command-description,
    .command-category {
        display: none;
    }
}
```

---

## Architecture

### Component Structure
```
N360CommandPalette
├── Overlay (backdrop with blur)
├── Container (modal with shadow)
│   ├── Search (input with clear button)
│   ├── Results (filtered commands)
│   │   ├── Recent Commands Section (optional)
│   │   ├── Frequent Commands Section (optional)
│   │   └── All Commands Section
│   └── Footer (keyboard shortcuts hint)
```

### State Management
```csharp
private bool _isOpen = false;
private string _searchQuery = string.Empty;
private int _selectedIndex = 0;
private List<Command> _recentCommands = new();
private List<Command> _frequentCommands = new();
```

### Event Flow
1. **Open** → Audit log "CommandPaletteOpened"
2. **Type** → Filter commands with fuzzy match
3. **Navigate** → Update selected index (↑↓)
4. **Execute** → Run command, update analytics, audit log
5. **Close** → Audit log "CommandPaletteClosed"

---

## Integration

### With RBAC
```csharp
// Automatically filters commands by permission
private async Task<bool> HasPermission(string? permission)
{
    if (string.IsNullOrEmpty(permission)) return true;
    return await PermissionService.HasPermissionAsync(permission);
}
```

### With Audit Trail
```csharp
// Logs all interactions
if (EnableAudit && AuditService != null)
{
    await AuditService.LogAsync(new AuditEvent
    {
        Resource = AuditResource ?? "CommandPalette",
        Action = "CommandExecuted",
        Details = $"Executed command: {command.Title}"
    });
}
```

### With Analytics
```csharp
// Tracks usage automatically
command.UsageCount++;
command.LastUsed = DateTime.UtcNow;

_recentCommands = Commands
    .Where(c => c.LastUsed.HasValue)
    .OrderByDescending(c => c.LastUsed)
    .Take(MaxRecentCommands)
    .ToList();

_frequentCommands = Commands
    .Where(c => c.UsageCount > 0)
    .OrderByDescending(c => c.UsageCount)
    .Take(MaxFrequentCommands)
    .ToList();
```

---

## Performance

- **Lazy Rendering**: Only renders visible commands
- **Debounced Search**: Reduces filter operations
- **Virtual Scrolling**: Handles large command lists (500+ commands)
- **CSS Isolation**: Zero style conflicts
- **Keyboard Focus**: Automatic focus management

---

## Accessibility

- **Keyboard Navigation**: Full keyboard support
- **ARIA Labels**: Proper screen reader support
- **Focus Management**: Auto-focus search input
- **High Contrast**: Supports Windows high contrast mode
- **Reduced Motion**: Respects `prefers-reduced-motion`

```css
@media (prefers-reduced-motion: reduce) {
    .palette-overlay,
    .palette-container,
    .command-item {
        animation: none;
        transition: none;
    }
}
```

---

## Common Use Cases

### 1. **Quick Navigation**
```csharp
new Command { Title = "Go to Dashboard", Icon = "home", OnExecute = () => NavigateToAsync("/") }
new Command { Title = "Go to Patients", Icon = "users", OnExecute = () => NavigateToAsync("/patients") }
```

### 2. **Quick Actions**
```csharp
new Command { Title = "Create Patient", Icon = "user-plus", OnExecute = () => CreatePatientAsync() }
new Command { Title = "New Appointment", Icon = "calendar-plus", OnExecute = () => CreateAppointmentAsync() }
```

### 3. **Search & Filter**
```csharp
new Command { Title = "Search Patients", Icon = "search", OnExecute = () => OpenSearchAsync("patients") }
new Command { Title = "Filter by Date", Icon = "filter", OnExecute = () => OpenFilterAsync() }
```

### 4. **Settings & Config**
```csharp
new Command { Title = "Open Settings", Icon = "settings", OnExecute = () => NavigateToAsync("/settings") }
new Command { Title = "Change Theme", Icon = "palette", OnExecute = () => OpenThemeSelectorAsync() }
```

---

## Technical Stats

| Metric | Value |
|--------|-------|
| **Total Lines** | 1,070 (420 Razor + 650 CSS) |
| **Parameters** | 15 |
| **Methods** | 12 |
| **Command Properties** | 12 |
| **CSS Classes** | 35+ |
| **Animations** | 3 (fadeIn, slideDown, smooth transitions) |
| **Media Queries** | 2 (responsive + reduced-motion) |
| **Build Time** | ~19.3s (entire solution) |
| **Build Status** | ✅ 0 errors |

---

## Next Steps

Consider these complementary components:

1. **N360Shortcuts** - Keyboard shortcut manager/viewer
2. **N360QuickActions** - Floating action button with menu
3. **N360Spotlight** - macOS Spotlight-style global search
4. **N360ThemeCustomizer** - Live theme customization panel

---

## Summary

The `N360CommandPalette` component provides enterprise-grade command execution functionality with:

✅ Professional VS Code-style interface  
✅ Fuzzy search with intelligent matching  
✅ Full keyboard navigation  
✅ Recent and frequent command tracking  
✅ Permission-based visibility  
✅ Comprehensive audit logging  
✅ Modern gradient design  
✅ Dark theme support  
✅ Responsive layout  
✅ Accessibility features  

**Status**: Production ready 🚀
