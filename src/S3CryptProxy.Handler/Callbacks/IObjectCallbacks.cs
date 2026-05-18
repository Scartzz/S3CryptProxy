namespace S3CryptProxy.Handler.Callbacks;

using System;
using System.Threading.Tasks;
using S3CryptProxy.Models;

/// <summary>
/// Callback methods for object operations.
/// </summary>
public interface IObjectCallbacks
{
    /// <summary>
    /// Abort a multipart upload.
    /// </summary>
    Task AbortMultipartUpload(S3Context ctx);

    /// <summary>
    /// Complete multipart upload.
    /// </summary>
    Task<CompleteMultipartUploadResult> CompleteMultipartUpload(S3Context ctx, CompleteMultipartUpload complete);

    /// <summary>
    /// Create multipart upload.
    /// </summary>
    Task<InitiateMultipartUploadResult> CreateMultipartUpload(S3Context ctx);

    /// <summary>
    /// Delete an object.
    /// </summary>
    Task Delete(S3Context ctx);

    /// <summary>
    /// Delete an object's ACL.
    /// </summary>
    Task DeleteAcl(S3Context ctx);

    /// <summary>
    /// Delete an object's tags.
    /// </summary>
    Task DeleteTagging(S3Context ctx);

    /// <summary>
    /// Delete multiple objects.
    /// </summary>
    Task<DeleteResult> DeleteMultiple(S3Context ctx, DeleteMultiple delete);

    /// <summary>
    /// Check for the existence of an object.
    /// Return the ObjectMetadata if it exists, null if it doesn't.
    /// </summary>
    Task<ObjectMetadata> Exists(S3Context ctx);

    /// <summary>
    /// Read an object.
    /// </summary>
    Task<S3Object> Read(S3Context ctx);

    /// <summary>
    /// Read an object's access control list.
    /// </summary>
    Task<AccessControlPolicy> ReadAcl(S3Context ctx);

    /// <summary>
    /// Read the parts associated with a multipart upload.
    /// </summary>
    Task<ListPartsResult> ReadParts(S3Context ctx);

    /// <summary>
    /// Read a range of bytes from an object.
    /// </summary>
    Task<S3Object> ReadRange(S3Context ctx);

    /// <summary>
    /// Read an object's tags.
    /// </summary>
    Task<Tagging> ReadTagging(S3Context ctx);

    /// <summary>
    /// Restore an archived object.
    /// </summary>
    Task<RestoreObjectResult> Restore(S3Context ctx, RestoreRequest restore);

    /// <summary>
    /// Read an object's legal hold status.
    /// </summary>
    Task<LegalHold> ReadLegalHold(S3Context ctx);

    /// <summary>
    /// Read an object's retention status.
    /// </summary>
    Task<Retention> ReadRetention(S3Context ctx);

    /// <summary>
    /// Select content from an object.
    /// </summary>
    Task SelectContent(S3Context ctx, SelectObjectContentRequest select);

    /// <summary>
    /// Upload part.
    /// </summary>
    Task UploadPart(S3Context ctx);

    /// <summary>
    /// Write an object.
    /// </summary>
    Task Write(S3Context ctx);

    /// <summary>
    /// Write an object's access control list, replacing the previous ACL.
    /// </summary>
    Task WriteAcl(S3Context ctx, AccessControlPolicy acl);

    /// <summary>
    /// Write tags to an object, replacing the previous tags.
    /// </summary>
    Task WriteTagging(S3Context ctx, Tagging tagging);

    /// <summary>
    /// Write a legal hold status to an object.
    /// </summary>
    Task WriteLegalHold(S3Context ctx, LegalHold legalHold);

    /// <summary>
    /// Write a retention status to an object.
    /// </summary>
    Task WriteRetention(S3Context ctx, Retention retention);
}
