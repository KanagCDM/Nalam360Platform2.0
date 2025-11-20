# Nalam360.Platform.Documentation

Auto-documentation generation from XML documentation comments with support for Markdown and YAML output formats.

## Features

- ✅ **XML Documentation Parser**: Extracts documentation from compiled assemblies and their XML doc files
- ✅ **Reflection-Based**: Uses .NET Reflection to analyze types, methods, properties, and parameters
- ✅ **Multiple Output Formats**: Generates Markdown and YAML documentation
- ✅ **Comprehensive Coverage**: Documents classes, interfaces, structs, enums, delegates, and records
- ✅ **Full Member Documentation**: Includes constructors, properties, methods, fields, and parameters
- ✅ **Generic Type Support**: Handles generic types and methods with type parameters
- ✅ **Inheritance Documentation**: Captures base types and implemented interfaces
- ✅ **Example Code Extraction**: Preserves `<example>` and `<remarks>` sections
- ✅ **Dependency Injection**: Built-in DI extensions for easy integration

## Installation

```bash
dotnet add package Nalam360.Platform.Documentation
```

## Quick Start

### Register Services

```csharp
using Nalam360.Platform.Documentation.DependencyInjection;

// In Program.cs or Startup.cs
builder.Services.AddDocumentationGeneration();
```

### Generate Markdown Documentation

```csharp
using System.Reflection;
using Nalam360.Platform.Documentation;

public class DocumentationGenerator
{
    private readonly IDocumentationService _documentationService;

    public DocumentationGenerator(IDocumentationService documentationService)
    {
        _documentationService = documentationService;
    }

    public async Task GenerateDocsAsync()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var xmlDocPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            $"{assembly.GetName().Name}.xml");
        var outputPath = "docs/API.md";

        await _documentationService.GenerateMarkdownAsync(
            assembly,
            xmlDocPath,
            outputPath);
    }
}
```

### Generate YAML Documentation

```csharp
public async Task GenerateYamlDocsAsync()
{
    var assembly = typeof(MyLibrary).Assembly;
    var xmlDocPath = "path/to/MyLibrary.xml";
    var outputPath = "docs/api.yaml";

    await _documentationService.GenerateYamlAsync(
        assembly,
        xmlDocPath,
        outputPath);
}
```

## Manual API Usage

### Using the Parser Directly

```csharp
using Nalam360.Platform.Documentation.Parsers;
using Nalam360.Platform.Documentation.Models;

var parser = new XmlDocumentationParser();
var assembly = Assembly.Load("MyLibrary");
var xmlDocPath = "MyLibrary.xml";

AssemblyDocumentation assemblyDoc = parser.ParseAssembly(assembly, xmlDocPath);

// Access parsed documentation
foreach (var ns in assemblyDoc.Namespaces)
{
    Console.WriteLine($"Namespace: {ns.Name}");
    
    foreach (var type in ns.Types)
    {
        Console.WriteLine($"  Type: {type.Name} ({type.Kind})");
        Console.WriteLine($"  Summary: {type.Summary}");
        
        foreach (var method in type.Methods)
        {
            Console.WriteLine($"    Method: {method.Name}");
            Console.WriteLine($"    Returns: {method.ReturnType}");
        }
    }
}
```

### Using Custom Generators

```csharp
using Nalam360.Platform.Documentation.Generators;

// Markdown Generator
var markdownGenerator = new MarkdownDocumentationGenerator();
string markdownContent = markdownGenerator.Generate(assemblyDoc);
await File.WriteAllTextAsync("API.md", markdownContent);

// YAML Generator
var yamlGenerator = new YamlDocumentationGenerator();
string yamlContent = yamlGenerator.Generate(assemblyDoc);
await File.WriteAllTextAsync("api.yaml", yamlContent);
```

## Documentation Models

### AssemblyDocumentation

```csharp
public class AssemblyDocumentation
{
    public string AssemblyName { get; set; }
    public string Version { get; set; }
    public string? Description { get; set; }
    public List<NamespaceDocumentation> Namespaces { get; set; }
    public DateTime GeneratedAt { get; set; }
}
```

### TypeDocumentation

```csharp
public class TypeDocumentation
{
    public string Name { get; set; }
    public string FullName { get; set; }
    public TypeKind Kind { get; set; }  // Class, Interface, Struct, Enum, Delegate, Record
    public string? Summary { get; set; }
    public string? Remarks { get; set; }
    public string? Example { get; set; }
    public bool IsPublic { get; set; }
    public bool IsSealed { get; set; }
    public bool IsAbstract { get; set; }
    public string? BaseType { get; set; }
    public List<string> Interfaces { get; set; }
    public List<string> GenericParameters { get; set; }
    public List<ConstructorDocumentation> Constructors { get; set; }
    public List<PropertyDocumentation> Properties { get; set; }
    public List<MethodDocumentation> Methods { get; set; }
    public List<FieldDocumentation> Fields { get; set; }
}
```

