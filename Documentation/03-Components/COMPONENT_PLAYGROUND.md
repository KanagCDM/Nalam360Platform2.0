# Component Playground & Documentation Site

## Overview
Interactive documentation and playground for all 112 Nalam360 Enterprise UI components with live code examples, property editors, and API reference.

## Features

### 🎨 Interactive Component Explorer
- **Live Preview**: Real-time component rendering with instant updates
- **Property Editor**: Visual editors for all component parameters
- **Code Generator**: Auto-generated Razor code snippets
- **Theme Switcher**: Test components in Light/Dark/High-Contrast themes
- **RTL Toggle**: Preview components in RTL mode
- **Responsive Preview**: Mobile/Tablet/Desktop viewports

### 📚 Documentation Features
- **API Reference**: Auto-generated from XML comments
- **Usage Examples**: Common scenarios with code snippets
- **Best Practices**: Performance tips and accessibility guidelines
- **Props Table**: Complete parameter documentation with types/defaults
- **Events Table**: All EventCallback parameters with descriptions
- **Search**: Full-text search across all components

### 🔧 Developer Tools
- **Copy Code**: One-click code snippet copying
- **Download Examples**: Export working examples as .razor files
- **Accessibility Audit**: Built-in a11y checker for each component
- **Performance Monitor**: Render time and memory usage tracking

## Implementation Plan

### Phase 1: Core Infrastructure (2 days)
```
docs/playground/
├── Nalam360.ComponentPlayground/
│   ├── Nalam360.ComponentPlayground.csproj
│   ├── Program.cs
│   ├── _Imports.razor
│   ├── App.razor
│   ├── Pages/
│   │   ├── Index.razor
│   │   ├── ComponentExplorer.razor
│   │   └── ApiReference.razor
│   ├── Shared/
│   │   ├── MainLayout.razor
│   │   ├── NavMenu.razor
│   │   ├── PropertyEditor.razor
│   │   ├── CodeViewer.razor
│   │   └── LivePreview.razor
│   ├── Services/
│   │   ├── ComponentMetadataService.cs
│   │   ├── CodeGeneratorService.cs
│   │   └── SearchService.cs
│   └── wwwroot/
│       ├── css/
│       ├── js/
│       └── index.html
└── README.md
```

### Phase 2: Component Pages (3 days)
Auto-generate pages for all 112 components:

**Template Structure:**
```razor
@page "/components/{component-name}"

<ComponentHeader Name="@ComponentName" 
                 Category="@Category" 
                 Status="@Status" />

<Tabs>
    <Tab Title="Overview">
        <ComponentDescription />
        <UsageExamples />
    </Tab>
    
    <Tab Title="Playground">
        <PropertyEditor @bind-Props="@ComponentProps" />
        <LivePreview Component="@ComponentType" Props="@ComponentProps" />
        <CodeViewer Code="@GeneratedCode" />
    </Tab>
    
    <Tab Title="API">
        <PropsTable Props="@ComponentParameters" />
        <EventsTable Events="@ComponentEvents" />
        <MethodsTable Methods="@ComponentMethods" />
    </Tab>
    
    <Tab Title="Examples">
        <Example Title="Basic Usage" Code="@BasicExample" />
        <Example Title="With Validation" Code="@ValidationExample" />
        <Example Title="Server-Side" Code="@ServerExample" />
        <Example Title="RBAC" Code="@RbacExample" />
    </Tab>
    
    <Tab Title="Accessibility">
        <AccessibilityChecklist />
        <KeyboardShortcuts />
        <ScreenReaderGuidance />
    </Tab>
</Tabs>
```

