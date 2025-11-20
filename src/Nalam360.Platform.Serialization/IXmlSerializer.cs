namespace Nalam360.Platform.Serialization;

/// <summary>
/// XML serializer interface.
/// </summary>
public interface IXmlSerializer
{
    /// <summary>
    /// Serializes an object to XML string.
    /// </summary>
    string Serialize<T>(T obj);

    /// <summary>
    /// Deserializes an XML string to an object.
    /// </summary>
    T? Deserialize<T>(string xml);

    /// <summary>
    /// Serializes an object to XML stream.
    /// </summary>
    void Serialize<T>(T obj, Stream stream);

    /// <summary>
    /// Deserializes an XML stream to an object.
    /// </summary>
    T? Deserialize<T>(Stream stream);
}
