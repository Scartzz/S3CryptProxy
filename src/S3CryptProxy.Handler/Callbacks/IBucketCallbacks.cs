namespace S3CryptProxy.Handler.Callbacks;

using System;
using System.Threading.Tasks;
using S3CryptProxy.Models;

/// <summary>
/// Callback methods for bucket operations.
/// </summary>
public interface IBucketCallbacks
{
    /// <summary>
    /// Delete a bucket.
    /// </summary>
    Task Delete(S3Context ctx);

    /// <summary>
    /// Delete a bucket's ACL.
    /// </summary>
    Task DeleteAcl(S3Context ctx);

    /// <summary>
    /// Delete a bucket's tags.
    /// </summary>
    Task DeleteTagging(S3Context ctx);

    /// <summary>
    /// Check for the existence of a bucket. 
    /// Return true if it exists, false if it doesn't.
    /// </summary>
    Task<bool> Exists(S3Context ctx);

    /// <summary>
    /// Enumerate a bucket.
    /// </summary>
    Task<ListBucketResult> Read(S3Context ctx);

    /// <summary>
    /// Read a bucket's access control policy.
    /// </summary>
    Task<AccessControlPolicy> ReadAcl(S3Context ctx);

    /// <summary>
    /// Retrieve location (region) constraint from the server for this bucket.
    /// </summary>
    Task<LocationConstraint> ReadLocation(S3Context ctx);

    /// <summary>
    /// Retrieve multipart uploads.
    /// </summary>
    Task<ListMultipartUploadsResult> ReadMultipartUploads(S3Context ctx);

    /// <summary>
    /// Read a bucket's tags.
    /// </summary>
    Task<Tagging> ReadTagging(S3Context ctx);

    /// <summary>
    /// Get a list of object versions in the bucket.
    /// </summary>
    Task<ListVersionsResult> ReadVersions(S3Context ctx);

    /// <summary>
    /// Get a bucket's versioning policy. 
    /// </summary>
    Task<VersioningConfiguration> ReadVersioning(S3Context ctx);

    /// <summary>
    /// Write a bucket.
    /// </summary>
    Task Write(S3Context ctx);

    /// <summary>
    /// Write an ACL to a bucket, deleting the previous ACL.
    /// </summary>
    Task WriteAcl(S3Context ctx, AccessControlPolicy acl);

    /// <summary>
    /// Write tags to a bucket, deleting the previous tags.
    /// </summary>
    Task WriteTagging(S3Context ctx, Tagging tagging);

    /// <summary>
    /// Set a bucket's versioning policy.
    /// </summary>
    Task WriteVersioning(S3Context ctx, VersioningConfiguration versioning);
}
