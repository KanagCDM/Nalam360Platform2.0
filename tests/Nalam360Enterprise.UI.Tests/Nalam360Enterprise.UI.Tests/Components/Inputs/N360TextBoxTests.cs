using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Nalam360Enterprise.UI.Components.Inputs;
using Nalam360Enterprise.UI.Core.Forms;
using Nalam360Enterprise.UI.Core.Security;
using Xunit;

namespace Nalam360Enterprise.UI.Tests.Components.Inputs;

public class N360TextBoxTests : TestContext
{
    public N360TextBoxTests()
    {
        // Register required services
        Services.AddScoped<IPermissionService, DefaultPermissionService>();
        Services.AddScoped<IAuditService, AuditService>();
    }

    [Fact]
    public void N360TextBox_ShouldRenderWithLabel()
    {
        // Arrange & Act
        var cut = RenderComponent<N360TextBox<string>>(parameters => parameters
            .Add(p => p.Label, "Test Label")
            .Add(p => p.Value, "Test Value"));

        // Assert
        var label = cut.Find(".n360-input-label");
        label.TextContent.Should().Contain("Test Label");
    }

    [Fact]
    public void N360TextBox_ShouldShowRequiredIndicator_WhenRequired()
    {
        // Arrange & Act
        var cut = RenderComponent<N360TextBox<string>>(parameters => parameters
            .Add(p => p.Label, "Test Label")
            .Add(p => p.IsRequired, true));

        // Assert
        var requiredIndicator = cut.Find(".n360-required-indicator");
        requiredIndicator.Should().NotBeNull();
        requiredIndicator.GetAttribute("aria-label").Should().Be("required");
    }

    [Fact]
    public void N360TextBox_ShouldShowHelpText_WhenProvided()
    {
        // Arrange & Act
        var cut = RenderComponent<N360TextBox<string>>(parameters => parameters
            .Add(p => p.HelpText, "This is help text"));

        // Assert
        var helpText = cut.Find(".n360-input-help-text");
        helpText.TextContent.Should().Be("This is help text");
    }

    [Fact]
    public async Task N360TextBox_ShouldValidate_WhenValidationRulesProvided()
    {
        // Arrange
        var validationRules = new List<IValidationRule<string>>
        {
            new RequiredRule<string>(),
            new StringLengthRule(3, 10)
        };

        var cut = RenderComponent<N360TextBox<string>>(parameters => parameters
            .Add(p => p.ValidationRules, validationRules));

        // Act
        var isValid = await cut.Instance.ValidateAsync();

        // Assert
        isValid.Should().BeFalse(); // Should fail because required rule fails with empty value
        cut.Instance.ValidationErrors.Should().NotBeEmpty();
        cut.Instance.ValidationErrors[0].Message.Should().Be("This field is required");
    }

    [Fact]
    public async Task N360TextBox_ShouldInvokeValueChanged_WhenValueChanges()
    {
        // Arrange
        string? changedValue = null;
        var cut = RenderComponent<N360TextBox<string>>(parameters => parameters
            .Add(p => p.Value, "Initial")
            .Add(p => p.ValueChanged, value => changedValue = value));

        // Act
        var input = cut.Find("input");
        await input.InputAsync(new Microsoft.AspNetCore.Components.ChangeEventArgs 
        { 
            Value = "Changed" 
        });

        // Assert
        changedValue.Should().Be("Changed");
    }

    [Fact]
    public void N360TextBox_ShouldBeDisabled_WhenNoPermission()
    {
        // Arrange
        var permissionService = new DefaultPermissionService();
        Services.AddSingleton<IPermissionService>(permissionService);
        // Don't add the required permission

        var cut = RenderComponent<N360TextBox<string>>(parameters => parameters
            .Add(p => p.RequiredPermission, "test.permission")
            .Add(p => p.Value, "Test"));

        // Act
        cut.WaitForState(() => cut.Instance.IsEffectivelyEnabled == false, TimeSpan.FromSeconds(2));

        // Assert - component should be rendered but disabled
        var input = cut.Find("input");
        input.HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public async Task N360TextBox_ShouldLogAudit_WhenAuditEnabled()
    {
        // Arrange
        var auditService = new AuditService();
        AuditMetadata? capturedMetadata = null;
        auditService.AuditEventOccurred += (sender, args) => capturedMetadata = args.Metadata;
        
        Services.AddSingleton<IAuditService>(auditService);

        var cut = RenderComponent<N360TextBox<string>>(parameters => parameters
            .Add(p => p.EnableAudit, true)
            .Add(p => p.AuditResource, "TestResource"));

        // Act
        var input = cut.Find("input");
        await input.InputAsync(new Microsoft.AspNetCore.Components.ChangeEventArgs 
        { 
            Value = "New Value" 
        });

        // Assert
        capturedMetadata.Should().NotBeNull();
        capturedMetadata!.Action.Should().Be("ValueChanged");
        capturedMetadata.Resource.Should().Be("TestResource");
    }

    [Fact]
    public void N360TextBox_ShouldApplyRtl_WhenIsRtlTrue()
    {
        // Arrange & Act
        var cut = RenderComponent<N360TextBox<string>>(parameters => parameters
            .Add(p => p.IsRtl, true));

        // Assert
        var container = cut.Find(".n360-text-input");
        container.GetAttribute("dir").Should().Be("rtl");
    }

    [Fact]
    public void N360TextBox_ShouldApplyCssClass_WhenProvided()
    {
        // Arrange & Act
        var cut = RenderComponent<N360TextBox<string>>(parameters => parameters
            .Add(p => p.CssClass, "custom-class"));

        // Assert
        var container = cut.Find(".n360-text-input");
        container.ClassList.Should().Contain("custom-class");
    }

    [Fact]
    public async Task N360TextBox_ShouldShowValidationErrors_AfterValidation()
    {
        // Arrange
        var validationRules = new List<IValidationRule<string>>
        {
            new EmailRule()
        };

        var cut = RenderComponent<N360TextBox<string>>(parameters => parameters
            .Add(p => p.Value, "invalid-email")
            .Add(p => p.ValidationRules, validationRules));

        // Act
        await cut.Instance.ValidateAsync();
        cut.Render();

        // Assert
        var errorContainer = cut.Find(".n360-input-validation-errors");
        errorContainer.Should().NotBeNull();
        errorContainer.TextContent.Should().Contain("valid email");
    }
}