### MethodDocumentation

```csharp
public class MethodDocumentation
{
    public string Name { get; set; }
    public string ReturnType { get; set; }
    public string? Summary { get; set; }
    public string? Remarks { get; set; }
    public string? Returns { get; set; }
    public bool IsPublic { get; set; }
    public bool IsStatic { get; set; }
    public bool IsAsync { get; set; }
    public List<ParameterDocumentation> Parameters { get; set; }
    public List<string> GenericParameters { get; set; }
}
```

## Markdown Output Example

```markdown
# MyLibrary API Documentation

**Version:** 1.0.0  
**Generated:** 2025-11-18 10:30:00 UTC

---

## Table of Contents

- [MyLibrary.Services](#mylibrary-services)
  - [UserService](#mylibrary-services-userservice)
  - [ProductService](#mylibrary-services-productservice)

---

## MyLibrary.Services

### UserService

```csharp
public class UserService : IUserService
```

**Summary:** Service for managing user operations.

#### Constructors

- `UserService(IUserRepository repository, ILogger<UserService> logger)`
  - Initializes a new instance of the UserService class.

#### Methods

##### `GetUserAsync(Guid id)`

Retrieves a user by their unique identifier.

**Parameters:**
- `id` (Guid): The unique identifier of the user.

**Returns:** A Task containing the user if found; otherwise, null.

---
```

## YAML Output Example

```yaml
# API Documentation (YAML)
assembly: MyLibrary
version: 1.0.0
generated: 2025-11-18T10:30:00Z

namespaces:
  - name: MyLibrary.Services
    types:
      - name: UserService
        fullName: MyLibrary.Services.UserService
        kind: Class
        isPublic: true
        summary: "Service for managing user operations."
        methods:
          - name: GetUserAsync
            returnType: Task<User>
            summary: "Retrieves a user by their unique identifier."
            parameters:
              - name: id
                type: Guid
```

## Advanced Scenarios

### Batch Documentation Generation

```csharp
public async Task GenerateAllAssemblyDocsAsync(string[] assemblyPaths)
{
    var parser = new XmlDocumentationParser();
    var markdownGen = new MarkdownDocumentationGenerator();
    
    foreach (var assemblyPath in assemblyPaths)
    {
        var assembly = Assembly.LoadFrom(assemblyPath);
        var xmlPath = Path.ChangeExtension(assemblyPath, ".xml");
        
        if (!File.Exists(xmlPath))
        {
            Console.WriteLine($"Skipping {assembly.GetName().Name} - XML doc not found");
            continue;
        }
        
        var assemblyDoc = parser.ParseAssembly(assembly, xmlPath);
        var outputPath = $"docs/{assembly.GetName().Name}.md";
        
        await markdownGen.GenerateToFileAsync(assemblyDoc, outputPath);
        Console.WriteLine($"Generated documentation: {outputPath}");
    }
}
```

### Custom Documentation Format

```csharp
public class HtmlDocumentationGenerator : IDocumentationGenerator
{
    public string Generate(AssemblyDocumentation assemblyDoc)
    {
        var html = new StringBuilder();
        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html><head><title>API Documentation</title></head><body>");
        html.AppendLine($"<h1>{assemblyDoc.AssemblyName} v{assemblyDoc.Version}</h1>");
        
        foreach (var ns in assemblyDoc.Namespaces)
        {
            html.AppendLine($"<h2>{ns.Name}</h2>");
            
            foreach (var type in ns.Types)
            {
                html.AppendLine($"<h3>{type.Name}</h3>");
                html.AppendLine($"<p>{type.Summary}</p>");
                
                // Add properties, methods, etc.
            }
        }
        
        html.AppendLine("</body></html>");
        return html.ToString();
    }
    
    public async Task GenerateToFileAsync(AssemblyDocumentation assemblyDoc, 
        string outputPath, CancellationToken ct = default)
    {
        var content = Generate(assemblyDoc);
        await File.WriteAllTextAsync(outputPath, content, ct);
    }
}
```

### Filtering Documentation

