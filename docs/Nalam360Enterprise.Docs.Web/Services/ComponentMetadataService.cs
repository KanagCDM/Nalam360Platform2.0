using System.Collections.Generic;
using System.Linq;

namespace Nalam360Enterprise.Docs.Web.Services;

public class ComponentMetadataService
{
    private readonly Dictionary<string, ComponentMetadata> _metadata = new();

    public ComponentMetadataService()
    {
        InitializeMetadata();
    }

    public ComponentMetadata? GetMetadata(string componentName)
    {
        return _metadata.TryGetValue(componentName, out var metadata) ? metadata : null;
    }

    private void InitializeMetadata()
    {
        // N360Button
        _metadata["N360Button"] = new ComponentMetadata
        {
            Name = "N360Button",
            Category = "Input",
            Description = "A flexible button component with multiple variants, sizes, and states for user interactions.",
            WhenToUse = new[]
            {
                "User needs to trigger an action",
                "Submitting or resetting forms",
                "Navigation between pages or sections",
                "Opening dialogs or modals"
            },
            KeyFeatures = new[]
            {
                "Multiple variants (Primary, Secondary, Success, Danger, etc.)",
                "Size options (Small, Medium, Large)",
                "Loading states with spinner",
                "Icon support (prefix and suffix)",
                "Disabled state",
                "Full accessibility with ARIA attributes",
                "Permission-based visibility",
                "Audit logging support"
            },
            Parameters = new[]
            {
                new ComponentParameter("Text", "string", "\"\"", "The button text content"),
                new ComponentParameter("Variant", "ButtonVariant", "Primary", "Visual style variant (Primary, Secondary, Success, Danger, Warning, Info, Link)"),
                new ComponentParameter("Size", "ButtonSize", "Medium", "Button size (Small, Medium, Large)"),
                new ComponentParameter("IsDisabled", "bool", "false", "Whether the button is disabled"),
                new ComponentParameter("IsLoading", "bool", "false", "Shows loading spinner when true"),
                new ComponentParameter("IconName", "string?", "null", "Name of the icon to display"),
                new ComponentParameter("IconPosition", "IconPosition", "Prefix", "Position of icon (Prefix or Suffix)"),
                new ComponentParameter("OnClick", "EventCallback<MouseEventArgs>", "default", "Event fired when button is clicked"),
                new ComponentParameter("CssClass", "string?", "null", "Additional CSS classes"),
                new ComponentParameter("RequiredPermission", "string?", "null", "Required permission to view/use button"),
                new ComponentParameter("EnableAudit", "bool", "false", "Enable audit logging for button clicks")
            },
            Events = new[]
            {
                new ComponentEvent("OnClick", "EventCallback<MouseEventArgs>", "Fired when the button is clicked"),
                new ComponentEvent("OnFocus", "EventCallback<FocusEventArgs>", "Fired when button receives focus"),
                new ComponentEvent("OnBlur", "EventCallback<FocusEventArgs>", "Fired when button loses focus")
            },
            CodeExamples = new[]
            {
                new CodeExample(
                    "Basic Button",
                    "Simple button with text",
                    "<N360Button Text=\"Click Me\" />",
                    "<button class=\"n360-btn n360-btn-primary n360-btn-medium\">Click Me</button>"
                ),
                new CodeExample(
                    "Primary with Icon",
                    "Primary button with icon prefix",
                    "<N360Button Text=\"Save\" Variant=\"ButtonVariant.Primary\" IconName=\"save\" />",
                    "<button class=\"n360-btn n360-btn-primary\"><i class=\"icon-save\"></i> Save</button>"
                ),
                new CodeExample(
                    "Loading State",
                    "Button with loading spinner during async operations",
                    "<N360Button Text=\"@(isLoading ? \"Saving...\" : \"Save\")\" IsLoading=\"@isLoading\" OnClick=\"@HandleSave\" />",
                    "<button disabled><span class=\"spinner-border spinner-border-sm\"></span> Saving...</button>"
                ),
                new CodeExample(
                    "Danger Button",
                    "Button for destructive actions",
                    "<N360Button Text=\"Delete\" Variant=\"ButtonVariant.Danger\" IconName=\"trash\" OnClick=\"@HandleDelete\" />",
                    "<button class=\"n360-btn n360-btn-danger\"><i class=\"icon-trash\"></i> Delete</button>"
                )
            },
            AccessibilityLevel = "AA",
            SupportsKeyboard = true,
            SupportsScreenReader = true,
            SupportsHighContrast = true,
            KeyboardShortcuts = new[]
            {
                new KeyboardShortcut("Tab", "Move focus to/from button"),
                new KeyboardShortcut("Enter", "Activate button"),
                new KeyboardShortcut("Space", "Activate button")
            },
            AriaAttributes = new[] { "aria-label", "aria-disabled", "aria-busy", "aria-pressed", "role=\"button\"" }
        };

        // N360Grid
        _metadata["N360Grid"] = new ComponentMetadata
        {
            Name = "N360Grid",
            Category = "Data",
            Description = "Enterprise-grade data grid with sorting, filtering, paging, grouping, and server-side data support.",
            WhenToUse = new[]
            {
                "Display large datasets in tabular format",
                "Need sorting, filtering, and searching capabilities",
                "Require server-side pagination for performance",
                "Export data to Excel, PDF, or CSV",
                "Inline editing of records"
            },
            KeyFeatures = new[]
            {
                "Server-side and client-side data binding",
                "Column sorting (single and multi-column)",
                "Advanced filtering (Excel-like filter menu)",
                "Paging with configurable page sizes",
                "Grouping and aggregations",
                "Row selection (single and multi-select)",
                "Inline editing with validation",
                "Column reordering and resizing",
                "Export to Excel, PDF, CSV",
                "Virtual scrolling for large datasets",
                "Frozen columns",
                "Cell templates and custom rendering",
                "Responsive design with adaptive UI"
            },
            Parameters = new[]
            {
                new ComponentParameter("DataSource", "IEnumerable<TItem>", "[]", "Data source for the grid"),
                new ComponentParameter("Columns", "RenderFragment", "required", "Grid column definitions"),
                new ComponentParameter("AllowSorting", "bool", "true", "Enable column sorting"),
                new ComponentParameter("AllowFiltering", "bool", "true", "Enable column filtering"),
                new ComponentParameter("AllowPaging", "bool", "true", "Enable pagination"),
                new ComponentParameter("PageSize", "int", "10", "Number of rows per page"),
                new ComponentParameter("AllowSelection", "bool", "false", "Enable row selection"),
                new ComponentParameter("SelectionMode", "SelectionMode", "Single", "Selection mode (Single or Multiple)"),
                new ComponentParameter("AllowGrouping", "bool", "false", "Enable column grouping"),
                new ComponentParameter("AllowExcelExport", "bool", "false", "Enable Excel export"),
                new ComponentParameter("Height", "string", "\"auto\"", "Grid height (CSS value)"),
                new ComponentParameter("OnRowSelected", "EventCallback<TItem>", "default", "Event fired when row is selected"),
                new ComponentParameter("OnDataBound", "EventCallback", "default", "Event fired after data binding")
            },
            Events = new[]
            {
                new ComponentEvent("OnRowSelected", "EventCallback<TItem>", "Fired when a row is selected"),
                new ComponentEvent("OnRowDeselected", "EventCallback<TItem>", "Fired when a row is deselected"),
                new ComponentEvent("OnSorted", "EventCallback<SortEventArgs>", "Fired when sorting is applied"),
                new ComponentEvent("OnFiltered", "EventCallback<FilterEventArgs>", "Fired when filtering is applied"),
                new ComponentEvent("OnPageChanged", "EventCallback<int>", "Fired when page is changed"),
                new ComponentEvent("OnCellEdit", "EventCallback<CellEditArgs>", "Fired during cell editing"),
                new ComponentEvent("OnDataBound", "EventCallback", "Fired after data is bound to grid")
            },
            CodeExamples = new[]
            {
                new CodeExample(
                    "Basic Grid",
                    "Simple grid with auto-generated columns",
                    @"<N360Grid DataSource=""@patients"">
    <GridColumns>
        <GridColumn Field=""@nameof(Patient.Id)"" HeaderText=""ID"" Width=""80"" />
        <GridColumn Field=""@nameof(Patient.Name)"" HeaderText=""Name"" Width=""200"" />
        <GridColumn Field=""@nameof(Patient.DateOfBirth)"" HeaderText=""DOB"" Format=""yyyy-MM-dd"" Width=""120"" />
    </GridColumns>
</N360Grid>",
                    "<table class=\"n360-grid\"><thead><tr><th>ID</th><th>Name</th><th>DOB</th></tr></thead><tbody>...</tbody></table>"
                ),
                new CodeExample(
                    "Grid with Selection",
                    "Grid with row selection enabled",
                    @"<N360Grid DataSource=""@patients"" 
          AllowSelection=""true"" 
          SelectionMode=""SelectionMode.Multiple""
          OnRowSelected=""@HandleRowSelected"">
    <GridColumns>
        <GridColumn Field=""@nameof(Patient.Name)"" HeaderText=""Name"" />
    </GridColumns>
</N360Grid>",
                    "<table class=\"n360-grid\">...</table>"
                )
            },
            AccessibilityLevel = "AA",
            SupportsKeyboard = true,
            SupportsScreenReader = true,
            SupportsHighContrast = true,
            KeyboardShortcuts = new[]
            {
                new KeyboardShortcut("Tab", "Navigate between cells"),
                new KeyboardShortcut("Arrow Keys", "Navigate grid cells"),
                new KeyboardShortcut("Enter", "Enter edit mode (if editing enabled)"),
                new KeyboardShortcut("Space", "Select/deselect row"),
                new KeyboardShortcut("Home", "Go to first cell in row"),
                new KeyboardShortcut("End", "Go to last cell in row"),
                new KeyboardShortcut("Page Up/Down", "Navigate pages")
            },
            AriaAttributes = new[] { "role=\"grid\"", "aria-rowcount", "aria-colcount", "aria-label", "aria-selected" }
        };

        // N360SmartChat
        _metadata["N360SmartChat"] = new ComponentMetadata
        {
            Name = "N360SmartChat",
            Category = "AI-Powered",
            Description = "AI-powered healthcare chatbot with natural language processing for patient interactions, symptom checking, and appointment scheduling.",
            WhenToUse = new[]
            {
                "Patient self-service portals",
                "Virtual health assistants",
                "Symptom triage and assessment",
                "Appointment booking and reminders",
                "Medical information queries",
                "Post-care follow-ups"
            },
            KeyFeatures = new[]
            {
                "Natural language understanding (NLU)",
                "Healthcare-specific intent recognition",
                "Symptom assessment with severity scoring",
                "Integration with EHR systems",
                "Multi-language support",
                "HIPAA-compliant message storage",
                "Context-aware conversations",
                "Escalation to human agents",
                "Medical terminology recognition",
                "Appointment scheduling integration",
                "Prescription refill requests",
                "Rich media support (images, documents)",
                "Typing indicators and read receipts"
            },
            Parameters = new[]
            {
                new ComponentParameter("BotName", "string", "\"Health Assistant\"", "Display name for the chatbot"),
                new ComponentParameter("WelcomeMessage", "string", "\"Hello! How can I help you today?\"", "Initial greeting message"),
                new ComponentParameter("EnableSymptomChecker", "bool", "false", "Enable symptom assessment feature"),
                new ComponentParameter("EnableAppointmentBooking", "bool", "false", "Enable appointment scheduling"),
                new ComponentParameter("OnMessageSent", "EventCallback<ChatMessage>", "default", "Event fired when user sends message"),
                new ComponentParameter("OnIntentDetected", "EventCallback<Intent>", "default", "Event fired when AI detects user intent"),
                new ComponentParameter("Height", "string", "\"600px\"", "Chat window height"),
                new ComponentParameter("AllowFileUpload", "bool", "false", "Allow users to upload files/images")
            },
            Events = new[]
            {
                new ComponentEvent("OnMessageSent", "EventCallback<ChatMessage>", "Fired when user sends a message"),
                new ComponentEvent("OnMessageReceived", "EventCallback<ChatMessage>", "Fired when bot responds"),
                new ComponentEvent("OnIntentDetected", "EventCallback<Intent>", "Fired when AI detects user intent"),
                new ComponentEvent("OnEscalationRequested", "EventCallback", "Fired when conversation needs human agent"),
                new ComponentEvent("OnFileUploaded", "EventCallback<FileUploadArgs>", "Fired when user uploads a file")
            },
            CodeExamples = new[]
            {
                new CodeExample(
                    "Basic Chat",
                    "Simple healthcare chatbot",
                    @"<N360SmartChat BotName=""Dr. Assistant"" 
               WelcomeMessage=""Hello! I'm here to help with your health questions."" />",
                    "<div class=\"chat-container\">...</div>"
                ),
                new CodeExample(
                    "Symptom Checker",
                    "Chat with symptom assessment enabled",
                    @"<N360SmartChat BotName=""Symptom Checker"" 
               EnableSymptomChecker=""true""
               OnIntentDetected=""@HandleIntent"" />",
                    "<div class=\"chat-container\">...</div>"
                )
            },
            AccessibilityLevel = "AA",
            SupportsKeyboard = true,
            SupportsScreenReader = true,
            SupportsHighContrast = true,
            KeyboardShortcuts = new[]
            {
                new KeyboardShortcut("Enter", "Send message"),
                new KeyboardShortcut("Shift+Enter", "New line in message"),
                new KeyboardShortcut("Ctrl+U", "Upload file"),
                new KeyboardShortcut("Escape", "Close chat window")
            },
            AriaAttributes = new[] { "role=\"log\"", "aria-live=\"polite\"", "aria-label", "aria-atomic" }
        };

        // N360NumericTextBox
        _metadata["N360NumericTextBox"] = CreateInputMetadata(
            "N360NumericTextBox", "Numeric input with formatting, min/max validation, and step increments",
            new[] { "Numeric data entry", "Currency input", "Quantity selection", "Age or measurement input" },
            new[] { "Min/max validation", "Decimal precision control", "Step increments", "Format display (currency, percentage)", "Thousand separators" }
        );

        // N360DatePicker
        _metadata["N360DatePicker"] = CreateInputMetadata(
            "N360DatePicker", "Date selection with calendar popup and keyboard input",
            new[] { "Date of birth input", "Appointment scheduling", "Date range selection", "Event dates" },
            new[] { "Calendar popup", "Min/max date restrictions", "Disabled dates", "Multiple date formats", "Today button", "Clear button" }
        );

        // N360TimePicker
        _metadata["N360TimePicker"] = CreateInputMetadata(
            "N360TimePicker", "Time selection component with 12/24 hour formats",
            new[] { "Appointment times", "Schedule input", "Time tracking" },
            new[] { "12/24 hour format", "Hour/minute/second selection", "AM/PM toggle", "Step intervals" }
        );

        // N360DateTimePicker
        _metadata["N360DateTimePicker"] = CreateInputMetadata(
            "N360DateTimePicker", "Combined date and time picker in one control",
            new[] { "Appointment scheduling", "Event date-time", "Timestamp input" },
            new[] { "Date and time in one control", "Timezone support", "Quick presets", "Format customization" }
        );

        // N360DropDownList
        _metadata["N360DropDownList"] = CreateInputMetadata(
            "N360DropDownList", "Dropdown selection with filtering and templates",
            new[] { "Single selection from list", "Category selection", "Status selection" },
            new[] { "Search/filter", "Custom item templates", "Virtual scrolling", "Grouping", "Value/text fields" }
        );

        // N360ComboBox
        _metadata["N360ComboBox"] = CreateInputMetadata(
            "N360ComboBox", "Autocomplete combo box with custom value support",
            new[] { "Searchable selection", "Allow custom entries", "Tag input" },
            new[] { "Autocomplete", "Allow custom values", "Filtering", "Highlighting matches" }
        );

        // N360MultiSelect
        _metadata["N360MultiSelect"] = CreateInputMetadata(
            "N360MultiSelect", "Multiple selection dropdown with chips display",
            new[] { "Multiple selection", "Tag selection", "Category multi-select" },
            new[] { "Multiple selection", "Chip display", "Select all", "Custom value support", "Max selection limit" }
        );

        // N360AutoComplete
        _metadata["N360AutoComplete"] = CreateInputMetadata(
            "N360AutoComplete", "Auto-complete text input with suggestions",
            new[] { "Search with suggestions", "Address input", "Medical term search" },
            new[] { "Async data loading", "Debouncing", "Highlight matches", "Custom templates", "Minimum characters" }
        );

        // N360CheckBox
        _metadata["N360CheckBox"] = CreateInputMetadata(
            "N360CheckBox", "Checkbox input for boolean values",
            new[] { "Agreement checkboxes", "Feature toggles", "Multi-select options" },
            new[] { "Indeterminate state", "Label support", "Disabled state", "Validation" }
        );

        // N360RadioButton
        _metadata["N360RadioButton"] = CreateInputMetadata(
            "N360RadioButton", "Radio button for single selection from group",
            new[] { "Single choice questions", "Option selection", "Gender selection" },
            new[] { "Group support", "Custom labels", "Disabled state", "Validation" }
        );

        // N360Switch
        _metadata["N360Switch"] = CreateInputMetadata(
            "N360Switch", "Toggle switch for on/off states",
            new[] { "Enable/disable features", "Boolean settings", "Status toggles" },
            new[] { "On/off labels", "Disabled state", "Loading state", "Color customization" }
        );

        // N360Slider
        _metadata["N360Slider"] = CreateInputMetadata(
            "N360Slider", "Range slider for numeric value selection",
            new[] { "Volume control", "Price range", "Age range", "Rating input" },
            new[] { "Single/range selection", "Step increments", "Tooltips", "Tick marks", "Custom formatting" }
        );

        // N360ColorPicker
        _metadata["N360ColorPicker"] = CreateInputMetadata(
            "N360ColorPicker", "Color selection component with palette and hex input",
            new[] { "Theme customization", "UI color selection", "Chart colors" },
            new[] { "Palette view", "Hex/RGB input", "Recent colors", "Opacity control", "Preset colors" }
        );

        // N360FileUpload
        _metadata["N360FileUpload"] = CreateInputMetadata(
            "N360FileUpload", "File upload with drag-and-drop and progress tracking",
            new[] { "Document upload", "Image upload", "Attachment upload", "Bulk file upload" },
            new[] { "Drag and drop", "Multiple files", "Progress tracking", "File type validation", "Size limits", "Preview" }
        );

        // N360Rating
        _metadata["N360Rating"] = CreateInputMetadata(
            "N360Rating", "Star rating component for feedback collection",
            new[] { "Product ratings", "Service feedback", "Quality ratings" },
            new[] { "Half star support", "Custom icons", "Read-only mode", "Tooltips", "Color customization" }
        );

        // N360SignaturePad
        _metadata["N360SignaturePad"] = CreateInputMetadata(
            "N360SignaturePad", "Digital signature capture with touch/mouse support",
            new[] { "Consent signatures", "Document signing", "Patient signatures" },
            new[] { "Touch/mouse drawing", "Clear button", "Export to image", "Stroke customization", "Validation" }
        );

        // N360MaskedTextBox
        _metadata["N360MaskedTextBox"] = CreateInputMetadata(
            "N360MaskedTextBox", "Text input with format masking (phone, SSN, credit card)",
            new[] { "Phone numbers", "SSN input", "Credit cards", "Custom formats" },
            new[] { "Format masking", "Custom masks", "Placeholder", "Validation", "Copy/paste support" }
        );

        // N360RichTextEditor
        _metadata["N360RichTextEditor"] = CreateInputMetadata(
            "N360RichTextEditor", "WYSIWYG rich text editor with formatting toolbar",
            new[] { "Clinical notes", "Report writing", "Email composition", "Content creation" },
            new[] { "Rich formatting", "Tables", "Images", "Links", "Undo/redo", "HTML export", "Spell check" }
        );

        // N360MarkdownEditor
        _metadata["N360MarkdownEditor"] = CreateInputMetadata(
            "N360MarkdownEditor", "Markdown editor with live preview",
            new[] { "Documentation", "Notes", "Technical writing" },
            new[] { "Live preview", "Syntax highlighting", "Toolbar shortcuts", "Export to HTML" }
        );

        // N360CodeEditor
        _metadata["N360CodeEditor"] = CreateInputMetadata(
            "N360CodeEditor", "Syntax-highlighted code editor",
            new[] { "Code snippets", "Configuration editing", "SQL queries", "Script editing" },
            new[] { "Syntax highlighting", "Multiple languages", "Line numbers", "Auto-complete", "Find/replace" }
        );

        // N360ChipList
        _metadata["N360ChipList"] = CreateInputMetadata(
            "N360ChipList", "Chip/tag input component for multiple values",
            new[] { "Tags", "Keywords", "Symptoms list", "Medication list" },
            new[] { "Add/remove chips", "Custom values", "Validation", "Max items", "Templates" }
        );

        // N360ButtonGroup
        _metadata["N360ButtonGroup"] = CreateInputMetadata(
            "N360ButtonGroup", "Grouped buttons for related actions",
            new[] { "View toggles", "Formatting toolbar", "Action groups" },
            new[] { "Horizontal/vertical", "Single/multiple selection", "Disabled items", "Icon support" }
        );

        // N360FAB
        _metadata["N360FAB"] = CreateInputMetadata(
            "N360FAB", "Floating action button for primary actions",
            new[] { "Primary action", "Quick actions", "Create new item" },
            new[] { "Extended/mini mode", "Speed dial", "Position control", "Icon support", "Tooltip" }
        );

        // N360ToggleButton
        _metadata["N360ToggleButton"] = CreateInputMetadata(
            "N360ToggleButton", "Toggle button for binary choices",
            new[] { "View toggles", "Filter toggles", "Option selection" },
            new[] { "On/off states", "Icon support", "Group support", "Disabled state" }
        );

        // N360SearchBox
        _metadata["N360SearchBox"] = CreateInputMetadata(
            "N360SearchBox", "Search input with suggestions and recent searches",
            new[] { "Patient search", "Medical records search", "General search" },
            new[] { "Search suggestions", "Recent searches", "Clear button", "Voice input", "Debouncing" }
        );

        // Data Components
        // N360TreeGrid
        _metadata["N360TreeGrid"] = CreateDataMetadata(
            "N360TreeGrid", "Hierarchical tree grid for nested data display",
            new[] { "Organizational hierarchy", "File systems", "Bill of materials", "Nested categories" },
            new[] { "Expand/collapse", "Lazy loading", "Sorting", "Filtering", "Editing", "Drag-drop reorder" }
        );

        // N360Pivot
        _metadata["N360Pivot"] = CreateDataMetadata(
            "N360Pivot", "Pivot table for data aggregation and analysis",
            new[] { "Data analysis", "Cross-tabulation", "Business intelligence", "Report generation" },
            new[] { "Drag-drop fields", "Aggregations", "Grouping", "Filtering", "Excel export", "Drill-down" }
        );

        // N360ListView
        _metadata["N360ListView"] = CreateDataMetadata(
            "N360ListView", "List view with templates and virtualization",
            new[] { "Patient lists", "Appointment lists", "Document lists", "Search results" },
            new[] { "Custom templates", "Virtual scrolling", "Selection", "Sorting", "Filtering", "Grouping" }
        );

        // N360DataTable
        _metadata["N360DataTable"] = CreateDataMetadata(
            "N360DataTable", "Enterprise data table with advanced features",
            new[] { "Complex data display", "Editable tables", "Master-detail views" },
            new[] { "Column management", "Row details", "Inline editing", "Custom cell renderers", "Aggregations" }
        );

        // N360VirtualScroll
        _metadata["N360VirtualScroll"] = CreateDataMetadata(
            "N360VirtualScroll", "Virtual scrolling for large lists",
            new[] { "Large datasets", "Infinite scrolling", "Performance optimization" },
            new[] { "Virtual rendering", "Dynamic heights", "Scroll to index", "Smooth scrolling" }
        );

        // N360Timeline
        _metadata["N360Timeline"] = CreateDataMetadata(
            "N360Timeline", "Timeline visualization for chronological data",
            new[] { "Medical history", "Event tracking", "Project timeline", "Activity log" },
            new[] { "Horizontal/vertical", "Custom templates", "Grouping", "Interactive", "Zoom controls" }
        );

        // N360Gantt
        _metadata["N360Gantt"] = CreateDataMetadata(
            "N360Gantt", "Gantt chart for project scheduling",
            new[] { "Project management", "Surgery scheduling", "Resource planning" },
            new[] { "Task dependencies", "Progress tracking", "Resource allocation", "Drag-drop", "Critical path" }
        );

        // N360TreeView
        _metadata["N360TreeView"] = CreateDataMetadata(
            "N360TreeView", "Tree navigation component",
            new[] { "Navigation menus", "File browsers", "Category trees", "Organization structure" },
            new[] { "Expand/collapse", "Selection", "Checkboxes", "Drag-drop", "Icons", "Lazy loading" }
        );

        // N360Spreadsheet
        _metadata["N360Spreadsheet"] = CreateDataMetadata(
            "N360Spreadsheet", "Excel-like spreadsheet component",
            new[] { "Data entry grids", "Calculation sheets", "Import/export Excel" },
            new[] { "Formulas", "Formatting", "Charts", "Multiple sheets", "Excel import/export", "Cell validation" }
        );

        // N360QueryBuilder
        _metadata["N360QueryBuilder"] = CreateDataMetadata(
            "N360QueryBuilder", "Visual query builder for complex filters",
            new[] { "Advanced search", "Report filters", "Data queries" },
            new[] { "Visual rules", "Nested conditions", "Multiple operators", "Export to SQL", "Templates" }
        );

        // N360FileManager
        _metadata["N360FileManager"] = CreateDataMetadata(
            "N360FileManager", "File browser and manager",
            new[] { "Document management", "Media gallery", "File uploads" },
            new[] { "Multiple views", "Upload/download", "Rename/delete", "Search", "Breadcrumb navigation" }
        );

        // N360Badge
        _metadata["N360Badge"] = CreateDataMetadata(
            "N360Badge", "Badge/label component for status indicators",
            new[] { "Status indicators", "Counts", "Notifications", "Labels" },
            new[] { "Multiple variants", "Dot mode", "Positioning", "Custom content" }
        );

        // N360Chip
        _metadata["N360Chip"] = CreateDataMetadata(
            "N360Chip", "Chip display component",
            new[] { "Tags display", "Selected items", "Filters", "Categories" },
            new[] { "Removable", "Clickable", "Icons", "Avatars", "Colors" }
        );

        // N360Avatar
        _metadata["N360Avatar"] = CreateDataMetadata(
            "N360Avatar", "User avatar component with fallback",
            new[] { "User profiles", "Comments", "Team members", "Patient photos" },
            new[] { "Image/initials", "Sizes", "Status indicator", "Grouping", "Fallback handling" }
        );

        // Charts Components
        // N360Dashboard
        _metadata["N360Dashboard"] = CreateChartMetadata(
            "N360Dashboard", "Dashboard layout with draggable widgets",
            new[] { "Analytics dashboards", "KPI displays", "Monitoring screens" },
            new[] { "Drag-drop layout", "Responsive grid", "Widget library", "Real-time updates", "Save layouts" }
        );

        // N360KPI
        _metadata["N360KPI"] = CreateChartMetadata(
            "N360KPI", "KPI display widget with trend indicators",
            new[] { "Key metrics", "Performance indicators", "Goal tracking" },
            new[] { "Trend indicators", "Sparklines", "Color thresholds", "Comparisons", "Drill-down" }
        );

        // N360Gauge
        _metadata["N360Gauge"] = CreateChartMetadata(
            "N360Gauge", "Circular gauge for progress/metrics",
            new[] { "Progress indicators", "Capacity meters", "Score displays" },
            new[] { "Circular/semi-circular", "Color ranges", "Needle/progress", "Animation", "Thresholds" }
        );

        // N360Heatmap
        _metadata["N360Heatmap"] = CreateChartMetadata(
            "N360Heatmap", "Data heatmap visualization",
            new[] { "Correlation matrices", "Time-based patterns", "Geographic data" },
            new[] { "Color gradients", "Tooltips", "Interactive", "Export", "Custom color schemes" }
        );

        // N360DiagramEditor
        _metadata["N360DiagramEditor"] = CreateChartMetadata(
            "N360DiagramEditor", "Diagram creation and editing tool",
            new[] { "Workflow diagrams", "Process flows", "Network diagrams", "Mind maps" },
            new[] { "Drag-drop shapes", "Connectors", "Auto-layout", "Export SVG/PNG", "Templates" }
        );

        // N360Sparkline
        _metadata["N360Sparkline"] = CreateChartMetadata(
            "N360Sparkline", "Inline sparkline charts",
            new[] { "Trend indicators", "Mini charts in tables", "Dashboard widgets" },
            new[] { "Line/bar/area", "Compact size", "Hover details", "Multiple series" }
        );

        // N360BulletChart
        _metadata["N360BulletChart"] = CreateChartMetadata(
            "N360BulletChart", "Bullet chart for KPI comparison",
            new[] { "Goal vs actual", "Performance metrics", "Budget tracking" },
            new[] { "Target markers", "Range zones", "Comparison bars", "Compact display" }
        );

        // N360TextBox
        _metadata["N360TextBox"] = new ComponentMetadata
        {
            Name = "N360TextBox",
            Category = "Input",
            Description = "Text input component with validation, formatting, and accessibility features.",
            WhenToUse = new[] { "Single-line text input", "Form data entry", "Search boxes", "User credentials" },
            KeyFeatures = new[] { "Built-in validation", "Mask support", "Placeholder text", "Icon support", "Clear button", "Character counter" },
            Parameters = new[]
            {
                new ComponentParameter("Value", "string", "\"\"", "Current text value"),
                new ComponentParameter("Placeholder", "string?", "null", "Placeholder text"),
                new ComponentParameter("MaxLength", "int?", "null", "Maximum character length"),
                new ComponentParameter("IsPassword", "bool", "false", "Render as password field"),
                new ComponentParameter("IsReadOnly", "bool", "false", "Make field read-only"),
                new ComponentParameter("ValidationRules", "ValidationRules?", "null", "Validation rules"),
                new ComponentParameter("OnValueChanged", "EventCallback<string>", "default", "Event when value changes")
            },
            Events = new[]
            {
                new ComponentEvent("OnValueChanged", "EventCallback<string>", "Fired when text value changes"),
                new ComponentEvent("OnFocus", "EventCallback", "Fired when input receives focus"),
                new ComponentEvent("OnBlur", "EventCallback", "Fired when input loses focus")
            },
            CodeExamples = new[]
            {
                new CodeExample("Basic TextBox", "Simple text input", "<N360TextBox @bind-Value=\"name\" Placeholder=\"Enter your name\" />", "<input type=\"text\" />"),
                new CodeExample("With Validation", "Text input with validation", "<N360TextBox @bind-Value=\"email\" ValidationRules=\"@emailRules\" />", "<input type=\"text\" />")
            },
            AccessibilityLevel = "AA",
            SupportsKeyboard = true,
            SupportsScreenReader = true,
            SupportsHighContrast = true,
            KeyboardShortcuts = new[] { new KeyboardShortcut("Tab", "Navigate focus"), new KeyboardShortcut("Ctrl+A", "Select all text") },
            AriaAttributes = new[] { "aria-label", "aria-invalid", "aria-describedby" }
        };

        // Healthcare Components - all comprehensive metadata
        AddHealthcareComponents();
        
        // AI Components - all comprehensive metadata
        AddAIComponents();
        
        // Navigation & Layout Components
        AddNavigationComponents();
        
        // Feedback & Notification Components
        AddFeedbackComponents();
        
        // Enterprise Components
        AddEnterpriseComponents();
    }

