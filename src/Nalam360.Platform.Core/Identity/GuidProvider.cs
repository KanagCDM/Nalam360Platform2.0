using System.Security.Cryptography;

namespace Nalam360.Platform.Core.Identity;

/// <summary>
/// Default GUID provider implementation
/// </summary>
public sealed class GuidProvider : IGuidProvider
{
    public Guid NewGuid() => Guid.NewGuid();

    public Guid NewSequentialGuid()
    {
        // Generate a sequential GUID for better database performance
        var guidBytes = Guid.NewGuid().ToByteArray();
        var now = DateTime.UtcNow;

        // Get the days and milliseconds which will be used to build the byte string
        var days = new TimeSpan(now.Ticks - new DateTime(1900, 1, 1).Ticks);
        var msecs = now.TimeOfDay;

        // Convert to a byte array
        var daysArray = BitConverter.GetBytes(days.Days);
        var msecsArray = BitConverter.GetBytes((long)(msecs.TotalMilliseconds / 3.333333));

        // Reverse the bytes to match SQL Server ordering
        Array.Reverse(daysArray);
        Array.Reverse(msecsArray);

        // Copy the bytes into the guid
        Array.Copy(daysArray, daysArray.Length - 2, guidBytes, guidBytes.Length - 6, 2);
        Array.Copy(msecsArray, msecsArray.Length - 4, guidBytes, guidBytes.Length - 4, 4);

        return new Guid(guidBytes);
    }
}
