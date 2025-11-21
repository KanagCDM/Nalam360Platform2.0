using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Nalam360Enterprise.UI.Core.Utilities;

/// <summary>
/// Extension methods for string manipulation
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Truncates a string to the specified length
    /// </summary>
    /// <param name="value">The string to truncate</param>
    /// <param name="maxLength">Maximum length</param>
    /// <param name="suffix">Suffix to append (default: "...")</param>
    /// <returns>Truncated string</returns>
    public static string Truncate(this string? value, int maxLength, string suffix = "...")
    {
        if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
        {
            return value ?? string.Empty;
        }

        return value.Substring(0, maxLength - suffix.Length) + suffix;
    }

    /// <summary>
    /// Converts a string to title case
    /// </summary>
    /// <param name="value">The string to convert</param>
    /// <returns>Title-cased string</returns>
    public static string ToTitleCase(this string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value.ToLower());
    }

    /// <summary>
    /// Converts a string to kebab-case
    /// </summary>
    /// <param name="value">The string to convert</param>
    /// <returns>Kebab-cased string</returns>
    public static string ToKebabCase(this string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        return Regex.Replace(value, "([a-z])([A-Z])", "$1-$2")
            .Replace(" ", "-")
            .ToLower();
    }

    /// <summary>
    /// Converts a string to camelCase
    /// </summary>
    /// <param name="value">The string to convert</param>
    /// <returns>CamelCase string</returns>
    public static string ToCamelCase(this string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        var words = value.Split(new[] { ' ', '-', '_' }, StringSplitOptions.RemoveEmptyEntries);
        if (words.Length == 0)
        {
            return string.Empty;
        }

        var result = new StringBuilder(words[0].ToLower());
        for (int i = 1; i < words.Length; i++)
        {
            result.Append(char.ToUpper(words[i][0]));
            result.Append(words[i].Substring(1).ToLower());
        }

        return result.ToString();
    }

    /// <summary>
    /// Converts a string to PascalCase
    /// </summary>
    /// <param name="value">The string to convert</param>
    /// <returns>PascalCase string</returns>
    public static string ToPascalCase(this string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        var words = value.Split(new[] { ' ', '-', '_' }, StringSplitOptions.RemoveEmptyEntries);
        var result = new StringBuilder();

        foreach (var word in words)
        {
            result.Append(char.ToUpper(word[0]));
            result.Append(word.Substring(1).ToLower());
        }

        return result.ToString();
    }

    /// <summary>
    /// Removes all whitespace from a string
    /// </summary>
    /// <param name="value">The string to process</param>
    /// <returns>String without whitespace</returns>
    public static string RemoveWhitespace(this string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        return new string(value.Where(c => !char.IsWhiteSpace(c)).ToArray());
    }

    /// <summary>
    /// Checks if a string contains only digits
    /// </summary>
    /// <param name="value">The string to check</param>
    /// <returns>True if contains only digits</returns>
    public static bool IsNumeric(this string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        return value.All(char.IsDigit);
    }

    /// <summary>
    /// Checks if a string is a valid email address
    /// </summary>
    /// <param name="value">The string to check</param>
    /// <returns>True if valid email</returns>
    public static bool IsValidEmail(this string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        try
        {
            var addr = new System.Net.Mail.MailAddress(value);
            return addr.Address == value;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Converts a string to Base64
    /// </summary>
    /// <param name="value">The string to encode</param>
    /// <returns>Base64-encoded string</returns>
    public static string ToBase64(this string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        var bytes = Encoding.UTF8.GetBytes(value);
        return Convert.ToBase64String(bytes);
    }

    /// <summary>
    /// Converts a Base64 string back to regular string
    /// </summary>
    /// <param name="value">The Base64 string</param>
    /// <returns>Decoded string</returns>
    public static string FromBase64(this string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        try
        {
            var bytes = Convert.FromBase64String(value);
            return Encoding.UTF8.GetString(bytes);
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Pluralizes a word based on count
    /// </summary>
    /// <param name="word">The word to pluralize</param>
    /// <param name="count">The count</param>
    /// <param name="pluralForm">Optional custom plural form</param>
    /// <returns>Singular or plural form</returns>
    public static string Pluralize(this string word, int count, string? pluralForm = null)
    {
        if (count == 1)
        {
            return word;
        }

        if (!string.IsNullOrEmpty(pluralForm))
        {
            return pluralForm;
        }

        // Simple pluralization rules
        if (word.EndsWith("y"))
        {
            return word.Substring(0, word.Length - 1) + "ies";
        }
        if (word.EndsWith("s") || word.EndsWith("x") || word.EndsWith("z") || word.EndsWith("ch") || word.EndsWith("sh"))
        {
            return word + "es";
        }

        return word + "s";
    }

    /// <summary>
    /// Converts a string to a slug (URL-friendly format)
    /// </summary>
    /// <param name="value">The string to convert</param>
    /// <returns>URL-friendly slug</returns>
    public static string ToSlug(this string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        // Convert to lowercase
        value = value.ToLower();

        // Remove accents
        value = RemoveAccents(value);

        // Replace spaces and underscores with hyphens
        value = Regex.Replace(value, @"[\s_]", "-");

        // Remove invalid chars
        value = Regex.Replace(value, @"[^a-z0-9\-]", "");

        // Replace multiple hyphens with single hyphen
        value = Regex.Replace(value, @"-+", "-");

        // Trim hyphens from start and end
        value = value.Trim('-');

        return value;
    }

    /// <summary>
    /// Removes accents from characters
    /// </summary>
    private static string RemoveAccents(string text)
    {
        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();

        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
    }
}
