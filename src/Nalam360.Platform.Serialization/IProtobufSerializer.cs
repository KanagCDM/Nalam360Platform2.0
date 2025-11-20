namespace Nalam360.Platform.Serialization;

/// <summary>
/// Protobuf serializer interface.
/// </summary>
public interface IProtobufSerializer
{
    /// <summary>
    /// Serializes an object to protobuf byte array.
    /// </summary>
    byte[] Serialize<T>(T obj) where T : Google.Protobuf.IMessage;

    /// <summary>
    /// Deserializes a protobuf byte array to an object.
    /// </summary>
    T Deserialize<T>(byte[] data) where T : Google.Protobuf.IMessage<T>, new();

    /// <summary>
    /// Serializes an object to protobuf stream.
    /// </summary>
    void Serialize<T>(T obj, Stream stream) where T : Google.Protobuf.IMessage;

    /// <summary>
    /// Deserializes a protobuf stream to an object.
    /// </summary>
    T Deserialize<T>(Stream stream) where T : Google.Protobuf.IMessage<T>, new();
}
