using System.Reflection;
using Nalam360.Platform.Documentation.Generators;
using Nalam360.Platform.Documentation.Parsers;

namespace Nalam360.Platform.Documentation;

/// <summary>
/// Facade service for generating documentation from assemblies.
/// </summary>
public interface IDocumentationService
{
    /// <summary>
    /// Generates Markdown documentation for an assembly.
    /// </summary>
    /// <param name="assembly">The assembly to document.</param>
    /// <param name="xmlDocPath">Path to the XML documentation file.</param>
    /// <param name="outputPath">Path to write the Markdown file.</param>
    /// <param name="ct">Cancellation token.</param>
    Task GenerateMarkdownAsync(Assembly assembly, string xmlDocPath, string outputPath, CancellationToken ct = default);

    /// <summary>
    /// Generates YAML documentation for an assembly.
    /// </summary>
    /// <param name="assembly">The assembly to document.</param>
    /// <param name="xmlDocPath">Path to the XML documentation file.</param>
    /// <param name="outputPath">Path to write the YAML file.</param>
    /// <param name="ct">Cancellation token.</param>
    Task GenerateYamlAsync(Assembly assembly, string xmlDocPath, string outputPath, CancellationToken ct = default);
}

/// <summary>
/// Default implementation of documentation service.
/// </summary>
public sealed class DocumentationService : IDocumentationService
{
    private readonly IDocumentationParser _parser;
    private readonly MarkdownDocumentationGenerator _markdownGenerator;
    private readonly YamlDocumentationGenerator _yamlGenerator;

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentationService"/> class.
    /// </summary>
    /// <param name="parser">The documentation parser.</param>
    public DocumentationService(IDocumentationParser parser)
    {
        _parser = parser;
        _markdownGenerator = new MarkdownDocumentationGenerator();
        _yamlGenerator = new YamlDocumentationGenerator();
    }

    /// <summary>
    /// Generates Markdown documentation for an assembly.
    /// </summary>
    public async Task GenerateMarkdownAsync(Assembly assembly, string xmlDocPath, string outputPath, CancellationToken ct = default)
    {
        var assemblyDoc = await _parser.ParseAssemblyAsync(assembly, xmlDocPath, ct);
        await _markdownGenerator.GenerateToFileAsync(assemblyDoc, outputPath, ct);
    }

    /// <summary>
    /// Generates YAML documentation for an assembly.
    /// </summary>
    public async Task GenerateYamlAsync(Assembly assembly, string xmlDocPath, string outputPath, CancellationToken ct = default)
    {
        var assemblyDoc = await _parser.ParseAssemblyAsync(assembly, xmlDocPath, ct);
        await _yamlGenerator.GenerateToFileAsync(assemblyDoc, outputPath, ct);
    }
}
