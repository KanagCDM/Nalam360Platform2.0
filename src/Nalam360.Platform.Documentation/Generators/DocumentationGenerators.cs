using System.Text;
using Nalam360.Platform.Documentation.Models;

namespace Nalam360.Platform.Documentation.Generators;

/// <summary>
/// Generates documentation in various formats.
/// </summary>
public interface IDocumentationGenerator
{
    /// <summary>
    /// Generates documentation content from assembly documentation.
    /// </summary>
    /// <param name="assemblyDoc">The assembly documentation.</param>
    /// <returns>The generated documentation content.</returns>
    string Generate(AssemblyDocumentation assemblyDoc);

    /// <summary>
    /// Generates documentation content and writes to a file.
    /// </summary>
    /// <param name="assemblyDoc">The assembly documentation.</param>
    /// <param name="outputPath">The output file path.</param>
    /// <param name="ct">Cancellation token.</param>
    Task GenerateToFileAsync(AssemblyDocumentation assemblyDoc, string outputPath, CancellationToken ct = default);
}

/// <summary>
/// Generates documentation in Markdown format.
/// </summary>
public sealed class MarkdownDocumentationGenerator : IDocumentationGenerator
{
    public string Generate(AssemblyDocumentation assemblyDoc)
    {
        var sb = new StringBuilder();

        // Header
        sb.AppendLine($"# {assemblyDoc.AssemblyName} API Documentation");
        sb.AppendLine();
        sb.AppendLine($"**Version:** {assemblyDoc.Version}  ");
        sb.AppendLine($"**Generated:** {assemblyDoc.GeneratedAt:yyyy-MM-dd HH:mm:ss} UTC");
        sb.AppendLine();

        if (!string.IsNullOrEmpty(assemblyDoc.Description))
        {
            sb.AppendLine($"**Description:** {assemblyDoc.Description}");
            sb.AppendLine();
        }

        sb.AppendLine("---");
        sb.AppendLine();

        // Table of Contents
        sb.AppendLine("## Table of Contents");
        sb.AppendLine();
        foreach (var ns in assemblyDoc.Namespaces.OrderBy(n => n.Name))
        {
            sb.AppendLine($"- [{ns.Name}](#{ns.Name.Replace('.', '-').ToLower()})");
            foreach (var type in ns.Types.OrderBy(t => t.Name))
            {
                sb.AppendLine($"  - [{type.Name}](#{type.FullName.Replace('.', '-').ToLower()})");
            }
        }
        sb.AppendLine();
        sb.AppendLine("---");
        sb.AppendLine();

        // Namespaces
        foreach (var ns in assemblyDoc.Namespaces.OrderBy(n => n.Name))
        {
            sb.AppendLine($"## {ns.Name}");
            sb.AppendLine();

            foreach (var type in ns.Types.OrderBy(t => t.Name))
            {
                GenerateTypeDocumentation(sb, type);
            }
        }

        return sb.ToString();
    }

    public async Task GenerateToFileAsync(AssemblyDocumentation assemblyDoc, string outputPath, CancellationToken ct = default)
    {
        var content = Generate(assemblyDoc);
        var directory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        await File.WriteAllTextAsync(outputPath, content, ct);
    }

