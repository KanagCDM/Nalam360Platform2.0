# API Documentation Generation System

## Overview
Automated API documentation generation for all 112 Nalam360 Enterprise UI components using XML comments, reflection, and markdown generation.

## Architecture

```
docs/
├── api/
│   ├── components/          # Generated component docs
│   │   ├── N360TextBox.md
│   │   ├── N360Button.md
│   │   └── ...
│   ├── core/                # Core service docs
│   │   ├── ThemeService.md
│   │   ├── PermissionService.md
│   │   └── ValidationRules.md
│   └── index.md             # API reference index
├── tools/
│   └── DocGenerator/
│       ├── DocGenerator.csproj
│       ├── Program.cs
│       ├── Generators/
│       │   ├── ComponentDocGenerator.cs
│       │   ├── ServiceDocGenerator.cs
│       │   └── MarkdownWriter.cs
│       └── Templates/
│           ├── ComponentTemplate.md
│           └── ServiceTemplate.md
└── README.md
```

## Implementation

### 1. Documentation Generator Tool

```xml
<!-- docs/tools/DocGenerator/DocGenerator.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="4.12.0" />
    <PackageReference Include="Microsoft.CodeAnalysis.Workspaces.MSBuild" Version="4.12.0" />
    <PackageReference Include="System.Reflection.Metadata" Version="9.0.1" />
    <PackageReference Include="Markdig" Version="0.38.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\..\src\Nalam360Enterprise.UI\Nalam360Enterprise.UI\Nalam360Enterprise.UI.csproj" />
  </ItemGroup>
</Project>
```

### 2. Main Program
```csharp
// docs/tools/DocGenerator/Program.cs
using System.Reflection;
using System.Xml.Linq;

namespace Nalam360.DocGenerator;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Nalam360 Enterprise UI - Documentation Generator");
        Console.WriteLine("================================================\n");

        var assemblyPath = args.Length > 0 
            ? args[0] 
            : "../../../src/Nalam360Enterprise.UI/Nalam360Enterprise.UI/bin/Release/net9.0/Nalam360Enterprise.UI.dll";
        
        var outputPath = args.Length > 1
            ? args[1]
            : "../../api";

        if (!File.Exists(assemblyPath))
        {
            Console.WriteLine($"Error: Assembly not found at {assemblyPath}");
            Console.WriteLine("Please build the project first: dotnet build -c Release");
            return;
        }

        var assembly = Assembly.LoadFrom(assemblyPath);
        var xmlPath = Path.ChangeExtension(assemblyPath, ".xml");
        var xmlDoc = File.Exists(xmlPath) ? XDocument.Load(xmlPath) : null;

        if (xmlDoc == null)
        {
            Console.WriteLine("Warning: XML documentation not found. Enable GenerateDocumentationFile in csproj.");
        }

        var generator = new DocumentationGenerator(assembly, xmlDoc, outputPath);
        await generator.GenerateAllAsync();

        Console.WriteLine("\n✅ Documentation generation complete!");
        Console.WriteLine($"📁 Output: {Path.GetFullPath(outputPath)}");
    }
}
```