### Phase 3: Code Generation (1 day)
```csharp
public class CodeGeneratorService
{
    public string GenerateRazorCode(ComponentMetadata component, Dictionary<string, object> props)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"<{component.Name}");
        
        foreach (var prop in props)
        {
            if (prop.Value != component.GetDefaultValue(prop.Key))
            {
                sb.AppendLine($"    {prop.Key}=\"{FormatValue(prop.Value)}\"");
            }
        }
        
        if (component.HasChildContent)
        {
            sb.AppendLine(">");
            sb.AppendLine("    <!-- Add content here -->");
            sb.AppendLine($"</{component.Name}>");
        }
        else
        {
            sb.AppendLine(" />");
        }
        
        return sb.ToString();
    }
}
```

### Phase 4: Search & Navigation (1 day)
```csharp
public class SearchService
{
    private readonly List<ComponentIndex> _index;
    
    public async Task<List<SearchResult>> SearchAsync(string query)
    {
        return _index
            .Where(c => c.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                       c.Description.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                       c.Tags.Any(t => t.Contains(query, StringComparison.OrdinalIgnoreCase)))
            .Select(c => new SearchResult(c))
            .ToList();
    }
}
```

## Component Categories Navigation

### Input Components (27)
- Text Inputs: TextBox, NumericTextBox, MaskedTextBox, InputNumber
- Date/Time: DatePicker, DateTimePicker, DateRangePicker, TimePicker
- Selection: DropDownList, MultiSelect, AutoComplete, ComboBox, Cascader, TreeSelect
- Boolean: CheckBox, RadioButton, Switch, Segmented
- Advanced: Slider, Rating, ColorPicker, Upload, OTP, PinInput, Mentions

### Data Components (4)
- Grid, TreeGrid, Pivot, ListView

### Navigation (13)
- Sidebar, TreeView, Tabs, Accordion, Breadcrumb, Toolbar, Menu
- ContextMenu, BottomNavigation, SpeedDial, Pager, Stepper, Tour

### Buttons (4)
- Button, ButtonGroup, Chip, FloatingActionButton

### Feedback (8)
- Toast, Spinner, Tooltip, Badge, Alert, Message, Popconfirm, Result

### Layout (8)
- Dialog, Card, Splitter, Dashboard, Drawer, Collapse, Space, Container

### Charts (1)
- Chart (Line, Bar, Pie, Area, Scatter, etc.)

### Scheduling (2)
- Schedule, Kanban

### Data Display (14)
- ProgressBar, Avatar, Image, Skeleton, Divider, Timeline, Empty
- Statistic, Transfer, Carousel, Description, QRCode, Barcode, Affix

### Advanced (1)
- Diagram

### Healthcare (3)
- PatientCard, VitalSignsInput, AppointmentScheduler

### Enterprise Business (22)
- DataTable, NotificationCenter, FilterBuilder, AuditViewer, CommentThread
- FileExplorer, TaskManager, ProjectPlanner, TeamCollaboration, WorkflowDesigner
- ReportBuilder, KanbanBoard, GanttChart, Dashboard, Scheduler
- Chat, Inbox, DataImporter, DataExporter, ApprovalCenter
- FormBuilder, UserDirectory, RoleManager

## Property Editor Types

### String Editor
```razor
<div class="prop-editor">
    <label>@PropName</label>
    <input type="text" @bind="@Value" />
</div>
```

### Boolean Editor
```razor
<div class="prop-editor">
    <label>
        <input type="checkbox" @bind="@Value" />
        @PropName
    </label>
</div>
```

### Enum Editor
```razor
<div class="prop-editor">
    <label>@PropName</label>
    <select @bind="@Value">
        @foreach (var option in EnumOptions)
        {
            <option value="@option">@option</option>
        }
    </select>
</div>
```

### Color Editor
```razor
<div class="prop-editor">
    <label>@PropName</label>
    <input type="color" @bind="@Value" />
</div>
```

### Number Editor
```razor
<div class="prop-editor">
    <label>@PropName</label>
    <input type="number" 
           @bind="@Value" 
           min="@Min" 
           max="@Max" 
           step="@Step" />
</div>
```

## Example Component Pages