```csharp
public AssemblyDocumentation FilterPublicApiOnly(AssemblyDocumentation assemblyDoc)
{
    var filtered = new AssemblyDocumentation
    {
        AssemblyName = assemblyDoc.AssemblyName,
        Version = assemblyDoc.Version,
        Description = assemblyDoc.Description,
        GeneratedAt = assemblyDoc.GeneratedAt
    };
    
    foreach (var ns in assemblyDoc.Namespaces)
    {
        var filteredNs = new NamespaceDocumentation { Name = ns.Name };
        
        // Only include public types
        filteredNs.Types.AddRange(
            ns.Types.Where(t => t.IsPublic && !t.Name.StartsWith("Internal")));
        
        if (filteredNs.Types.Count > 0)
            filtered.Namespaces.Add(filteredNs);
    }
    
    return filtered;
}
```

## Configuration

### Enable XML Documentation Generation

Ensure your project generates XML documentation files:

```xml
<PropertyGroup>
  <GenerateDocumentationFile>true</GenerateDocumentationFile>
  <DocumentationFile>bin\$(Configuration)\$(TargetFramework)\$(AssemblyName).xml</DocumentationFile>
</PropertyGroup>
```

### Suppress Warnings

If you prefer warnings for missing XML comments:

```xml
<PropertyGroup>
  <NoWarn>$(NoWarn);CS1591</NoWarn>  <!-- Missing XML comment -->
</PropertyGroup>
```

## Best Practices

1. **Complete XML Comments**: Provide comprehensive XML documentation for all public APIs
2. **Use Summary Tags**: Always include `<summary>` for types and members
3. **Document Parameters**: Use `<param>` tags for method parameters
4. **Document Returns**: Use `<returns>` for method return values
5. **Add Examples**: Include `<example>` tags with code samples
6. **Include Remarks**: Use `<remarks>` for additional implementation details
7. **Cross-Reference**: Use `<see cref=""/>` for linking to other types

### XML Documentation Example

```csharp
/// <summary>
/// Represents a user in the system.
/// </summary>
/// <remarks>
/// Users are authenticated via OAuth2 and have role-based permissions.
/// </remarks>
public class User : Entity<Guid>
{
    /// <summary>
    /// Gets or sets the user's email address.
    /// </summary>
    /// <example>
    /// <code>
    /// var user = new User { Email = "john@example.com" };
    /// </code>
    /// </example>
    public string Email { get; set; }
    
    /// <summary>
    /// Validates the user's credentials.
    /// </summary>
    /// <param name="password">The password to validate.</param>
    /// <returns>True if the password is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when password is null.</exception>
    public bool ValidatePassword(string password)
    {
        if (password == null)
            throw new ArgumentNullException(nameof(password));
            
        return PasswordHasher.Verify(password, PasswordHash);
    }
}
```

## Integration with CI/CD

### GitHub Actions

```yaml
name: Generate API Documentation

on:
  push:
    branches: [ main ]

jobs:
  generate-docs:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'
      
      - name: Build with XML docs
        run: dotnet build --configuration Release
      
      - name: Generate documentation
        run: dotnet run --project tools/DocGenerator
      
      - name: Commit documentation
        run: |
          git config --local user.email "action@github.com"
          git config --local user.name "GitHub Action"
          git add docs/*.md
          git commit -m "Update API documentation" || echo "No changes"
          git push
```

### Azure DevOps

```yaml
trigger:
  - main

pool:
  vmImage: 'ubuntu-latest'

steps:
- task: UseDotNet@2
  inputs:
    version: '8.0.x'

- script: dotnet build --configuration Release
  displayName: 'Build with XML documentation'

- script: dotnet run --project tools/DocGenerator
  displayName: 'Generate API documentation'

- script: |
    git add docs/*.md
    git commit -m "Update API documentation"
    git push
  displayName: 'Commit documentation'
```

## Performance Considerations

- **Assembly Loading**: Load assemblies once and cache `AssemblyDocumentation` objects
- **Parallel Processing**: Use `Parallel.ForEach` for multiple assemblies
- **Incremental Generation**: Only regenerate docs when XML files change
- **Caching**: Cache parsed XML documentation for frequently accessed assemblies

## Troubleshooting

### XML Documentation File Not Found

```csharp
if (!File.Exists(xmlDocPath))
{
    Console.WriteLine($"Warning: XML documentation not found at {xmlDocPath}");
    Console.WriteLine("Ensure GenerateDocumentationFile is enabled in your .csproj");
    return;
}
```

### Missing Documentation

```csharp
// Check for undocumented members
var undocumentedTypes = assemblyDoc.Namespaces
    .SelectMany(ns => ns.Types)
    .Where(t => string.IsNullOrEmpty(t.Summary))
    .ToList();

if (undocumentedTypes.Any())
{
    Console.WriteLine($"Warning: {undocumentedTypes.Count} types lack documentation");
}
```

## License

Copyright © 2025 Nalam360. All rights reserved.