### 3. Documentation Generator
```csharp
// docs/tools/DocGenerator/Generators/ComponentDocGenerator.cs
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace Nalam360.DocGenerator.Generators;

public class DocumentationGenerator
{
    private readonly Assembly _assembly;
    private readonly XDocument? _xmlDoc;
    private readonly string _outputPath;

    public DocumentationGenerator(Assembly assembly, XDocument? xmlDoc, string outputPath)
    {
        _assembly = assembly;
        _xmlDoc = xmlDoc;
        _outputPath = outputPath;
    }

    public async Task GenerateAllAsync()
    {
        Console.WriteLine("Scanning assembly for components...");
        
        var components = _assembly.GetTypes()
            .Where(t => t.Namespace?.StartsWith("Nalam360Enterprise.UI.Components") == true)
            .Where(t => t.Name.StartsWith("N360"))
            .OrderBy(t => t.Name)
            .ToList();

        Console.WriteLine($"Found {components.Count} components\n");

        // Create output directories
        Directory.CreateDirectory(Path.Combine(_outputPath, "components"));
        Directory.CreateDirectory(Path.Combine(_outputPath, "core"));

        // Generate component documentation
        foreach (var component in components)
        {
            Console.Write($"Generating {component.Name}... ");
            var markdown = GenerateComponentMarkdown(component);
            var filePath = Path.Combine(_outputPath, "components", $"{component.Name}.md");
            await File.WriteAllTextAsync(filePath, markdown);
            Console.WriteLine("✓");
        }

        // Generate core services documentation
        var services = new[]
        {
            _assembly.GetType("Nalam360Enterprise.UI.Core.Theming.ThemeService"),
            _assembly.GetType("Nalam360Enterprise.UI.Core.Security.IPermissionService"),
            _assembly.GetType("Nalam360Enterprise.UI.Core.Security.IAuditService"),
            _assembly.GetType("Nalam360Enterprise.UI.Core.Forms.ValidationRules")
        };

        foreach (var service in services.Where(s => s != null))
        {
            Console.Write($"Generating {service!.Name}... ");
            var markdown = GenerateServiceMarkdown(service);
            var filePath = Path.Combine(_outputPath, "core", $"{service.Name}.md");
            await File.WriteAllTextAsync(filePath, markdown);
            Console.WriteLine("✓");
        }

        // Generate index
        Console.Write("Generating index... ");
        var indexMarkdown = GenerateIndexMarkdown(components);
        await File.WriteAllTextAsync(Path.Combine(_outputPath, "index.md"), indexMarkdown);
        Console.WriteLine("✓");
    }

    private string GenerateComponentMarkdown(Type component)
    {
        var sb = new StringBuilder();
        var xmlSummary = GetXmlSummary(component);

        // Header
        sb.AppendLine($"# {component.Name}");
        sb.AppendLine();
        sb.AppendLine($"**Namespace:** `{component.Namespace}`  ");
        sb.AppendLine($"**Assembly:** {_assembly.GetName().Name}");
        sb.AppendLine();

        // Summary
        if (!string.IsNullOrEmpty(xmlSummary))
        {
            sb.AppendLine("## Summary");
            sb.AppendLine(xmlSummary);
            sb.AppendLine();
        }

        // Usage Example
        sb.AppendLine("## Basic Usage");
        sb.AppendLine("```razor");
        sb.AppendLine($"<{component.Name}");
        
        var props = GetPublicProperties(component);
        if (props.Any())
        {
            var firstProp = props.First();
            sb.AppendLine($"    {firstProp.Name}=\"value\"");
        }
        
        sb.AppendLine($"    />");
        sb.AppendLine("```");
        sb.AppendLine();

        // Properties
        if (props.Any())
        {
            sb.AppendLine("## Properties");
            sb.AppendLine();
            sb.AppendLine("| Name | Type | Default | Description |");
            sb.AppendLine("|------|------|---------|-------------|");

            foreach (var prop in props)
            {
                var propType = GetFriendlyTypeName(prop.PropertyType);
                var defaultValue = GetDefaultValue(prop);
                var description = GetXmlSummary(prop) ?? "";
                
                sb.AppendLine($"| `{prop.Name}` | `{propType}` | `{defaultValue}` | {description} |");
            }
            sb.AppendLine();
        }

        // Events
        var events = GetEventCallbacks(component);
        if (events.Any())
        {
            sb.AppendLine("## Events");
            sb.AppendLine();
            sb.AppendLine("| Name | Type | Description |");
            sb.AppendLine("|------|------|-------------|");

            foreach (var evt in events)
            {
                var eventType = GetFriendlyTypeName(evt.PropertyType);
                var description = GetXmlSummary(evt) ?? "";
                
                sb.AppendLine($"| `{evt.Name}` | `{eventType}` | {description} |");
            }
            sb.AppendLine();
        }

        // Methods
        var methods = GetPublicMethods(component);
        if (methods.Any())
        {
            sb.AppendLine("## Methods");
            sb.AppendLine();

            foreach (var method in methods)
            {
                var returnType = GetFriendlyTypeName(method.ReturnType);
                var parameters = string.Join(", ", method.GetParameters()
                    .Select(p => $"{GetFriendlyTypeName(p.ParameterType)} {p.Name}"));
                var description = GetXmlSummary(method) ?? "";

                sb.AppendLine($"### `{method.Name}`");
                sb.AppendLine();
                sb.AppendLine($"```csharp");
                sb.AppendLine($"{returnType} {method.Name}({parameters})");
                sb.AppendLine($"```");
                sb.AppendLine();
                if (!string.IsNullOrEmpty(description))
                {
                    sb.AppendLine(description);
                    sb.AppendLine();
                }
            }
        }

        // Examples
        sb.AppendLine("## Examples");
        sb.AppendLine();
        sb.AppendLine("### Example 1: Basic Setup");
        sb.AppendLine("```razor");
        sb.AppendLine($"<{component.Name}");
        sb.AppendLine($"    Value=\"@myValue\"");
        sb.AppendLine($"    ValueChanged=\"@OnValueChanged\" />");
        sb.AppendLine("```");
        sb.AppendLine();

        if (props.Any(p => p.Name == "RequiredPermission"))
        {
            sb.AppendLine("### Example 2: With RBAC");
            sb.AppendLine("```razor");
            sb.AppendLine($"<{component.Name}");
            sb.AppendLine($"    Value=\"@myValue\"");
            sb.AppendLine($"    RequiredPermission=\"data.edit\"");
            sb.AppendLine($"    HideIfNoPermission=\"true\" />");
            sb.AppendLine("```");
            sb.AppendLine();
        }

        if (props.Any(p => p.Name == "ValidationRules"))
        {
            sb.AppendLine("### Example 3: With Validation");
            sb.AppendLine("```razor");
            sb.AppendLine($"<{component.Name}");
            sb.AppendLine($"    @bind-Value=\"@email\"");
            sb.AppendLine($"    ValidationRules=\"@emailRules\" />");
            sb.AppendLine();
            sb.AppendLine("@code {");
            sb.AppendLine("    private string email = \"\";");
            sb.AppendLine("    private ValidationRules emailRules = new ValidationRules()");
            sb.AppendLine("        .Required(\"Email is required\")");
            sb.AppendLine("        .Email(\"Invalid email format\");");
            sb.AppendLine("}");
            sb.AppendLine("```");
            sb.AppendLine();
        }

        // Related Components
        sb.AppendLine("## See Also");
        sb.AppendLine($"- [Component Inventory](../../COMPONENT_INVENTORY.md)");
        sb.AppendLine($"- [Quick Reference](../../QUICK_REFERENCE.md)");
        sb.AppendLine();

        return sb.ToString();
    }

    private string GenerateServiceMarkdown(Type service)
    {
        var sb = new StringBuilder();
        var xmlSummary = GetXmlSummary(service);

        sb.AppendLine($"# {service.Name}");
        sb.AppendLine();
        sb.AppendLine($"**Namespace:** `{service.Namespace}`  ");
        sb.AppendLine($"**Type:** {(service.IsInterface ? "Interface" : "Class")}");
        sb.AppendLine();

        if (!string.IsNullOrEmpty(xmlSummary))
        {
            sb.AppendLine("## Summary");
            sb.AppendLine(xmlSummary);
            sb.AppendLine();
        }

        // Methods
        var methods = service.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        if (methods.Any())
        {
            sb.AppendLine("## Methods");
            sb.AppendLine();

            foreach (var method in methods)
            {
                var returnType = GetFriendlyTypeName(method.ReturnType);
                var parameters = string.Join(", ", method.GetParameters()
                    .Select(p => $"{GetFriendlyTypeName(p.ParameterType)} {p.Name}"));
                var description = GetXmlSummary(method) ?? "";

                sb.AppendLine($"### `{method.Name}`");
                sb.AppendLine();
                sb.AppendLine($"```csharp");
                sb.AppendLine($"{returnType} {method.Name}({parameters})");
                sb.AppendLine($"```");
                sb.AppendLine();
                if (!string.IsNullOrEmpty(description))
                {
                    sb.AppendLine(description);
                    sb.AppendLine();
                }
            }
        }

        // Properties
        var props = service.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        if (props.Any())
        {
            sb.AppendLine("## Properties");
            sb.AppendLine();
            sb.AppendLine("| Name | Type | Description |");
            sb.AppendLine("|------|------|-------------|");

            foreach (var prop in props)
            {
                var propType = GetFriendlyTypeName(prop.PropertyType);
                var description = GetXmlSummary(prop) ?? "";
                
                sb.AppendLine($"| `{prop.Name}` | `{propType}` | {description} |");
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private string GenerateIndexMarkdown(List<Type> components)
    {
        var sb = new StringBuilder();

        sb.AppendLine("# API Reference");
        sb.AppendLine();
        sb.AppendLine("Complete API documentation for Nalam360 Enterprise UI component library.");
        sb.AppendLine();

        // Group by category
        var categories = new Dictionary<string, List<Type>>
        {
            ["Input"] = new(),
            ["Data"] = new(),
            ["Navigation"] = new(),
            ["Button"] = new(),
            ["Feedback"] = new(),
            ["Layout"] = new(),
            ["Chart"] = new(),
            ["Scheduling"] = new(),
            ["Display"] = new(),
            ["Advanced"] = new(),
            ["Healthcare"] = new(),
            ["Enterprise"] = new()
        };

        foreach (var component in components)
        {
            var category = DetermineCategory(component.Name);
            if (categories.ContainsKey(category))
            {
                categories[category].Add(component);
            }
        }

        foreach (var (category, items) in categories.Where(c => c.Value.Any()))
        {
            sb.AppendLine($"## {category} Components");
            sb.AppendLine();

            foreach (var component in items.OrderBy(c => c.Name))
            {
                var summary = GetXmlSummary(component) ?? "";
                var firstLine = summary.Split('\n').FirstOrDefault() ?? "";
                sb.AppendLine($"- [`{component.Name}`](components/{component.Name}.md) - {firstLine}");
            }
            sb.AppendLine();
        }

        sb.AppendLine("## Core Services");
        sb.AppendLine();
        sb.AppendLine("- [`ThemeService`](core/ThemeService.md) - Theme management and switching");
        sb.AppendLine("- [`IPermissionService`](core/IPermissionService.md) - RBAC permission checking");
        sb.AppendLine("- [`IAuditService`](core/IAuditService.md) - Audit logging service");
        sb.AppendLine("- [`ValidationRules`](core/ValidationRules.md) - Form validation API");
        sb.AppendLine();

        return sb.ToString();
    }

    private string? GetXmlSummary(MemberInfo member)
    {
        if (_xmlDoc == null) return null;

        var memberName = $"T:{member.DeclaringType?.FullName}.{member.Name}";
        if (member is Type type)
        {
            memberName = $"T:{type.FullName}";
        }
        else if (member is MethodInfo method)
        {
            memberName = $"M:{method.DeclaringType?.FullName}.{method.Name}";
        }
        else if (member is PropertyInfo property)
        {
            memberName = $"P:{property.DeclaringType?.FullName}.{property.Name}";
        }

        var element = _xmlDoc.Descendants("member")
            .FirstOrDefault(e => e.Attribute("name")?.Value == memberName);

        return element?.Element("summary")?.Value.Trim();
    }

    private List<PropertyInfo> GetPublicProperties(Type type)
    {
        return type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(p => !p.PropertyType.Name.StartsWith("EventCallback"))
            .OrderBy(p => p.Name)
            .ToList();
    }

    private List<PropertyInfo> GetEventCallbacks(Type type)
    {
        return type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(p => p.PropertyType.Name.StartsWith("EventCallback"))
            .OrderBy(p => p.Name)
            .ToList();
    }

    private List<MethodInfo> GetPublicMethods(Type type)
    {
        return type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(m => !m.IsSpecialName) // Exclude property getters/setters
            .OrderBy(m => m.Name)
            .ToList();
    }

    private string GetFriendlyTypeName(Type type)
    {
        if (type == typeof(string)) return "string";
        if (type == typeof(int)) return "int";
        if (type == typeof(bool)) return "bool";
        if (type == typeof(double)) return "double";
        if (type == typeof(DateTime)) return "DateTime";
        if (type == typeof(void)) return "void";

        if (type.IsGenericType)
        {
            var genericType = type.GetGenericTypeDefinition();
            var genericArgs = string.Join(", ", type.GetGenericArguments().Select(GetFriendlyTypeName));
            return $"{genericType.Name.Split('`')[0]}<{genericArgs}>";
        }

        return type.Name;
    }

    private string GetDefaultValue(PropertyInfo property)
    {
        var defaultAttr = property.GetCustomAttribute<System.ComponentModel.DefaultValueAttribute>();
        if (defaultAttr != null)
        {
            return defaultAttr.Value?.ToString() ?? "null";
        }

        if (property.PropertyType.IsValueType)
        {
            return "default";
        }

        return "null";
    }

    private string DetermineCategory(string componentName)
    {
        if (componentName.Contains("TextBox") || componentName.Contains("Input") || 
            componentName.Contains("Picker") || componentName.Contains("Select") ||
            componentName.Contains("CheckBox") || componentName.Contains("Radio") ||
            componentName.Contains("Switch") || componentName.Contains("Slider") ||
            componentName.Contains("Upload") || componentName.Contains("Rating"))
            return "Input";

        if (componentName.Contains("Grid") || componentName.Contains("List") || 
            componentName.Contains("Pivot"))
            return "Data";

        if (componentName.Contains("Nav") || componentName.Contains("Menu") || 
            componentName.Contains("Bread") || componentName.Contains("Tab") ||
            componentName.Contains("Accordion") || componentName.Contains("Tree") ||
            componentName.Contains("Toolbar") || componentName.Contains("Sidebar"))
            return "Navigation";

        if (componentName.Contains("Button") || componentName.Contains("Chip"))
            return "Button";

        if (componentName.Contains("Toast") || componentName.Contains("Spinner") ||
            componentName.Contains("Tooltip") || componentName.Contains("Badge") ||
            componentName.Contains("Alert") || componentName.Contains("Message"))
            return "Feedback";

        if (componentName.Contains("Dialog") || componentName.Contains("Card") ||
            componentName.Contains("Splitter") || componentName.Contains("Dashboard") ||
            componentName.Contains("Drawer") || componentName.Contains("Container"))
            return "Layout";

        if (componentName.Contains("Chart"))
            return "Chart";

        if (componentName.Contains("Schedule") || componentName.Contains("Kanban") ||
            componentName.Contains("Calendar"))
            return "Scheduling";

        if (componentName.Contains("Patient") || componentName.Contains("Vital") ||
            componentName.Contains("Appointment"))
            return "Healthcare";

        if (componentName.Contains("DataTable") || componentName.Contains("Notification") ||
            componentName.Contains("Filter") || componentName.Contains("Audit") ||
            componentName.Contains("Comment") || componentName.Contains("File") ||
            componentName.Contains("Task") || componentName.Contains("Project") ||
            componentName.Contains("Team") || componentName.Contains("Workflow") ||
            componentName.Contains("Report") || componentName.Contains("Gantt") ||
            componentName.Contains("Chat") || componentName.Contains("Inbox") ||
            componentName.Contains("Importer") || componentName.Contains("Exporter") ||
            componentName.Contains("Approval") || componentName.Contains("Form") ||
            componentName.Contains("User") || componentName.Contains("Role"))
            return "Enterprise";

        return "Display";
    }
}
```

## Usage

### Build and Run
```bash
# 1. Build the UI library with XML docs
cd src/Nalam360Enterprise.UI/Nalam360Enterprise.UI
dotnet build -c Release /p:GenerateDocumentationFile=true

# 2. Build the doc generator
cd ../../../docs/tools/DocGenerator
dotnet build

# 3. Run the generator
dotnet run
```

### PowerShell Script
```powershell
# generate-docs.ps1
Write-Host "Building Nalam360 Enterprise UI..." -ForegroundColor Cyan
dotnet build src/Nalam360Enterprise.UI/Nalam360Enterprise.UI/Nalam360Enterprise.UI.csproj `
    -c Release `
    /p:GenerateDocumentationFile=true

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}

Write-Host "`nGenerating API documentation..." -ForegroundColor Cyan
dotnet run --project docs/tools/DocGenerator/DocGenerator.csproj

Write-Host "`n✅ Documentation generated successfully!" -ForegroundColor Green
Write-Host "📁 Location: docs/api/" -ForegroundColor Yellow
```

### CI/CD Integration
```yaml
# .github/workflows/docs.yml
name: Generate Documentation

on:
  push:
    branches: [ main ]
    paths:
      - 'src/Nalam360Enterprise.UI/**'
  workflow_dispatch:

jobs:
  generate-docs:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: 9.0.x
    
    - name: Build UI library
      run: |
        dotnet build src/Nalam360Enterprise.UI/Nalam360Enterprise.UI/Nalam360Enterprise.UI.csproj \
          -c Release \
          /p:GenerateDocumentationFile=true
    
    - name: Generate API docs
      run: |
        dotnet run --project docs/tools/DocGenerator/DocGenerator.csproj
    
    - name: Commit docs
      run: |
        git config user.name "GitHub Actions"
        git config user.email "actions@github.com"
        git add docs/api/
        git diff --quiet && git diff --staged --quiet || git commit -m "docs: update API documentation [skip ci]"
        git push
```

## Output Examples

### Component Page Example (N360TextBox.md)
```markdown
# N360TextBox

**Namespace:** `Nalam360Enterprise.UI.Components.Inputs`  
**Assembly:** Nalam360Enterprise.UI

## Summary
Enterprise text input component with validation, RBAC, and audit support.

## Basic Usage
```razor
<N360TextBox
    Value="value"
    />
```

## Properties

| Name | Type | Default | Description |
|------|------|---------|-------------|
| `Value` | `string` | `null` | The current value of the text box |
| `Placeholder` | `string` | `null` | Placeholder text |
| `Label` | `string` | `null` | Field label |
| `Multiline` | `bool` | `false` | Enable multi-line mode |
| `ValidationRules` | `ValidationRules` | `null` | Validation schema |

## Events

| Name | Type | Description |
|------|------|-------------|
| `ValueChanged` | `EventCallback<string>` | Fired when value changes |
| `OnFocus` | `EventCallback` | Fired when input receives focus |

## Methods

### `FocusAsync`

```csharp
Task FocusAsync()
```

Sets focus to the input element.

## Examples

### Example 1: Basic Setup
```razor
<N360TextBox
    Value="@myValue"
    ValueChanged="@OnValueChanged" />
```

### Example 2: With RBAC
```razor
<N360TextBox
    Value="@myValue"
    RequiredPermission="data.edit"
    HideIfNoPermission="true" />
```

### Example 3: With Validation
```razor
<N360TextBox
    @bind-Value="@email"
    ValidationRules="@emailRules" />

@code {
    private string email = "";
    private ValidationRules emailRules = new ValidationRules()
        .Required("Email is required")
        .Email("Invalid email format");
}
```

## See Also
- [Component Inventory](../../COMPONENT_INVENTORY.md)
- [Quick Reference](../../QUICK_REFERENCE.md)
```

### Index Page Example
```markdown
# API Reference

Complete API documentation for Nalam360 Enterprise UI component library.

## Input Components

- [`N360TextBox`](components/N360TextBox.md) - Enterprise text input with validation
- [`N360NumericTextBox`](components/N360NumericTextBox.md) - Numeric input
- [`N360DatePicker`](components/N360DatePicker.md) - Date selection
...

## Data Components

- [`N360Grid`](components/N360Grid.md) - Advanced data grid
- [`N360TreeGrid`](components/N360TreeGrid.md) - Hierarchical grid
...

## Core Services

- [`ThemeService`](core/ThemeService.md) - Theme management
- [`IPermissionService`](core/IPermissionService.md) - RBAC
- [`IAuditService`](core/IAuditService.md) - Audit logging
- [`ValidationRules`](core/ValidationRules.md) - Validation API
```

## Features

✅ **Automatic XML Comment Extraction**  
✅ **Property Documentation with Types**  
✅ **Event Documentation**  
✅ **Method Signatures and Descriptions**  
✅ **Usage Examples**  
✅ **Category Grouping**  
✅ **Cross-References**  
✅ **Markdown Output**  
✅ **CI/CD Integration**

## Future Enhancements
- [ ] Generate interactive HTML docs
- [ ] Add search functionality
- [ ] Include code samples from test files
- [ ] Generate OpenAPI/Swagger docs for services
- [ ] Create PDF documentation
- [ ] Add version history tracking
- [ ] Generate TypeScript definitions
- [ ] Create VS Code snippets from examples
