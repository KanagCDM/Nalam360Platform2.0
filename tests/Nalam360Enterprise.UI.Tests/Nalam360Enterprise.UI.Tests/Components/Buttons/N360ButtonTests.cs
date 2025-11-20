using Bunit;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Nalam360Enterprise.UI.Components.Buttons;
using Nalam360Enterprise.UI.Core.Security;

namespace Nalam360Enterprise.UI.Tests.Components.Buttons;

public class N360ButtonTests : TestContext
{
    public N360ButtonTests()
    {
        // Register mock services
        Services.AddSingleton<IPermissionService>(new MockPermissionService(hasPermission: true));
    }
    
    [Fact]
    public void N360Button_Renders_WithText()
    {
        // Arrange
        var expectedText = "Click Me";
        
        // Act
        var cut = RenderComponent<N360Button>(parameters => parameters
            .Add(p => p.Text, expectedText));
        
        // Assert
        Assert.Contains(expectedText, cut.Markup);
    }
    
    [Fact]
    public void N360Button_WithRequiredPermission_ShowsWhenHasPermission()
    {
        // Act
        var cut = RenderComponent<N360Button>(parameters => parameters
            .Add(p => p.Text, "Button")
            .Add(p => p.RequiredPermission, "test.permission")
            .Add(p => p.HideIfNoPermission, false));
        
        // Assert
        var button = cut.Find("button");
        Assert.NotNull(button);
    }
    
    [Fact]
    public void N360Button_WithRequiredPermission_HidesWhenNoPermission()
    {
        // Arrange
        Services.AddSingleton<IPermissionService>(new MockPermissionService(hasPermission: false));
        
        // Act
        var cut = RenderComponent<N360Button>(parameters => parameters
            .Add(p => p.Text, "Button")
            .Add(p => p.RequiredPermission, "test.permission")
            .Add(p => p.HideIfNoPermission, true));
        
        // Assert
        Assert.Empty(cut.Markup);
    }
    
    [Fact]
    public void N360Button_Click_TriggersOnClick()
    {
        // Arrange
        var clicked = false;
        
        var cut = RenderComponent<N360Button>(parameters => parameters
            .Add(p => p.Text, "Click")
            .Add(p => p.OnClick, () => { clicked = true; return Task.CompletedTask; }));
        
        // Act
        var button = cut.Find("button");
        button.Click();
        
        // Assert
        Assert.True(clicked);
    }
    
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void N360Button_Disabled_SetsDisabledAttribute(bool disabled)
    {
        // Act
        var cut = RenderComponent<N360Button>(parameters => parameters
            .Add(p => p.Text, "Button")
            .Add(p => p.Disabled, disabled));
        
        // Assert
        var button = cut.Find("button");
        if (disabled)
            Assert.True(button.HasAttribute("disabled"));
        else
            Assert.False(button.HasAttribute("disabled"));
    }
}

// Mock permission service
public class MockPermissionService : IPermissionService
{
    private readonly bool _hasPermission;
    
    public MockPermissionService(bool hasPermission)
    {
        _hasPermission = hasPermission;
    }
    
    public Task<bool> HasPermissionAsync(string permission)
    {
        return Task.FromResult(_hasPermission);
    }
    
    public Task<bool> HasAnyPermissionAsync(params string[] permissions)
    {
        return Task.FromResult(_hasPermission);
    }
    
    public Task<bool> HasAllPermissionsAsync(params string[] permissions)
    {
        return Task.FromResult(_hasPermission);
    }
    
    public Task<bool> IsInRoleAsync(string role)
    {
        return Task.FromResult(_hasPermission);
    }
    
    public Task<IEnumerable<string>> GetUserPermissionsAsync()
    {
        return Task.FromResult<IEnumerable<string>>(new List<string>());
    }
}