    private void AddHealthcareComponents()
    {
        _metadata["N360PatientCard"] = CreateHealthcareMetadata(
            "N360PatientCard", "Patient information card with demographics and quick access",
            new[] { "Patient dashboards", "EMR systems", "Quick patient reference" },
            new[] { "Demographics", "Photo", "MRN", "Allergies badge", "Quick actions", "Status indicators" }
        );

        _metadata["N360VitalSignsInput"] = CreateHealthcareMetadata(
            "N360VitalSignsInput", "Vital signs data entry with validation",
            new[] { "Patient vitals recording", "Nursing assessments", "Triage" },
            new[] { "BP, HR, RR, Temp, SpO2", "Normal ranges", "Alerts", "History tracking", "Unit conversion" }
        );

        _metadata["N360AppointmentScheduler"] = CreateHealthcareMetadata(
            "N360AppointmentScheduler", "Medical appointment scheduling calendar",
            new[] { "Appointment booking", "Provider scheduling", "Resource allocation" },
            new[] { "Calendar views", "Drag-drop", "Recurring appointments", "Room assignment", "Reminders" }
        );

        _metadata["N360MedicalHistory"] = CreateHealthcareMetadata(
            "N360MedicalHistory", "Patient medical history timeline viewer",
            new[] { "EMR history", "Longitudinal view", "Clinical summary" },
            new[] { "Timeline view", "Filter by type", "Search", "Print summary", "ICD codes" }
        );

        _metadata["N360PrescriptionWriter"] = CreateHealthcareMetadata(
            "N360PrescriptionWriter", "Electronic prescription form with drug database",
            new[] { "E-prescribing", "Medication orders", "Prescription printing" },
            new[] { "Drug search", "Dosage calculator", "Interaction check", "Favorites", "DEA compliance" }
        );

        _metadata["N360LabResults"] = CreateHealthcareMetadata(
            "N360LabResults", "Laboratory results display with trends",
            new[] { "Lab reports", "Results review", "Trend analysis" },
            new[] { "Normal range indicators", "Trend charts", "Critical alerts", "Print reports", "Historical comparison" }
        );

        _metadata["N360RadiologyViewer"] = CreateHealthcareMetadata(
            "N360RadiologyViewer", "Medical imaging viewer (DICOM)",
            new[] { "X-ray viewing", "CT/MRI review", "Image analysis" },
            new[] { "Pan/zoom", "Windowing", "Measurements", "Annotations", "DICOM support", "Series navigation" }
        );

        _metadata["N360DiagnosisCodePicker"] = CreateHealthcareMetadata(
            "N360DiagnosisCodePicker", "ICD-10 diagnosis code selector",
            new[] { "Diagnosis coding", "Medical billing", "Problem lists" },
            new[] { "ICD-10 search", "Favorites", "Recent codes", "Code descriptions", "Validation" }
        );

        _metadata["N360DrugInteractionChecker"] = CreateHealthcareMetadata(
            "N360DrugInteractionChecker", "Medication interaction validator",
            new[] { "Medication safety", "Drug checking", "Allergy checks" },
            new[] { "Drug-drug interactions", "Allergy checking", "Severity levels", "Alternative suggestions", "FDA database" }
        );

        _metadata["N360TreatmentPlan"] = CreateHealthcareMetadata(
            "N360TreatmentPlan", "Treatment plan builder and tracker",
            new[] { "Care planning", "Treatment protocols", "Goal tracking" },
            new[] { "Goals", "Interventions", "Progress tracking", "Templates", "Multi-disciplinary" }
        );

        _metadata["N360ClinicalNotes"] = CreateHealthcareMetadata(
            "N360ClinicalNotes", "Clinical documentation editor",
            new[] { "Progress notes", "SOAP notes", "Consultation notes" },
            new[] { "Templates", "Voice-to-text", "Smart phrases", "Sign/lock", "Addendums" }
        );

        _metadata["N360Immunization"] = CreateHealthcareMetadata(
            "N360Immunization", "Vaccination record tracker",
            new[] { "Immunization records", "Vaccine scheduling", "Registry reporting" },
            new[] { "Schedule tracking", "Lot numbers", "Adverse reactions", "Due dates", "CDC compliance" }
        );

        _metadata["N360AllergyTracker"] = CreateHealthcareMetadata(
            "N360AllergyTracker", "Patient allergy management",
            new[] { "Allergy documentation", "Safety alerts", "Drug allergies" },
            new[] { "Allergy list", "Severity", "Reactions", "Date recorded", "Alert badges" }
        );

        _metadata["N360BedManagement"] = CreateHealthcareMetadata(
            "N360BedManagement", "Hospital bed allocation system",
            new[] { "Bed tracking", "Admissions", "Capacity management" },
            new[] { "Real-time status", "Floor plans", "Transfer tracking", "Cleaning status", "Analytics" }
        );

        _metadata["N360SurgeryScheduler"] = CreateHealthcareMetadata(
            "N360SurgeryScheduler", "Operating room scheduling",
            new[] { "OR scheduling", "Surgery booking", "Resource allocation" },
            new[] { "Room scheduling", "Staff assignment", "Equipment tracking", "Pre-op checklist", "Time tracking" }
        );

        _metadata["N360EmergencyTriage"] = CreateHealthcareMetadata(
            "N360EmergencyTriage", "ER triage assessment system",
            new[] { "Emergency triage", "Acuity scoring", "Priority queuing" },
            new[] { "ESI levels", "Chief complaint", "Vital signs", "Wait time tracking", "Priority alerts" }
        );

        _metadata["N360DischargeInstructions"] = CreateHealthcareMetadata(
            "N360DischargeInstructions", "Patient discharge forms",
            new[] { "Discharge planning", "Patient education", "Follow-up" },
            new[] { "Templates", "Medication list", "Follow-up appointments", "Instructions", "Patient signature" }
        );

        _metadata["N360ConsentForm"] = CreateHealthcareMetadata(
            "N360ConsentForm", "Medical consent management",
            new[] { "Informed consent", "Procedure consent", "Research consent" },
            new[] { "Digital signature", "Witness", "Templates", "Language support", "Version control" }
        );

        _metadata["N360InsuranceVerification"] = CreateHealthcareMetadata(
            "N360InsuranceVerification", "Insurance eligibility checker",
            new[] { "Benefit verification", "Eligibility checks", "Authorization" },
            new[] { "Real-time verification", "Coverage details", "Copay calculation", "Prior auth", "Claim history" }
        );

        _metadata["N360ReferralManager"] = CreateHealthcareMetadata(
            "N360ReferralManager", "Patient referral tracking",
            new[] { "Referral management", "Specialist coordination", "Care continuity" },
            new[] { "Referral creation", "Status tracking", "Document sharing", "Follow-up", "Network directory" }
        );

        _metadata["N360TelemedicineConsole"] = CreateHealthcareMetadata(
            "N360TelemedicineConsole", "Virtual care consultation interface",
            new[] { "Telemedicine visits", "Remote consultations", "Virtual care" },
            new[] { "Video conferencing", "Screen sharing", "Chat", "Document sharing", "Recording", "Waiting room" }
        );

        _metadata["N360HealthRiskAssessment"] = CreateHealthcareMetadata(
            "N360HealthRiskAssessment", "Health risk assessment questionnaire",
            new[] { "Risk screening", "Health assessments", "Preventive care" },
            new[] { "Questionnaires", "Scoring algorithms", "Risk stratification", "Recommendations", "Reporting" }
        );

        _metadata["N360CareCoordination"] = CreateHealthcareMetadata(
            "N360CareCoordination", "Care team collaboration hub",
            new[] { "Team collaboration", "Care transitions", "Multi-disciplinary care" },
            new[] { "Task management", "Communication", "Document sharing", "Care plans", "Handoffs" }
        );

        _metadata["N360PatientPortal"] = CreateHealthcareMetadata(
            "N360PatientPortal", "Patient self-service portal",
            new[] { "Patient access", "Self-service", "Patient engagement" },
            new[] { "Medical records", "Appointments", "Messaging", "Bill pay", "Test results", "Rx refills" }
        );

        _metadata["N360MedicationAdministration"] = CreateHealthcareMetadata(
            "N360MedicationAdministration", "Medication administration record (MAR)",
            new[] { "Med administration", "eMAR", "Medication tracking" },
            new[] { "Barcode scanning", "5 rights check", "Timing", "Documentation", "Missed dose alerts" }
        );

        _metadata["N360VitalSignsMonitor"] = CreateHealthcareMetadata(
            "N360VitalSignsMonitor", "Real-time vital signs monitoring dashboard",
            new[] { "ICU monitoring", "Post-op monitoring", "Remote monitoring" },
            new[] { "Real-time display", "Trend graphs", "Alerts", "Multi-patient view", "Device integration" }
        );
    }

