namespace Nalam360Enterprise.UI.Core.Theming;

/// <summary>
/// Represents the available theme variants for the component library
/// </summary>
public enum Theme
{
    /// <summary>
    /// Light theme (default)
    /// </summary>
    Light,

    /// <summary>
    /// Dark theme
    /// </summary>
    Dark,

    /// <summary>
    /// High contrast theme for accessibility
    /// </summary>
    HighContrast,

    /// <summary>
    /// Custom theme defined by user
    /// </summary>
    Custom
}

/// <summary>
/// Represents text direction for internationalization
/// </summary>
public enum TextDirection
{
    /// <summary>
    /// Left-to-right text direction (default)
    /// </summary>
    Ltr,

    /// <summary>
    /// Right-to-left text direction (for Arabic, Hebrew, etc.)
    /// </summary>
    Rtl
}

/// <summary>
/// Configuration for the Nalam360 Enterprise UI theme
/// </summary>
public class ThemeConfiguration
{
    /// <summary>
    /// Gets or sets the current theme
    /// </summary>
    public Theme CurrentTheme { get; set; } = Theme.Light;

    /// <summary>
    /// Gets or sets the text direction
    /// </summary>
    public TextDirection TextDirection { get; set; } = TextDirection.Ltr;

    /// <summary>
    /// Gets or sets whether to use Syncfusion's built-in themes
    /// </summary>
    public bool UseSyncfusionTheme { get; set; } = true;

    /// <summary>
    /// Gets or sets the Syncfusion theme name (e.g., "material", "bootstrap5", "fluent")
    /// </summary>
    public string SyncfusionThemeName { get; set; } = "material";

    /// <summary>
    /// Gets or sets custom CSS class to apply to root element
    /// </summary>
    public string? CustomThemeClass { get; set; }

    /// <summary>
    /// Gets or sets whether to enable automatic theme detection based on system preferences
    /// </summary>
    public bool AutoDetectTheme { get; set; } = false;

    /// <summary>
    /// Gets or sets custom color overrides
    /// </summary>
    public Dictionary<string, string> CustomColors { get; set; } = new();
}
