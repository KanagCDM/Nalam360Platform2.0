namespace Nalam360Enterprise.UI.Core.Utilities;

/// <summary>
/// File operations and utilities for the Nalam360 Enterprise UI library
/// </summary>
public static class FileHelper
{
    private static readonly Dictionary<string, string> MimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        // Images
        { ".jpg", "image/jpeg" },
        { ".jpeg", "image/jpeg" },
        { ".png", "image/png" },
        { ".gif", "image/gif" },
        { ".bmp", "image/bmp" },
        { ".webp", "image/webp" },
        { ".svg", "image/svg+xml" },
        { ".ico", "image/x-icon" },
        
        // Documents
        { ".pdf", "application/pdf" },
        { ".doc", "application/msword" },
        { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
        { ".xls", "application/vnd.ms-excel" },
        { ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
        { ".ppt", "application/vnd.ms-powerpoint" },
        { ".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation" },
        { ".txt", "text/plain" },
        { ".csv", "text/csv" },
        { ".rtf", "application/rtf" },
        
        // Archives
        { ".zip", "application/zip" },
        { ".rar", "application/x-rar-compressed" },
        { ".7z", "application/x-7z-compressed" },
        { ".tar", "application/x-tar" },
        { ".gz", "application/gzip" },
        
        // Audio
        { ".mp3", "audio/mpeg" },
        { ".wav", "audio/wav" },
        { ".ogg", "audio/ogg" },
        { ".m4a", "audio/mp4" },
        
        // Video
        { ".mp4", "video/mp4" },
        { ".avi", "video/x-msvideo" },
        { ".mov", "video/quicktime" },
        { ".wmv", "video/x-ms-wmv" },
        { ".flv", "video/x-flv" },
        { ".webm", "video/webm" },
        
        // Web
        { ".html", "text/html" },
        { ".htm", "text/html" },
        { ".css", "text/css" },
        { ".js", "application/javascript" },
        { ".json", "application/json" },
        { ".xml", "application/xml" },
        
        // Programming
        { ".cs", "text/x-csharp" },
        { ".java", "text/x-java" },
        { ".py", "text/x-python" },
        { ".cpp", "text/x-c++src" },
        { ".c", "text/x-csrc" },
        { ".h", "text/x-chdr" },
        
        // Default
        { "", "application/octet-stream" }
    };

    /// <summary>
    /// Formats file size from bytes to human-readable format
    /// </summary>
    /// <param name="bytes">File size in bytes</param>
    /// <param name="decimalPlaces">Number of decimal places (default: 2)</param>
    /// <returns>Formatted file size (e.g., "1.5 MB")</returns>
    public static string FormatFileSize(long bytes, int decimalPlaces = 2)
    {
        if (bytes < 0)
            return "0 B";

        string[] sizes = { "B", "KB", "MB", "GB", "TB", "PB" };
        
        if (bytes == 0)
            return "0 B";

        int order = 0;
        double size = bytes;

        while (size >= 1024 && order < sizes.Length - 1)
        {
            order++;
            size /= 1024;
        }

        return $"{size.ToString($"F{decimalPlaces}")} {sizes[order]}";
    }

    /// <summary>
    /// Gets the MIME type for a file based on its extension
    /// </summary>
    /// <param name="fileName">File name or path</param>
    /// <returns>MIME type string</returns>
    public static string GetMimeType(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return "application/octet-stream";

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        
        return MimeTypes.TryGetValue(extension, out var mimeType) 
            ? mimeType 
            : "application/octet-stream";
    }

    /// <summary>
    /// Validates if a file extension is allowed
    /// </summary>
    /// <param name="fileName">File name or path</param>
    /// <param name="allowedExtensions">Array of allowed extensions (e.g., ".jpg", ".png")</param>
    /// <returns>True if extension is allowed</returns>
    public static bool IsValidExtension(string fileName, params string[] allowedExtensions)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return false;

        if (allowedExtensions == null || allowedExtensions.Length == 0)
            return true;

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        
        return allowedExtensions.Any(ext => 
            ext.ToLowerInvariant() == extension);
    }

