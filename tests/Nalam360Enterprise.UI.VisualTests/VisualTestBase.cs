namespace Nalam360Enterprise.UI.VisualTests;

/// <summary>
/// Base class for all visual regression tests
/// Provides common setup and utilities for screenshot comparison
/// </summary>
[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class VisualTestBase : PageTest
{
    protected string BaseUrl => "http://localhost:5000";
    protected string ScreenshotsPath => Path.Combine(TestContext.CurrentContext.TestDirectory, "screenshots");
    protected string BaselinesPath => Path.Combine(TestContext.CurrentContext.TestDirectory, "baselines");

    [SetUp]
    public async Task SetupAsync()
    {
        // Ensure screenshots directory exists
        Directory.CreateDirectory(ScreenshotsPath);
        Directory.CreateDirectory(BaselinesPath);

        // Navigate to the sample app
        await Page.GotoAsync(BaseUrl, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 30000
        });

        // Wait for app to be fully loaded
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    /// <summary>
    /// Takes a screenshot and compares it with the baseline
    /// </summary>
    protected async Task AssertVisualAsync(string componentName, string? selector = null, PageScreenshotOptions? options = null)
    {
        var screenshotName = $"{componentName}.png";
        var screenshotPath = Path.Combine(ScreenshotsPath, screenshotName);
        var baselinePath = Path.Combine(BaselinesPath, screenshotName);

        // Take screenshot
        if (selector != null)
        {
            var element = await Page.QuerySelectorAsync(selector);
            Assert.That(element, Is.Not.Null, $"Element with selector '{selector}' not found");
            await element!.ScreenshotAsync(new LocatorScreenshotOptions
            {
                Path = screenshotPath,
                Timeout = 5000
            });
        }
        else
        {
            await Page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = screenshotPath,
                FullPage = options?.FullPage ?? false,
                Timeout = 5000
            });
        }

        // Compare with baseline if exists, otherwise create baseline
        if (File.Exists(baselinePath))
        {
            var baseline = await File.ReadAllBytesAsync(baselinePath);
            var screenshot = await File.ReadAllBytesAsync(screenshotPath);

            // Simple byte comparison (in production, use image diff library)
            var areSame = baseline.SequenceEqual(screenshot);
            if (!areSame)
            {
                TestContext.WriteLine($"Visual difference detected for {componentName}");
                TestContext.WriteLine($"Baseline: {baselinePath}");
                TestContext.WriteLine($"Current: {screenshotPath}");
                
                // In CI, this should fail the test
                // For local dev, just warn
                if (Environment.GetEnvironmentVariable("CI") == "true")
                {
                    Assert.Fail($"Visual regression detected for {componentName}");
                }
            }
        }
        else
        {
            // Create baseline
            File.Copy(screenshotPath, baselinePath);
            TestContext.WriteLine($"Created baseline for {componentName} at {baselinePath}");
        }
    }

    /// <summary>
    /// Waits for a component to be visible
    /// </summary>
    protected async Task WaitForComponentAsync(string selector, int timeout = 5000)
    {
        await Page.WaitForSelectorAsync(selector, new PageWaitForSelectorOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = timeout
        });
    }

    /// <summary>
    /// Sets the theme for visual testing
    /// </summary>
    protected async Task SetThemeAsync(string theme)
    {
        await Page.EvaluateAsync($@"
            localStorage.setItem('theme', '{theme}');
            location.reload();
        ");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    /// <summary>
    /// Tests component in multiple viewport sizes
    /// </summary>
    protected async Task TestResponsiveAsync(string componentName, string selector, int[] viewportWidths)
    {
        foreach (var width in viewportWidths)
        {
            await Page.SetViewportSizeAsync(width, 768);
            await Task.Delay(500); // Allow layout to settle
            await AssertVisualAsync($"{componentName}_{width}px", selector);
        }
    }
}
