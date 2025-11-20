using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace Nalam360Enterprise.UI.VisualTests;

/// <summary>
/// Base class for all Playwright visual regression tests
/// </summary>
public class PlaywrightTestBase : PageTest
{
    protected const string BaseUrl = "http://localhost:5000";
    protected const int DefaultTimeout = 30000; // 30 seconds

    /// <summary>
    /// Takes a screenshot and compares it with the baseline
    /// </summary>
    protected async Task AssertVisualMatchAsync(string screenshotName, ILocator? element = null)
    {
        var options = new PageScreenshotOptions
        {
            Path = $"screenshots/{screenshotName}.png",
            FullPage = false,
            Animations = ScreenshotAnimations.Disabled
        };

        if (element != null)
        {
            await element.ScreenshotAsync(new LocatorScreenshotOptions
            {
                Path = $"screenshots/{screenshotName}.png",
                Animations = ScreenshotAnimations.Disabled
            });
        }
        else
        {
            await Page.ScreenshotAsync(options);
        }

        // Compare with baseline (using Playwright's built-in visual comparison)
        await Expect(Page).ToHaveScreenshotAsync($"{screenshotName}.png", new()
        {
            MaxDiffPixels = 100, // Allow up to 100 pixels difference
            Threshold = 0.2 // 20% threshold for pixel difference
        });
    }

    /// <summary>
    /// Waits for the page to be fully loaded and ready
    /// </summary>
    protected async Task WaitForPageReadyAsync()
    {
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
        
        // Wait for Blazor to be ready
        await Page.WaitForFunctionAsync("() => window['Blazor'] !== undefined");
    }

    /// <summary>
    /// Navigates to a component demo page
    /// </summary>
    protected async Task NavigateToComponentAsync(string componentPath)
    {
        await Page.GotoAsync($"{BaseUrl}/{componentPath}");
        await WaitForPageReadyAsync();
    }

    /// <summary>
    /// Hides dynamic elements (like clocks, timestamps) before taking screenshots
    /// </summary>
    protected async Task HideDynamicElementsAsync()
    {
        // Hide common dynamic elements
        await Page.EvaluateAsync(@"
            const selectors = [
                '[data-dynamic]',
                '.timestamp',
                '.current-time',
                '.last-updated'
            ];
            selectors.forEach(selector => {
                document.querySelectorAll(selector).forEach(el => el.style.visibility = 'hidden');
            });
        ");
    }

    /// <summary>
    /// Sets viewport size for responsive testing
    /// </summary>
    protected async Task SetViewportSizeAsync(int width, int height)
    {
        await Page.SetViewportSizeAsync(width, height);
    }

    /// <summary>
    /// Tests component in different themes
    /// </summary>
    protected async Task TestAllThemesAsync(string componentPath, string screenshotBaseName)
    {
        var themes = new[] { "light", "dark", "highcontrast" };

        foreach (var theme in themes)
        {
            await NavigateToComponentAsync(componentPath);
            
            // Change theme via JavaScript
            await Page.EvaluateAsync($"window.localStorage.setItem('theme', '{theme}')");
            await Page.ReloadAsync();
            await WaitForPageReadyAsync();
            
            await HideDynamicElementsAsync();
            await AssertVisualMatchAsync($"{screenshotBaseName}-{theme}");
        }
    }

    /// <summary>
    /// Tests component in different viewport sizes
    /// </summary>
    protected async Task TestResponsiveAsync(string componentPath, string screenshotBaseName)
    {
        var viewports = new[]
        {
            (320, 568, "mobile"),    // iPhone SE
            (768, 1024, "tablet"),   // iPad
            (1920, 1080, "desktop")  // Full HD
        };

        foreach (var (width, height, device) in viewports)
        {
            await SetViewportSizeAsync(width, height);
            await NavigateToComponentAsync(componentPath);
            await WaitForPageReadyAsync();
            await HideDynamicElementsAsync();
            await AssertVisualMatchAsync($"{screenshotBaseName}-{device}");
        }
    }
}
