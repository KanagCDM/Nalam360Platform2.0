using Microsoft.JSInterop;

namespace Nalam360Enterprise.UI.Core.Theming;

/// <summary>
/// Service for managing theme state and applying themes across the application
/// </summary>
public class ThemeService : IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ThemeConfiguration _configuration;
    private DotNetObjectReference<ThemeService>? _dotNetReference;

    public ThemeService(IJSRuntime jsRuntime, ThemeConfiguration configuration)
    {
        _jsRuntime = jsRuntime;
        _configuration = configuration;
    }

    /// <summary>
    /// Event raised when the theme changes
    /// </summary>
    public event EventHandler<Theme>? ThemeChanged;

    /// <summary>
    /// Gets the current theme
    /// </summary>
    public Theme CurrentTheme => _configuration.CurrentTheme;

    /// <summary>
    /// Gets the current text direction
    /// </summary>
    public TextDirection TextDirection => _configuration.TextDirection;

    /// <summary>
    /// Sets the theme
    /// </summary>
    public async Task SetThemeAsync(Theme theme)
    {
        if (_configuration.CurrentTheme != theme)
        {
            _configuration.CurrentTheme = theme;
            await ApplyThemeAsync();
            ThemeChanged?.Invoke(this, theme);
        }
    }

    /// <summary>
    /// Sets the text direction
    /// </summary>
    public async Task SetTextDirectionAsync(TextDirection direction)
    {
        if (_configuration.TextDirection != direction)
        {
            _configuration.TextDirection = direction;
            await ApplyTextDirectionAsync();
        }
    }

    /// <summary>
    /// Applies the current theme to the document
    /// </summary>
    private async Task ApplyThemeAsync()
    {
        var themeAttribute = _configuration.CurrentTheme switch
        {
            Theme.Dark => "dark",
            Theme.HighContrast => "high-contrast",
            Theme.Custom when !string.IsNullOrEmpty(_configuration.CustomThemeClass) => _configuration.CustomThemeClass,
            _ => "light"
        };

        try
        {
            await _jsRuntime.InvokeVoidAsync("eval", $"document.documentElement.setAttribute('data-theme', '{themeAttribute}')");
        }
        catch (JSException)
        {
            // Handle JS interop exceptions gracefully
        }
    }

    /// <summary>
    /// Applies the current text direction to the document
    /// </summary>
    private async Task ApplyTextDirectionAsync()
    {
        var dirAttribute = _configuration.TextDirection == TextDirection.Rtl ? "rtl" : "ltr";

        try
        {
            await _jsRuntime.InvokeVoidAsync("eval", $"document.documentElement.setAttribute('dir', '{dirAttribute}')");
        }
        catch (JSException)
        {
            // Handle JS interop exceptions gracefully
        }
    }

    /// <summary>
    /// Initializes the theme service and applies the initial theme
    /// </summary>
    public async Task InitializeAsync()
    {
        _dotNetReference = DotNetObjectReference.Create(this);

        await ApplyThemeAsync();
        await ApplyTextDirectionAsync();

        if (_configuration.AutoDetectTheme)
        {
            await DetectSystemThemeAsync();
        }
    }

    /// <summary>
    /// Detects and applies the system theme preference
    /// </summary>
    private async Task DetectSystemThemeAsync()
    {
        try
        {
            var prefersDark = await _jsRuntime.InvokeAsync<bool>("eval", 
                "window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches");
            
            if (prefersDark)
            {
                await SetThemeAsync(Theme.Dark);
            }
        }
        catch (JSException)
        {
            // Handle JS interop exceptions gracefully
        }
    }

    /// <summary>
    /// Toggles between light and dark themes
    /// </summary>
    public async Task ToggleThemeAsync()
    {
        var newTheme = _configuration.CurrentTheme == Theme.Light ? Theme.Dark : Theme.Light;
        await SetThemeAsync(newTheme);
    }

    public async ValueTask DisposeAsync()
    {
        _dotNetReference?.Dispose();
        await Task.CompletedTask;
    }
}
