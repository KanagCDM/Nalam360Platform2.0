using Nalam360.Platform.Core.Results;
using Xunit;

namespace Nalam360.Platform.Tests.Core.Results;

public class ResultTests
{
    [Fact]
    public void Success_CreatesSuccessResult()
    {
        // Act
        var result = Result.Success();
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
    }
    
    [Fact]
    public void Failure_CreatesFailureResult()
    {
        // Arrange
        var error = Error.Validation("Test error message");
        
        // Act
        var result = Result.Failure(error);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Equal("VALIDATION_ERROR", result.Error.Code);
        Assert.Equal("Test error message", result.Error.Message);
    }
    
    [Fact]
    public void SuccessWithValue_CreatesSuccessResultWithValue()
    {
        // Arrange
        var expectedValue = 42;
        
        // Act
        var result = Result<int>.Success(expectedValue);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(42, result.Value);
    }
    
    [Fact]
    public void FailureWithValue_CreatesFailureResultWithoutValue()
    {
        // Arrange
        var error = Error.NotFound("Item", "123");
        
        // Act
        var result = Result<int>.Failure(error);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Throws<InvalidOperationException>(() => result.Value);
        Assert.NotNull(result.Error);
    }
    
    [Theory]
    [InlineData("VALIDATION_ERROR", "Validation")]
    [InlineData("NOT_FOUND", "NotFound")]
    [InlineData("UNAUTHORIZED", "Unauthorized")]
    [InlineData("CONFLICT", "Conflict")]
    public void Error_CreatesDifferentErrorTypes(string expectedCode, string errorType)
    {
        // Arrange & Act
        var error = errorType switch
        {
            "Validation" => Error.Validation("Validation error"),
            "NotFound" => Error.NotFound("Entity", "Id"),
            "Unauthorized" => Error.Unauthorized("Message"),
            "Conflict" => Error.Conflict("Message"),
            _ => throw new ArgumentException("Invalid error type")
        };
        
        // Assert
        Assert.Equal(expectedCode, error.Code);
        Assert.NotEmpty(error.Message);
    }
    
    [Fact]
    public void Match_OnSuccess_ExecutesSuccessFunction()
    {
        // Arrange
        var result = Result<int>.Success(42);
        var executed = false;
        
        // Act
        var output = result.Match(
            onSuccess: value => { executed = true; return value.ToString(); },
            onFailure: error => "Failure"
        );
        
        // Assert
        Assert.True(executed);
        Assert.Equal("42", output);
    }
    
    [Fact]
    public void Match_OnFailure_ExecutesFailureFunction()
    {
        // Arrange
        var result = Result<int>.Failure(Error.Validation("Test validation error"));
        var executed = false;
        
        // Act
        var output = result.Match(
            onSuccess: value => value.ToString(),
            onFailure: error => { executed = true; return "Error"; }
        );
        
        // Assert
        Assert.True(executed);
        Assert.Equal("Error", output);
    }
}
