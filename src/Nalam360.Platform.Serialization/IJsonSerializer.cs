using System.Text.Json;

namespace Nalam360.Platform.Serialization;

/// <summary>
/// JSON serializer interface.
/// </summary>
public interface IJsonSerializer
{
    /// <summary>
    /// Serializes an object to JSON.
    /// </summary>
    string Serialize<T>(T value);

    /// <summary>
    /// Deserializes JSON to an object.
    /// </summary>
    T? Deserialize<T>(string json);
}

/// <summary>
/// System.Text.Json implementation.
/// </summary>
public class SystemTextJsonSerializer : IJsonSerializer
{
    private readonly JsonSerializerOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="SystemTextJsonSerializer"/> class.
    /// </summary>
    /// <param name="options">Optional JSON serializer options.</param>
    public SystemTextJsonSerializer(JsonSerializerOptions? options = null)
    {
        _options = options ?? new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
    }

    /// <inheritdoc />
    public string Serialize<T>(T value)
    {
        return JsonSerializer.Serialize(value, _options);
    }

    /// <inheritdoc />
    public T? Deserialize<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json, _options);
    }
}
