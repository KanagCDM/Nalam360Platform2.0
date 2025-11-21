using System.Text;
using System.Text.Json;

namespace Nalam360Enterprise.UI.Core.Utilities;

/// <summary>
/// Export utilities for converting data to various formats (CSV, JSON, XML)
/// Note: Excel and PDF export require additional NuGet packages
/// </summary>
public static class ExportHelper
{
    /// <summary>
    /// Converts a collection to CSV format
    /// </summary>
    /// <typeparam name="T">Type of objects in the collection</typeparam>
    /// <param name="data">Data to export</param>
    /// <param name="includeHeaders">Include column headers (default: true)</param>
    /// <param name="delimiter">Column delimiter (default: comma)</param>
    /// <returns>CSV string</returns>
    public static string ToCsv<T>(IEnumerable<T> data, bool includeHeaders = true, string delimiter = ",")
    {
        if (data == null || !data.Any())
            return string.Empty;

        var sb = new StringBuilder();
        var properties = typeof(T).GetProperties()
            .Where(p => p.CanRead && IsSimpleType(p.PropertyType))
            .ToList();

        // Write headers
        if (includeHeaders)
        {
            var headers = properties.Select(p => EscapeCsvValue(p.Name, delimiter));
            sb.AppendLine(string.Join(delimiter, headers));
        }

        // Write data rows
        foreach (var item in data)
        {
            var values = properties.Select(p =>
            {
                var value = p.GetValue(item);
                return EscapeCsvValue(value?.ToString() ?? string.Empty, delimiter);
            });
            sb.AppendLine(string.Join(delimiter, values));
        }

        return sb.ToString();
    }

    /// <summary>
    /// Converts a collection to TSV (Tab-Separated Values) format
    /// </summary>
    /// <typeparam name="T">Type of objects in the collection</typeparam>
    /// <param name="data">Data to export</param>
    /// <param name="includeHeaders">Include column headers (default: true)</param>
    /// <returns>TSV string</returns>
    public static string ToTsv<T>(IEnumerable<T> data, bool includeHeaders = true)
    {
        return ToCsv(data, includeHeaders, "\t");
    }

