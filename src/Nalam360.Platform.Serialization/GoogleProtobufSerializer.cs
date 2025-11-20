using Google.Protobuf;

namespace Nalam360.Platform.Serialization;

/// <summary>
/// Google Protobuf serializer implementation.
/// </summary>
public class GoogleProtobufSerializer : IProtobufSerializer
{
    /// <inheritdoc/>
    public byte[] Serialize<T>(T obj) where T : IMessage
    {
        if (obj == null)
            throw new ArgumentNullException(nameof(obj));

        return obj.ToByteArray();
    }

    /// <inheritdoc/>
    public T Deserialize<T>(byte[] data) where T : IMessage<T>, new()
    {
        if (data == null || data.Length == 0)
            throw new ArgumentNullException(nameof(data));

        var parser = new MessageParser<T>(() => new T());
        return parser.ParseFrom(data);
    }

    /// <inheritdoc/>
    public void Serialize<T>(T obj, Stream stream) where T : IMessage
    {
        if (obj == null)
            throw new ArgumentNullException(nameof(obj));
        if (stream == null)
            throw new ArgumentNullException(nameof(stream));

        obj.WriteTo(stream);
    }

    /// <inheritdoc/>
    public T Deserialize<T>(Stream stream) where T : IMessage<T>, new()
    {
        if (stream == null)
            throw new ArgumentNullException(nameof(stream));

        var parser = new MessageParser<T>(() => new T());
        return parser.ParseFrom(stream);
    }
}
