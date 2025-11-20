using System;
using System.Collections.Generic;

namespace Nalam360Enterprise.UI.Components.Enterprise;

/// <summary>
/// Represents a type of report element
/// </summary>
public enum ReportElementType
{
    Text,
    Table,
    Chart,
    Image,
    Line,
    Rectangle,
    Barcode,
    QRCode,
    PageBreak,
    Header,
    Footer
}

/// <summary>
/// Represents a chart type
/// </summary>
public enum ChartType
{
    Bar,
    Column,
    Line,
    Pie,
    Doughnut,
    Area,
    Scatter,
    Radar
}

/// <summary>
/// Represents text alignment
/// </summary>
public enum TextAlignment
{
    Left,
    Center,
    Right,
    Justify
}

/// <summary>
/// Represents vertical alignment
/// </summary>
public enum VerticalAlignment
{
    Top,
    Middle,
    Bottom
}

/// <summary>
/// Represents a report page size
/// </summary>
public enum PageSize
{
    A4,
    Letter,
    Legal,
    A3,
    Tabloid,
    Custom
}

/// <summary>
/// Represents page orientation
/// </summary>
public enum PageOrientation
{
    Portrait,
    Landscape
}

/// <summary>
/// Represents a report element on the canvas
/// </summary>
public class ReportElement
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public ReportElementType Type { get; set; }
    public string Label { get; set; } = string.Empty;
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; } = 200;
    public double Height { get; set; } = 100;
    public int ZIndex { get; set; }
    public bool IsSelected { get; set; }
    public bool IsLocked { get; set; }
    public bool IsVisible { get; set; } = true;
    public Dictionary<string, object> Properties { get; set; } = new();
    
    // Text properties
    public string? Text { get; set; }
    public string FontFamily { get; set; } = "Arial";
    public double FontSize { get; set; } = 12;
    public bool IsBold { get; set; }
    public bool IsItalic { get; set; }
    public bool IsUnderline { get; set; }
    public string TextColor { get; set; } = "#000000";
    public TextAlignment TextAlign { get; set; } = TextAlignment.Left;
    public VerticalAlignment VerticalAlign { get; set; } = VerticalAlignment.Top;
    
    // Border and background
    public string? BorderColor { get; set; }
    public double BorderWidth { get; set; }
    public string? BackgroundColor { get; set; }
    public string? BorderStyle { get; set; } = "solid";
    
    // Padding
    public double PaddingTop { get; set; }
    public double PaddingRight { get; set; }
    public double PaddingBottom { get; set; }
    public double PaddingLeft { get; set; }
    
    // Chart specific
    public ChartType? ChartType { get; set; }
    public string? DataSource { get; set; }
    public List<string> ChartSeries { get; set; } = new();
    public string? XAxisField { get; set; }
    public string? YAxisField { get; set; }
    
    // Table specific
    public int? Columns { get; set; }
    public int? Rows { get; set; }
    public List<List<string>>? TableData { get; set; }
    public bool HasHeader { get; set; }
    
    // Image specific
    public string? ImageUrl { get; set; }
    public string? ImageFit { get; set; } = "contain";
    
    // Barcode/QR specific
    public string? BarcodeValue { get; set; }
    public string? BarcodeFormat { get; set; } = "CODE128";
    
    // Data binding
    public string? DataField { get; set; }
    public string? Expression { get; set; }
    
    public string GetIcon() => Type switch
    {
        ReportElementType.Text => "📝",
        ReportElementType.Table => "📊",
        ReportElementType.Chart => "📈",
        ReportElementType.Image => "🖼️",
        ReportElementType.Line => "➖",
        ReportElementType.Rectangle => "▭",
        ReportElementType.Barcode => "⊞",
        ReportElementType.QRCode => "⊡",
        ReportElementType.PageBreak => "✂️",
        ReportElementType.Header => "⬆️",
        ReportElementType.Footer => "⬇️",
        _ => "📄"
    };
}

/// <summary>
/// Represents page settings for a report
/// </summary>
public class PageSettings
{
    public PageSize Size { get; set; } = PageSize.A4;
    public PageOrientation Orientation { get; set; } = PageOrientation.Portrait;
    public double Width { get; set; } = 210; // mm
    public double Height { get; set; } = 297; // mm
    public double MarginTop { get; set; } = 10;
    public double MarginRight { get; set; } = 10;
    public double MarginBottom { get; set; } = 10;
    public double MarginLeft { get; set; } = 10;
    public string? BackgroundColor { get; set; } = "#ffffff";
    public bool ShowGrid { get; set; } = true;
    public int GridSize { get; set; } = 10;
}

/// <summary>
/// Represents a complete report definition
/// </summary>
public class ReportDefinition
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "New Report";
    public string? Description { get; set; }
    public string Version { get; set; } = "1.0";
    public PageSettings PageSettings { get; set; } = new();
    public List<ReportElement> Elements { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
    public List<DataSource> DataSources { get; set; } = new();
    public Dictionary<string, string> Parameters { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Validates the report definition
    /// </summary>
    public List<string> Validate()
    {
        var errors = new List<string>();
        
        if (string.IsNullOrWhiteSpace(Name))
        {
            errors.Add("Report name is required");
        }
        
        if (Elements.Count == 0)
        {
            errors.Add("Report must contain at least one element");
        }
        
        // Check for overlapping elements (warning)
        for (int i = 0; i < Elements.Count; i++)
        {
            for (int j = i + 1; j < Elements.Count; j++)
            {
                if (ElementsOverlap(Elements[i], Elements[j]))
                {
                    errors.Add($"Warning: Elements '{Elements[i].Label}' and '{Elements[j].Label}' overlap");
                }
            }
        }
        
        // Validate data bindings
        foreach (var element in Elements)
        {
            if (!string.IsNullOrWhiteSpace(element.DataField))
            {
                var dataSourceExists = DataSources.Exists(ds => 
                    ds.Fields.Contains(element.DataField));
                
                if (!dataSourceExists)
                {
                    errors.Add($"Element '{element.Label}' references unknown data field '{element.DataField}'");
                }
            }
        }
        
        return errors;
    }
    
    private bool ElementsOverlap(ReportElement a, ReportElement b)
    {
        if (!a.IsVisible || !b.IsVisible) return false;
        
        return !(a.X + a.Width < b.X ||
                 b.X + b.Width < a.X ||
                 a.Y + a.Height < b.Y ||
                 b.Y + b.Height < a.Y);
    }
}

/// <summary>
/// Represents a data source for the report
/// </summary>
public class DataSource
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
    public string Query { get; set; } = string.Empty;
    public List<string> Fields { get; set; } = new();
    public Dictionary<string, Type> FieldTypes { get; set; } = new();
}

/// <summary>
/// Represents an export format
/// </summary>
public enum ExportFormat
{
    PDF,
    Excel,
    Word,
    HTML,
    CSV,
    Image
}

/// <summary>
/// Represents export options
/// </summary>
public class ExportOptions
{
    public ExportFormat Format { get; set; } = ExportFormat.PDF;
    public string FileName { get; set; } = "report";
    public bool IncludeHeaders { get; set; } = true;
    public bool IncludeFooters { get; set; } = true;
    public int? ImageDpi { get; set; } = 300;
    public string? ImageFormat { get; set; } = "png";
}
