namespace S3CryptProxy.Handler.Callbacks;

using System.Threading.Tasks;
using S3CryptProxy.Models;

/// <summary>
/// Callback methods for service operations.
/// </summary>
public interface IServiceCallbacks
{
    /// <summary>
    /// List all buckets.
    /// </summary>
    public Task<ListAllMyBucketsResult> ListBuckets(S3Context ctx);

    /// <summary>
    /// Service exists.
    /// </summary>
    public Task<string> ServiceExists(S3Context ctx);

    /// <summary>
    /// Method to invoke to retrieve the base64-encoded secret key for a given requestor.
    /// </summary>
    public Task<string> GetSecretKey(S3Context ctx);
}