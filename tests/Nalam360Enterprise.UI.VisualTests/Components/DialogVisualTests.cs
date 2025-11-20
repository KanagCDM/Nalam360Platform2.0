using Microsoft.Playwright;
using NUnit.Framework;

namespace Nalam360Enterprise.UI.VisualTests.Components;

[TestFixture]
public class DialogVisualTests : PlaywrightTestBase
{
    [Test]
    public async Task Dialog_DefaultState_MatchesBaseline()
    {
        await NavigateToComponentAsync("components/dialog");
        
        // Click button to open dialog
        await Page.ClickAsync("[data-testid='open-dialog-btn']");
        
        // Wait for dialog to appear
        var dialog = Page.Locator("[data-testid='dialog-default']");
        await Expect(dialog).ToBeVisibleAsync();
        await Page.WaitForTimeoutAsync(500); // Wait for animation
        
        await AssertVisualMatchAsync("dialog-default", dialog);
    }

    [Test]
    public async Task Dialog_WithBackdrop_MatchesBaseline()
    {
        await NavigateToComponentAsync("components/dialog");
        
        await Page.ClickAsync("[data-testid='open-dialog-backdrop-btn']");
        await Page.WaitForTimeoutAsync(500);
        
        // Screenshot full page to capture backdrop
        await AssertVisualMatchAsync("dialog-with-backdrop");
    }

    [Test]
    public async Task Dialog_Sizes_MatchBaseline()
    {
        await NavigateToComponentAsync("components/dialog");
        
        var sizes = new[] { "small", "medium", "large", "fullscreen" };
        
        foreach (var size in sizes)
        {
            await Page.ClickAsync($"[data-testid='open-dialog-{size}-btn']");
            await Page.WaitForTimeoutAsync(500);
            
            var dialog = Page.Locator($"[data-testid='dialog-{size}']");
            await Expect(dialog).ToBeVisibleAsync();
            
            await AssertVisualMatchAsync($"dialog-size-{size}", dialog);
            
            // Close dialog
            await Page.ClickAsync($"[data-testid='dialog-{size}'] .close-btn");
            await Page.WaitForTimeoutAsync(300);
        }
    }

    [Test]
    public async Task Dialog_WithFooter_MatchesBaseline()
    {
        await NavigateToComponentAsync("components/dialog");
        
        await Page.ClickAsync("[data-testid='open-dialog-footer-btn']");
        await Page.WaitForTimeoutAsync(500);
        
        var dialog = Page.Locator("[data-testid='dialog-footer']");
        await Expect(dialog).ToBeVisibleAsync();
        
        await AssertVisualMatchAsync("dialog-with-footer", dialog);
    }

    [Test]
    public async Task Dialog_Draggable_MatchesBaseline()
    {
        await NavigateToComponentAsync("components/dialog");
        
        await Page.ClickAsync("[data-testid='open-dialog-draggable-btn']");
        await Page.WaitForTimeoutAsync(500);
        
        var dialog = Page.Locator("[data-testid='dialog-draggable']");
        var header = dialog.Locator(".e-dlg-header");
        
        // Drag dialog
        await header.DragToAsync(dialog, new() { TargetPosition = new() { X = 200, Y = 100 } });
        await Page.WaitForTimeoutAsync(300);
        
        await AssertVisualMatchAsync("dialog-draggable", dialog);
    }

    [Test]
    public async Task Dialog_AllThemes_MatchBaseline()
    {
        await TestAllThemesAsync("components/dialog", "dialog-themes");
    }
}
