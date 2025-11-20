using Nalam360.Platform.Security.Cryptography;
using Xunit;

namespace Nalam360.Platform.Tests.Security.Cryptography;

public class Pbkdf2PasswordHasherTests
{
    private readonly IPasswordHasher _hasher;
    
    public Pbkdf2PasswordHasherTests()
    {
        _hasher = new Pbkdf2PasswordHasher();
    }
    
    [Fact]
    public void HashPassword_ReturnsNonEmptyHash()
    {
        // Arrange
        var password = "MySecurePassword123!";
        
        // Act
        var hash = _hasher.HashPassword(password);
        
        // Assert
        Assert.NotNull(hash);
        Assert.NotEmpty(hash);
    }
    
    [Fact]
    public void HashPassword_SamePassword_ProducesDifferentHashes()
    {
        // Arrange
        var password = "MySecurePassword123!";
        
        // Act
        var hash1 = _hasher.HashPassword(password);
        var hash2 = _hasher.HashPassword(password);
        
        // Assert
        Assert.NotEqual(hash1, hash2); // Due to random salt
    }
    
    [Fact]
    public void VerifyPassword_WithCorrectPassword_ReturnsTrue()
    {
        // Arrange
        var password = "MySecurePassword123!";
        var hash = _hasher.HashPassword(password);
        
        // Act
        var result = _hasher.VerifyPassword(password, hash);
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public void VerifyPassword_WithIncorrectPassword_ReturnsFalse()
    {
        // Arrange
        var password = "MySecurePassword123!";
        var wrongPassword = "WrongPassword456!";
        var hash = _hasher.HashPassword(password);
        
        // Act
        var result = _hasher.VerifyPassword(wrongPassword, hash);
        
        // Assert
        Assert.False(result);
    }
    
    [Theory]
    [InlineData("password123")]
    [InlineData("P@ssw0rd!")]
    [InlineData("VeryLongPasswordWith123Numbers!@#")]
    [InlineData("短")]
    public void HashAndVerify_WorksWithDifferentPasswords(string password)
    {
        // Arrange & Act
        var hash = _hasher.HashPassword(password);
        var isValid = _hasher.VerifyPassword(password, hash);
        
        // Assert
        Assert.True(isValid);
    }
    
    [Fact]
    public void VerifyPassword_WithNullPassword_ThrowsException()
    {
        // Arrange
        var hash = _hasher.HashPassword("password");
        
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _hasher.VerifyPassword(null!, hash));
    }
    
    [Fact]
    public void VerifyPassword_WithNullHash_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _hasher.VerifyPassword("password", null!));
    }
}
