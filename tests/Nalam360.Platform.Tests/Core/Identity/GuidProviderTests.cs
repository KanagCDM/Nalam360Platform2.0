using Nalam360.Platform.Core.Identity;
using Xunit;

namespace Nalam360.Platform.Tests.Core.Identity;

public class GuidProviderTests
{
    [Fact]
    public void NewGuid_ReturnsUniqueGuid()
    {
        // Arrange
        var provider = new GuidProvider();
        
        // Act
        var guid1 = provider.NewGuid();
        var guid2 = provider.NewGuid();
        
        // Assert
        Assert.NotEqual(Guid.Empty, guid1);
        Assert.NotEqual(Guid.Empty, guid2);
        Assert.NotEqual(guid1, guid2);
    }
    
    [Fact]
    public void NewGuid_CalledMultipleTimes_GeneratesUniqueGuids()
    {
        // Arrange
        var provider = new GuidProvider();
        var guids = new HashSet<Guid>();
        
        // Act
        for (int i = 0; i < 1000; i++)
        {
            guids.Add(provider.NewGuid());
        }
        
        // Assert
        Assert.Equal(1000, guids.Count); // All unique
    }
}