    /// <summary>
    /// Checks if a file is an image based on extension
    /// </summary>
    /// <param name="fileName">File name or path</param>
    /// <returns>True if file is an image</returns>
    public static bool IsImage(string fileName)
    {
        return IsValidExtension(fileName, ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".svg");
    }

    /// <summary>
    /// Checks if a file is a document based on extension
    /// </summary>
    /// <param name="fileName">File name or path</param>
    /// <returns>True if file is a document</returns>
    public static bool IsDocument(string fileName)
    {
        return IsValidExtension(fileName, ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".txt", ".csv");
    }

    /// <summary>
    /// Checks if a file is a video based on extension
    /// </summary>
    /// <param name="fileName">File name or path</param>
    /// <returns>True if file is a video</returns>
    public static bool IsVideo(string fileName)
    {
        return IsValidExtension(fileName, ".mp4", ".avi", ".mov", ".wmv", ".flv", ".webm");
    }

    /// <summary>
    /// Checks if a file is an audio file based on extension
    /// </summary>
    /// <param name="fileName">File name or path</param>
    /// <returns>True if file is an audio file</returns>
    public static bool IsAudio(string fileName)
    {
        return IsValidExtension(fileName, ".mp3", ".wav", ".ogg", ".m4a");
    }

    /// <summary>
    /// Sanitizes a file name by removing invalid characters
    /// </summary>
    /// <param name="fileName">Original file name</param>
    /// <param name="replacement">Replacement character for invalid chars (default: "_")</param>
    /// <returns>Sanitized file name</returns>
    public static string SanitizeFileName(string fileName, string replacement = "_")
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return string.Empty;

        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = fileName;

        foreach (var c in invalidChars)
        {
            sanitized = sanitized.Replace(c.ToString(), replacement);
        }

        // Remove leading/trailing spaces and dots
        sanitized = sanitized.Trim(' ', '.');

        // Limit length to 255 characters (common file system limit)
        if (sanitized.Length > 255)
        {
            var extension = Path.GetExtension(sanitized);
            var nameWithoutExt = Path.GetFileNameWithoutExtension(sanitized);
            var maxNameLength = 255 - extension.Length;
            sanitized = nameWithoutExt.Substring(0, Math.Min(nameWithoutExt.Length, maxNameLength)) + extension;
        }

