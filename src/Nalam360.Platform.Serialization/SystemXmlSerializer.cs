using System.Xml;
using System.Xml.Serialization;

namespace Nalam360.Platform.Serialization;

/// <summary>
/// System.Xml.Serialization implementation of XML serializer.
/// </summary>
public class SystemXmlSerializer : IXmlSerializer
{
    private readonly XmlWriterSettings _writerSettings;
    private readonly XmlReaderSettings _readerSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="SystemXmlSerializer"/> class.
    /// </summary>
    public SystemXmlSerializer()
    {
        _writerSettings = new XmlWriterSettings
        {
            Indent = true,
            OmitXmlDeclaration = false,
            Encoding = System.Text.Encoding.UTF8
        };

        _readerSettings = new XmlReaderSettings
        {
            IgnoreWhitespace = true,
            IgnoreComments = true
        };
    }

    /// <inheritdoc/>
    public string Serialize<T>(T obj)
    {
        if (obj == null)
            throw new ArgumentNullException(nameof(obj));

        using var stringWriter = new StringWriter();
        using var xmlWriter = XmlWriter.Create(stringWriter, _writerSettings);
        
        var serializer = new XmlSerializer(typeof(T));
        serializer.Serialize(xmlWriter, obj);
        
        return stringWriter.ToString();
    }

    /// <inheritdoc/>
    public T? Deserialize<T>(string xml)
    {
        if (string.IsNullOrEmpty(xml))
            throw new ArgumentNullException(nameof(xml));

        using var stringReader = new StringReader(xml);
        using var xmlReader = XmlReader.Create(stringReader, _readerSettings);
        
        var serializer = new XmlSerializer(typeof(T));
        return (T?)serializer.Deserialize(xmlReader);
    }

    /// <inheritdoc/>
    public void Serialize<T>(T obj, Stream stream)
    {
        if (obj == null)
            throw new ArgumentNullException(nameof(obj));
        if (stream == null)
            throw new ArgumentNullException(nameof(stream));

        using var xmlWriter = XmlWriter.Create(stream, _writerSettings);
        
        var serializer = new XmlSerializer(typeof(T));
        serializer.Serialize(xmlWriter, obj);
    }

    /// <inheritdoc/>
    public T? Deserialize<T>(Stream stream)
    {
        if (stream == null)
            throw new ArgumentNullException(nameof(stream));

        using var xmlReader = XmlReader.Create(stream, _readerSettings);
        
        var serializer = new XmlSerializer(typeof(T));
        return (T?)serializer.Deserialize(xmlReader);
    }
}
