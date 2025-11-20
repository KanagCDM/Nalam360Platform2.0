using System.Reflection;
using System.Xml.Linq;
using Nalam360.Platform.Documentation.Models;

namespace Nalam360.Platform.Documentation.Parsers;

/// <summary>
/// Parses XML documentation files and extracts documentation metadata.
/// </summary>
public interface IDocumentationParser
{
    /// <summary>
    /// Parses an assembly and its XML documentation file.
    /// </summary>
    /// <param name="assembly">The assembly to parse.</param>
    /// <param name="xmlDocPath">Path to the XML documentation file.</param>
    /// <returns>The parsed assembly documentation.</returns>
    AssemblyDocumentation ParseAssembly(Assembly assembly, string xmlDocPath);

    /// <summary>
    /// Parses an assembly and its XML documentation file asynchronously.
    /// </summary>
    /// <param name="assembly">The assembly to parse.</param>
    /// <param name="xmlDocPath">Path to the XML documentation file.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The parsed assembly documentation.</returns>
    Task<AssemblyDocumentation> ParseAssemblyAsync(Assembly assembly, string xmlDocPath, CancellationToken ct = default);
}

/// <summary>
/// Default implementation of XML documentation parser.
/// </summary>
public sealed class XmlDocumentationParser : IDocumentationParser
{
    private XDocument? _xmlDoc;
    private readonly Dictionary<string, XElement> _memberCache = new();

    public AssemblyDocumentation ParseAssembly(Assembly assembly, string xmlDocPath)
    {
        if (!File.Exists(xmlDocPath))
            throw new FileNotFoundException($"XML documentation file not found: {xmlDocPath}");

        _xmlDoc = XDocument.Load(xmlDocPath);
        CacheMemberDocumentation();

        var assemblyDoc = new AssemblyDocumentation
        {
            AssemblyName = assembly.GetName().Name ?? "Unknown",
            Version = assembly.GetName().Version?.ToString() ?? "0.0.0",
            GeneratedAt = DateTime.UtcNow
        };

        var types = assembly.GetTypes()
            .Where(t => t.IsPublic && !t.IsNested)
            .OrderBy(t => t.Namespace)
            .ThenBy(t => t.Name);

        var namespaceGroups = types.GroupBy(t => t.Namespace ?? "Global");

        foreach (var nsGroup in namespaceGroups)
        {
            var nsDoc = new NamespaceDocumentation
            {
                Name = nsGroup.Key
            };

            foreach (var type in nsGroup)
            {
                var typeDoc = ParseType(type);
                if (typeDoc != null)
                    nsDoc.Types.Add(typeDoc);
            }

            if (nsDoc.Types.Count > 0)
                assemblyDoc.Namespaces.Add(nsDoc);
        }

        return assemblyDoc;
    }

    public async Task<AssemblyDocumentation> ParseAssemblyAsync(Assembly assembly, string xmlDocPath, CancellationToken ct = default)
    {
        return await Task.Run(() => ParseAssembly(assembly, xmlDocPath), ct);
    }

    private void CacheMemberDocumentation()
    {
        if (_xmlDoc == null) return;

        var members = _xmlDoc.Descendants("member");
        foreach (var member in members)
        {
            var name = member.Attribute("name")?.Value;
            if (name != null)
                _memberCache[name] = member;
        }
    }

    private TypeDocumentation? ParseType(Type type)
    {
        var memberName = $"T:{type.FullName}";
        var xmlElement = _memberCache.GetValueOrDefault(memberName);

        var typeDoc = new TypeDocumentation
        {
            Name = type.Name,
            FullName = type.FullName ?? type.Name,
            Kind = GetTypeKind(type),
            IsPublic = type.IsPublic,
            IsSealed = type.IsSealed,
            IsAbstract = type.IsAbstract,
            Summary = xmlElement?.Element("summary")?.Value.Trim(),
            Remarks = xmlElement?.Element("remarks")?.Value.Trim(),
            Example = xmlElement?.Element("example")?.Value.Trim()
        };

        // Base type
        if (type.BaseType != null && type.BaseType != typeof(object))
            typeDoc.BaseType = type.BaseType.Name;

        // Interfaces
        typeDoc.Interfaces.AddRange(type.GetInterfaces().Select(i => i.Name));

        // Generic parameters
        if (type.IsGenericType)
            typeDoc.GenericParameters.AddRange(type.GetGenericArguments().Select(t => t.Name));

        // Constructors
        foreach (var ctor in type.GetConstructors(BindingFlags.Public | BindingFlags.Instance))
        {
            typeDoc.Constructors.Add(ParseConstructor(ctor));
        }

        // Properties
        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
        {
            typeDoc.Properties.Add(ParseProperty(prop));
        }

        // Methods
        foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
        {
            if (!method.IsSpecialName) // Exclude property getters/setters
                typeDoc.Methods.Add(ParseMethod(method));
        }

        // Fields (for enums)
        if (type.IsEnum)
        {
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                typeDoc.Fields.Add(ParseField(field));
            }
        }

