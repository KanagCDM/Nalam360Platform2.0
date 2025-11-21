using System.Drawing;
using System.Globalization;

namespace Nalam360Enterprise.UI.Core.Utilities;

/// <summary>
/// Extension methods for color manipulation and conversion
/// </summary>
public static class ColorExtensions
{
    /// <summary>
    /// Converts a hex color string to RGB values
    /// </summary>
    /// <param name="hex">Hex color string (e.g., "#FF5733" or "FF5733")</param>
    /// <returns>Tuple of (R, G, B) values</returns>
    public static (int R, int G, int B) HexToRgb(string hex)
    {
        if (string.IsNullOrEmpty(hex))
        {
            throw new ArgumentException("Hex color cannot be null or empty", nameof(hex));
        }

        hex = hex.TrimStart('#');

        if (hex.Length != 6)
        {
            throw new ArgumentException("Hex color must be 6 characters (RRGGBB)", nameof(hex));
        }

        var r = int.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
        var g = int.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
        var b = int.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);

        return (r, g, b);
    }

    /// <summary>
    /// Converts RGB values to hex color string
    /// </summary>
    /// <param name="r">Red (0-255)</param>
    /// <param name="g">Green (0-255)</param>
    /// <param name="b">Blue (0-255)</param>
    /// <returns>Hex color string with # prefix</returns>
    public static string RgbToHex(int r, int g, int b)
    {
        ValidateRgbValue(r, nameof(r));
        ValidateRgbValue(g, nameof(g));
        ValidateRgbValue(b, nameof(b));

        return $"#{r:X2}{g:X2}{b:X2}";
    }

    /// <summary>
    /// Converts hex color to RGBA string
    /// </summary>
    /// <param name="hex">Hex color string</param>
    /// <param name="alpha">Alpha value (0.0-1.0)</param>
    /// <returns>RGBA string (e.g., "rgba(255, 87, 51, 0.5)")</returns>
    public static string HexToRgba(string hex, double alpha = 1.0)
    {
        var (r, g, b) = HexToRgb(hex);
        return $"rgba({r}, {g}, {b}, {alpha:F2})";
    }

    /// <summary>
    /// Converts RGB to HSL color space
    /// </summary>
    /// <param name="r">Red (0-255)</param>
    /// <param name="g">Green (0-255)</param>
    /// <param name="b">Blue (0-255)</param>
    /// <returns>Tuple of (H: 0-360, S: 0-100, L: 0-100)</returns>
    public static (double H, double S, double L) RgbToHsl(int r, int g, int b)
    {
        ValidateRgbValue(r, nameof(r));
        ValidateRgbValue(g, nameof(g));
        ValidateRgbValue(b, nameof(b));

        var rNorm = r / 255.0;
        var gNorm = g / 255.0;
        var bNorm = b / 255.0;

        var max = Math.Max(rNorm, Math.Max(gNorm, bNorm));
        var min = Math.Min(rNorm, Math.Min(gNorm, bNorm));
        var delta = max - min;

        var h = 0.0;
        var s = 0.0;
        var l = (max + min) / 2.0;

        if (delta != 0)
        {
            s = l > 0.5 ? delta / (2.0 - max - min) : delta / (max + min);

            if (max == rNorm)
            {
                h = ((gNorm - bNorm) / delta) + (gNorm < bNorm ? 6 : 0);
            }
            else if (max == gNorm)
            {
                h = ((bNorm - rNorm) / delta) + 2;
            }
            else
            {
                h = ((rNorm - gNorm) / delta) + 4;
            }

            h /= 6.0;
        }

        return (h * 360, s * 100, l * 100);
    }

    /// <summary>
    /// Converts HSL to RGB color space
    /// </summary>
    /// <param name="h">Hue (0-360)</param>
    /// <param name="s">Saturation (0-100)</param>
    /// <param name="l">Lightness (0-100)</param>
    /// <returns>Tuple of (R, G, B) values</returns>
    public static (int R, int G, int B) HslToRgb(double h, double s, double l)
    {
        h = h % 360 / 360.0;
        s = s / 100.0;
        l = l / 100.0;

        double r, g, b;

        if (s == 0)
        {
            r = g = b = l;
        }
        else
        {
            var q = l < 0.5 ? l * (1 + s) : l + s - l * s;
            var p = 2 * l - q;

            r = HueToRgb(p, q, h + 1.0 / 3.0);
            g = HueToRgb(p, q, h);
            b = HueToRgb(p, q, h - 1.0 / 3.0);
        }

        return ((int)Math.Round(r * 255), (int)Math.Round(g * 255), (int)Math.Round(b * 255));
    }

    /// <summary>
    /// Lightens a color by a percentage
    /// </summary>
    /// <param name="hex">Hex color string</param>
    /// <param name="percent">Percentage to lighten (0-100)</param>
    /// <returns>Lightened hex color</returns>
    public static string Lighten(string hex, double percent)
    {
        var (r, g, b) = HexToRgb(hex);
        var (h, s, l) = RgbToHsl(r, g, b);

        l = Math.Min(100, l + percent);

        var (newR, newG, newB) = HslToRgb(h, s, l);
        return RgbToHex(newR, newG, newB);
    }

    /// <summary>
    /// Darkens a color by a percentage
    /// </summary>
    /// <param name="hex">Hex color string</param>
    /// <param name="percent">Percentage to darken (0-100)</param>
    /// <returns>Darkened hex color</returns>
    public static string Darken(string hex, double percent)
    {
        var (r, g, b) = HexToRgb(hex);
        var (h, s, l) = RgbToHsl(r, g, b);

        l = Math.Max(0, l - percent);

        var (newR, newG, newB) = HslToRgb(h, s, l);
        return RgbToHex(newR, newG, newB);
    }

    /// <summary>
    /// Adjusts the saturation of a color
    /// </summary>
    /// <param name="hex">Hex color string</param>
    /// <param name="percent">Percentage to adjust (-100 to 100)</param>
    /// <returns>Adjusted hex color</returns>
    public static string Saturate(string hex, double percent)
    {
        var (r, g, b) = HexToRgb(hex);
        var (h, s, l) = RgbToHsl(r, g, b);

        s = Math.Max(0, Math.Min(100, s + percent));

        var (newR, newG, newB) = HslToRgb(h, s, l);
        return RgbToHex(newR, newG, newB);
    }

    /// <summary>
    /// Gets the complementary color (opposite on color wheel)
    /// </summary>
    /// <param name="hex">Hex color string</param>
    /// <returns>Complementary hex color</returns>
    public static string Complement(string hex)
    {
        var (r, g, b) = HexToRgb(hex);
        var (h, s, l) = RgbToHsl(r, g, b);

        h = (h + 180) % 360;

        var (newR, newG, newB) = HslToRgb(h, s, l);
        return RgbToHex(newR, newG, newB);
    }

    /// <summary>
    /// Mixes two colors together
    /// </summary>
    /// <param name="hex1">First hex color</param>
    /// <param name="hex2">Second hex color</param>
    /// <param name="weight">Weight of first color (0.0-1.0)</param>
    /// <returns>Mixed hex color</returns>
    public static string Mix(string hex1, string hex2, double weight = 0.5)
    {
        weight = Math.Max(0, Math.Min(1, weight));

        var (r1, g1, b1) = HexToRgb(hex1);
        var (r2, g2, b2) = HexToRgb(hex2);

        var r = (int)Math.Round(r1 * weight + r2 * (1 - weight));
        var g = (int)Math.Round(g1 * weight + g2 * (1 - weight));
        var b = (int)Math.Round(b1 * weight + b2 * (1 - weight));

        return RgbToHex(r, g, b);
    }

    /// <summary>
    /// Calculates the relative luminance of a color
    /// </summary>
    /// <param name="hex">Hex color string</param>
    /// <returns>Luminance value (0.0-1.0)</returns>
    public static double GetLuminance(string hex)
    {
        var (r, g, b) = HexToRgb(hex);

        var rSrgb = r / 255.0;
        var gSrgb = g / 255.0;
        var bSrgb = b / 255.0;

        var rLinear = rSrgb <= 0.03928 ? rSrgb / 12.92 : Math.Pow((rSrgb + 0.055) / 1.055, 2.4);
        var gLinear = gSrgb <= 0.03928 ? gSrgb / 12.92 : Math.Pow((gSrgb + 0.055) / 1.055, 2.4);
        var bLinear = bSrgb <= 0.03928 ? bSrgb / 12.92 : Math.Pow((bSrgb + 0.055) / 1.055, 2.4);

        return 0.2126 * rLinear + 0.7152 * gLinear + 0.0722 * bLinear;
    }

    /// <summary>
    /// Calculates the contrast ratio between two colors
    /// </summary>
    /// <param name="hex1">First hex color</param>
    /// <param name="hex2">Second hex color</param>
    /// <returns>Contrast ratio (1.0-21.0)</returns>
    public static double GetContrastRatio(string hex1, string hex2)
    {
        var lum1 = GetLuminance(hex1);
        var lum2 = GetLuminance(hex2);

        var lighter = Math.Max(lum1, lum2);
        var darker = Math.Min(lum1, lum2);

        return (lighter + 0.05) / (darker + 0.05);
    }

    /// <summary>
    /// Checks if a color meets WCAG AA contrast requirements
    /// </summary>
    /// <param name="foreground">Foreground hex color</param>
    /// <param name="background">Background hex color</param>
    /// <param name="isLargeText">Whether the text is large (18pt+ or 14pt+ bold)</param>
    /// <returns>True if contrast meets WCAG AA</returns>
    public static bool MeetsWcagAA(string foreground, string background, bool isLargeText = false)
    {
        var ratio = GetContrastRatio(foreground, background);
        return isLargeText ? ratio >= 3.0 : ratio >= 4.5;
    }

    /// <summary>
    /// Checks if a color meets WCAG AAA contrast requirements
    /// </summary>
    /// <param name="foreground">Foreground hex color</param>
    /// <param name="background">Background hex color</param>
    /// <param name="isLargeText">Whether the text is large (18pt+ or 14pt+ bold)</param>
    /// <returns>True if contrast meets WCAG AAA</returns>
    public static bool MeetsWcagAAA(string foreground, string background, bool isLargeText = false)
    {
        var ratio = GetContrastRatio(foreground, background);
        return isLargeText ? ratio >= 4.5 : ratio >= 7.0;
    }

    /// <summary>
    /// Gets the best contrasting text color (black or white) for a background
    /// </summary>
    /// <param name="backgroundColor">Background hex color</param>
    /// <returns>"#000000" or "#FFFFFF"</returns>
    public static string GetContrastingTextColor(string backgroundColor)
    {
        var luminance = GetLuminance(backgroundColor);
        return luminance > 0.5 ? "#000000" : "#FFFFFF";
    }

    private static double HueToRgb(double p, double q, double t)
    {
        if (t < 0) t += 1;
        if (t > 1) t -= 1;
        if (t < 1.0 / 6.0) return p + (q - p) * 6 * t;
        if (t < 1.0 / 2.0) return q;
        if (t < 2.0 / 3.0) return p + (q - p) * (2.0 / 3.0 - t) * 6;
        return p;
    }

    private static void ValidateRgbValue(int value, string paramName)
    {
        if (value < 0 || value > 255)
        {
            throw new ArgumentOutOfRangeException(paramName, $"RGB value must be between 0 and 255, got {value}");
        }
    }
}
