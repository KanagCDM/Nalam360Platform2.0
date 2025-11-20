namespace Nalam360.Platform.Core.Time;

/// <summary>
/// Abstraction for time operations to enable testability
/// </summary>
public interface ITimeProvider
{
    /// <summary>
    /// Gets the current UTC date and time
    /// </summary>
    DateTime UtcNow { get; }

    /// <summary>
    /// Gets the current local date and time
    /// </summary>
    DateTime Now { get; }

    /// <summary>
    /// Gets the current UTC offset
    /// </summary>
    DateTimeOffset UtcNowOffset { get; }

    /// <summary>
    /// Gets the current local offset
    /// </summary>
    DateTimeOffset NowOffset { get; }

    /// <summary>
    /// Gets the current Unix timestamp in seconds
    /// </summary>
    long UnixTimestampSeconds { get; }

    /// <summary>
    /// Gets the current Unix timestamp in milliseconds
    /// </summary>
    long UnixTimestampMilliseconds { get; }
}
