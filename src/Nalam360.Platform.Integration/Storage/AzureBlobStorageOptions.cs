namespace Nalam360.Platform.Integration.Storage;

/// <summary>
/// Configuration options for Azure Blob Storage.
/// </summary>
public class AzureBlobStorageOptions
{
    /// <summary>
    /// Gets or sets the Azure Storage connection string.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;
}
