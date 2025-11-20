namespace Nalam360.Platform.Core.Identity;

/// <summary>
/// Abstraction for GUID generation to enable testability
/// </summary>
public interface IGuidProvider
{
    /// <summary>
    /// Creates a new GUID
    /// </summary>
    Guid NewGuid();

    /// <summary>
    /// Creates a new sequential GUID optimized for database indexing
    /// </summary>
    Guid NewSequentialGuid();
}