    private void GenerateTypeDocumentation(StringBuilder sb, TypeDocumentation type)
    {
        // Type header
        sb.AppendLine($"### {type.Name}");
        sb.AppendLine();

        // Type signature
        var signature = new StringBuilder();
        if (type.IsPublic) signature.Append("public ");
        if (type.IsAbstract && !type.Kind.Equals(TypeKind.Interface)) signature.Append("abstract ");
        if (type.IsSealed && type.Kind == TypeKind.Class) signature.Append("sealed ");

        signature.Append(type.Kind.ToString().ToLower());
        signature.Append($" {type.Name}");

        if (type.GenericParameters.Count > 0)
            signature.Append($"<{string.Join(", ", type.GenericParameters)}>");

        if (!string.IsNullOrEmpty(type.BaseType))
            signature.Append($" : {type.BaseType}");

        if (type.Interfaces.Count > 0)
        {
            var prefix = string.IsNullOrEmpty(type.BaseType) ? " : " : ", ";
            signature.Append($"{prefix}{string.Join(", ", type.Interfaces)}");
        }

        sb.AppendLine("```csharp");
        sb.AppendLine(signature.ToString());
        sb.AppendLine("```");
        sb.AppendLine();

        // Summary
        if (!string.IsNullOrEmpty(type.Summary))
        {
            sb.AppendLine($"**Summary:** {type.Summary}");
            sb.AppendLine();
        }

        // Remarks
        if (!string.IsNullOrEmpty(type.Remarks))
        {
            sb.AppendLine($"**Remarks:** {type.Remarks}");
            sb.AppendLine();
        }

        // Example
        if (!string.IsNullOrEmpty(type.Example))
        {
            sb.AppendLine("**Example:**");
            sb.AppendLine("```csharp");
            sb.AppendLine(type.Example);
            sb.AppendLine("```");
            sb.AppendLine();
        }

        // Constructors
        if (type.Constructors.Count > 0)
        {
            sb.AppendLine("#### Constructors");
            sb.AppendLine();
            foreach (var ctor in type.Constructors)
            {
                sb.Append($"- `{type.Name}(");
                sb.Append(string.Join(", ", ctor.Parameters.Select(p => $"{p.Type} {p.Name}")));
                sb.AppendLine(")`");
                if (!string.IsNullOrEmpty(ctor.Summary))
                    sb.AppendLine($"  - {ctor.Summary}");
            }
            sb.AppendLine();
        }

        // Properties
        if (type.Properties.Count > 0)
        {
            sb.AppendLine("#### Properties");
            sb.AppendLine();
            sb.AppendLine("| Name | Type | Summary |");
            sb.AppendLine("|------|------|---------|");
            foreach (var prop in type.Properties.OrderBy(p => p.Name))
            {
                sb.AppendLine($"| `{prop.Name}` | `{prop.Type}` | {prop.Summary ?? "-"} |");
            }
            sb.AppendLine();
        }

        // Methods
        if (type.Methods.Count > 0)
        {
            sb.AppendLine("#### Methods");
            sb.AppendLine();
            foreach (var method in type.Methods.OrderBy(m => m.Name))
            {
                sb.Append($"##### `{method.Name}");
                if (method.GenericParameters.Count > 0)
                    sb.Append($"<{string.Join(", ", method.GenericParameters)}>");
                sb.Append("(");
                sb.Append(string.Join(", ", method.Parameters.Select(p => $"{p.Type} {p.Name}")));
                sb.AppendLine(")`");
                sb.AppendLine();

                if (!string.IsNullOrEmpty(method.Summary))
                {
                    sb.AppendLine(method.Summary);
                    sb.AppendLine();
                }

                if (method.Parameters.Count > 0)
                {
                    sb.AppendLine("**Parameters:**");
                    foreach (var param in method.Parameters)
                    {
                        sb.AppendLine($"- `{param.Name}` ({param.Type}): {param.Description ?? "-"}");
                    }
                    sb.AppendLine();
                }

                if (!string.IsNullOrEmpty(method.Returns))
                {
                    sb.AppendLine($"**Returns:** {method.Returns}");
                    sb.AppendLine();
                }

                if (!string.IsNullOrEmpty(method.Remarks))
                {
                    sb.AppendLine($"**Remarks:** {method.Remarks}");
                    sb.AppendLine();
                }
            }
        }

        // Fields (for enums)
        if (type.Fields.Count > 0)
        {
            sb.AppendLine("#### Values");
            sb.AppendLine();
            sb.AppendLine("| Name | Value | Summary |");
            sb.AppendLine("|------|-------|---------|");
            foreach (var field in type.Fields)
            {
                sb.AppendLine($"| `{field.Name}` | `{field.Value}` | {field.Summary ?? "-"} |");
            }
            sb.AppendLine();
        }

        sb.AppendLine("---");
        sb.AppendLine();
    }
}

