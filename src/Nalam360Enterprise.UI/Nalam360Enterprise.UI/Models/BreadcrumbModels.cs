using System;
using System.Collections.Generic;

namespace Nalam360Enterprise.UI.Models
{
    /// <summary>
    /// Represents a breadcrumb navigation item
    /// </summary>
    public class BreadcrumbItem
    {
        public string Name { get; set; } = string.Empty;
        public string? Url { get; set; }
        public string? Icon { get; set; }
        public bool IsActive { get; set; }
        public bool IsDisabled { get; set; }
        public Dictionary<string, object>? Metadata { get; set; }
        public string? Tooltip { get; set; }
    }

    /// <summary>
    /// Breadcrumb display options
    /// </summary>
    public class BreadcrumbOptions
    {
        public string Separator { get; set; } = "/";
        public bool ShowHome { get; set; } = true;
        public string? HomeIcon { get; set; } = "🏠";
        public string? HomeUrl { get; set; } = "/";
        public int MaxVisibleItems { get; set; } = 5;
        public bool AutoCollapse { get; set; } = true;
        public bool ShowIcons { get; set; } = true;
        public bool ShowTooltips { get; set; }
    }
}
