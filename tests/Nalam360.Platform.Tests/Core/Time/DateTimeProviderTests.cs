using Nalam360.Platform.Core.Time;
using Xunit;

namespace Nalam360.Platform.Tests.Core.Time;

public class TimeProviderTests
{
    [Fact]
    public void UtcNow_ReturnsUtcDateTime()
    {
        // Arrange
        var provider = new SystemTimeProvider();
        var before = DateTime.UtcNow;
        
        // Act
        var result = provider.UtcNow;
        var after = DateTime.UtcNow;
        
        // Assert
        Assert.Equal(DateTimeKind.Utc, result.Kind);
        Assert.True(result >= before && result <= after);
    }
    
    [Fact]
    public void Now_ReturnsLocalDateTime()
    {
        // Arrange
        var provider = new SystemTimeProvider();
        var before = DateTime.Now;
        
        // Act
        var result = provider.Now;
        var after = DateTime.Now;
        
        // Assert
        Assert.True(result >= before && result <= after);
    }
}

public class TestTimeProvider : ITimeProvider
{
    private readonly DateTime _fixedDateTime;
    
    public TestTimeProvider(DateTime fixedDateTime)
    {
        _fixedDateTime = fixedDateTime;
    }
    
    public DateTime UtcNow => _fixedDateTime;
    public DateTime Now => _fixedDateTime.ToLocalTime();
    public DateTimeOffset UtcNowOffset => new DateTimeOffset(_fixedDateTime, TimeSpan.Zero);
    public DateTimeOffset NowOffset => new DateTimeOffset(_fixedDateTime.ToLocalTime());
    public long UnixTimestampSeconds => _fixedDateTime.Ticks / TimeSpan.TicksPerSecond;
    public long UnixTimestampMilliseconds => _fixedDateTime.Ticks / TimeSpan.TicksPerMillisecond;
}

public class TestTimeProviderTests
{
    [Fact]
    public void TestProvider_ReturnsFixedDateTime()
    {
        // Arrange
        var fixedTime = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var provider = new TestTimeProvider(fixedTime);
        
        // Act
        var result1 = provider.UtcNow;
        var result2 = provider.UtcNow;
        
        // Assert
        Assert.Equal(fixedTime, result1);
        Assert.Equal(fixedTime, result2);
        Assert.Equal(result1, result2); // Always returns same value
    }
}