/// <summary>
/// Generates documentation in YAML format.
/// </summary>
public sealed class YamlDocumentationGenerator : IDocumentationGenerator
{
    public string Generate(AssemblyDocumentation assemblyDoc)
    {
        var sb = new StringBuilder();

        sb.AppendLine("# API Documentation (YAML)");
        sb.AppendLine($"assembly: {assemblyDoc.AssemblyName}");
        sb.AppendLine($"version: {assemblyDoc.Version}");
        sb.AppendLine($"generated: {assemblyDoc.GeneratedAt:yyyy-MM-dd'T'HH:mm:ss'Z'}");
        
        if (!string.IsNullOrEmpty(assemblyDoc.Description))
            sb.AppendLine($"description: {assemblyDoc.Description}");
        
        sb.AppendLine();
        sb.AppendLine("namespaces:");

        foreach (var ns in assemblyDoc.Namespaces.OrderBy(n => n.Name))
        {
            sb.AppendLine($"  - name: {ns.Name}");
            sb.AppendLine("    types:");

            foreach (var type in ns.Types.OrderBy(t => t.Name))
            {
                GenerateTypeYaml(sb, type, "      ");
            }
        }

        return sb.ToString();
    }

    public async Task GenerateToFileAsync(AssemblyDocumentation assemblyDoc, string outputPath, CancellationToken ct = default)
    {
        var content = Generate(assemblyDoc);
        var directory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        await File.WriteAllTextAsync(outputPath, content, ct);
    }

    private void GenerateTypeYaml(StringBuilder sb, TypeDocumentation type, string indent)
    {
        sb.AppendLine($"{indent}- name: {type.Name}");
        sb.AppendLine($"{indent}  fullName: {type.FullName}");
        sb.AppendLine($"{indent}  kind: {type.Kind}");
        sb.AppendLine($"{indent}  isPublic: {type.IsPublic.ToString().ToLower()}");
        
        if (!string.IsNullOrEmpty(type.Summary))
            sb.AppendLine($"{indent}  summary: \"{EscapeYaml(type.Summary)}\"");

        if (!string.IsNullOrEmpty(type.BaseType))
            sb.AppendLine($"{indent}  baseType: {type.BaseType}");

        if (type.Interfaces.Count > 0)
        {
            sb.AppendLine($"{indent}  interfaces:");
            foreach (var iface in type.Interfaces)
                sb.AppendLine($"{indent}    - {iface}");
        }

        if (type.Properties.Count > 0)
        {
            sb.AppendLine($"{indent}  properties:");
            foreach (var prop in type.Properties)
            {
                sb.AppendLine($"{indent}    - name: {prop.Name}");
                sb.AppendLine($"{indent}      type: {prop.Type}");
                if (!string.IsNullOrEmpty(prop.Summary))
                    sb.AppendLine($"{indent}      summary: \"{EscapeYaml(prop.Summary)}\"");
            }
        }

        if (type.Methods.Count > 0)
        {
            sb.AppendLine($"{indent}  methods:");
            foreach (var method in type.Methods)
            {
                sb.AppendLine($"{indent}    - name: {method.Name}");
                sb.AppendLine($"{indent}      returnType: {method.ReturnType}");
                if (!string.IsNullOrEmpty(method.Summary))
                    sb.AppendLine($"{indent}      summary: \"{EscapeYaml(method.Summary)}\"");
                
                if (method.Parameters.Count > 0)
                {
                    sb.AppendLine($"{indent}      parameters:");
                    foreach (var param in method.Parameters)
                    {
                        sb.AppendLine($"{indent}        - name: {param.Name}");
                        sb.AppendLine($"{indent}          type: {param.Type}");
                    }
                }
            }
        }
    }

    private static string EscapeYaml(string text)
    {
        return text.Replace("\"", "\\\"").Replace("\n", " ").Replace("\r", "");
    }
}