    /// <summary>
    /// Converts an object to JSON string
    /// </summary>
    /// <typeparam name="T">Type of object</typeparam>
    /// <param name="data">Data to export</param>
    /// <param name="indented">Format with indentation (default: true)</param>
    /// <returns>JSON string</returns>
    public static string ToJson<T>(T data, bool indented = true)
    {
        if (data == null)
            return "{}";

        var options = new JsonSerializerOptions
        {
            WriteIndented = indented,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        return JsonSerializer.Serialize(data, options);
    }

    /// <summary>
    /// Converts a collection to JSON array string
    /// </summary>
    /// <typeparam name="T">Type of objects in the collection</typeparam>
    /// <param name="data">Data to export</param>
    /// <param name="indented">Format with indentation (default: true)</param>
    /// <returns>JSON array string</returns>
    public static string ToJsonArray<T>(IEnumerable<T> data, bool indented = true)
    {
        if (data == null || !data.Any())
            return "[]";

        return ToJson(data.ToList(), indented);
    }

    /// <summary>
    /// Converts a collection to XML string
    /// </summary>
    /// <typeparam name="T">Type of objects in the collection</typeparam>
    /// <param name="data">Data to export</param>
    /// <param name="rootElementName">Root element name (default: "Items")</param>
    /// <param name="itemElementName">Item element name (default: "Item")</param>
    /// <returns>XML string</returns>
    public static string ToXml<T>(IEnumerable<T> data, string rootElementName = "Items", string itemElementName = "Item")
    {
        if (data == null || !data.Any())
            return $"<{rootElementName}></{rootElementName}>";

        var sb = new StringBuilder();
        sb.AppendLine($"<?xml version=\"1.0\" encoding=\"utf-8\"?>");
        sb.AppendLine($"<{rootElementName}>");

        var properties = typeof(T).GetProperties()
            .Where(p => p.CanRead && IsSimpleType(p.PropertyType))
            .ToList();

        foreach (var item in data)
        {
            sb.AppendLine($"  <{itemElementName}>");
            
            foreach (var prop in properties)
            {
                var value = prop.GetValue(item);
                var escapedValue = EscapeXmlValue(value?.ToString() ?? string.Empty);
                sb.AppendLine($"    <{prop.Name}>{escapedValue}</{prop.Name}>");
            }
            
            sb.AppendLine($"  </{itemElementName}>");
        }

        sb.AppendLine($"</{rootElementName}>");
        return sb.ToString();
    }

    /// <summary>
    /// Converts a collection to HTML table
    /// </summary>
    /// <typeparam name="T">Type of objects in the collection</typeparam>
    /// <param name="data">Data to export</param>
    /// <param name="includeHeaders">Include table headers (default: true)</param>
    /// <param name="tableClass">CSS class for table (default: null)</param>
    /// <returns>HTML table string</returns>
    public static string ToHtmlTable<T>(IEnumerable<T> data, bool includeHeaders = true, string? tableClass = null)
    {
        if (data == null || !data.Any())
            return string.Empty;

        var sb = new StringBuilder();
        var classAttr = !string.IsNullOrEmpty(tableClass) ? $" class=\"{tableClass}\"" : "";
        sb.AppendLine($"<table{classAttr}>");

        var properties = typeof(T).GetProperties()
            .Where(p => p.CanRead && IsSimpleType(p.PropertyType))
            .ToList();

        // Write headers
        if (includeHeaders)
        {
            sb.AppendLine("  <thead>");
            sb.AppendLine("    <tr>");
            foreach (var prop in properties)
            {
                sb.AppendLine($"      <th>{EscapeHtmlValue(prop.Name)}</th>");
            }
            sb.AppendLine("    </tr>");
            sb.AppendLine("  </thead>");
        }

        // Write data rows
        sb.AppendLine("  <tbody>");
        foreach (var item in data)
        {
            sb.AppendLine("    <tr>");
            foreach (var prop in properties)
            {
                var value = prop.GetValue(item);
                sb.AppendLine($"      <td>{EscapeHtmlValue(value?.ToString() ?? string.Empty)}</td>");
            }
            sb.AppendLine("    </tr>");
        }
        sb.AppendLine("  </tbody>");

        sb.AppendLine("</table>");
        return sb.ToString();
    }

    /// <summary>
    /// Converts a collection to Markdown table
    /// </summary>
    /// <typeparam name="T">Type of objects in the collection</typeparam>
    /// <param name="data">Data to export</param>
    /// <returns>Markdown table string</returns>
    public static string ToMarkdownTable<T>(IEnumerable<T> data)
    {
        if (data == null || !data.Any())
            return string.Empty;

        var sb = new StringBuilder();
        var properties = typeof(T).GetProperties()
            .Where(p => p.CanRead && IsSimpleType(p.PropertyType))
            .ToList();

        // Write headers
        sb.AppendLine("| " + string.Join(" | ", properties.Select(p => p.Name)) + " |");
        sb.AppendLine("| " + string.Join(" | ", properties.Select(_ => "---")) + " |");

        // Write data rows
        foreach (var item in data)
        {
            var values = properties.Select(p =>
            {
                var value = p.GetValue(item);
                return EscapeMarkdownValue(value?.ToString() ?? string.Empty);
            });
            sb.AppendLine("| " + string.Join(" | ", values) + " |");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Creates a downloadable file content from string data
    /// </summary>
    /// <param name="content">File content as string</param>
    /// <param name="fileName">File name</param>
    /// <param name="mimeType">MIME type (default: text/plain)</param>
    /// <returns>Tuple of (byte array, file name, MIME type)</returns>
    public static (byte[] Content, string FileName, string MimeType) CreateDownloadFile(
        string content,
        string fileName,
        string? mimeType = null)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        var mime = mimeType ?? FileHelper.GetMimeType(fileName);
        
        return (bytes, fileName, mime);
    }

    /// <summary>
    /// Converts data to CSV and prepares for download
    /// </summary>
    /// <typeparam name="T">Type of objects in the collection</typeparam>
    /// <param name="data">Data to export</param>
    /// <param name="fileName">File name (default: "export.csv")</param>
    /// <returns>Tuple of (byte array, file name, MIME type)</returns>
    public static (byte[] Content, string FileName, string MimeType) ExportToCsv<T>(
        IEnumerable<T> data,
        string fileName = "export.csv")
    {
        var csv = ToCsv(data);
        return CreateDownloadFile(csv, fileName, "text/csv");
    }

    /// <summary>
    /// Converts data to JSON and prepares for download
    /// </summary>
    /// <typeparam name="T">Type of objects in the collection</typeparam>
    /// <param name="data">Data to export</param>
    /// <param name="fileName">File name (default: "export.json")</param>
    /// <returns>Tuple of (byte array, file name, MIME type)</returns>
    public static (byte[] Content, string FileName, string MimeType) ExportToJson<T>(
        IEnumerable<T> data,
        string fileName = "export.json")
    {
        var json = ToJsonArray(data);
        return CreateDownloadFile(json, fileName, "application/json");
    }

    /// <summary>
    /// Converts data to XML and prepares for download
    /// </summary>
    /// <typeparam name="T">Type of objects in the collection</typeparam>
    /// <param name="data">Data to export</param>
    /// <param name="fileName">File name (default: "export.xml")</param>
    /// <returns>Tuple of (byte array, file name, MIME type)</returns>
    public static (byte[] Content, string FileName, string MimeType) ExportToXml<T>(
        IEnumerable<T> data,
        string fileName = "export.xml")
    {
        var xml = ToXml(data);
        return CreateDownloadFile(xml, fileName, "application/xml");
    }

    /// <summary>
    /// Converts data to HTML table and prepares for download
    /// </summary>
    /// <typeparam name="T">Type of objects in the collection</typeparam>
    /// <param name="data">Data to export</param>
    /// <param name="fileName">File name (default: "export.html")</param>
    /// <param name="pageTitle">HTML page title</param>
    /// <returns>Tuple of (byte array, file name, MIME type)</returns>
    public static (byte[] Content, string FileName, string MimeType) ExportToHtml<T>(
        IEnumerable<T> data,
        string fileName = "export.html",
        string pageTitle = "Export")
    {
        var table = ToHtmlTable(data, tableClass: "export-table");
        var html = $@"<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <title>{EscapeHtmlValue(pageTitle)}</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 20px; }}
        .export-table {{ border-collapse: collapse; width: 100%; }}
        .export-table th, .export-table td {{ border: 1px solid #ddd; padding: 8px; text-align: left; }}
        .export-table th {{ background-color: #4CAF50; color: white; }}
        .export-table tr:nth-child(even) {{ background-color: #f2f2f2; }}
    </style>
</head>
<body>
    <h1>{EscapeHtmlValue(pageTitle)}</h1>
    {table}
</body>
</html>";
        
        return CreateDownloadFile(html, fileName, "text/html");
    }

    #region Helper Methods

    private static bool IsSimpleType(Type type)
    {
        return type.IsPrimitive
            || type.IsEnum
            || type == typeof(string)
            || type == typeof(decimal)
            || type == typeof(DateTime)
            || type == typeof(DateTimeOffset)
            || type == typeof(TimeSpan)
            || type == typeof(Guid)
            || Nullable.GetUnderlyingType(type) != null;
    }

    private static string EscapeCsvValue(string value, string delimiter)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        // Escape if contains delimiter, quotes, or newlines
        if (value.Contains(delimiter) || value.Contains("\"") || value.Contains("\n") || value.Contains("\r"))
        {
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        return value;
    }

    private static string EscapeXmlValue(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        return value
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&apos;");
    }

    private static string EscapeHtmlValue(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        return value
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&#39;");
    }

    private static string EscapeMarkdownValue(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        return value
            .Replace("|", "\\|")
            .Replace("\n", " ")
            .Replace("\r", "");
    }

    #endregion

    /// <summary>
    /// Note: For Excel export, install ClosedXML or EPPlus package:
    /// dotnet add package ClosedXML
    /// 
    /// Example implementation:
    /// public static byte[] ToExcel<T>(IEnumerable<T> data, string sheetName = "Sheet1")
    /// {
    ///     using var workbook = new XLWorkbook();
    ///     var worksheet = workbook.Worksheets.Add(sheetName);
    ///     worksheet.Cell(1, 1).InsertTable(data);
    ///     
    ///     using var stream = new MemoryStream();
    ///     workbook.SaveAs(stream);
    ///     return stream.ToArray();
    /// }
    /// </summary>
    public static string GetExcelExportInstructions()
    {
        return @"To enable Excel export, install ClosedXML NuGet package:
dotnet add package ClosedXML

Then implement ToExcel method using ClosedXML.";
    }

    /// <summary>
    /// Note: For PDF export, install QuestPDF package:
    /// dotnet add package QuestPDF
    /// 
    /// Example implementation:
    /// public static byte[] ToPdf<T>(IEnumerable<T> data, string title = "Report")
    /// {
    ///     var document = Document.Create(container => {
    ///         container.Page(page => {
    ///             page.Content().Table(table => {
    ///                 // Define columns and rows
    ///             });
    ///         });
    ///     });
    ///     
    ///     return document.GeneratePdf();
    /// }
    /// </summary>
    public static string GetPdfExportInstructions()
    {
        return @"To enable PDF export, install QuestPDF NuGet package:
dotnet add package QuestPDF

Then implement ToPdf method using QuestPDF.";
    }
}
