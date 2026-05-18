namespace S3CryptProxy.Test;

using System;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using S3CryptProxy.Handler;
using S3CryptProxy.Handler.Callbacks;
using S3CryptProxy.Models;

public class ServiceActions : IServiceCallbacks
{
    private readonly AmazonS3Client _s3Client;
    private readonly string _defaultRegion;
    private readonly Func<S3Context, Task<string>> _getSecretKey;

    public ServiceActions(AmazonS3Client s3Client, string defaultRegion, Func<S3Context, Task<string>> getSecretKey)
    {
        this._s3Client = s3Client;
        this._defaultRegion = defaultRegion;
        this._getSecretKey = getSecretKey;
    }

    public async Task<ListAllMyBucketsResult> ListBuckets(S3Context ctx)
    {
        string maxBuckets = ctx.Request.RetrieveQueryValue("max-buckets");
        
        var result = await this._s3Client.ListBucketsAsync(new ListBucketsRequest()
        {
            ContinuationToken = ctx.Request.RetrieveQueryValue("continuation-token"),
            BucketRegion = ctx.Request.RetrieveQueryValue("bucket-region"),
            MaxBuckets = maxBuckets is null ? null : int.Parse(maxBuckets),
            Prefix = ctx.Request.RetrieveQueryValue("prefix"),
        });
        
        ListAllMyBucketsResult returnResult = new ListAllMyBucketsResult();
        foreach (S3Bucket resultBucket in result.Buckets)
        {
            returnResult.Buckets.BucketList.Add(new Bucket()
            {
                Name = resultBucket.BucketName,
                CreationDate = resultBucket.CreationDate ?? DateTime.UtcNow,
                BucketRegion = resultBucket.BucketRegion,
                BucketArn = resultBucket.BucketArn,
            });
        }
        
        if (result.ContinuationToken is not null)
            returnResult.ContinuationToken = result.ContinuationToken;
        if (result.Prefix is not null)
            returnResult.Prefix = result.Prefix;

        return returnResult;
    }

    public async Task<string> ServiceExists(S3Context ctx)
    {
        return _defaultRegion;
    }

    public async Task<string> GetSecretKey(S3Context ctx)
    {
        return await _getSecretKey(ctx);
    }
}