namespace Nalam360.Platform.Documentation.Models;

/// <summary>
/// Represents documentation for a .NET assembly.
/// </summary>
public sealed class AssemblyDocumentation
{
    /// <summary>
    /// Gets or sets the assembly name.
    /// </summary>
    public string AssemblyName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the assembly version.
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the assembly description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the list of namespaces in the assembly.
    /// </summary>
    public List<NamespaceDocumentation> Namespaces { get; set; } = new();

    /// <summary>
    /// Gets or sets the generation timestamp.
    /// </summary>
    public DateTime GeneratedAt { get; set; }
}

/// <summary>
/// Represents documentation for a namespace.
/// </summary>
public sealed class NamespaceDocumentation
{
    /// <summary>
    /// Gets or sets the namespace name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of types in the namespace.
    /// </summary>
    public List<TypeDocumentation> Types { get; set; } = new();
}

/// <summary>
/// Represents documentation for a type (class, interface, struct, enum).
/// </summary>
public sealed class TypeDocumentation
{
    /// <summary>
    /// Gets or sets the type name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the full type name including namespace.
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type kind (class, interface, struct, enum).
    /// </summary>
    public TypeKind Kind { get; set; }

    /// <summary>
    /// Gets or sets the XML summary documentation.
    /// </summary>
    public string? Summary { get; set; }

    /// <summary>
    /// Gets or sets the XML remarks documentation.
    /// </summary>
    public string? Remarks { get; set; }

    /// <summary>
    /// Gets or sets the XML example documentation.
    /// </summary>
    public string? Example { get; set; }

    /// <summary>
    /// Gets or sets whether the type is public.
    /// </summary>
    public bool IsPublic { get; set; }

    /// <summary>
    /// Gets or sets whether the type is sealed.
    /// </summary>
    public bool IsSealed { get; set; }

    /// <summary>
    /// Gets or sets whether the type is abstract.
    /// </summary>
    public bool IsAbstract { get; set; }

    /// <summary>
    /// Gets or sets the base type name.
    /// </summary>
    public string? BaseType { get; set; }

    /// <summary>
    /// Gets or sets the implemented interfaces.
    /// </summary>
    public List<string> Interfaces { get; set; } = new();

    /// <summary>
    /// Gets or sets the generic type parameters.
    /// </summary>
    public List<string> GenericParameters { get; set; } = new();

    /// <summary>
    /// Gets or sets the type's constructors.
    /// </summary>
    public List<ConstructorDocumentation> Constructors { get; set; } = new();

    /// <summary>
    /// Gets or sets the type's properties.
    /// </summary>
    public List<PropertyDocumentation> Properties { get; set; } = new();

    /// <summary>
    /// Gets or sets the type's methods.
    /// </summary>
    public List<MethodDocumentation> Methods { get; set; } = new();

    /// <summary>
    /// Gets or sets the type's fields (for enums).
    /// </summary>
    public List<FieldDocumentation> Fields { get; set; } = new();
}

/// <summary>
/// Represents documentation for a constructor.
/// </summary>
public sealed class ConstructorDocumentation
{
    /// <summary>
    /// Gets or sets the constructor summary.
    /// </summary>
    public string? Summary { get; set; }

    /// <summary>
    /// Gets or sets whether the constructor is public.
    /// </summary>
    public bool IsPublic { get; set; }

    /// <summary>
    /// Gets or sets the constructor parameters.
    /// </summary>
    public List<ParameterDocumentation> Parameters { get; set; } = new();
}

/// <summary>
/// Represents documentation for a property.
/// </summary>
public sealed class PropertyDocumentation
{
    /// <summary>
    /// Gets or sets the property name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the property type.
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the XML summary documentation.
    /// </summary>
    public string? Summary { get; set; }

    /// <summary>
    /// Gets or sets whether the property is public.
    /// </summary>
    public bool IsPublic { get; set; }

    /// <summary>
    /// Gets or sets whether the property has a getter.
    /// </summary>
    public bool HasGetter { get; set; }

    /// <summary>
    /// Gets or sets whether the property has a setter.
    /// </summary>
    public bool HasSetter { get; set; }
}

/// <summary>
/// Represents documentation for a method.
/// </summary>
public sealed class MethodDocumentation
{
    /// <summary>
    /// Gets or sets the method name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the return type.
    /// </summary>
    public string ReturnType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the XML summary documentation.
    /// </summary>
    public string? Summary { get; set; }

    /// <summary>
    /// Gets or sets the XML remarks documentation.
    /// </summary>
    public string? Remarks { get; set; }

    /// <summary>
    /// Gets or sets the XML returns documentation.
    /// </summary>
    public string? Returns { get; set; }

    /// <summary>
    /// Gets or sets whether the method is public.
    /// </summary>
    public bool IsPublic { get; set; }

    /// <summary>
    /// Gets or sets whether the method is static.
    /// </summary>
    public bool IsStatic { get; set; }

    /// <summary>
    /// Gets or sets whether the method is async.
    /// </summary>
    public bool IsAsync { get; set; }

    /// <summary>
    /// Gets or sets the method parameters.
    /// </summary>
    public List<ParameterDocumentation> Parameters { get; set; } = new();

    /// <summary>
    /// Gets or sets the generic type parameters.
    /// </summary>
    public List<string> GenericParameters { get; set; } = new();
}

/// <summary>
/// Represents documentation for a parameter.
/// </summary>
public sealed class ParameterDocumentation
{
    /// <summary>
    /// Gets or sets the parameter name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the parameter type.
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the XML param documentation.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets whether the parameter has a default value.
    /// </summary>
    public bool HasDefaultValue { get; set; }

    /// <summary>
    /// Gets or sets the default value if present.
    /// </summary>
    public string? DefaultValue { get; set; }
}

/// <summary>
/// Represents documentation for a field (typically enum members).
/// </summary>
public sealed class FieldDocumentation
{
    /// <summary>
    /// Gets or sets the field name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the field type.
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the XML summary documentation.
    /// </summary>
    public string? Summary { get; set; }

    /// <summary>
    /// Gets or sets the constant value (for enums).
    /// </summary>
    public object? Value { get; set; }
}

/// <summary>
/// Represents the kind of type.
/// </summary>
public enum TypeKind
{
    /// <summary>
    /// A class type.
    /// </summary>
    Class,

    /// <summary>
    /// An interface type.
    /// </summary>
    Interface,

    /// <summary>
    /// A struct type.
    /// </summary>
    Struct,

    /// <summary>
    /// An enum type.
    /// </summary>
    Enum,

    /// <summary>
    /// A delegate type.
    /// </summary>
    Delegate,

    /// <summary>
    /// A record type.
    /// </summary>
    Record
}
