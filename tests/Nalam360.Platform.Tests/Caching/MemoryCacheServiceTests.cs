using Nalam360.Platform.Caching;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace Nalam360.Platform.Tests.Caching;

public class MemoryCacheServiceTests
{
    private readonly ICacheService _cacheService;
    
    public MemoryCacheServiceTests()
    {
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        _cacheService = new MemoryCacheService(memoryCache);
    }
    
    [Fact]
    public async Task GetAsync_NonExistentKey_ReturnsNull()
    {
        // Act
        var result = await _cacheService.GetAsync<string>("non-existent-key");
        
        // Assert
        Assert.Null(result);
    }
    
    [Fact]
    public async Task SetAndGet_StoresAndRetrievesValue()
    {
        // Arrange
        var key = "test-key";
        var value = "test-value";
        
        // Act
        await _cacheService.SetAsync(key, value, TimeSpan.FromMinutes(5));
        var result = await _cacheService.GetAsync<string>(key);
        
        // Assert
        Assert.Equal(value, result);
    }
    
    [Fact]
    public async Task Remove_RemovesValueFromCache()
    {
        // Arrange
        var key = "test-key";
        var value = "test-value";
        await _cacheService.SetAsync(key, value, TimeSpan.FromMinutes(5));
        
        // Act
        await _cacheService.RemoveAsync(key);
        var result = await _cacheService.GetAsync<string>(key);
        
        // Assert
        Assert.Null(result);
    }
    
    [Fact]
    public async Task GetOrSetAsync_WhenNotCached_ExecutesFactory()
    {
        // Arrange
        var key = "test-key";
        var factoryExecuted = false;
        
        // Act
        var result = await _cacheService.GetOrSetAsync<string>(key, async (ct) =>
        {
            factoryExecuted = true;
            return await Task.FromResult("factory-value");
        }, TimeSpan.FromMinutes(5));
        
        // Assert
        Assert.True(factoryExecuted);
        Assert.Equal("factory-value", result);
    }
    
    [Fact]
    public async Task GetOrSetAsync_WhenCached_DoesNotExecuteFactory()
    {
        // Arrange
        var key = "test-key";
        await _cacheService.SetAsync(key, "cached-value", TimeSpan.FromMinutes(5));
        var factoryExecuted = false;
        
        // Act
        var result = await _cacheService.GetOrSetAsync<string>(key, async (ct) =>
        {
            factoryExecuted = true;
            return await Task.FromResult("factory-value");
        }, TimeSpan.FromMinutes(5));
        
        // Assert
        Assert.False(factoryExecuted);
        Assert.Equal("cached-value", result);
    }
    
    [Fact]
    public async Task SetAsync_WithComplexObject_StoresAndRetrievesCorrectly()
    {
        // Arrange
        var key = "complex-key";
        var value = new TestObject { Id = 1, Name = "Test", Items = new List<string> { "A", "B" } };
        
        // Act
        await _cacheService.SetAsync(key, value, TimeSpan.FromMinutes(5));
        var result = await _cacheService.GetAsync<TestObject>(key);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(value.Id, result.Id);
        Assert.Equal(value.Name, result.Name);
        Assert.Equal(value.Items, result.Items);
    }
    
    private class TestObject
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<string> Items { get; set; } = new();
    }
}