        return sanitized;
    }

    /// <summary>
    /// Generates a unique file name by appending a number if file exists
    /// </summary>
    /// <param name="fileName">Original file name</param>
    /// <param name="existingFiles">List of existing file names</param>
    /// <returns>Unique file name</returns>
    public static string GetUniqueFileName(string fileName, IEnumerable<string> existingFiles)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return Guid.NewGuid().ToString("N");

        var existing = existingFiles?.ToHashSet(StringComparer.OrdinalIgnoreCase) 
            ?? new HashSet<string>();

        if (!existing.Contains(fileName))
            return fileName;

        var nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
        var extension = Path.GetExtension(fileName);
        var counter = 1;

        string newName;
        do
        {
            newName = $"{nameWithoutExt} ({counter}){extension}";
            counter++;
        } while (existing.Contains(newName));

        return newName;
    }

    /// <summary>
    /// Validates file size against maximum allowed size
    /// </summary>
    /// <param name="fileSize">File size in bytes</param>
    /// <param name="maxSizeInMb">Maximum allowed size in megabytes</param>
    /// <returns>True if file size is within limit</returns>
    public static bool IsValidFileSize(long fileSize, double maxSizeInMb)
    {
        if (fileSize < 0)
            return false;

        var maxSizeInBytes = (long)(maxSizeInMb * 1024 * 1024);
        return fileSize <= maxSizeInBytes;
    }

    /// <summary>
    /// Gets file extension without the dot
    /// </summary>
    /// <param name="fileName">File name or path</param>
    /// <returns>Extension without dot (e.g., "jpg" instead of ".jpg")</returns>
    public static string GetExtensionWithoutDot(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return string.Empty;

        var extension = Path.GetExtension(fileName);
        return extension.TrimStart('.');
    }

    /// <summary>
    /// Converts base64 string to byte array
    /// </summary>
    /// <param name="base64String">Base64 encoded string</param>
    /// <returns>Byte array</returns>
    public static byte[] Base64ToBytes(string base64String)
    {
        if (string.IsNullOrWhiteSpace(base64String))
            return Array.Empty<byte>();

        // Remove data URL prefix if present (e.g., "data:image/png;base64,")
        if (base64String.Contains(","))
        {
            base64String = base64String.Substring(base64String.IndexOf(',') + 1);
        }

        try
        {
            return Convert.FromBase64String(base64String);
        }
        catch
        {
            return Array.Empty<byte>();
        }
    }

    /// <summary>
    /// Converts byte array to base64 string
    /// </summary>
    /// <param name="bytes">Byte array</param>
    /// <param name="includeDataUrl">Include data URL prefix (default: false)</param>
    /// <param name="mimeType">MIME type for data URL</param>
    /// <returns>Base64 encoded string</returns>
    public static string BytesToBase64(byte[] bytes, bool includeDataUrl = false, string? mimeType = null)
    {
        if (bytes == null || bytes.Length == 0)
            return string.Empty;

        var base64 = Convert.ToBase64String(bytes);

        if (includeDataUrl)
        {
            var mime = mimeType ?? "application/octet-stream";
            return $"data:{mime};base64,{base64}";
        }

        return base64;
    }

    /// <summary>
    /// Gets a safe file name for download (replaces spaces with underscores, removes special chars)
    /// </summary>
    /// <param name="fileName">Original file name</param>
    /// <returns>Safe file name for download</returns>
    public static string GetSafeDownloadFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return "download";

        var sanitized = SanitizeFileName(fileName);
        
        // Replace spaces with underscores for better URL compatibility
        sanitized = sanitized.Replace(" ", "_");
        
        // Remove any remaining special characters except dots, dashes, and underscores
        sanitized = new string(sanitized.Where(c => 
            char.IsLetterOrDigit(c) || c == '.' || c == '-' || c == '_').ToArray());

        return string.IsNullOrWhiteSpace(sanitized) ? "download" : sanitized;
    }

    /// <summary>
    /// Gets icon class or emoji for file type
    /// </summary>
    /// <param name="fileName">File name or path</param>
    /// <returns>Emoji representing file type</returns>
    public static string GetFileIcon(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return "📄";

        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        return extension switch
        {
            ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or ".webp" => "🖼️",
            ".pdf" => "📕",
            ".doc" or ".docx" => "📘",
            ".xls" or ".xlsx" => "📗",
            ".ppt" or ".pptx" => "📙",
            ".txt" => "📄",
            ".csv" => "📊",
            ".zip" or ".rar" or ".7z" or ".tar" or ".gz" => "🗜️",
            ".mp3" or ".wav" or ".ogg" or ".m4a" => "🎵",
            ".mp4" or ".avi" or ".mov" or ".wmv" or ".flv" or ".webm" => "🎬",
            ".html" or ".htm" => "🌐",
            ".css" => "🎨",
            ".js" => "📜",
            ".json" => "📋",
            ".xml" => "📰",
            ".cs" => "💻",
            ".java" => "☕",
            ".py" => "🐍",
            _ => "📄"
        };
    }

    /// <summary>
    /// Parses file size string to bytes (e.g., "1.5 MB" to 1572864)
    /// </summary>
    /// <param name="sizeString">Size string (e.g., "1.5 MB", "500 KB")</param>
    /// <returns>Size in bytes, or null if parsing fails</returns>
    public static long? ParseFileSize(string sizeString)
    {
        if (string.IsNullOrWhiteSpace(sizeString))
            return null;

        var parts = sizeString.Trim().Split(' ');
        if (parts.Length != 2)
            return null;

        if (!double.TryParse(parts[0], out var value))
            return null;

        var unit = parts[1].ToUpperInvariant();
        var multiplier = unit switch
        {
            "B" => 1L,
            "KB" => 1024L,
            "MB" => 1024L * 1024,
            "GB" => 1024L * 1024 * 1024,
            "TB" => 1024L * 1024 * 1024 * 1024,
            _ => 0L
        };

        return multiplier == 0 ? null : (long)(value * multiplier);
    }
}
