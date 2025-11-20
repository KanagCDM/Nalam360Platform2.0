namespace Nalam360.Platform.Core.Time;

/// <summary>
/// Default implementation using system time
/// </summary>
public sealed class SystemTimeProvider : ITimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
    public DateTime Now => DateTime.Now;
    public DateTimeOffset UtcNowOffset => DateTimeOffset.UtcNow;
    public DateTimeOffset NowOffset => DateTimeOffset.Now;
    public long UnixTimestampSeconds => DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    public long UnixTimestampMilliseconds => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
}