    private void AddAIComponents()
    {
        _metadata["N360DrugRecommendation"] = CreateAIMetadata(
            "N360DrugRecommendation", "AI-powered medication suggestions",
            new[] { "Treatment recommendations", "Medication selection", "Clinical decision support" },
            new[] { "ML-based suggestions", "Evidence-based", "Patient-specific", "Alternative options", "Rationale display" }
        );

        _metadata["N360RiskStratification"] = CreateAIMetadata(
            "N360RiskStratification", "ML patient risk scoring",
            new[] { "Risk assessment", "Patient stratification", "Predictive analytics" },
            new[] { "Multiple risk models", "Real-time scoring", "Risk factors", "Trend analysis", "Interventions" }
        );

        _metadata["N360DiagnosticAssistant"] = CreateAIMetadata(
            "N360DiagnosticAssistant", "AI differential diagnosis helper",
            new[] { "Diagnosis support", "Clinical reasoning", "Medical decision making" },
            new[] { "Symptom analysis", "Differential diagnosis", "Probability scoring", "Evidence links", "Guidelines" }
        );

        _metadata["N360ImageAnalysis"] = CreateAIMetadata(
            "N360ImageAnalysis", "Medical image AI analysis",
            new[] { "Radiology AI", "Image interpretation", "CAD systems" },
            new[] { "Abnormality detection", "Segmentation", "Classification", "Heatmaps", "Confidence scores" }
        );

        _metadata["N360NaturalLanguageSearch"] = CreateAIMetadata(
            "N360NaturalLanguageSearch", "AI-powered medical record search",
            new[] { "Clinical search", "Record retrieval", "Research queries" },
            new[] { "Natural language", "Semantic search", "Context-aware", "Highlighting", "Relevance ranking" }
        );

        _metadata["N360SentimentAnalysis"] = CreateAIMetadata(
            "N360SentimentAnalysis", "Patient feedback sentiment analysis",
            new[] { "Patient satisfaction", "Feedback analysis", "Quality monitoring" },
            new[] { "Sentiment scoring", "Topic extraction", "Trend analysis", "Alerts", "Actionable insights" }
        );

        _metadata["N360AutoDocumentation"] = CreateAIMetadata(
            "N360AutoDocumentation", "AI clinical note generation",
            new[] { "Note generation", "Documentation assistance", "Ambient scribing" },
            new[] { "Voice-to-text", "Smart templates", "Context-aware", "Code suggestions", "Time-saving" }
        );

        _metadata["N360ReadmissionPredictor"] = CreateAIMetadata(
            "N360ReadmissionPredictor", "ML readmission risk prediction",
            new[] { "Readmission prevention", "Discharge planning", "Care transitions" },
            new[] { "30-day prediction", "Risk factors", "Intervention suggestions", "Model explainability", "Validation" }
        );

        _metadata["N360AnomalyDetection"] = CreateAIMetadata(
            "N360AnomalyDetection", "Anomaly detection in patient vitals",
            new[] { "Early warning", "Patient monitoring", "ICU alerts" },
            new[] { "Real-time detection", "Baseline learning", "Alert thresholds", "Trend analysis", "False positive reduction" }
        );

        _metadata["N360PatientClassification"] = CreateAIMetadata(
            "N360PatientClassification", "ML patient categorization",
            new[] { "Patient segmentation", "Population health", "Care pathways" },
            new[] { "Automatic classification", "Cohort identification", "Risk groups", "Care recommendations", "Reporting" }
        );

        _metadata["N360TrendAnalysis"] = CreateAIMetadata(
            "N360TrendAnalysis", "Healthcare trend forecasting",
            new[] { "Capacity planning", "Disease surveillance", "Resource forecasting" },
            new[] { "Time series analysis", "Forecasting", "Seasonality", "Confidence intervals", "Visualizations" }
        );

        _metadata["N360SmartScheduling"] = CreateAIMetadata(
            "N360SmartScheduling", "AI-optimized appointment scheduling",
            new[] { "Smart scheduling", "No-show prediction", "Resource optimization" },
            new[] { "Pattern learning", "Optimization", "Overbooking management", "Patient preferences", "Provider efficiency" }
        );

        _metadata["N360VoiceToText"] = CreateAIMetadata(
            "N360VoiceToText", "Medical voice transcription",
            new[] { "Clinical documentation", "Voice notes", "Dictation" },
            new[] { "Medical vocabulary", "Speaker identification", "Real-time transcription", "Punctuation", "Editing" }
        );

        _metadata["N360CodingAssistant"] = CreateAIMetadata(
            "N360CodingAssistant", "AI medical coding suggestions",
            new[] { "Medical coding", "Billing accuracy", "Documentation improvement" },
            new[] { "ICD/CPT suggestions", "NLP analysis", "Code validation", "Documentation queries", "Audit support" }
        );

        _metadata["N360ResourceOptimization"] = CreateAIMetadata(
            "N360ResourceOptimization", "ML resource allocation",
            new[] { "Capacity planning", "Staff scheduling", "Resource management" },
            new[] { "Demand forecasting", "Optimal allocation", "Constraint handling", "Scenario analysis", "Cost optimization" }
        );
    }

