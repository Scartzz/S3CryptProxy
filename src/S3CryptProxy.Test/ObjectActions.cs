namespace S3CryptProxy.Test;

using System.Threading.Tasks;
using Amazon.S3;
using Microsoft.Extensions.Logging;
using S3CryptProxy.Handler;
using S3CryptProxy.Handler.Callbacks;
using S3CryptProxy.Models;

public class ObjectActions : IObjectCallbacks
{
    private readonly AmazonS3Client _client;
    private readonly ILogger<ObjectActions> _logger;

    public ObjectActions(AmazonS3Client client, ILogger<ObjectActions> logger)
    {
        this._client = client;
        this._logger = logger;
    }

    public async Task AbortMultipartUpload(S3Context ctx)
    {
        throw new System.NotImplementedException();
    }

    public async Task<CompleteMultipartUploadResult> CompleteMultipartUpload(S3Context ctx, CompleteMultipartUpload complete)
    {
        throw new System.NotImplementedException();
    }

    public async Task<InitiateMultipartUploadResult> CreateMultipartUpload(S3Context ctx)
    {
        throw new System.NotImplementedException();
    }

    public async Task Delete(S3Context ctx)
    {
        throw new System.NotImplementedException();
    }

    public async Task DeleteAcl(S3Context ctx)
    {
        throw new System.NotImplementedException();
    }

    public async Task DeleteTagging(S3Context ctx)
    {
        throw new System.NotImplementedException();
    }

    public async Task<DeleteResult> DeleteMultiple(S3Context ctx, DeleteMultiple delete)
    {
        throw new System.NotImplementedException();
    }

    public async Task<ObjectMetadata> Exists(S3Context ctx)
    {
        throw new System.NotImplementedException();
    }

    public async Task<S3Object> Read(S3Context ctx)
    {
        throw new System.NotImplementedException();
    }

    public async Task<AccessControlPolicy> ReadAcl(S3Context ctx)
    {
        throw new System.NotImplementedException();
    }

    public async Task<ListPartsResult> ReadParts(S3Context ctx)
    {
        throw new System.NotImplementedException();
    }

    public async Task<S3Object> ReadRange(S3Context ctx)
    {
        throw new System.NotImplementedException();
    }

    public async Task<Tagging> ReadTagging(S3Context ctx)
    {
        throw new System.NotImplementedException();
    }

    public async Task<RestoreObjectResult> Restore(S3Context ctx, RestoreRequest restore)
    {
        throw new System.NotImplementedException();
    }

    public async Task<LegalHold> ReadLegalHold(S3Context ctx)
    {
        throw new System.NotImplementedException();
    }

    public async Task<Retention> ReadRetention(S3Context ctx)
    {
        throw new System.NotImplementedException();
    }

    public async Task SelectContent(S3Context ctx, SelectObjectContentRequest select)
    {
        throw new System.NotImplementedException();
    }

    public async Task UploadPart(S3Context ctx)
    {
        throw new System.NotImplementedException();
    }

    public async Task Write(S3Context ctx)
    {
        throw new System.NotImplementedException();
    }

    public async Task WriteAcl(S3Context ctx, AccessControlPolicy acl)
    {
        throw new System.NotImplementedException();
    }

    public async Task WriteTagging(S3Context ctx, Tagging tagging)
    {
        throw new System.NotImplementedException();
    }

    public async Task WriteLegalHold(S3Context ctx, LegalHold legalHold)
    {
        throw new System.NotImplementedException();
    }

    public async Task WriteRetention(S3Context ctx, Retention retention)
    {
        throw new System.NotImplementedException();
    }
}