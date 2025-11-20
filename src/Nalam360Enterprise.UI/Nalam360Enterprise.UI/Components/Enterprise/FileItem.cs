namespace Nalam360Enterprise.UI.Components.Enterprise
{
    /// <summary>
    /// Represents a file or folder in the file explorer.
    /// </summary>
    public class FileItem
    {
        /// <summary>
        /// Gets or sets the unique identifier for the file.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets the file or folder name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the full path to the file or folder.
        /// </summary>
        public string Path { get; set; } = "/";

        /// <summary>
        /// Gets or sets the parent folder path.
        /// </summary>
        public string? ParentPath { get; set; }

        /// <summary>
        /// Gets or sets whether this is a folder.
        /// </summary>
        public bool IsFolder { get; set; }

        /// <summary>
        /// Gets or sets the file size in bytes.
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// Gets or sets the file type/extension.
        /// </summary>
        public string FileType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the MIME type of the file.
        /// </summary>
        public string? MimeType { get; set; }

        /// <summary>
        /// Gets or sets the date when the file was created.
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the date when the file was last modified.
        /// </summary>
        public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the date when the file was last accessed.
        /// </summary>
        public DateTime? AccessedDate { get; set; }

        /// <summary>
        /// Gets or sets the owner user ID.
        /// </summary>
        public string? OwnerId { get; set; }

        /// <summary>
        /// Gets or sets the owner user name.
        /// </summary>
        public string? OwnerName { get; set; }

        /// <summary>
        /// Gets or sets the thumbnail URL for preview.
        /// </summary>
        public string? ThumbnailUrl { get; set; }

        /// <summary>
        /// Gets or sets the download URL.
        /// </summary>
        public string? DownloadUrl { get; set; }

        /// <summary>
        /// Gets or sets whether the file is favorited.
        /// </summary>
        public bool IsFavorite { get; set; }

        /// <summary>
        /// Gets or sets whether the file is shared.
        /// </summary>
        public bool IsShared { get; set; }

        /// <summary>
        /// Gets or sets the share permissions.
        /// </summary>
        public FileSharePermissions SharePermissions { get; set; } = FileSharePermissions.None;

        /// <summary>
        /// Gets or sets whether the file is locked.
        /// </summary>
        public bool IsLocked { get; set; }

        /// <summary>
        /// Gets or sets the user who locked the file.
        /// </summary>
        public string? LockedBy { get; set; }

        /// <summary>
        /// Gets or sets the file status.
        /// </summary>
        public FileStatus Status { get; set; } = FileStatus.Available;

        /// <summary>
        /// Gets or sets the file version number.
        /// </summary>
        public int Version { get; set; } = 1;

        /// <summary>
        /// Gets or sets the version history.
        /// </summary>
        public List<FileVersion>? VersionHistory { get; set; }

        /// <summary>
        /// Gets or sets custom metadata.
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets file tags for categorization.
        /// </summary>
        public List<string> Tags { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the description or notes.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets whether the file is hidden.
        /// </summary>
        public bool IsHidden { get; set; }

        /// <summary>
        /// Gets or sets whether the file is read-only.
        /// </summary>
        public bool IsReadOnly { get; set; }

        /// <summary>
        /// Gets or sets whether the file is a system file.
        /// </summary>
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the checksum/hash for integrity verification.
        /// </summary>
        public string? Checksum { get; set; }

        /// <summary>
        /// Gets or sets the encryption status.
        /// </summary>
        public bool IsEncrypted { get; set; }

        /// <summary>
        /// Gets or sets the compression status.
        /// </summary>
        public bool IsCompressed { get; set; }

        /// <summary>
        /// Gets or sets the number of child items (for folders).
        /// </summary>
        public int ChildCount { get; set; }

        /// <summary>
        /// Gets or sets the total size of folder contents (for folders).
        /// </summary>
        public long? TotalSize { get; set; }

        /// <summary>
        /// Gets the file extension.
        /// </summary>
        public string Extension => System.IO.Path.GetExtension(Name);

        /// <summary>
        /// Gets the file name without extension.
        /// </summary>
        public string NameWithoutExtension => System.IO.Path.GetFileNameWithoutExtension(Name);

        /// <summary>
        /// Determines if the file is an image.
        /// </summary>
        public bool IsImage()
        {
            var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".svg", ".webp" };
            return imageExtensions.Contains(Extension.ToLower());
        }

        /// <summary>
        /// Determines if the file is a video.
        /// </summary>
        public bool IsVideo()
        {
            var videoExtensions = new[] { ".mp4", ".avi", ".mov", ".wmv", ".flv", ".webm", ".mkv" };
            return videoExtensions.Contains(Extension.ToLower());
        }

        /// <summary>
        /// Determines if the file is an audio file.
        /// </summary>
        public bool IsAudio()
        {
            var audioExtensions = new[] { ".mp3", ".wav", ".flac", ".aac", ".ogg", ".m4a" };
            return audioExtensions.Contains(Extension.ToLower());
        }

        /// <summary>
        /// Determines if the file is a document.
        /// </summary>
        public bool IsDocument()
        {
            var docExtensions = new[] { ".doc", ".docx", ".pdf", ".txt", ".rtf", ".odt" };
            return docExtensions.Contains(Extension.ToLower());
        }

        /// <summary>
        /// Determines if the file is a spreadsheet.
        /// </summary>
        public bool IsSpreadsheet()
        {
            var spreadsheetExtensions = new[] { ".xls", ".xlsx", ".csv", ".ods" };
            return spreadsheetExtensions.Contains(Extension.ToLower());
        }

        /// <summary>
        /// Determines if the file is a presentation.
        /// </summary>
        public bool IsPresentation()
        {
            var presentationExtensions = new[] { ".ppt", ".pptx", ".odp" };
            return presentationExtensions.Contains(Extension.ToLower());
        }

        /// <summary>
        /// Determines if the file is an archive.
        /// </summary>
        public bool IsArchive()
        {
            var archiveExtensions = new[] { ".zip", ".rar", ".7z", ".tar", ".gz" };
            return archiveExtensions.Contains(Extension.ToLower());
        }

        /// <summary>
        /// Determines if the file can be previewed in browser.
        /// </summary>
        public bool CanPreview()
        {
            return IsImage() || IsVideo() || IsAudio() || 
                   Extension.ToLower() is ".pdf" or ".txt" or ".html";
        }
    }

    /// <summary>
    /// Represents the status of a file.
    /// </summary>
    public enum FileStatus
    {
        /// <summary>
        /// File is available for access.
        /// </summary>
        Available,

        /// <summary>
        /// File is being uploaded.
        /// </summary>
        Uploading,

        /// <summary>
        /// File is being downloaded.
        /// </summary>
        Downloading,

        /// <summary>
        /// File is being processed.
        /// </summary>
        Processing,

        /// <summary>
        /// File is locked by another user.
        /// </summary>
        Locked,

        /// <summary>
        /// File is in quarantine (virus scan).
        /// </summary>
        Quarantined,

        /// <summary>
        /// File is archived.
        /// </summary>
        Archived,

        /// <summary>
        /// File is deleted (in trash).
        /// </summary>
        Deleted,

        /// <summary>
        /// File has an error.
        /// </summary>
        Error
    }

    /// <summary>
    /// Represents share permissions for a file.
    /// </summary>
    [Flags]
    public enum FileSharePermissions
    {
        /// <summary>
        /// No permissions.
        /// </summary>
        None = 0,

        /// <summary>
        /// Can view the file.
        /// </summary>
        View = 1,

        /// <summary>
        /// Can download the file.
        /// </summary>
        Download = 2,

        /// <summary>
        /// Can edit the file.
        /// </summary>
        Edit = 4,

        /// <summary>
        /// Can delete the file.
        /// </summary>
        Delete = 8,

        /// <summary>
        /// Can share the file with others.
        /// </summary>
        Share = 16,

        /// <summary>
        /// Full control over the file.
        /// </summary>
        FullControl = View | Download | Edit | Delete | Share
    }

    /// <summary>
    /// Represents a version of a file.
    /// </summary>
    public class FileVersion
    {
        /// <summary>
        /// Gets or sets the version number.
        /// </summary>
        public int VersionNumber { get; set; }

        /// <summary>
        /// Gets or sets the version date.
        /// </summary>
        public DateTime VersionDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the user who created this version.
        /// </summary>
        public string? CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the file size of this version.
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// Gets or sets version comments.
        /// </summary>
        public string? Comments { get; set; }

        /// <summary>
        /// Gets or sets the download URL for this version.
        /// </summary>
        public string? DownloadUrl { get; set; }

        /// <summary>
        /// Gets or sets whether this is the current version.
        /// </summary>
        public bool IsCurrent { get; set; }
    }
}