    private void AddNavigationComponents()
    {
        _metadata["N360Sidebar"] = CreateNavigationMetadata(
            "N360Sidebar", "Collapsible sidebar navigation",
            new[] { "App navigation", "Menu sidebar", "Left navigation" },
            new[] { "Collapse/expand", "Icons", "Badges", "Multi-level", "Responsive" }
        );

        _metadata["N360AppBar"] = CreateNavigationMetadata(
            "N360AppBar", "Application top bar with actions",
            new[] { "Page header", "Top navigation", "App toolbar" },
            new[] { "Logo", "Navigation links", "Search", "User menu", "Notifications", "Sticky" }
        );

        _metadata["N360Tabs"] = CreateNavigationMetadata(
            "N360Tabs", "Tabbed navigation component",
            new[] { "Content sections", "Multi-view navigation", "Settings pages" },
            new[] { "Horizontal/vertical", "Icons", "Badges", "Closeable", "Drag reorder", "Lazy loading" }
        );

        _metadata["N360Breadcrumb"] = CreateNavigationMetadata(
            "N360Breadcrumb", "Breadcrumb navigation trail",
            new[] { "Page hierarchy", "Navigation context", "Back navigation" },
            new[] { "Auto-generated", "Custom separators", "Icons", "Click navigation", "Overflow handling" }
        );

        _metadata["N360Menu"] = CreateNavigationMetadata(
            "N360Menu", "Context menu component",
            new[] { "Navigation menu", "Action menu", "Dropdown menu" },
            new[] { "Nested items", "Icons", "Shortcuts", "Dividers", "Disabled items", "Templates" }
        );

        _metadata["N360ContextMenu"] = CreateNavigationMetadata(
            "N360ContextMenu", "Right-click context menu",
            new[] { "Right-click actions", "Context actions", "Quick actions" },
            new[] { "Position control", "Nested menus", "Icons", "Shortcuts", "Custom triggers" }
        );

        _metadata["N360Toolbar"] = CreateNavigationMetadata(
            "N360Toolbar", "Action toolbar component",
            new[] { "Page actions", "Editor toolbars", "Grid toolbars" },
            new[] { "Buttons", "Dropdowns", "Separators", "Overflow menu", "Responsive", "Icons" }
        );

        _metadata["N360Stepper"] = CreateNavigationMetadata(
            "N360Stepper", "Multi-step form wizard",
            new[] { "Multi-step forms", "Onboarding", "Wizards", "Checkout flows" },
            new[] { "Linear/non-linear", "Validation", "Progress indicator", "Back/next", "Step icons", "Summary" }
        );

        _metadata["N360Accordion"] = CreateNavigationMetadata(
            "N360Accordion", "Expandable accordion panels",
            new[] { "FAQs", "Collapsible sections", "Content organization" },
            new[] { "Expand/collapse", "Multiple open", "Icons", "Animation", "Nested accordions" }
        );

        _metadata["N360Carousel"] = CreateNavigationMetadata(
            "N360Carousel", "Image/content carousel",
            new[] { "Image galleries", "Featured content", "Testimonials", "Product showcases" },
            new[] { "Auto-play", "Navigation arrows", "Indicators", "Touch swipe", "Loop", "Lazy loading" }
        );

        _metadata["N360Drawer"] = CreateNavigationMetadata(
            "N360Drawer", "Slide-out drawer panel",
            new[] { "Side panels", "Filters", "Details panel", "Mobile navigation" },
            new[] { "Left/right/top/bottom", "Overlay/push", "Backdrop", "Responsive", "Nested drawers" }
        );

        _metadata["N360BottomSheet"] = CreateNavigationMetadata(
            "N360BottomSheet", "Bottom sheet modal",
            new[] { "Mobile actions", "Quick actions", "Filters", "Details" },
            new[] { "Swipe to dismiss", "Partial/full height", "Backdrop", "Snap points", "Mobile-optimized" }
        );

        _metadata["N360Navigation"] = CreateNavigationMetadata(
            "N360Navigation", "Main navigation component",
            new[] { "Primary navigation", "App menu", "Site navigation" },
            new[] { "Multi-level", "Icons", "Badges", "Active states", "Responsive", "Mega menu" }
        );
    }

