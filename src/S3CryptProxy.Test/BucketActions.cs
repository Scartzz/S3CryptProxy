namespace S3CryptProxy.Test;

using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Logging;
using S3CryptProxy.Handler;
using S3CryptProxy.Handler.Callbacks;
using S3CryptProxy.Models;
using Owner = S3CryptProxy.Models.Owner;
using RestoreStatus = S3CryptProxy.Models.RestoreStatus;
using Tagging = S3CryptProxy.Models.Tagging;

public class BucketActions : IBucketCallbacks
{
    private readonly AmazonS3Client _client;
    private readonly CryptUtils _cryptUtils;
    private readonly ILogger<BucketActions> _logger;

    public BucketActions(AmazonS3Client client, CryptUtils cryptUtils, ILogger<BucketActions> logger)
    {
        this._client = client;
        this._cryptUtils = cryptUtils;
        this._logger = logger;
    }

    public async Task<bool> Exists(S3Context ctx)
    {
        try
        {
            var resp = await this._client.HeadBucketAsync(new HeadBucketRequest()
            {
                BucketName = ctx.Request.Bucket,
            });
            return resp.HttpStatusCode == HttpStatusCode.OK;
        }
        catch (Exception e)
        {
            this._logger.LogWarning(e, "Bucket does not exist: " + ctx.Request.Bucket);
            return false;
        }
    }
    public async Task<LocationConstraint> ReadLocation(S3Context ctx)
    {
        var resp = await this._client.GetBucketLocationAsync(ctx.Request.Bucket);
        return new LocationConstraint(resp.Location.Value);
    }
    public async Task<ListBucketResult> Read(S3Context ctx)
    {
        string listType = ctx.Request.RetrieveQueryValue("list-type");
        if (string.IsNullOrWhiteSpace(listType))
        {
            var result = await this._client.ListObjectsAsync(new ListObjectsRequest()
            {
                BucketName = ctx.Request.Bucket,
                Delimiter = ctx.Request.Delimiter,
                Prefix = this._cryptUtils.GetStorageKey(ctx.Request.Prefix),
                Marker = ctx.Request.Marker,
                MaxKeys = ctx.Request.MaxKeys,
            });
            return new ListBucketResult()
            {
                NextMarker = result.NextMarker,
                Prefix = this._cryptUtils.GetRealKey(result.Prefix),
                Delimiter = result.Delimiter,
                Marker = result.Marker,
                MaxKeys = result.MaxKeys ?? 100_000,
                EncodingType = result.Encoding?.Value,
                IsTruncated = result.IsTruncated ?? false,
                CommonPrefixes = result.CommonPrefixes.Select(x => new CommonPrefixes(x)).ToList(),
                KeyCount = (result.S3Objects ?? []).Count,
                Name = ctx.Request.Bucket,
                Contents = (result.S3Objects ?? []).Select(x =>
                {
                    return new ObjectMetadata()
                    {
                        Key = this._cryptUtils.GetRealKey(x.Key),
                        LastModified = x.LastModified ?? DateTime.MinValue,
                        Size = this._cryptUtils.GetRealSize(x.Size ?? 0),
                        ETag = x.ETag,
                        Owner = x.Owner is null ? null : new Owner(x.Owner?.Id, x.Owner?.DisplayName),
                        StorageClass = x.StorageClass?.Value,
                        RestoreStatus = x.RestoreStatus is null ? null : new RestoreStatus()
                        {
                            ExpiryDate = x.RestoreStatus?.RestoreExpiryDate, 
                            OngoingRequest = x.RestoreStatus?.IsRestoreInProgress ?? false
                        },
                    };
                }).ToList()
            };
        }

        if (listType == "2")
        {
            var result = await this._client.ListObjectsV2Async(new ListObjectsV2Request()
            {
                BucketName = ctx.Request.Bucket,
                Delimiter = ctx.Request.Delimiter,
                Prefix = this._cryptUtils.GetStorageKey(ctx.Request.Prefix),
                ContinuationToken = ctx.Request.ContinuationToken,
                MaxKeys = ctx.Request.MaxKeys,
                FetchOwner = ctx.Request.RetrieveQueryValue("fetch-owner") == "true",
                StartAfter = this._cryptUtils.GetStorageKey(ctx.Request.RetrieveQueryValue("start-after")),
            });
            return new ListBucketResult()
            {
                NextContinuationToken = result.NextContinuationToken,
                Prefix = this._cryptUtils.GetRealKey(result.Prefix),
                Delimiter = result.Delimiter,
                ContinuationToken = result.ContinuationToken,
                MaxKeys = result.MaxKeys ?? 100_000,
                EncodingType = result.Encoding?.Value,
                IsTruncated = result.IsTruncated ?? false,
                CommonPrefixes = (result.CommonPrefixes ?? []).Select(x => new CommonPrefixes(x)).ToList(),
                KeyCount = (result.S3Objects ?? []).Count,
                Name = ctx.Request.Bucket,
                Contents = (result.S3Objects ?? []).Select(x =>
                {
                    return new ObjectMetadata()
                    {
                        Key = this._cryptUtils.GetRealKey(x.Key),
                        LastModified = x.LastModified ?? DateTime.MinValue,
                        Size = this._cryptUtils.GetRealSize(x.Size ?? 0),
                        ETag = x.ETag,
                        Owner = x.Owner is null ? null : new Owner(x.Owner?.Id, x.Owner?.DisplayName),
                        StorageClass = x.StorageClass?.Value,
                        RestoreStatus = x.RestoreStatus is null ? null : new RestoreStatus()
                        {
                            ExpiryDate = x.RestoreStatus?.RestoreExpiryDate, 
                            OngoingRequest = x.RestoreStatus?.IsRestoreInProgress ?? false
                        },
                    };
                }).ToList()
            };
        }
        
        throw new System.NotImplementedException();
    }
    public async Task<ListMultipartUploadsResult> ReadMultipartUploads(S3Context ctx)
    {
        throw new System.NotImplementedException();
    }
    public async Task<ListVersionsResult> ReadVersions(S3Context ctx)
    {
        throw new System.NotImplementedException();
    }

    public async Task Delete(S3Context ctx)
    {
        throw new System.NotImplementedException();
        await this._client.DeleteBucketAsync(ctx.Request.Bucket);
    }
    public async Task Write(S3Context ctx)
    {
        throw new System.NotImplementedException();
        await this._client.PutBucketAsync(new PutBucketRequest()
        {
            BucketName = ctx.Request.Bucket,
        });
    }

    public async Task<VersioningConfiguration> ReadVersioning(S3Context ctx)
    {
        throw new System.NotImplementedException();
    }
    public async Task WriteVersioning(S3Context ctx, VersioningConfiguration versioning)
    {
        throw new System.NotImplementedException();
    }
    
    public async Task DeleteTagging(S3Context ctx)
    {
        throw new System.NotImplementedException();
    }
    public async Task<Tagging> ReadTagging(S3Context ctx)
    {
        throw new System.NotImplementedException();
    }
    public async Task WriteTagging(S3Context ctx, Tagging tagging)
    {
        throw new System.NotImplementedException();
    }
    
    public async Task DeleteAcl(S3Context ctx)
    {
        throw new System.NotImplementedException();
    }
    public async Task<AccessControlPolicy> ReadAcl(S3Context ctx)
    {
        throw new System.NotImplementedException();
    }
    public async Task WriteAcl(S3Context ctx, AccessControlPolicy acl)
    {
        throw new System.NotImplementedException();
    }
}