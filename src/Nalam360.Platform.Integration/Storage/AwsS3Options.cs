namespace Nalam360.Platform.Integration.Storage;

/// <summary>
/// Configuration options for AWS S3.
/// </summary>
public class AwsS3Options
{
    /// <summary>
    /// Gets or sets the AWS access key.
    /// </summary>
    public string AccessKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the AWS secret key.
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the AWS region.
    /// </summary>
    public string Region { get; set; } = "us-east-1";
}