        return typeDoc;
    }

    private ConstructorDocumentation ParseConstructor(ConstructorInfo ctor)
    {
        var memberName = $"M:{ctor.DeclaringType?.FullName}.#ctor({string.Join(",", ctor.GetParameters().Select(p => p.ParameterType.FullName))})";
        var xmlElement = _memberCache.GetValueOrDefault(memberName);

        return new ConstructorDocumentation
        {
            IsPublic = ctor.IsPublic,
            Summary = xmlElement?.Element("summary")?.Value.Trim(),
            Parameters = ctor.GetParameters().Select(p => ParseParameter(p, xmlElement)).ToList()
        };
    }

    private PropertyDocumentation ParseProperty(PropertyInfo prop)
    {
        var memberName = $"P:{prop.DeclaringType?.FullName}.{prop.Name}";
        var xmlElement = _memberCache.GetValueOrDefault(memberName);

        return new PropertyDocumentation
        {
            Name = prop.Name,
            Type = GetFriendlyTypeName(prop.PropertyType),
            IsPublic = prop.GetMethod?.IsPublic ?? prop.SetMethod?.IsPublic ?? false,
            HasGetter = prop.CanRead,
            HasSetter = prop.CanWrite,
            Summary = xmlElement?.Element("summary")?.Value.Trim()
        };
    }

    private MethodDocumentation ParseMethod(MethodInfo method)
    {
        var paramTypes = string.Join(",", method.GetParameters().Select(p => p.ParameterType.FullName));
        var memberName = $"M:{method.DeclaringType?.FullName}.{method.Name}({paramTypes})";
        var xmlElement = _memberCache.GetValueOrDefault(memberName);

        return new MethodDocumentation
        {
            Name = method.Name,
            ReturnType = GetFriendlyTypeName(method.ReturnType),
            IsPublic = method.IsPublic,
            IsStatic = method.IsStatic,
            IsAsync = method.ReturnType.Name.Contains("Task"),
            Summary = xmlElement?.Element("summary")?.Value.Trim(),
            Remarks = xmlElement?.Element("remarks")?.Value.Trim(),
            Returns = xmlElement?.Element("returns")?.Value.Trim(),
            Parameters = method.GetParameters().Select(p => ParseParameter(p, xmlElement)).ToList(),
            GenericParameters = method.IsGenericMethod 
                ? method.GetGenericArguments().Select(t => t.Name).ToList() 
                : new List<string>()
        };
    }

    private ParameterDocumentation ParseParameter(ParameterInfo param, XElement? xmlElement)
    {
        var paramDoc = xmlElement?.Elements("param")
            .FirstOrDefault(e => e.Attribute("name")?.Value == param.Name);

        return new ParameterDocumentation
        {
            Name = param.Name ?? "unknown",
            Type = GetFriendlyTypeName(param.ParameterType),
            Description = paramDoc?.Value.Trim(),
            HasDefaultValue = param.HasDefaultValue,
            DefaultValue = param.DefaultValue?.ToString()
        };
    }

    private FieldDocumentation ParseField(FieldInfo field)
    {
        var memberName = $"F:{field.DeclaringType?.FullName}.{field.Name}";
        var xmlElement = _memberCache.GetValueOrDefault(memberName);

        return new FieldDocumentation
        {
            Name = field.Name,
            Type = GetFriendlyTypeName(field.FieldType),
            Summary = xmlElement?.Element("summary")?.Value.Trim(),
            Value = field.IsLiteral ? field.GetRawConstantValue() : null
        };
    }

    private static TypeKind GetTypeKind(Type type)
    {
        if (type.IsEnum) return TypeKind.Enum;
        if (type.IsInterface) return TypeKind.Interface;
        if (type.IsValueType) return TypeKind.Struct;
        if (typeof(Delegate).IsAssignableFrom(type)) return TypeKind.Delegate;
        if (type.IsClass && type.BaseType?.Name == "Record") return TypeKind.Record;
        return TypeKind.Class;
    }

    private static string GetFriendlyTypeName(Type type)
    {
        if (!type.IsGenericType)
            return type.Name;

        var genericArgs = type.GetGenericArguments();
        var baseName = type.Name.Split('`')[0];
        var argNames = string.Join(", ", genericArgs.Select(GetFriendlyTypeName));
        return $"{baseName}<{argNames}>";
    }
}