    private void AddFeedbackComponents()
    {
        _metadata["N360Toast"] = CreateFeedbackMetadata(
            "N360Toast", "Toast notification",
            new[] { "Success messages", "Error alerts", "Info notifications" },
            new[] { "Auto-dismiss", "Position control", "Action buttons", "Stacking", "Animation" }
        );

        _metadata["N360Dialog"] = CreateFeedbackMetadata(
            "N360Dialog", "Modal dialog component",
            new[] { "Confirmations", "Forms", "Alerts", "Details view" },
            new[] { "Sizes", "Header/footer", "Backdrop", "Draggable", "Fullscreen", "Nested dialogs" }
        );

        _metadata["N360Alert"] = CreateFeedbackMetadata(
            "N360Alert", "Alert message component",
            new[] { "Inline alerts", "Warnings", "Information", "Error messages" },
            new[] { "Types (success, error, warning, info)", "Icons", "Dismissible", "Actions", "Inline/banner" }
        );

        _metadata["N360Snackbar"] = CreateFeedbackMetadata(
            "N360Snackbar", "Snackbar notification",
            new[] { "Action feedback", "Undo actions", "Brief messages" },
            new[] { "Auto-dismiss", "Action button", "Queuing", "Positioning", "Mobile-friendly" }
        );

        _metadata["N360Tooltip"] = CreateFeedbackMetadata(
            "N360Tooltip", "Tooltip overlay",
            new[] { "Help text", "Explanations", "Icon tooltips", "Truncated text" },
            new[] { "Positioning", "Delay", "HTML content", "Arrow", "Triggers (hover, focus, click)" }
        );

        _metadata["N360Progress"] = CreateFeedbackMetadata(
            "N360Progress", "Progress indicator",
            new[] { "Loading states", "Upload progress", "Task completion" },
            new[] { "Linear/circular", "Determinate/indeterminate", "Labels", "Colors", "Animation" }
        );

        _metadata["N360Skeleton"] = CreateFeedbackMetadata(
            "N360Skeleton", "Loading skeleton",
            new[] { "Content loading", "Placeholder", "Perceived performance" },
            new[] { "Various shapes", "Animation", "Custom structure", "Responsive", "Dark mode" }
        );

        _metadata["N360Spinner"] = CreateFeedbackMetadata(
            "N360Spinner", "Loading spinner",
            new[] { "Loading indicator", "Processing state", "Async operations" },
            new[] { "Sizes", "Colors", "Overlay", "Text label", "Backdrop" }
        );
    }

