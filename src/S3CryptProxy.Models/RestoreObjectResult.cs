namespace S3CryptProxy.Models;

/// <summary>
/// Result from a restore object request.
/// </summary>
public class RestoreObjectResult
{
    /// <summary>
    /// True if the object already had an active restored copy and the operation updated its expiry.
    /// False if a new restore was initiated.
    /// </summary>
    public bool AlreadyRestored { get; set; } = false;

    /// <summary>
    /// Restore output path for restore-select operations.
    /// Reserved for future support.
    /// </summary>
    public string RestoreOutputPath { get; set; } = null;
}