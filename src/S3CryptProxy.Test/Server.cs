namespace S3CryptProxy.Test;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using S3CryptProxy.Handler;
using S3CryptProxy.Handler.Interfaces;
using S3CryptProxy.Models;
using S3CryptProxy.Server;

/*
 * Note: This must be run as administrator if the S3Server constructor uses '*', '+', or '0.0.0.0' as the listener hostname.
 *       Administrator not required if using 'localhost'.
 *       S3 clients will report failed operation if interacting with this node; it returns a simple 200 to each request.
 */

public class Server : IHostedService, IDisposable
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<Server> _logger;
    private readonly S3ServerSettings Settings = new S3ServerSettings(S3Watson.Create("localhost", 8000, false));
    private readonly AmazonS3Client _client;
    
    private S3Server _server = null;
    private bool _runForever = true;

    private readonly string Location = "us-west-1";
    private readonly ObjectMetadata ObjectMetadata = new ObjectMetadata("hello.txt", DateTime.Now, "6cd3556deb0da54bca060b4c39479839", 13, new Owner("admin", "Administrator"));
    private readonly Owner Owner = new Owner("admin", "Administrator");
    private readonly Grantee Grantee = new Grantee("admin", "Administrator", null, "CanonicalUser", "admin@admin.com");
    private readonly Tag Tag = new Tag("key", "value");
    private readonly ConcurrentDictionary<string, RestoreStatus> RestoreStatuses = new ConcurrentDictionary<string, RestoreStatus>(StringComparer.Ordinal);

    private readonly bool RandomizeHeadResponses = false;
    private readonly Random Random = new Random(Int32.MaxValue);

    public Server(ILoggerFactory loggerFactory, ILogger<Server> logger, IOptions<ServerSettings> settingsOptions)
    {
        this._loggerFactory = loggerFactory;
        this._logger = logger;
        
        var settings = settingsOptions.Value;
        this._client = new AmazonS3Client(settings.AccessKey, settings.SecretKey, new AmazonS3Config()
        {
            ServiceURL = settings.Endpoint,
            ForcePathStyle = true,
            Timeout = TimeSpan.FromSeconds(10),
        });
        
        Settings.Webserver.Hostname = "localhost";
        Settings.Webserver.Port = 8000;
        Settings.Webserver.Ssl.Enable = false;

        Settings.LoggerFactory = _loggerFactory;

        Settings.DefaultRequestHandler = DefaultRequestHandler;
        Settings.PreRequestHandler = PreRequestHandler;
        Settings.PostRequestHandler = PostRequestHandler;
        
        CryptUtils cryptUtils = new CryptUtils();

        Settings.ServiceCallbacks = new ServiceActions(this._client, "eu-central-1", async context =>
        {
            if (context.Request.AccessKey == settings.AccessKey)
                return settings.SecretKey;
            return null;
        });
        Settings.BucketCallbacks = new BucketActions(this._client, cryptUtils, loggerFactory.CreateLogger<BucketActions>());
        Settings.ObjectCallbacks = new ObjectActions(this._client, loggerFactory.CreateLogger<ObjectActions>());

        _server = new S3Server(Settings);
    }
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        this._logger.LogInformation("");
        this._logger.LogInformation("This program must be run as administrator");
        this._logger.LogInformation("");
        
        _server.Start();
        this._logger.LogInformation("Listening on http://" + Settings.Webserver.Hostname + ":" + Settings.Webserver.Port);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        this._server.Stop();
    }

    #region S3-API-Handlers

    #region Pre-Post-Default

    private async Task<bool> PreRequestHandler(S3Context ctx)
    {
        // this._logger.LogInformation(SerializationHelper.SerializeJson(ctx, true));
        return false;
    }

    private async Task DefaultRequestHandler(S3Context ctx)
    {
        this._logger.LogInformation("DefaultRequestHandler " + ctx.Http.Request.Method.ToString() + " " + ctx.Http.Request.UrlRawWithoutQuery);
        await ctx.Response.Send(ErrorCode.InvalidRequest);
    }

    private async Task PostRequestHandler(S3Context ctx)
    {
        this._logger.LogInformation("Request complete: " + ctx.Http.Request?.Method.ToString() + " " + ctx.Http.Request?.UrlRawWithQuery + ": " + ctx.Response.StatusCode);
    }

    #endregion

    #region Bucket-APIs

    
    

    private async Task<ListVersionsResult> BucketReadVersions(S3Context ctx)
    {
        this._logger.LogInformation("BucketReadVersions: " + ctx.Request.Bucket);

        List<ObjectVersion> versions = new List<ObjectVersion>()
        {
            new ObjectVersion("version1.key", "1", true, DateTime.UtcNow, "098f6bcd4621d373cade4e832627b4f6", 500, Owner)
        };

        List<DeleteMarker> deleteMarkers = new List<DeleteMarker>()
        {
            new DeleteMarker("deleted1.key", "2", true, DateTime.UtcNow, Owner)
        };

        List<VersionedEntity> entities = new List<VersionedEntity>();
        entities.AddRange(deleteMarkers);
        entities.AddRange(versions);

        ListVersionsResult lvr = new ListVersionsResult(
            "default", 
            versions, 
            deleteMarkers, 
            ctx.Request.MaxKeys,
            ctx.Request.Prefix,
            ctx.Request.Marker,
            null,
            false,
            "us-west-1");

        return lvr;
    }

    private async Task BucketWriteVersioning(S3Context ctx, VersioningConfiguration ver)
    {
        this._logger.LogInformation("BucketWriteVersioning: " + ctx.Request.Bucket);
        this._logger.LogInformation(ctx.Request.DataAsString + Environment.NewLine);
    }

    private async Task BucketWrite(S3Context ctx)
    {
        this._logger.LogInformation("BucketWrite: " + ctx.Request.Bucket);
    }

    private async Task BucketWriteAcl(S3Context ctx, AccessControlPolicy acp)
    {
        this._logger.LogInformation("BucketWriteAcl: " + ctx.Request.Bucket);
        this._logger.LogInformation(ctx.Request.DataAsString + Environment.NewLine);
    }

    private async Task BucketWriteTags(S3Context ctx, Tagging tags)
    {
        this._logger.LogInformation("BucketWriteTags: " + ctx.Request.Bucket);
        this._logger.LogInformation(ctx.Request.DataAsString + Environment.NewLine);
    }

    private async Task<ListMultipartUploadsResult> BucketReadMultipartUploads(S3Context ctx)
    {
        this._logger.LogInformation("BucketReadMultipartUploads: " + ctx.Request.Bucket);

        List<Upload> uploads = new List<Upload>()
        {
            new Upload
            {
                Key = "test-key.txt",
                UploadId = "upload-id-123",
                Owner = Owner,
                Initiator = Owner,
                Initiated = DateTime.UtcNow,
                StorageClass = StorageClassEnum.Standard
            }
        };

        ListMultipartUploadsResult result = new ListMultipartUploadsResult();
        result.Bucket = ctx.Request.Bucket;
        result.Uploads = uploads;
        result.Prefix = ctx.Request.Prefix;
        result.Delimiter = ctx.Request.Delimiter;
        result.KeyMarker = ctx.Request.Marker;
        result.MaxUploads = ctx.Request.MaxKeys;
        result.IsTruncated = false;

        return result;
    }

    #endregion

    #region Object-APIs

    private async Task ObjectDelete(S3Context ctx)
    {
        this._logger.LogInformation("ObjectDelete: " + ctx.Request.Bucket + "/" + ctx.Request.Key);
    }

    private async Task ObjectDeleteAcl(S3Context ctx)
    {
        this._logger.LogInformation("ObjectDeleteAcl: " + ctx.Request.Bucket + "/" + ctx.Request.Key);
    }

    private async Task<DeleteResult> ObjectDeleteMultiple(S3Context ctx, DeleteMultiple del)
    {
        this._logger.LogInformation("ObjectDelete: " + ctx.Request.Bucket);
        this._logger.LogInformation(ctx.Request.DataAsString + Environment.NewLine);

        DeleteResult result = new DeleteResult(
            new List<Deleted>()
            {
                new Deleted("hello.txt", "1", false)
            },
            null);

        return result;
    }

    private async Task ObjectDeleteTags(S3Context ctx)
    {
        this._logger.LogInformation("ObjectDeleteTags: " + ctx.Request.Bucket + "/" + ctx.Request.Key);
    }

    private async Task<ObjectMetadata> ObjectExists(S3Context ctx)
    {
        this._logger.LogInformation("ObjectExists: " + ctx.Request.Bucket + "/" + ctx.Request.Key);

        if (ctx.Request.Key == "archived-object.txt"
            || ctx.Request.Key == "archived-in-progress.txt"
            || ctx.Request.Key == "archived-restored.txt")
        {
            ObjectMetadata archived = new ObjectMetadata(
                ctx.Request.Key,
                DateTime.UtcNow,
                "6cd3556deb0da54bca060b4c39479839",
                13,
                Owner,
                "GLACIER");

            archived.ContentType = "text/plain";
            archived.RestoreStatus = GetRestoreStatus(ctx.Request.Key);
            return archived;
        }

        if (!RandomizeHeadResponses) return ObjectMetadata;

        int val = Random.Next(100);
        if (val % 2 == 0) return ObjectMetadata;
        else return null;
    }

    private async Task<S3Object> ObjectRead(S3Context ctx)
    {
        this._logger.LogInformation("ObjectRead: " + ctx.Request.Bucket + "/" + ctx.Request.Key);

        if (ctx.Request.Key == "archived-object.txt"
            || ctx.Request.Key == "archived-in-progress.txt"
            || ctx.Request.Key == "archived-restored.txt")
        {
            RestoreStatus status = GetRestoreStatus(ctx.Request.Key);
            if (status == null || status.OngoingRequest)
                throw new S3Exception(new Error(ErrorCode.InvalidObjectState));

            S3Object archived = new S3Object(ctx.Request.Key, "1", true, DateTime.UtcNow, "6cd3556deb0da54bca060b4c39479839", 5, Owner, "hello", "text/plain", StorageClassEnum.Glacier);
            archived.RestoreStatus = status;
            return archived;
        }

        return new S3Object("hello.txt", "1", true, DateTime.Now, "6cd3556deb0da54bca060b4c39479839", 13, new Owner("admin", "Administrator"), "Hello, world!", "text/plain");
    }

    private async Task<AccessControlPolicy> ObjectReadAcl(S3Context ctx)
    {
        this._logger.LogInformation("ObjectReadAcl: " + ctx.Request.Bucket + "/" + ctx.Request.Key);

        AccessControlList acl = new AccessControlList(
            new List<Grant>()
            {
                new Grant(Grantee, PermissionEnum.FullControl)
            });

        AccessControlPolicy policy = new AccessControlPolicy(
            Owner,
            acl);

        return policy;
    }

    private async Task<LegalHold> ObjectReadLegalHold(S3Context ctx)
    {
        this._logger.LogInformation("ObjectReadLegalHold: " + ctx.Request.Bucket + "/" + ctx.Request.Key);

        LegalHold legalHold = new LegalHold("OFF");

        return legalHold;
    }

    private async Task<S3Object> ObjectReadRange(S3Context ctx)
    {
        this._logger.LogInformation("ObjectReadRange: " + ctx.Request.Bucket + "/" + ctx.Request.Key);

        S3Object s3Obj = await ObjectRead(ctx).ConfigureAwait(false);

        string data = s3Obj.DataString;
        data = data.Substring((int)ctx.Request.RangeStart, (int)((int)ctx.Request.RangeEnd - (int)ctx.Request.RangeStart));
        int len = data.Length;
        s3Obj.DataString = data;
        s3Obj.Size = len;
        return s3Obj;
    }

    private async Task<Retention> ObjectReadRetention(S3Context ctx)
    {
        this._logger.LogInformation("ObjectReadRetention: " + ctx.Request.Bucket + "/" + ctx.Request.Key);

        Retention ret = new Retention(RetentionModeEnum.Governance, DateTime.Now.AddDays(100));
            
        return ret;
    }

    private async Task<Tagging> ObjectReadTags(S3Context ctx)
    {
        this._logger.LogInformation("ObjectReadTags: " + ctx.Request.Bucket + "/" + ctx.Request.Key);

        Tagging tagging = new Tagging(new TagSet(new List<Tag> { Tag }));

        return tagging;
    }

    private async Task<RestoreObjectResult> ObjectRestore(S3Context ctx, RestoreRequest request)
    {
        this._logger.LogInformation("ObjectRestore: " + ctx.Request.Bucket + "/" + ctx.Request.Key);
        this._logger.LogInformation(ctx.Request.DataAsString + Environment.NewLine);

        if (ctx.Request.Key == "active-tier-object.txt")
            throw new S3Exception(new Error(ErrorCode.ObjectAlreadyInActiveTierError));

        if (ctx.Request.Key == "archived-in-progress.txt")
            throw new S3Exception(new Error(ErrorCode.RestoreAlreadyInProgress));

        if (ctx.Request.Key == "archived-restored.txt")
        {
            RestoreStatuses[ctx.Request.Key] = new RestoreStatus
            {
                OngoingRequest = false,
                ExpiryDate = DateTime.UtcNow.AddDays(request.Days.Value)
            };

            return new RestoreObjectResult
            {
                AlreadyRestored = true
            };
        }

        RestoreStatuses[ctx.Request.Key] = new RestoreStatus
        {
            OngoingRequest = true
        };

        return new RestoreObjectResult
        {
            AlreadyRestored = false
        };
    }

    private async Task ObjectWrite(S3Context ctx)
    {
        this._logger.LogInformation("ObjectWrite      : " + ctx.Request.Bucket + "/" + ctx.Request.Key);
        this._logger.LogInformation("Content type     : " + ctx.Request.ContentType);
        this._logger.LogInformation("Chunked transfer : " + ctx.Request.Chunked);

        if (ctx.Request.Chunked)
        {
            while (true)
            {
                IS3HttpRequestChunk chunk = ctx.Request.ReadChunk().Result;
                this._logger.LogInformation(SerializationHelper.SerializeJson(chunk, true));

                Console.Write(chunk.Length + ": ");

                if (chunk.Length > 0)
                {
                    this._logger.LogInformation(chunk.Length + "/" + chunk.IsFinal + ": " + Encoding.UTF8.GetString(chunk.Data));
                }
                if (chunk.IsFinal)
                {
                    this._logger.LogInformation("Final chunk encountered");
                    break;
                }                    
            }
        }
        else
        {
            this._logger.LogInformation(ctx.Request.ContentLength + ": " + ctx.Request.DataAsString);
        }
    }

    private async Task ObjectWriteAcl(S3Context ctx, AccessControlPolicy acp)
    {
        this._logger.LogInformation("ObjectWriteAcl: " + ctx.Request.Bucket + "/" + ctx.Request.Key);
        this._logger.LogInformation(ctx.Request.DataAsString + Environment.NewLine);
    }

    private async Task ObjectWriteLegalHold(S3Context ctx, LegalHold legalHold)
    {
        this._logger.LogInformation("ObjectWriteLegalHold: " + ctx.Request.Bucket + "/" + ctx.Request.Key);
        this._logger.LogInformation(ctx.Request.DataAsString + Environment.NewLine);
    }

    private async Task ObjectWriteRetention(S3Context ctx, Retention retention)
    {
        this._logger.LogInformation("ObjectWriteRetention: " + ctx.Request.Bucket + "/" + ctx.Request.Key);
        this._logger.LogInformation(ctx.Request.DataAsString + Environment.NewLine);
    }

    private async Task ObjectWriteTags(S3Context ctx, Tagging tags)
    {
        this._logger.LogInformation("ObjectWriteTags: " + ctx.Request.Bucket + "/" + ctx.Request.Key);
        this._logger.LogInformation(ctx.Request.DataAsString + Environment.NewLine);
    }

    private async Task<InitiateMultipartUploadResult> ObjectCreateMultipartUpload(S3Context ctx)
    {
        this._logger.LogInformation("ObjectCreateMultipartUpload: " + ctx.Request.Bucket + "/" + ctx.Request.Key);

        string uploadId = Guid.NewGuid().ToString();
        InitiateMultipartUploadResult result = new InitiateMultipartUploadResult(
            ctx.Request.Bucket,
            ctx.Request.Key,
            uploadId);

        return result;
    }

    private async Task ObjectUploadPart(S3Context ctx)
    {
        this._logger.LogInformation("ObjectUploadPart: " + ctx.Request.Bucket + "/" + ctx.Request.Key);
        this._logger.LogInformation("  Upload ID   : " + ctx.Request.UploadId);
        this._logger.LogInformation("  Part Number : " + ctx.Request.PartNumber);
        this._logger.LogInformation("  Content Length: " + ctx.Request.ContentLength);
    }

    private async Task<ListPartsResult> ObjectReadParts(S3Context ctx)
    {
        this._logger.LogInformation("ObjectReadParts: " + ctx.Request.Bucket + "/" + ctx.Request.Key);
        this._logger.LogInformation("  Upload ID: " + ctx.Request.UploadId);

        List<Part> parts = new List<Part>()
        {
            new Part
            {
                PartNumber = 1,
                LastModified = DateTime.UtcNow,
                ETag = "5d41402abc4b2a76b9719d911017c592",
                Size = 1024
            },
            new Part
            {
                PartNumber = 2,
                LastModified = DateTime.UtcNow,
                ETag = "7d793037a0760186574b0282f2f435e7",
                Size = 2048
            }
        };

        ListPartsResult result = new ListPartsResult();
        result.Bucket = ctx.Request.Bucket;
        result.Key = ctx.Request.Key;
        result.UploadId = ctx.Request.UploadId;
        result.Owner = Owner;
        result.Initiator = Owner;
        result.StorageClass = StorageClassEnum.Standard;
        result.PartNumberMarker = 1;
        result.MaxParts = 1000;
        result.IsTruncated = false;
        result.Parts = parts;

        return result;
    }

    private async Task<CompleteMultipartUploadResult> ObjectCompleteMultipartUpload(S3Context ctx, CompleteMultipartUpload complete)
    {
        this._logger.LogInformation("ObjectCompleteMultipartUpload: " + ctx.Request.Bucket + "/" + ctx.Request.Key);
        this._logger.LogInformation("  Upload ID: " + ctx.Request.UploadId);
        this._logger.LogInformation("  Parts: " + complete.Parts.Count);

        CompleteMultipartUploadResult result = new CompleteMultipartUploadResult();
        result.Location = "http://localhost:8000/" + ctx.Request.Bucket + "/" + ctx.Request.Key;
        result.Bucket = ctx.Request.Bucket;
        result.Key = ctx.Request.Key;
        result.ETag = "9b2c3e7a8d1f4e6b5c2a1d8f7e4b3c2a";

        return result;
    }

    private async Task ObjectAbortMultipartUpload(S3Context ctx)
    {
        this._logger.LogInformation("ObjectAbortMultipartUpload: " + ctx.Request.Bucket + "/" + ctx.Request.Key);
        this._logger.LogInformation("  Upload ID: " + ctx.Request.UploadId);
    }

    private async Task ObjectSelectContent(S3Context ctx, SelectObjectContentRequest request)
    {
        this._logger.LogInformation("ObjectSelectContent: " + ctx.Request.Bucket + "/" + ctx.Request.Key);
        this._logger.LogInformation("  Expression: " + request.Expression);
        this._logger.LogInformation("  Expression Type: " + request.ExpressionType);
    }

    #endregion

    #endregion

    #region Misc

    private RestoreStatus GetRestoreStatus(string key)
    {
        if (RestoreStatuses.TryGetValue(key, out RestoreStatus status))
            return status;

        if (key == "archived-in-progress.txt")
        {
            return new RestoreStatus
            {
                OngoingRequest = true
            };
        }

        if (key == "archived-restored.txt")
        {
            return new RestoreStatus
            {
                OngoingRequest = false,
                ExpiryDate = DateTime.UtcNow.AddDays(2)
            };
        }

        return null;
    }

    #endregion

    public void Dispose()
    {
        this._client?.Dispose();
        this._server?.Dispose();
    }
}