    private void AddEnterpriseComponents()
    {
        _metadata["N360FormBuilder"] = CreateEnterpriseMetadata(
            "N360FormBuilder", "Drag-and-drop form builder",
            new[] { "Dynamic forms", "Survey creation", "Form design" },
            new[] { "Visual designer", "Field library", "Validation rules", "Conditional logic", "Templates", "Export" }
        );

        _metadata["N360WorkflowDesigner"] = CreateEnterpriseMetadata(
            "N360WorkflowDesigner", "Visual workflow builder",
            new[] { "Process automation", "Workflow design", "Approval flows" },
            new[] { "Drag-drop nodes", "Connections", "Conditions", "Actions", "Testing", "Version control" }
        );

        _metadata["N360ReportViewer"] = CreateEnterpriseMetadata(
            "N360ReportViewer", "Report viewer and generator",
            new[] { "Report display", "Data reports", "Analytics" },
            new[] { "Multiple formats", "Export (PDF, Excel)", "Parameterized", "Pagination", "Print" }
        );

        _metadata["N360NotificationCenter"] = CreateEnterpriseMetadata(
            "N360NotificationCenter", "Notification management hub",
            new[] { "User notifications", "Alerts center", "Message hub" },
            new[] { "Unread count", "Categories", "Mark as read", "Actions", "Real-time updates", "History" }
        );

        _metadata["N360UserDirectory"] = CreateEnterpriseMetadata(
            "N360UserDirectory", "Employee/user directory",
            new[] { "Staff directory", "Contact list", "Team directory" },
            new[] { "Search", "Filters", "Org chart", "Contact cards", "Export", "Presence indicators" }
        );

        _metadata["N360RoleManager"] = CreateEnterpriseMetadata(
            "N360RoleManager", "Role management interface",
            new[] { "Permission management", "Role assignment", "Access control" },
            new[] { "Role creation", "Permission matrix", "User assignment", "Inheritance", "Audit log" }
        );

        _metadata["N360AuditLog"] = CreateEnterpriseMetadata(
            "N360AuditLog", "Activity audit log viewer",
            new[] { "Compliance", "Activity tracking", "Security auditing" },
            new[] { "Timeline view", "Filtering", "Search", "Export", "User actions", "System events" }
        );

        _metadata["N360Settings"] = CreateEnterpriseMetadata(
            "N360Settings", "Application settings panel",
            new[] { "User preferences", "App configuration", "System settings" },
            new[] { "Categorized settings", "Search", "Reset", "Validation", "Save states", "Import/export" }
        );

        _metadata["N360ProfileEditor"] = CreateEnterpriseMetadata(
            "N360ProfileEditor", "User profile editor",
            new[] { "User profiles", "Account management", "Profile updates" },
            new[] { "Photo upload", "Field validation", "Password change", "Preferences", "2FA setup" }
        );

        _metadata["N360ActivityLog"] = CreateEnterpriseMetadata(
            "N360ActivityLog", "User activity timeline",
            new[] { "Activity feed", "User actions", "History tracking" },
            new[] { "Timeline", "Filtering", "Grouping", "Real-time updates", "Export" }
        );

        _metadata["N360Calendar"] = CreateEnterpriseMetadata(
            "N360Calendar", "Full calendar component",
            new[] { "Event calendar", "Scheduling", "Appointments" },
            new[] { "Multiple views", "Drag-drop events", "Recurring events", "Time zones", "Resources", "Export" }
        );

        _metadata["N360Scheduler"] = CreateEnterpriseMetadata(
            "N360Scheduler", "Resource scheduler",
            new[] { "Resource scheduling", "Room booking", "Equipment scheduling" },
            new[] { "Timeline view", "Resource conflicts", "Drag-drop", "Recurring", "Availability" }
        );

        _metadata["N360Kanban"] = CreateEnterpriseMetadata(
            "N360Kanban", "Kanban board component",
            new[] { "Task management", "Project boards", "Workflow visualization" },
            new[] { "Drag-drop cards", "Swimlanes", "WIP limits", "Card templates", "Filters", "Search" }
        );

        _metadata["N360TaskManager"] = CreateEnterpriseMetadata(
            "N360TaskManager", "Task management interface",
            new[] { "Task lists", "To-do management", "Work tracking" },
            new[] { "Priorities", "Due dates", "Assignments", "Tags", "Subtasks", "Filters", "Bulk actions" }
        );

        _metadata["N360ProjectPlanner"] = CreateEnterpriseMetadata(
            "N360ProjectPlanner", "Project planning tool",
            new[] { "Project management", "Planning", "Milestones" },
            new[] { "Timeline", "Dependencies", "Resources", "Budget tracking", "Reporting", "Templates" }
        );

        _metadata["N360TeamCollaboration"] = CreateEnterpriseMetadata(
            "N360TeamCollaboration", "Team collaboration hub",
            new[] { "Team workspace", "Collaboration", "Communication" },
            new[] { "Chat", "File sharing", "Tasks", "Calendar", "Mentions", "Notifications", "Video calls" }
        );

        _metadata["N360DocumentEditor"] = CreateEnterpriseMetadata(
            "N360DocumentEditor", "Collaborative document editor",
            new[] { "Document editing", "Real-time collaboration", "Rich text" },
            new[] { "Real-time collaboration", "Comments", "Version history", "Track changes", "Export" }
        );

        _metadata["N360Inbox"] = CreateEnterpriseMetadata(
            "N360Inbox", "Message inbox component",
            new[] { "Internal messaging", "Email client", "Message management" },
            new[] { "Folders", "Search", "Filters", "Compose", "Attachments", "Read receipts", "Archive" }
        );

        _metadata["N360MultiStepForm"] = CreateEnterpriseMetadata(
            "N360MultiStepForm", "Complex multi-step form",
            new[] { "Complex forms", "Data collection", "Application forms" },
            new[] { "Step validation", "Progress tracking", "Save draft", "Review step", "Conditional steps" }
        );

        _metadata["N360Wizard"] = CreateEnterpriseMetadata(
            "N360Wizard", "Setup wizard component",
            new[] { "Onboarding", "Setup flows", "Configuration wizards" },
            new[] { "Step navigation", "Validation", "Summary", "Skip steps", "Progress indicator" }
        );

        _metadata["N360Survey"] = CreateEnterpriseMetadata(
            "N360Survey", "Survey form builder and viewer",
            new[] { "Surveys", "Questionnaires", "Feedback forms" },
            new[] { "Multiple question types", "Logic branching", "Templates", "Response collection", "Analytics" }
        );

        _metadata["N360Poll"] = CreateEnterpriseMetadata(
            "N360Poll", "Polling component",
            new[] { "Quick polls", "Voting", "Opinion gathering" },
            new[] { "Multiple choice", "Real-time results", "Anonymous voting", "Result visualization", "Time limits" }
        );
    }

