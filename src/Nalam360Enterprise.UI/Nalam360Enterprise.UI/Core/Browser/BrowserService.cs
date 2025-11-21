using Microsoft.JSInterop;

namespace Nalam360Enterprise.UI.Core.Browser;

/// <summary>
/// Service for browser-specific operations (clipboard, downloads, print, etc.)
/// </summary>
public interface IBrowserService
{
    /// <summary>
    /// Copies text to clipboard
    /// </summary>
    /// <param name="text">Text to copy</param>
    Task<bool> CopyToClipboardAsync(string text);

    /// <summary>
    /// Reads text from clipboard
    /// </summary>
    /// <returns>Clipboard text</returns>
    Task<string?> ReadFromClipboardAsync();

    /// <summary>
    /// Downloads a file
    /// </summary>
    /// <param name="fileName">File name</param>
    /// <param name="content">File content</param>
    /// <param name="mimeType">MIME type</param>
    Task DownloadFileAsync(string fileName, string content, string mimeType = "application/octet-stream");

    /// <summary>
    /// Downloads a file from base64 content
    /// </summary>
    /// <param name="fileName">File name</param>
    /// <param name="base64Content">Base64 encoded content</param>
    /// <param name="mimeType">MIME type</param>
    Task DownloadBase64FileAsync(string fileName, string base64Content, string mimeType = "application/octet-stream");

    /// <summary>
    /// Prints the current page
    /// </summary>
    Task PrintPageAsync();

    /// <summary>
    /// Prints a specific element
    /// </summary>
    /// <param name="elementSelector">CSS selector for element</param>
    Task PrintElementAsync(string elementSelector);

    /// <summary>
    /// Gets browser information
    /// </summary>
    /// <returns>Browser information</returns>
    Task<BrowserInfo> GetBrowserInfoAsync();

    /// <summary>
    /// Gets viewport size
    /// </summary>
    /// <returns>Viewport dimensions</returns>
    Task<ViewportSize> GetViewportSizeAsync();

    /// <summary>
    /// Scrolls to an element
    /// </summary>
    /// <param name="elementSelector">CSS selector for element</param>
    /// <param name="behavior">Scroll behavior (smooth or auto)</param>
    Task ScrollToElementAsync(string elementSelector, string behavior = "smooth");

    /// <summary>
    /// Scrolls to top of page
    /// </summary>
    /// <param name="behavior">Scroll behavior (smooth or auto)</param>
    Task ScrollToTopAsync(string behavior = "smooth");

    /// <summary>
    /// Scrolls to bottom of page
    /// </summary>
    /// <param name="behavior">Scroll behavior (smooth or auto)</param>
    Task ScrollToBottomAsync(string behavior = "smooth");

    /// <summary>
    /// Focuses an element
    /// </summary>
    /// <param name="elementSelector">CSS selector for element</param>
    Task FocusElementAsync(string elementSelector);
}

/// <summary>
/// Browser information
/// </summary>
public class BrowserInfo
{
    public string UserAgent { get; set; } = string.Empty;
    public bool IsChrome { get; set; }
    public bool IsFirefox { get; set; }
    public bool IsSafari { get; set; }
    public bool IsEdge { get; set; }
    public bool IsOpera { get; set; }
    public bool IsMobile { get; set; }
    public bool IsTouch { get; set; }
    public string Language { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
}

/// <summary>
/// Viewport size
/// </summary>
public class ViewportSize
{
    public int Width { get; set; }
    public int Height { get; set; }
}

/// <summary>
/// Default implementation of IBrowserService
/// </summary>
public class BrowserService : IBrowserService
{
    private readonly IJSRuntime _jsRuntime;

    public BrowserService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
    }

    public async Task<bool> CopyToClipboardAsync(string text)
    {
        try
        {
            var result = await _jsRuntime.InvokeAsync<dynamic>("Nalam360Interop.clipboard.copyToClipboard", text);
            return result.success;
        }
        catch
        {
            return false;
        }
    }

    public async Task<string?> ReadFromClipboardAsync()
    {
        try
        {
            var result = await _jsRuntime.InvokeAsync<dynamic>("Nalam360Interop.clipboard.readFromClipboard");
            return result.success ? result.data : null;
        }
        catch
        {
            return null;
        }
    }

    public async Task DownloadFileAsync(string fileName, string content, string mimeType = "application/octet-stream")
    {
        await _jsRuntime.InvokeVoidAsync("Nalam360Interop.file.downloadFile", fileName, content, mimeType);
    }

    public async Task DownloadBase64FileAsync(string fileName, string base64Content, string mimeType = "application/octet-stream")
    {
        await _jsRuntime.InvokeVoidAsync("Nalam360Interop.file.downloadBase64", fileName, base64Content, mimeType);
    }

    public async Task PrintPageAsync()
    {
        await _jsRuntime.InvokeVoidAsync("Nalam360Interop.print.printPage");
    }

    public async Task PrintElementAsync(string elementSelector)
    {
        await _jsRuntime.InvokeVoidAsync("Nalam360Interop.print.printElement", elementSelector);
    }

    public async Task<BrowserInfo> GetBrowserInfoAsync()
    {
        var result = await _jsRuntime.InvokeAsync<BrowserInfo>("Nalam360Interop.browser.getBrowserInfo");
        return result;
    }

    public async Task<ViewportSize> GetViewportSizeAsync()
    {
        var result = await _jsRuntime.InvokeAsync<ViewportSize>("Nalam360Interop.browser.getViewportSize");
        return result;
    }

    public async Task ScrollToElementAsync(string elementSelector, string behavior = "smooth")
    {
        await _jsRuntime.InvokeVoidAsync("Nalam360Interop.scroll.scrollToElement", elementSelector, behavior);
    }

    public async Task ScrollToTopAsync(string behavior = "smooth")
    {
        await _jsRuntime.InvokeVoidAsync("Nalam360Interop.scroll.scrollToTop", behavior);
    }

    public async Task ScrollToBottomAsync(string behavior = "smooth")
    {
        await _jsRuntime.InvokeVoidAsync("Nalam360Interop.scroll.scrollToBottom", behavior);
    }

    public async Task FocusElementAsync(string elementSelector)
    {
        await _jsRuntime.InvokeVoidAsync("Nalam360Interop.focus.focusElement", elementSelector);
    }
}