### N360TextBox Example
```markdown
# N360TextBox

Enterprise text input component with validation, RBAC, and audit support.

## Basic Usage
```razor
<N360TextBox @bind-Value="@userName" 
             Placeholder="Enter your name"
             Label="User Name" />
```

## With Validation
```razor
<N360TextBox @bind-Value="@email"
             Placeholder="email@example.com"
             Label="Email"
             ValidationRules="@emailRules" />

@code {
    private string email = "";
    private ValidationRules emailRules = new ValidationRules()
        .Required("Email is required")
        .Email("Invalid email format");
}
```

## With RBAC
```razor
<N360TextBox @bind-Value="@sensitiveData"
             RequiredPermission="data.edit.sensitive"
             HideIfNoPermission="true" />
```

## Properties

| Name | Type | Default | Description |
|------|------|---------|-------------|
| Value | string | "" | The input value |
| Placeholder | string | null | Placeholder text |
| Label | string | null | Field label |
| Multiline | bool | false | Enable textarea mode |
| Rows | int | 3 | Rows when multiline |
| MaxLength | int? | null | Maximum character count |
| Readonly | bool | false | Disable editing |
| Disabled | bool | false | Disable component |
| ValidationRules | ValidationRules | null | Validation schema |
| RequiredPermission | string | null | RBAC permission |
| EnableAudit | bool | false | Log changes |

## Events

| Name | Type | Description |
|------|------|-------------|
| ValueChanged | EventCallback<string> | Fired when value changes |
| OnFocus | EventCallback | Fired on focus |
| OnBlur | EventCallback | Fired on blur |

## Accessibility
- ✅ ARIA labels
- ✅ Keyboard navigation
- ✅ Screen reader support
- ✅ Focus indicators
- ✅ Error announcements
```

## Search Index Structure
```json
{
  "components": [
    {
      "name": "N360TextBox",
      "category": "Input",
      "description": "Enterprise text input with validation and RBAC",
      "tags": ["input", "text", "form", "validation"],
      "path": "/components/n360textbox",
      "status": "stable",
      "version": "1.0.0"
    }
  ]
}
```

## Hosting

### Development
```bash
cd docs/playground/Nalam360.ComponentPlayground
dotnet run
# Navigate to http://localhost:5000
```

### Production
Deploy to:
- **GitHub Pages**: Static site generation
- **Azure Static Web Apps**: Full Blazor WASM
- **Netlify/Vercel**: Static hosting
- **Self-hosted**: Blazor Server mode

## Automation

### Auto-Generate Component Pages
```powershell
# PowerShell script to generate pages for all components
$components = Get-ChildItem "src/Nalam360Enterprise.UI/Components/**/*.razor" -Recurse

foreach ($component in $components) {
    $name = $component.BaseName
    $template = Get-Content "templates/ComponentPage.template"
    $content = $template -replace "{ComponentName}", $name
    
    $outputPath = "docs/playground/Pages/Components/$name.razor"
    Set-Content -Path $outputPath -Value $content
}
```

### XML Comments to Markdown
```csharp
public class DocumentationGenerator
{
    public void GenerateMarkdownFromXml(string assemblyPath, string outputPath)
    {
        var assembly = Assembly.LoadFrom(assemblyPath);
        var xmlPath = Path.ChangeExtension(assemblyPath, ".xml");
        var xmlDoc = XDocument.Load(xmlPath);
        
        foreach (var type in assembly.GetTypes().Where(t => t.IsComponent()))
        {
            var markdown = GenerateComponentMarkdown(type, xmlDoc);
            File.WriteAllText(Path.Combine(outputPath, $"{type.Name}.md"), markdown);
        }
    }
}
```

## Future Enhancements
- [ ] AI-powered code suggestions
- [ ] Component comparison view
- [ ] Performance benchmarks dashboard
- [ ] Version history and changelogs
- [ ] Component dependency graph
- [ ] Export to Figma/Sketch
- [ ] Collaborative playground (shared URLs)
- [ ] Mobile app for component preview