    // Helper methods to create metadata templates
    private ComponentMetadata CreateInputMetadata(string name, string description, string[] whenToUse, string[] keyFeatures)
    {
        return new ComponentMetadata
        {
            Name = name,
            Category = "Input",
            Description = description,
            WhenToUse = whenToUse,
            KeyFeatures = keyFeatures,
            Parameters = new[]
            {
                new ComponentParameter("Value", "TValue", "default", "The component value"),
                new ComponentParameter("OnValueChanged", "EventCallback<TValue>", "default", "Event when value changes"),
                new ComponentParameter("IsDisabled", "bool", "false", "Whether component is disabled"),
                new ComponentParameter("ValidationRules", "ValidationRules?", "null", "Validation rules"),
                new ComponentParameter("CssClass", "string?", "null", "Additional CSS classes")
            },
            Events = new[]
            {
                new ComponentEvent("OnValueChanged", "EventCallback<TValue>", "Fired when value changes"),
                new ComponentEvent("OnFocus", "EventCallback", "Fired when component receives focus"),
                new ComponentEvent("OnBlur", "EventCallback", "Fired when component loses focus")
            },
            CodeExamples = new[]
            {
                new CodeExample("Basic Usage", $"Simple {name} example", $"<{name} @bind-Value=\"value\" />", "<input />")
            },
            AccessibilityLevel = "AA",
            SupportsKeyboard = true,
            SupportsScreenReader = true,
            SupportsHighContrast = true,
            KeyboardShortcuts = new[] { new KeyboardShortcut("Tab", "Navigate focus") },
            AriaAttributes = new[] { "aria-label", "aria-required", "aria-invalid" }
        };
    }

