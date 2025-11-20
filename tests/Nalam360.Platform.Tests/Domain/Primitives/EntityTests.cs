using Nalam360.Platform.Domain.Primitives;
using Xunit;

namespace Nalam360.Platform.Tests.Domain.Primitives;

// Test entity
public class TestEntity : Entity<Guid>
{
    public string Name { get; set; } = string.Empty;
    
    public TestEntity(Guid id, string name)
    {
        Id = id;
        Name = name;
    }
}

public class EntityTests
{
    [Fact]
    public void Entities_WithSameId_AreEqual()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity1 = new TestEntity(id, "Entity 1");
        var entity2 = new TestEntity(id, "Entity 2");
        
        // Act & Assert
        Assert.Equal(entity1, entity2);
        Assert.True(entity1 == entity2);
        Assert.False(entity1 != entity2);
    }
    
    [Fact]
    public void Entities_WithDifferentIds_AreNotEqual()
    {
        // Arrange
        var entity1 = new TestEntity(Guid.NewGuid(), "Entity 1");
        var entity2 = new TestEntity(Guid.NewGuid(), "Entity 2");
        
        // Act & Assert
        Assert.NotEqual(entity1, entity2);
        Assert.False(entity1 == entity2);
        Assert.True(entity1 != entity2);
    }
    
    [Fact]
    public void Entity_GetHashCode_UsesId()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity = new TestEntity(id, "Test");
        
        // Act
        var hashCode = entity.GetHashCode();
        
        // Assert
        Assert.Equal(id.GetHashCode(), hashCode);
    }
}

// Test value object
public class Address : ValueObject
{
    public string Street { get; }
    public string City { get; }
    public string ZipCode { get; }
    
    public Address(string street, string city, string zipCode)
    {
        Street = street;
        City = city;
        ZipCode = zipCode;
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return ZipCode;
    }
}

public class ValueObjectTests
{
    [Fact]
    public void ValueObjects_WithSameValues_AreEqual()
    {
        // Arrange
        var address1 = new Address("123 Main St", "New York", "10001");
        var address2 = new Address("123 Main St", "New York", "10001");
        
        // Act & Assert
        Assert.Equal(address1, address2);
        Assert.True(address1 == address2);
        Assert.False(address1 != address2);
    }
    
    [Fact]
    public void ValueObjects_WithDifferentValues_AreNotEqual()
    {
        // Arrange
        var address1 = new Address("123 Main St", "New York", "10001");
        var address2 = new Address("456 Oak Ave", "Boston", "02101");
        
        // Act & Assert
        Assert.NotEqual(address1, address2);
        Assert.False(address1 == address2);
        Assert.True(address1 != address2);
    }
    
    [Fact]
    public void ValueObjects_WithSameValues_HaveSameHashCode()
    {
        // Arrange
        var address1 = new Address("123 Main St", "New York", "10001");
        var address2 = new Address("123 Main St", "New York", "10001");
        
        // Act
        var hash1 = address1.GetHashCode();
        var hash2 = address2.GetHashCode();
        
        // Assert
        Assert.Equal(hash1, hash2);
    }
}
