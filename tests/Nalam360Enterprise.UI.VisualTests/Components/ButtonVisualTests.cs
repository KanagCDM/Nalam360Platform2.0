using Microsoft.Playwright;
using NUnit.Framework;

namespace Nalam360Enterprise.UI.VisualTests.Components;

[TestFixture]
public class ButtonVisualTests : PlaywrightTestBase
{
    [Test]
    public async Task Button_DefaultState_MatchesBaseline()
    {
        await NavigateToComponentAsync("components/button");
        await HideDynamicElementsAsync();
        
        var button = Page.Locator("[data-testid='button-default']");
        await Expect(button).ToBeVisibleAsync();
        
        await AssertVisualMatchAsync("button-default", button);
    }

    [Test]
    public async Task Button_HoverState_MatchesBaseline()
    {
        await NavigateToComponentAsync("components/button");
        
        var button = Page.Locator("[data-testid='button-primary']");
        await button.HoverAsync();
        await Page.WaitForTimeoutAsync(500); // Wait for hover animation
        
        await AssertVisualMatchAsync("button-hover", button);
    }

    [Test]
    public async Task Button_DisabledState_MatchesBaseline()
    {
        await NavigateToComponentAsync("components/button");
        
        var button = Page.Locator("[data-testid='button-disabled']");
        await Expect(button).ToBeDisabledAsync();
        
        await AssertVisualMatchAsync("button-disabled", button);
    }

    [Test]
    public async Task Button_AllVariants_MatchBaseline()
    {
        await NavigateToComponentAsync("components/button");
        await HideDynamicElementsAsync();
        
        var variants = new[] { "primary", "secondary", "success", "warning", "error", "info" };
        
        foreach (var variant in variants)
        {
            var button = Page.Locator($"[data-testid='button-{variant}']");
            await Expect(button).ToBeVisibleAsync();
            await AssertVisualMatchAsync($"button-{variant}", button);
        }
    }

    [Test]
    public async Task Button_AllSizes_MatchBaseline()
    {
        await NavigateToComponentAsync("components/button");
        
        var sizes = new[] { "small", "medium", "large" };
        
        foreach (var size in sizes)
        {
            var button = Page.Locator($"[data-testid='button-{size}']");
            await Expect(button).ToBeVisibleAsync();
            await AssertVisualMatchAsync($"button-size-{size}", button);
        }
    }

    [Test]
    public async Task Button_AllThemes_MatchBaseline()
    {
        await TestAllThemesAsync("components/button", "button-themes");
    }

    [Test]
    public async Task Button_Responsive_MatchesBaseline()
    {
        await TestResponsiveAsync("components/button", "button-responsive");
    }

    [Test]
    public async Task Button_WithIcon_MatchesBaseline()
    {
        await NavigateToComponentAsync("components/button");
        
        var button = Page.Locator("[data-testid='button-with-icon']");
        await Expect(button).ToBeVisibleAsync();
        
        await AssertVisualMatchAsync("button-with-icon", button);
    }

    [Test]
    public async Task Button_Loading_MatchesBaseline()
    {
        await NavigateToComponentAsync("components/button");
        
        var button = Page.Locator("[data-testid='button-loading']");
        await button.ClickAsync();
        
        // Wait for loading state
        await Page.WaitForSelectorAsync("[data-testid='button-loading'] .spinner");
        
        await AssertVisualMatchAsync("button-loading", button);
    }
}