    private ComponentMetadata CreateDataMetadata(string name, string description, string[] whenToUse, string[] keyFeatures)
    {
        return new ComponentMetadata
        {
            Name = name,
            Category = "Data",
            Description = description,
            WhenToUse = whenToUse,
            KeyFeatures = keyFeatures,
            Parameters = new[]
            {
                new ComponentParameter("DataSource", "IEnumerable<TItem>", "[]", "Data source"),
                new ComponentParameter("OnSelectionChanged", "EventCallback<TItem>", "default", "Event when selection changes"),
                new ComponentParameter("Height", "string", "\"auto\"", "Component height")
            },
            Events = new[] { new ComponentEvent("OnSelectionChanged", "EventCallback<TItem>", "Fired when selection changes") },
            CodeExamples = new[] { new CodeExample("Basic Usage", $"Simple {name}", $"<{name} DataSource=\"@data\" />", "") },
            AccessibilityLevel = "AA",
            SupportsKeyboard = true,
            SupportsScreenReader = true,
            SupportsHighContrast = true,
            KeyboardShortcuts = new[] { new KeyboardShortcut("Arrow Keys", "Navigate"), new KeyboardShortcut("Enter", "Select") },
            AriaAttributes = new[] { "role", "aria-label", "aria-selected" }
        };
    }

    private ComponentMetadata CreateChartMetadata(string name, string description, string[] whenToUse, string[] keyFeatures)
    {
        return new ComponentMetadata
        {
            Name = name,
            Category = "Charts",
            Description = description,
            WhenToUse = whenToUse,
            KeyFeatures = keyFeatures,
            Parameters = new[]
            {
                new ComponentParameter("Data", "object", "required", "Chart data"),
                new ComponentParameter("Width", "string", "\"100%\"", "Chart width"),
                new ComponentParameter("Height", "string", "\"400px\"", "Chart height")
            },
            Events = new[] { new ComponentEvent("OnDataPointClick", "EventCallback", "Fired when data point clicked") },
            CodeExamples = new[] { new CodeExample("Basic Chart", $"Simple {name}", $"<{name} Data=\"@chartData\" />", "") },
            AccessibilityLevel = "AA",
            SupportsKeyboard = true,
            SupportsScreenReader = true,
            SupportsHighContrast = true,
            KeyboardShortcuts = new[] { new KeyboardShortcut("Tab", "Navigate elements") },
            AriaAttributes = new[] { "role=\"img\"", "aria-label" }
        };
    }

    private ComponentMetadata CreateHealthcareMetadata(string name, string description, string[] whenToUse, string[] keyFeatures)
    {
        return new ComponentMetadata
        {
            Name = name,
            Category = "Healthcare",
            Description = description,
            WhenToUse = whenToUse,
            KeyFeatures = keyFeatures,
            Parameters = new[]
            {
                new ComponentParameter("PatientId", "Guid?", "null", "Patient identifier"),
                new ComponentParameter("OnDataChanged", "EventCallback", "default", "Event when data changes")
            },
            Events = new[] { new ComponentEvent("OnDataChanged", "EventCallback", "Fired when data changes") },
            CodeExamples = new[] { new CodeExample("Basic Usage", $"Simple {name}", $"<{name} PatientId=\"@patientId\" />", "") },
            AccessibilityLevel = "AA",
            SupportsKeyboard = true,
            SupportsScreenReader = true,
            SupportsHighContrast = true,
            KeyboardShortcuts = new[] { new KeyboardShortcut("Tab", "Navigate"), new KeyboardShortcut("Enter", "Select") },
            AriaAttributes = new[] { "aria-label", "role" }
        };
    }

    private ComponentMetadata CreateAIMetadata(string name, string description, string[] whenToUse, string[] keyFeatures)
    {
        return new ComponentMetadata
        {
            Name = name,
            Category = "AI-Powered",
            Description = description,
            WhenToUse = whenToUse,
            KeyFeatures = keyFeatures,
            Parameters = new[]
            {
                new ComponentParameter("ModelEndpoint", "string", "required", "AI model endpoint URL"),
                new ComponentParameter("OnPredictionComplete", "EventCallback", "default", "Event when prediction completes")
            },
            Events = new[] { new ComponentEvent("OnPredictionComplete", "EventCallback", "Fired when AI prediction completes") },
            CodeExamples = new[] { new CodeExample("Basic Usage", $"Simple {name}", $"<{name} ModelEndpoint=\"@endpoint\" />", "") },
            AccessibilityLevel = "AA",
            SupportsKeyboard = true,
            SupportsScreenReader = true,
            SupportsHighContrast = true,
            KeyboardShortcuts = Array.Empty<KeyboardShortcut>(),
            AriaAttributes = new[] { "aria-label", "aria-live=\"polite\"" }
        };
    }

    private ComponentMetadata CreateNavigationMetadata(string name, string description, string[] whenToUse, string[] keyFeatures)
    {
        return new ComponentMetadata
        {
            Name = name,
            Category = "Navigation",
            Description = description,
            WhenToUse = whenToUse,
            KeyFeatures = keyFeatures,
            Parameters = new[]
            {
                new ComponentParameter("Items", "List<NavigationItem>", "[]", "Navigation items"),
                new ComponentParameter("OnItemSelected", "EventCallback<NavigationItem>", "default", "Event when item selected")
            },
            Events = new[] { new ComponentEvent("OnItemSelected", "EventCallback", "Fired when navigation item selected") },
            CodeExamples = new[] { new CodeExample("Basic Usage", $"Simple {name}", $"<{name} Items=\"@navItems\" />", "") },
            AccessibilityLevel = "AA",
            SupportsKeyboard = true,
            SupportsScreenReader = true,
            SupportsHighContrast = true,
            KeyboardShortcuts = new[] { new KeyboardShortcut("Arrow Keys", "Navigate"), new KeyboardShortcut("Enter", "Select") },
            AriaAttributes = new[] { "role=\"navigation\"", "aria-label" }
        };
    }

    private ComponentMetadata CreateFeedbackMetadata(string name, string description, string[] whenToUse, string[] keyFeatures)
    {
        return new ComponentMetadata
        {
            Name = name,
            Category = "Feedback",
            Description = description,
            WhenToUse = whenToUse,
            KeyFeatures = keyFeatures,
            Parameters = new[]
            {
                new ComponentParameter("Message", "string", "\"\"", "Message to display"),
                new ComponentParameter("Type", "MessageType", "Info", "Message type (Success, Error, Warning, Info)"),
                new ComponentParameter("OnClose", "EventCallback", "default", "Event when closed")
            },
            Events = new[] { new ComponentEvent("OnClose", "EventCallback", "Fired when component is closed") },
            CodeExamples = new[] { new CodeExample("Basic Usage", $"Simple {name}", $"<{name} Message=\"Success!\" Type=\"MessageType.Success\" />", "") },
            AccessibilityLevel = "AA",
            SupportsKeyboard = true,
            SupportsScreenReader = true,
            SupportsHighContrast = true,
            KeyboardShortcuts = new[] { new KeyboardShortcut("Escape", "Close") },
            AriaAttributes = new[] { "role=\"alert\"", "aria-live=\"assertive\"" }
        };
    }

    private ComponentMetadata CreateEnterpriseMetadata(string name, string description, string[] whenToUse, string[] keyFeatures)
    {
        return new ComponentMetadata
        {
            Name = name,
            Category = "Enterprise",
            Description = description,
            WhenToUse = whenToUse,
            KeyFeatures = keyFeatures,
            Parameters = new[]
            {
                new ComponentParameter("Data", "object", "required", "Component data"),
                new ComponentParameter("OnAction", "EventCallback", "default", "Event when action performed")
            },
            Events = new[] { new ComponentEvent("OnAction", "EventCallback", "Fired when action is performed") },
            CodeExamples = new[] { new CodeExample("Basic Usage", $"Simple {name}", $"<{name} Data=\"@data\" />", "") },
            AccessibilityLevel = "AA",
            SupportsKeyboard = true,
            SupportsScreenReader = true,
            SupportsHighContrast = true,
            KeyboardShortcuts = new[] { new KeyboardShortcut("Tab", "Navigate"), new KeyboardShortcut("Ctrl+S", "Save") },
            AriaAttributes = new[] { "aria-label", "role" }
        };
    }
}

public class ComponentMetadata
{
    public required string Name { get; init; }
    public required string Category { get; init; }
    public required string Description { get; init; }
    public required string[] WhenToUse { get; init; }
    public required string[] KeyFeatures { get; init; }
    public required ComponentParameter[] Parameters { get; init; }
    public required ComponentEvent[] Events { get; init; }
    public ComponentMethod[] Methods { get; init; } = Array.Empty<ComponentMethod>();
    public required CodeExample[] CodeExamples { get; init; }
    public required string AccessibilityLevel { get; init; }
    public required bool SupportsKeyboard { get; init; }
    public required bool SupportsScreenReader { get; init; }
    public required bool SupportsHighContrast { get; init; }
    public required KeyboardShortcut[] KeyboardShortcuts { get; init; }
    public required string[] AriaAttributes { get; init; }
}

public record ComponentParameter(string Name, string Type, string DefaultValue, string Description);
public record ComponentEvent(string Name, string Type, string Description);
public record ComponentMethod(string Name, string ReturnType, string Description);
public record CodeExample(string Title, string Description, string Code, string PreviewHtml);
public record KeyboardShortcut(string Key, string Action);
