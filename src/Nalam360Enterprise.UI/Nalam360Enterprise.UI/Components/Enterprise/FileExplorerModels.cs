namespace Nalam360Enterprise.UI.Components.Enterprise
{
    /// <summary>
    /// Represents a quick access item in the file explorer sidebar.
    /// </summary>
    public class QuickAccessItem
    {
        /// <summary>
        /// Gets or sets the display label.
        /// </summary>
        public string Label { get; set; } = "";

        /// <summary>
        /// Gets or sets the path to navigate to.
        /// </summary>
        public string Path { get; set; } = "";

        /// <summary>
        /// Gets or sets the icon emoji or symbol.
        /// </summary>
        public string Icon { get; set; } = "📁";
    }

    /// <summary>
    /// Represents an item in the upload queue.
    /// </summary>
    public class UploadItem
    {
        /// <summary>
        /// Gets or sets the file name.
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// Gets or sets the file size in bytes.
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// Gets or sets the upload progress percentage (0-100).
        /// </summary>
        public int Progress { get; set; }

        /// <summary>
        /// Gets or sets the upload status.
        /// </summary>
        public UploadStatus Status { get; set; } = UploadStatus.Queued;

        /// <summary>
        /// Gets or sets the error message if upload failed.
        /// </summary>
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// Represents the status of an upload.
    /// </summary>
    public enum UploadStatus
    {
        /// <summary>
        /// Queued for upload.
        /// </summary>
        Queued,

        /// <summary>
        /// Currently uploading.
        /// </summary>
        Uploading,

        /// <summary>
        /// Upload completed successfully.
        /// </summary>
        Completed,

        /// <summary>
        /// Upload failed.
        /// </summary>
        Failed,

        /// <summary>
        /// Upload cancelled.
        /// </summary>
        Cancelled
    }
}
