namespace S3CryptProxy.Handler;

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Threading.Tasks;
using AWSSignatureGenerator;
using Microsoft.Extensions.Logging;
using S3CryptProxy.Handler.Callbacks;
using S3CryptProxy.Handler.Interfaces;
using S3CryptProxy.Models;
using S3CryptProxy.Shared;

/// <summary>
/// S3 server.  
/// Bucket names must not be in the hostname; they must be in the URL path. 
/// </summary>
public class S3Server : IDisposable
{
    #region Public-Members

    /// <summary>
    /// Determine if the server is listening.
    /// </summary>
    public bool IsListening
    {
        get
        {
            return this._webserver.IsListening;
        }
    }

    /// <summary>
    /// Settings.
    /// </summary>
    public S3ServerSettings Settings
    {
        get
        {
            return this._settings;
        }
    }

    /// <summary>
    /// Access the underlying webserver.
    /// </summary>
    public IS3Server Webserver
    {
        get
        {
            return this._webserver;
        }
    }

    #endregion

    #region Private-Members

    private string _header = "[S3Server] ";
    private bool _disposed = false;

    private IS3Server _webserver;
    private S3ServerSettings _settings;
    private ILogger<S3Server> _logger;

    #endregion

    #region Constructors-and-Factories

    /// <summary>
    /// Instantiate.
    /// </summary>
    /// <param name="settings">Settings.</param>
    public S3Server(S3ServerSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(settings.Webserver, nameof(settings.Webserver));
        ArgumentNullException.ThrowIfNull(settings.LoggerFactory, nameof(settings.LoggerFactory));
        ArgumentNullException.ThrowIfNull(settings.ServiceCallbacks, nameof(settings.ServiceCallbacks));
        ArgumentNullException.ThrowIfNull(settings.BucketCallbacks, nameof(settings.BucketCallbacks));
        ArgumentNullException.ThrowIfNull(settings.ObjectCallbacks, nameof(settings.ObjectCallbacks));
        
        this._logger = settings.LoggerFactory.CreateLogger<S3Server>();

        this._settings = settings;

        if (this._settings.Webserver.DefaultHeaders != null && this._settings.Webserver.DefaultHeaders != null)
        {
            Dictionary<string, string> updatedHeaders = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

            foreach (KeyValuePair<string, string> header in this._settings.Webserver.DefaultHeaders)
            {
                string key = header.Key;
                if (string.IsNullOrEmpty(key)) continue;
                if (key.ToLower().Equals("accept-charset"))
                {
                    updatedHeaders.Add("Accept-Charset", "utf8"); // Minio support
                }
                else
                {
                    updatedHeaders.Add(header.Key, header.Value);
                }
            }

            this._settings.Webserver.DefaultHeaders = updatedHeaders;
        }

        this._webserver = this._settings.Webserver.CreateServer(this.RequestHandler);
    }

    #endregion

    #region Public-Methods

    /// <summary>
    /// Tear down the client and dispose of background workers.
    /// Do not use the object after disposal.
    /// </summary>
    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Start accepting new connections.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if EnableSignatures is true but Service.GetSecretKey is not set.</exception>
    public void Start()
    {
        this._webserver.Start();
    }

    /// <summary>
    /// Stop accepting new connections.
    /// </summary>
    public void Stop()
    {
        this._webserver.Stop();
    }

    #endregion

    #region Private-Methods

    /// <summary>
    /// Dispose of the server.
    /// </summary>
    /// <param name="disposing">Disposing.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (this._disposed)
        {
            return;
        }

        if (disposing)
        {
            this._logger?.LogInformation(this._header + "dispose requested");

            if (this._webserver != null) this._webserver.Dispose();
        }

        this._webserver = null;
        this._disposed = true;
    }

    private async Task RequestHandler(IS3HttpContext ctx)
    {
        if (ctx == null) throw new ArgumentNullException(nameof(ctx));

        bool success = false;
        bool exists = false;
        S3Object s3Obj = null;
        ObjectMetadata md = null;
        AccessControlPolicy acp = null;
        LegalHold legalHold = null;
        Retention retention = null;
        Tagging tagging = null;
        ListAllMyBucketsResult buckets = null;
        ListBucketResult listBucketResult = null;
        ListVersionsResult listVersionResult = null;
        LocationConstraint location = null;
        BucketLoggingStatus bucketLogging = null;
        VersioningConfiguration versionConfig = null;
        WebsiteConfiguration wc = null;
        DeleteMultiple delMultiple = null;
        DeleteResult delResult = null;
        Error error = null;
        InitiateMultipartUploadResult initiateMultipart = null;
        ListMultipartUploadsResult listMultipartUploads = null;
        ListPartsResult listParts = null;
        CompleteMultipartUpload completeMultipartRequest = null;
        CompleteMultipartUploadResult completeMultipartResult = null;
        SelectObjectContentRequest selectRequest = null;
        RestoreRequest restoreRequest = null;
        RestoreObjectResult restoreResult = null;

        S3Context s3Ctx = null;

        using (s3Ctx = new S3Context(ctx, this._settings.LoggerFactory))
        {
            try
            {
                s3Ctx.Response.Headers.Add(Constants.HeaderRequestId, s3Ctx.Request.RequestId);
                s3Ctx.Response.Headers.Add(Constants.HeaderTraceId, s3Ctx.Request.TraceId);
                s3Ctx.Response.Headers.Add(Constants.HeaderConnection, "close");

                if (this._logger.IsEnabled(LogLevel.Trace))
                {
                    this._logger.LogTrace(this._header + "HTTP request: " + Environment.NewLine + SerializationHelper.SerializeJson(s3Ctx.Http, true));
                    this._logger.LogTrace(this._header + "S3 request: " + Environment.NewLine + SerializationHelper.SerializeJson(s3Ctx.Request, true));
                }
                
                if (this._settings.PreRequestHandler != null)
                {
                    success = await this._settings.PreRequestHandler(s3Ctx).ConfigureAwait(false);
                    if (success)
                    {
                        await s3Ctx.Response.Send().ConfigureAwait(false);
                        return;
                    }
                }

                if (true)
                {
                    if (true)
                    {
                        string secretKey = await this.Settings.ServiceCallbacks.GetSecretKey(s3Ctx);
                        if (string.IsNullOrEmpty(secretKey))
                        {
                            this._logger?.LogInformation(this._header + "unable to retrieve secret key for signature " + s3Ctx.Request.Signature);
                            throw new S3Exception(new Error(ErrorCode.AccessDenied));
                        }

                        if (s3Ctx.Request.SignatureVersion == S3SignatureVersion.Version2)
                        {
                            this._logger?.LogInformation(this._header + "invalid v2 signature '" + s3Ctx.Request.Signature + "'");
                            throw new S3Exception(new Error(ErrorCode.SignatureDoesNotMatch));
                        }
                        else if (s3Ctx.Request.SignatureVersion == S3SignatureVersion.Version4)
                        {
                            string contentSha256 = null;
                            if (s3Ctx.Http.Request.Headers != null)
                                contentSha256 = s3Ctx.Http.Request.Headers["x-amz-content-sha256"];
                            V4PayloadHashEnum payloadHashMode = V4PayloadHashEnum.Signed;

                            if (!string.IsNullOrEmpty(contentSha256))
                            {
                                if (contentSha256.Equals("STREAMING-AWS4-HMAC-SHA256-PAYLOAD-TRAILER", StringComparison.Ordinal))
                                    payloadHashMode = V4PayloadHashEnum.StreamingSignedTrailer;
                                else if (contentSha256.Equals("STREAMING-AWS4-HMAC-SHA256-PAYLOAD", StringComparison.Ordinal))
                                    payloadHashMode = V4PayloadHashEnum.StreamingSigned;
                                else if (contentSha256.Equals("UNSIGNED-PAYLOAD", StringComparison.Ordinal))
                                    payloadHashMode = V4PayloadHashEnum.Unsigned;
                            }

                            string timestamp = null;
                            if (s3Ctx.Http.Request.Headers != null)
                                timestamp = s3Ctx.Http.Request.Headers["x-amz-date"];
                            if (string.IsNullOrEmpty(timestamp))
                                timestamp = DateTime.UtcNow.ToString(Constants.AmazonTimestampFormatCompact);

                            object requestBody = null;
                            if (payloadHashMode == V4PayloadHashEnum.Signed)
                                requestBody = s3Ctx.Http.Request.DataAsBytes;

                            string sigFullUrl = s3Ctx.Http.Request.UrlFull;
                            if (!string.IsNullOrEmpty(sigFullUrl) && !Uri.TryCreate(sigFullUrl, UriKind.Absolute, out _))
                            {
                                string hostHeader = s3Ctx.Request.Host;
                                if (!string.IsNullOrEmpty(hostHeader))
                                {
                                    string hostValue = hostHeader.Contains(":") ? hostHeader.Split(':')[0] : hostHeader;
                                    sigFullUrl = S3Request.ReplaceWildcardHostname(sigFullUrl, hostValue);
                                }
                            }

                            V4SignatureResult result = new V4SignatureResult(
                                timestamp,
                                s3Ctx.Http.Request.Method.ToString().ToUpper(),
                                sigFullUrl,
                                s3Ctx.Request.AccessKey,
                                secretKey,
                                s3Ctx.Request.Region,
                                "s3",
                                s3Ctx.Http.Request.Headers,
                                s3Ctx.Request.SignedHeaders,
                                requestBody,
                                payloadHashMode);

                            if (this._logger != null)
                            {
                                this._logger?.LogDebug(this._header + Environment.NewLine + result);
                                this._logger?.LogDebug(this._header + "signature validation:"
                                                                    + " provided=" + s3Ctx.Request.Signature
                                                                    + " expected=" + result.Signature
                                                                    + " match=" + result.Signature.Equals(s3Ctx.Request.Signature));
                            }

                            if (!result.Signature.Equals(s3Ctx.Request.Signature))
                            {
                                this._logger?.LogWarning(this._header + "invalid v4 signature '" + s3Ctx.Request.Signature + "'");
                                throw new S3Exception(new Error(ErrorCode.SignatureDoesNotMatch));
                            }
                        }
                        else
                        {
                            this._logger?.LogWarning(this._header + "unknown signature version");
                            throw new S3Exception(new Error(ErrorCode.AccessDenied));
                        }
                    }
                }

                switch (s3Ctx.Request.RequestType)
                {
                    #region Service

                    case S3RequestType.ServiceExists:
                        string region = await this.Settings.ServiceCallbacks.ServiceExists(s3Ctx).ConfigureAwait(false);
                        if (!string.IsNullOrEmpty(region)) s3Ctx.Response.Headers.Add(Constants.HeaderBucketRegion, region);

                        s3Ctx.Response.StatusCode = 200;
                        await s3Ctx.Response.Send().ConfigureAwait(false);
                        return;
                    case S3RequestType.ListBuckets:
                        buckets = await this.Settings.ServiceCallbacks.ListBuckets(s3Ctx).ConfigureAwait(false);
                        s3Ctx.Response.StatusCode = 200;
                        s3Ctx.Response.ContentType = Constants.ContentTypeXml;
                        await s3Ctx.Response.Send(SerializationHelper.SerializeXml(buckets)).ConfigureAwait(false);
                        return;
                    #endregion

                    #region Bucket

                    case S3RequestType.BucketDelete:
                        if (this.Settings.BucketCallbacks.Delete != null)
                        {
                            await this.Settings.BucketCallbacks.Delete(s3Ctx).ConfigureAwait(false);
                            s3Ctx.Response.StatusCode = 204;
                            s3Ctx.Response.ContentType = Constants.ContentTypeText;
                            await s3Ctx.Response.Send().ConfigureAwait(false);
                            return;
                        }
                        break;

                    case S3RequestType.BucketDeleteAcl:
                        if (this.Settings.BucketCallbacks.DeleteAcl != null)
                        {
                            await this.Settings.BucketCallbacks.DeleteAcl(s3Ctx).ConfigureAwait(false);
                            s3Ctx.Response.StatusCode = 204;
                            s3Ctx.Response.ContentType = Constants.ContentTypeText;
                            await s3Ctx.Response.Send().ConfigureAwait(false);
                            return;
                        }
                        break;

                    case S3RequestType.BucketDeleteTags:
                        if (this.Settings.BucketCallbacks.DeleteTagging != null)
                        {
                            await this.Settings.BucketCallbacks.DeleteTagging(s3Ctx).ConfigureAwait(false);
                            s3Ctx.Response.StatusCode = 204;
                            s3Ctx.Response.ContentType = Constants.ContentTypeText;
                            await s3Ctx.Response.Send().ConfigureAwait(false);
                            return;
                        }
                        break;

                    case S3RequestType.BucketExists:
                        if (this.Settings.BucketCallbacks.Exists != null)
                        {
                            exists = await this.Settings.BucketCallbacks.Exists(s3Ctx).ConfigureAwait(false);
                            if (exists)
                            {
                                s3Ctx.Response.StatusCode = 200;
                                s3Ctx.Response.ContentType = Constants.ContentTypeText;
                                await s3Ctx.Response.Send().ConfigureAwait(false);
                            }
                            else
                            {
                                error = new Error(ErrorCode.NoSuchBucket);
                                s3Ctx.Response.StatusCode = 404;
                                s3Ctx.Response.ContentType = Constants.ContentTypeXml;
                                await s3Ctx.Response.Send(SerializationHelper.SerializeXml(error)).ConfigureAwait(false);
                            }
                            return;
                        }
                        break;

                    case S3RequestType.BucketRead:
                        if (this.Settings.BucketCallbacks.Read != null)
                        {
                            listBucketResult = await this.Settings.BucketCallbacks.Read(s3Ctx).ConfigureAwait(false);
                                
                            if (!string.IsNullOrEmpty(listBucketResult.BucketRegion))
                                s3Ctx.Response.Headers.Add("x-amz-bucket-region", listBucketResult.BucketRegion);

                            s3Ctx.Response.StatusCode = 200;
                            s3Ctx.Response.ContentType = Constants.ContentTypeXml;
                            await s3Ctx.Response.Send(SerializationHelper.SerializeXml(listBucketResult)).ConfigureAwait(false);
                            return;
                        }
                        break;

                    case S3RequestType.BucketReadAcl:
                        if (this.Settings.BucketCallbacks.ReadAcl != null)
                        {
                            acp = await this.Settings.BucketCallbacks.ReadAcl(s3Ctx).ConfigureAwait(false);
                            s3Ctx.Response.StatusCode = 200;
                            s3Ctx.Response.ContentType = Constants.ContentTypeXml;
                            await s3Ctx.Response.Send(SerializationHelper.SerializeXml(acp)).ConfigureAwait(false);
                            return;
                        }
                        break;

                    case S3RequestType.BucketReadLocation:
                        if (this.Settings.BucketCallbacks.ReadLocation != null)
                        {
                            location = await this.Settings.BucketCallbacks.ReadLocation(s3Ctx).ConfigureAwait(false);
                            s3Ctx.Response.StatusCode = 200;
                            s3Ctx.Response.ContentType = Constants.ContentTypeXml;
                            await s3Ctx.Response.Send(SerializationHelper.SerializeXml(location)).ConfigureAwait(false);
                            return;
                        }
                        break;

                    case S3RequestType.BucketReadMultipartUploads:
                        if (this.Settings.BucketCallbacks.ReadMultipartUploads != null)
                        {
                            listMultipartUploads = await this.Settings.BucketCallbacks.ReadMultipartUploads(s3Ctx).ConfigureAwait(false);
                            s3Ctx.Response.StatusCode = 200;
                            s3Ctx.Response.ContentType = Constants.ContentTypeXml;
                            await s3Ctx.Response.Send(SerializationHelper.SerializeXml(listMultipartUploads)).ConfigureAwait(false);
                            return;
                        }
                        break;

                    case S3RequestType.BucketReadTags:
                        if (this.Settings.BucketCallbacks.ReadTagging != null)
                        {
                            tagging = await this.Settings.BucketCallbacks.ReadTagging(s3Ctx).ConfigureAwait(false);
                            s3Ctx.Response.StatusCode = 200;
                            s3Ctx.Response.ContentType = Constants.ContentTypeXml;
                            await s3Ctx.Response.Send(SerializationHelper.SerializeXml(tagging)).ConfigureAwait(false);
                            return;
                        }
                        break;

                    case S3RequestType.BucketReadVersioning:
                        if (this.Settings.BucketCallbacks.ReadVersioning != null)
                        {
                            versionConfig = await this.Settings.BucketCallbacks.ReadVersioning(s3Ctx).ConfigureAwait(false);
                            s3Ctx.Response.StatusCode = 200;
                            s3Ctx.Response.ContentType = Constants.ContentTypeXml;
                            await s3Ctx.Response.Send(SerializationHelper.SerializeXml(versionConfig)).ConfigureAwait(false);
                            return;
                        }
                        break;

                    case S3RequestType.BucketReadVersions:
                        if (this.Settings.BucketCallbacks.ReadVersions != null)
                        {
                            listVersionResult = await this.Settings.BucketCallbacks.ReadVersions(s3Ctx).ConfigureAwait(false);
                            s3Ctx.Response.StatusCode = 200;
                            s3Ctx.Response.ContentType = Constants.ContentTypeXml;
                            await s3Ctx.Response.Send(SerializationHelper.SerializeXml(listVersionResult)).ConfigureAwait(false);
                            return;
                        }
                        break;

                    case S3RequestType.BucketWrite:
                        if (this.Settings.BucketCallbacks.Write != null)
                        {
                            await this.Settings.BucketCallbacks.Write(s3Ctx).ConfigureAwait(false);
                            s3Ctx.Response.StatusCode = 200;
                            s3Ctx.Response.ContentType = Constants.ContentTypeText;
                            await s3Ctx.Response.Send().ConfigureAwait(false);
                            return;
                        }
                        break;

                    case S3RequestType.BucketWriteAcl:
                        if (this.Settings.BucketCallbacks.WriteAcl != null)
                        {
                            try
                            {
                                acp = SerializationHelper.DeserializeXml<AccessControlPolicy>(s3Ctx.Request.DataAsString);
                            }
                            catch (InvalidOperationException ioe)
                            {
                                ioe.Data.Add("Context", s3Ctx);
                                ioe.Data.Add("RequestBody", s3Ctx.Request.DataAsString);
                                this._logger?.LogInformation(this._header + "XML exception: " + Environment.NewLine + ioe.ToString());
                                await s3Ctx.Response.Send(ErrorCode.MalformedXml).ConfigureAwait(false);
                                return;
                            }

                            await this.Settings.BucketCallbacks.WriteAcl(s3Ctx, acp).ConfigureAwait(false);
                            s3Ctx.Response.StatusCode = 200;
                            s3Ctx.Response.ContentType = Constants.ContentTypeText;
                            await s3Ctx.Response.Send().ConfigureAwait(false);
                            return;
                        }
                        break;

                    case S3RequestType.BucketWriteTags:
                        if (this.Settings.BucketCallbacks.WriteTagging != null)
                        {
                            try
                            {
                                tagging = SerializationHelper.DeserializeXml<Tagging>(s3Ctx.Request.DataAsString);
                            }
                            catch (InvalidOperationException ioe)
                            {
                                ioe.Data.Add("Context", s3Ctx);
                                ioe.Data.Add("RequestBody", s3Ctx.Request.DataAsString);
                                this._logger?.LogInformation(this._header + "XML exception: " + Environment.NewLine + ioe.ToString());
                                await s3Ctx.Response.Send(ErrorCode.MalformedXml).ConfigureAwait(false);
                                return;
                            }

                            await this.Settings.BucketCallbacks.WriteTagging(s3Ctx, tagging).ConfigureAwait(false);
                            s3Ctx.Response.StatusCode = 200;
                            s3Ctx.Response.ContentType = Constants.ContentTypeText;
                            await s3Ctx.Response.Send().ConfigureAwait(false);
                            return;
                        }
                        break;

                    case S3RequestType.BucketWriteVersioning:
                        if (this.Settings.BucketCallbacks.WriteVersioning != null)
                        {
                            try
                            {
                                versionConfig = SerializationHelper.DeserializeXml<VersioningConfiguration>(s3Ctx.Request.DataAsString);
                            }
                            catch (InvalidOperationException ioe)
                            {
                                ioe.Data.Add("Context", s3Ctx);
                                ioe.Data.Add("RequestBody", s3Ctx.Request.DataAsString);
                                this._logger?.LogInformation(this._header + "XML exception: " + Environment.NewLine + ioe.ToString());
                                await s3Ctx.Response.Send(ErrorCode.MalformedXml).ConfigureAwait(false);
                                return;
                            }

                            await this.Settings.BucketCallbacks.WriteVersioning(s3Ctx, versionConfig).ConfigureAwait(false);
                            s3Ctx.Response.StatusCode = 200;
                            s3Ctx.Response.ContentType = Constants.ContentTypeText;
                            await s3Ctx.Response.Send().ConfigureAwait(false);
                            return;
                        }
                        break;

                    #endregion

                    #region Object

                    case S3RequestType.ObjectAbortMultipartUpload:
                        if (this.Settings.ObjectCallbacks.AbortMultipartUpload != null)
                        {
                            await this.Settings.ObjectCallbacks.AbortMultipartUpload(s3Ctx).ConfigureAwait(false);
                            s3Ctx.Response.StatusCode = 204;
                            s3Ctx.Response.ContentType = Constants.ContentTypeText;
                            await s3Ctx.Response.Send().ConfigureAwait(false);
                            return;
                        }
                        break;

                    case S3RequestType.ObjectCompleteMultipartUpload:
                        if (this.Settings.ObjectCallbacks.CompleteMultipartUpload != null)
                        {
                            try
                            {
                                completeMultipartRequest = SerializationHelper.DeserializeXml<CompleteMultipartUpload>(s3Ctx.Request.DataAsString);
                            }
                            catch (InvalidOperationException ioe)
                            {
                                ioe.Data.Add("Context", s3Ctx);
                                ioe.Data.Add("RequestBody", s3Ctx.Request.DataAsString);
                                this._logger?.LogInformation(this._header + "XML exception: " + Environment.NewLine + ioe.ToString());
                                await s3Ctx.Response.Send(ErrorCode.MalformedXml).ConfigureAwait(false);
                                return;
                            }

                            completeMultipartResult = await this.Settings.ObjectCallbacks.CompleteMultipartUpload(s3Ctx, completeMultipartRequest).ConfigureAwait(false);
                            s3Ctx.Response.StatusCode = 200;
                            s3Ctx.Response.ContentType = Constants.ContentTypeXml;
                            await s3Ctx.Response.Send(SerializationHelper.SerializeXml(completeMultipartResult)).ConfigureAwait(false);
                            return;
                        }
                        break;

                    case S3RequestType.ObjectCreateMultipartUpload:
                        if (this.Settings.ObjectCallbacks.CreateMultipartUpload != null)
                        {
                            initiateMultipart = await this.Settings.ObjectCallbacks.CreateMultipartUpload(s3Ctx).ConfigureAwait(false);
                            s3Ctx.Response.StatusCode = 200;
                            s3Ctx.Response.ContentType = Constants.ContentTypeXml;
                            await s3Ctx.Response.Send(SerializationHelper.SerializeXml(initiateMultipart)).ConfigureAwait(false);
                            return;
                        }
                        break;

                    case S3RequestType.ObjectDelete:
                        if (this.Settings.ObjectCallbacks.Delete != null)
                        {
                            await this.Settings.ObjectCallbacks.Delete(s3Ctx).ConfigureAwait(false);
                            s3Ctx.Response.StatusCode = 204;
                            s3Ctx.Response.ContentType = Constants.ContentTypeText;
                            await s3Ctx.Response.Send().ConfigureAwait(false);
                            return;
                        }
                        break;

                    case S3RequestType.ObjectDeleteAcl:
                        if (this.Settings.ObjectCallbacks.DeleteAcl != null)
                        {
                            await this.Settings.ObjectCallbacks.DeleteAcl(s3Ctx).ConfigureAwait(false);
                            s3Ctx.Response.StatusCode = 204;
                            s3Ctx.Response.ContentType = Constants.ContentTypeText;
                            await s3Ctx.Response.Send().ConfigureAwait(false);
                            return;
                        }
                        break;

                    case S3RequestType.ObjectDeleteMultiple:
                        if (this.Settings.ObjectCallbacks.DeleteMultiple != null)
                        {
                            try
                            {
                                delMultiple = SerializationHelper.DeserializeXml<DeleteMultiple>(s3Ctx.Request.DataAsString);
                            }
                            catch (InvalidOperationException ioe)
                            {
                                ioe.Data.Add("Context", s3Ctx);
                                ioe.Data.Add("RequestBody", s3Ctx.Request.DataAsString);
                                this._logger?.LogInformation(this._header + "XML exception: " + Environment.NewLine + ioe.ToString());
                                await s3Ctx.Response.Send(ErrorCode.MalformedXml).ConfigureAwait(false);
                                return;
                            }

                            delResult = await this.Settings.ObjectCallbacks.DeleteMultiple(s3Ctx, delMultiple).ConfigureAwait(false);
                            s3Ctx.Response.StatusCode = 200;
                            s3Ctx.Response.ContentType = Constants.ContentTypeXml;
                            await s3Ctx.Response.Send(SerializationHelper.SerializeXml(delResult)).ConfigureAwait(false);
                            return;
                        }
                        break;

                    case S3RequestType.ObjectDeleteTags:
                        if (this.Settings.ObjectCallbacks.DeleteTagging != null)
                        {
                            await this.Settings.ObjectCallbacks.DeleteTagging(s3Ctx).ConfigureAwait(false);
                            s3Ctx.Response.StatusCode = 204;
                            s3Ctx.Response.ContentType = Constants.ContentTypeText;
                            await s3Ctx.Response.Send().ConfigureAwait(false);
                            return;
                        }
                        break;

                    case S3RequestType.ObjectExists:
                        if (this.Settings.ObjectCallbacks.Exists != null)
                        {
                            md = await this.Settings.ObjectCallbacks.Exists(s3Ctx).ConfigureAwait(false);
                            if (md != null)
                            {
                                if (!string.IsNullOrEmpty(md.ETag)) s3Ctx.Response.Headers.Add(Constants.HeaderETag, md.ETag);

                                s3Ctx.Response.Headers.Add(Constants.HeaderLastModified, md.LastModified.ToString(Constants.AmazonTimestampFormatVerbose, CultureInfo.InvariantCulture));
                                s3Ctx.Response.Headers.Add(Constants.HeaderStorageClass, md.StorageClass.ToString());
                                s3Ctx.Response.Headers.Add(Constants.HeaderAcceptRanges, "bytes");
                                AddRestoreHeader(s3Ctx.Response.Headers, md.RestoreStatus);

                                s3Ctx.Response.StatusCode = 200;
                                s3Ctx.Response.ContentLength = md.Size;
                                s3Ctx.Response.ContentType = md.ContentType;
                                await s3Ctx.Response.Send().ConfigureAwait(false);
                            }
                            else
                            {
                                error = new Error(ErrorCode.NoSuchKey);
                                s3Ctx.Response.StatusCode = 404;
                                s3Ctx.Response.ContentType = Constants.ContentTypeXml;
                                await s3Ctx.Response.Send(SerializationHelper.SerializeXml(error)).ConfigureAwait(false);
                            }
                            return;
                        }
                        break;

                    case S3RequestType.ObjectRead:
                        if (this.Settings.ObjectCallbacks.Read != null)
                        {
                            s3Obj = await this.Settings.ObjectCallbacks.Read(s3Ctx).ConfigureAwait(false);

                            if (s3Obj != null)
                            {
                                if (!string.IsNullOrEmpty(s3Obj.ETag)) s3Ctx.Response.Headers.Add(Constants.HeaderETag, s3Obj.ETag);

                                s3Ctx.Response.Headers.Add(Constants.HeaderLastModified, s3Obj.LastModified.ToString(Constants.AmazonTimestampFormatVerbose, CultureInfo.InvariantCulture));
                                s3Ctx.Response.Headers.Add(Constants.HeaderStorageClass, s3Obj.StorageClass.ToString());
                                s3Ctx.Response.Headers.Add(Constants.HeaderAcceptRanges, "bytes");
                                AddRestoreHeader(s3Ctx.Response.Headers, s3Obj.RestoreStatus);

                                s3Ctx.Response.StatusCode = 200;
                                s3Ctx.Response.ContentType = s3Obj.ContentType;
                                s3Ctx.Response.ContentLength = s3Obj.Size;

                                await s3Ctx.Response.Send(s3Obj.Size, s3Obj.Data).ConfigureAwait(false);
                            }
                            else
                            {
                                error = new Error(ErrorCode.NoSuchKey);
                                s3Ctx.Response.StatusCode = 404;
                                s3Ctx.Response.ContentType = Constants.ContentTypeXml;
                                await s3Ctx.Response.Send(SerializationHelper.SerializeXml(error)).ConfigureAwait(false);
                            }
                            return;
                        }
                        break;

                    case S3RequestType.ObjectReadAcl:
                        if (this.Settings.ObjectCallbacks.ReadAcl != null)
                        {
                            acp = await this.Settings.ObjectCallbacks.ReadAcl(s3Ctx).ConfigureAwait(false);
                            s3Ctx.Response.StatusCode = 200;
                            s3Ctx.Response.ContentType = Constants.ContentTypeXml;
                            await s3Ctx.Response.Send(SerializationHelper.SerializeXml(acp)).ConfigureAwait(false);
                            return;
                        }
                        break;

                    case S3RequestType.ObjectReadLegalHold:
                        if (this.Settings.ObjectCallbacks.ReadLegalHold != null)
                        {
                            legalHold = await this.Settings.ObjectCallbacks.ReadLegalHold(s3Ctx).ConfigureAwait(false);
                            s3Ctx.Response.StatusCode = 200;
                            s3Ctx.Response.ContentType = Constants.ContentTypeXml;
                            await s3Ctx.Response.Send(SerializationHelper.SerializeXml(legalHold)).ConfigureAwait(false);
                            return;
                        }
                        break;

                    case S3RequestType.ObjectReadParts:
                        if (this.Settings.ObjectCallbacks.ReadParts != null)
                        {
                            listParts = await this.Settings.ObjectCallbacks.ReadParts(s3Ctx).ConfigureAwait(false);
                            s3Ctx.Response.StatusCode = 200;
                            s3Ctx.Response.ContentType = Constants.ContentTypeXml;
                            await s3Ctx.Response.Send(SerializationHelper.SerializeXml(listParts)).ConfigureAwait(false);
                            return;
                        }
                        break;

                    case S3RequestType.ObjectReadRange:
                        if (this.Settings.ObjectCallbacks.ReadRange != null)
                        {
                            s3Obj = await this.Settings.ObjectCallbacks.ReadRange(s3Ctx).ConfigureAwait(false);

                            if (s3Obj != null)
                            {
                                if (!string.IsNullOrEmpty(s3Obj.ETag)) s3Ctx.Response.Headers.Add(Constants.HeaderETag, s3Obj.ETag);

                                s3Ctx.Response.Headers.Add(Constants.HeaderLastModified, s3Obj.LastModified.ToString(Constants.AmazonTimestampFormatVerbose, CultureInfo.InvariantCulture));
                                s3Ctx.Response.Headers.Add(Constants.HeaderStorageClass, s3Obj.StorageClass.ToString());
                                s3Ctx.Response.Headers.Add(Constants.HeaderAcceptRanges, "bytes");
                                AddRestoreHeader(s3Ctx.Response.Headers, s3Obj.RestoreStatus);

                                if (s3Ctx.Request.RangeStart != null)
                                {
                                    long rangeEnd = s3Ctx.Request.RangeEnd ?? (s3Ctx.Request.RangeStart.Value + s3Obj.Size - 1);
                                    s3Ctx.Response.Headers.Add("Content-Range", "bytes " + s3Ctx.Request.RangeStart.Value + "-" + rangeEnd + "/*");
                                }

                                s3Ctx.Response.StatusCode = 206;
                                s3Ctx.Response.ContentType = s3Obj.ContentType;
                                s3Ctx.Response.ContentLength = s3Obj.Size;

                                await s3Ctx.Response.Send(s3Obj.Size, s3Obj.Data).ConfigureAwait(false);
                            }
                            else
                            {
                                error = new Error(ErrorCode.NoSuchKey);
                                s3Ctx.Response.StatusCode = 404;
                                s3Ctx.Response.ContentType = Constants.ContentTypeXml;
                                await s3Ctx.Response.Send(SerializationHelper.SerializeXml(error)).ConfigureAwait(false);
                            }
                            return;
                        }
                        break;

                    case S3RequestType.ObjectReadRetention:
                        if (this.Settings.ObjectCallbacks.ReadRetention != null)
                        {
                            retention = await this.Settings.ObjectCallbacks.ReadRetention(s3Ctx).ConfigureAwait(false);
                            s3Ctx.Response.StatusCode = 200;
                            s3Ctx.Response.ContentType = Constants.ContentTypeXml;
                            await s3Ctx.Response.Send(SerializationHelper.SerializeXml(retention)).ConfigureAwait(false);
                            return;
                        }
                        break;

                    case S3RequestType.ObjectReadTags:
                        if (this.Settings.ObjectCallbacks.ReadTagging != null)
                        {
                            tagging = await this.Settings.ObjectCallbacks.ReadTagging(s3Ctx).ConfigureAwait(false);
                            s3Ctx.Response.StatusCode = 200;
                            s3Ctx.Response.ContentType = Constants.ContentTypeXml;
                            await s3Ctx.Response.Send(SerializationHelper.SerializeXml(tagging)).ConfigureAwait(false);
                            return;
                        }
                        break;

                    case S3RequestType.ObjectRestore:
                        if (this.Settings.ObjectCallbacks.Restore != null)
                        {
                            if (string.IsNullOrWhiteSpace(s3Ctx.Request.DataAsString))
                                throw new S3Exception(new Error(ErrorCode.MissingRequestBodyError));

                            try
                            {
                                restoreRequest = SerializationHelper.DeserializeXml<RestoreRequest>(s3Ctx.Request.DataAsString);
                            }
                            catch (ArgumentOutOfRangeException aoore)
                            {
                                aoore.Data.Add("Context", s3Ctx);
                                aoore.Data.Add("RequestBody", s3Ctx.Request.DataAsString);
                                this._logger?.LogInformation(this._header + "restore request validation exception: " + Environment.NewLine + aoore.ToString());
                                throw new S3Exception(new Error(ErrorCode.InvalidArgument), aoore);
                            }
                            catch (InvalidOperationException ioe)
                            {
                                ioe.Data.Add("Context", s3Ctx);
                                ioe.Data.Add("RequestBody", s3Ctx.Request.DataAsString);
                                this._logger?.LogInformation(this._header + "XML exception: " + Environment.NewLine + ioe.ToString());
                                throw new S3Exception(new Error(ErrorCode.MalformedXml), ioe);
                            }

                            if (restoreRequest == null)
                                throw new S3Exception(new Error(ErrorCode.MalformedXml));

                            if (restoreRequest.HasUnsupportedRestoreSelectFields)
                                throw new S3Exception(new Error(ErrorCode.NotImplemented));

                            if (restoreRequest.Days == null)
                                throw new S3Exception(new Error(ErrorCode.InvalidRequest));

                            restoreResult = await this.Settings.ObjectCallbacks.Restore(s3Ctx, restoreRequest).ConfigureAwait(false);
                            if (restoreResult == null) restoreResult = new RestoreObjectResult();

                            if (!string.IsNullOrEmpty(restoreResult.RestoreOutputPath))
                                s3Ctx.Response.Headers.Add(Constants.HeaderRestoreOutputPath, restoreResult.RestoreOutputPath);

                            s3Ctx.Response.StatusCode = (restoreResult.AlreadyRestored ? 200 : 202);
                            s3Ctx.Response.ContentType = Constants.ContentTypeText;
                            await s3Ctx.Response.Send().ConfigureAwait(false);
                            return;
                        }
                        break;

                    case S3RequestType.ObjectSelectContent:
                        if (this.Settings.ObjectCallbacks.SelectContent != null)
                        {
                            try
                            {
                                selectRequest = SerializationHelper.DeserializeXml<SelectObjectContentRequest>(s3Ctx.Request.DataAsString);
                            }
                            catch (InvalidOperationException ioe)
                            {
                                ioe.Data.Add("Context", s3Ctx);
                                ioe.Data.Add("RequestBody", s3Ctx.Request.DataAsString);
                                this._logger?.LogInformation(this._header + "XML exception: " + Environment.NewLine + ioe.ToString());
                                await s3Ctx.Response.Send(ErrorCode.MalformedXml).ConfigureAwait(false);
                                return;
                            }

                            await this.Settings.ObjectCallbacks.SelectContent(s3Ctx, selectRequest).ConfigureAwait(false);
                            s3Ctx.Response.StatusCode = 200;
                            s3Ctx.Response.ContentType = Constants.ContentTypeText;
                            await s3Ctx.Response.Send().ConfigureAwait(false);
                            return;
                        }
                        break;

                    case S3RequestType.ObjectUploadPart:
                        if (this.Settings.ObjectCallbacks.UploadPart != null)
                        {
                            await this.Settings.ObjectCallbacks.UploadPart(s3Ctx).ConfigureAwait(false);
                            s3Ctx.Response.StatusCode = 200;
                            s3Ctx.Response.ContentType = Constants.ContentTypeText;
                            await s3Ctx.Response.Send().ConfigureAwait(false);
                            return;
                        }
                        break;

                    case S3RequestType.ObjectWrite:
                        if (this.Settings.ObjectCallbacks.Write != null)
                        {
                            long effectiveContentLength = s3Ctx.Request.ContentLength;
                            string decodedLenStr = s3Ctx.Request.RetrieveHeaderValue("x-amz-decoded-content-length");
                            if (!string.IsNullOrEmpty(decodedLenStr) && long.TryParse(decodedLenStr, out long decodedLen))
                                effectiveContentLength = decodedLen;

                            if (effectiveContentLength > this._settings.OperationLimits.MaxPutObjectSize)
                            {
                                error = new Error(ErrorCode.EntityTooLarge);
                                s3Ctx.Response.StatusCode = 400;
                                s3Ctx.Response.ContentType = Constants.ContentTypeXml;
                                await s3Ctx.Response.Send(SerializationHelper.SerializeXml(error)).ConfigureAwait(false);
                                return;
                            }

                            await this.Settings.ObjectCallbacks.Write(s3Ctx).ConfigureAwait(false);
                            s3Ctx.Response.StatusCode = 200;
                            s3Ctx.Response.ContentType = Constants.ContentTypeText;
                            await s3Ctx.Response.Send().ConfigureAwait(false);
                            return;
                        }
                        break;

                    case S3RequestType.ObjectWriteAcl:
                        if (this.Settings.ObjectCallbacks.WriteAcl != null)
                        {
                            try
                            {
                                acp = SerializationHelper.DeserializeXml<AccessControlPolicy>(s3Ctx.Request.DataAsString);
                            }
                            catch (InvalidOperationException ioe)
                            {
                                ioe.Data.Add("Context", s3Ctx);
                                ioe.Data.Add("RequestBody", s3Ctx.Request.DataAsString);
                                this._logger?.LogInformation(this._header + "XML exception: " + Environment.NewLine + ioe.ToString());
                                await s3Ctx.Response.Send(ErrorCode.MalformedXml).ConfigureAwait(false);
                                return;
                            }

                            await this.Settings.ObjectCallbacks.WriteAcl(s3Ctx, acp).ConfigureAwait(false);
                            s3Ctx.Response.StatusCode = 200;
                            s3Ctx.Response.ContentType = Constants.ContentTypeText;
                            await s3Ctx.Response.Send().ConfigureAwait(false);
                            return;
                        }
                        break;

                    case S3RequestType.ObjectWriteLegalHold:
                        if (this.Settings.ObjectCallbacks.WriteLegalHold != null)
                        {
                            try
                            {
                                legalHold = SerializationHelper.DeserializeXml<LegalHold>(s3Ctx.Request.DataAsString);
                            }
                            catch (InvalidOperationException ioe)
                            {
                                ioe.Data.Add("Context", s3Ctx);
                                ioe.Data.Add("RequestBody", s3Ctx.Request.DataAsString);
                                this._logger?.LogInformation(this._header + "XML exception: " + Environment.NewLine + ioe.ToString());
                                await s3Ctx.Response.Send(ErrorCode.MalformedXml).ConfigureAwait(false);
                                return;
                            }

                            await this.Settings.ObjectCallbacks.WriteLegalHold(s3Ctx, legalHold).ConfigureAwait(false);
                            s3Ctx.Response.StatusCode = 200;
                            s3Ctx.Response.ContentType = Constants.ContentTypeText;
                            await s3Ctx.Response.Send().ConfigureAwait(false);
                            return;
                        }
                        break;

                    case S3RequestType.ObjectWriteRetention:
                        if (this.Settings.ObjectCallbacks.WriteRetention != null)
                        {
                            try
                            {
                                retention = SerializationHelper.DeserializeXml<Retention>(s3Ctx.Request.DataAsString);
                            }
                            catch (InvalidOperationException ioe)
                            {
                                ioe.Data.Add("Context", s3Ctx);
                                ioe.Data.Add("RequestBody", s3Ctx.Request.DataAsString);
                                this._logger?.LogInformation(this._header + "XML exception: " + Environment.NewLine + ioe.ToString());
                                await s3Ctx.Response.Send(ErrorCode.MalformedXml).ConfigureAwait(false);
                                return;
                            }

                            await this.Settings.ObjectCallbacks.WriteRetention(s3Ctx, retention).ConfigureAwait(false);
                            s3Ctx.Response.StatusCode = 200;
                            s3Ctx.Response.ContentType = Constants.ContentTypeText;
                            await s3Ctx.Response.Send().ConfigureAwait(false);
                            return;
                        }
                        break;

                    case S3RequestType.ObjectWriteTags:
                        if (this.Settings.ObjectCallbacks.WriteTagging != null)
                        {
                            try
                            {
                                tagging = SerializationHelper.DeserializeXml<Tagging>(s3Ctx.Request.DataAsString);
                            }
                            catch (InvalidOperationException ioe)
                            {
                                ioe.Data.Add("Context", s3Ctx);
                                ioe.Data.Add("RequestBody", s3Ctx.Request.DataAsString);
                                this._logger?.LogInformation(this._header + "XML exception: " + Environment.NewLine + ioe.ToString());
                                await s3Ctx.Response.Send(ErrorCode.MalformedXml).ConfigureAwait(false);
                                return;
                            }

                            await this.Settings.ObjectCallbacks.WriteTagging(s3Ctx, tagging).ConfigureAwait(false);
                            s3Ctx.Response.StatusCode = 200;
                            s3Ctx.Response.ContentType = Constants.ContentTypeText;
                            await s3Ctx.Response.Send().ConfigureAwait(false);
                            return;
                        }
                        break;

                    #endregion
                }

                if (this._settings.DefaultRequestHandler != null)
                {
                    await this._settings.DefaultRequestHandler(s3Ctx).ConfigureAwait(false);
                    return;
                }

                if (s3Ctx.Request.RequestType != S3RequestType.Unknown)
                {
                    this._logger?.LogInformation(this._header + "no callback registered for request type " + s3Ctx.Request.RequestType.ToString());
                    await s3Ctx.Response.Send(ErrorCode.NotImplemented).ConfigureAwait(false);
                }
                else
                {
                    await s3Ctx.Response.Send(ErrorCode.InvalidRequest).ConfigureAwait(false);
                }
                return;
            }
            catch (S3Exception s3E)
            {
                this._logger?.LogInformation(this._header + "S3 exception:" + Environment.NewLine + s3E.ToString());

                if (s3Ctx != null)
                {
                    s3Ctx.Response.StatusCode = s3E.HttpStatusCode;
                    s3Ctx.Response.ContentType = Constants.ContentTypeXml;
                    await s3Ctx.Response.Send(s3E.Error).ConfigureAwait(false);
                }

                return;
            }
            catch (Exception e)
            {
                this._logger?.LogInformation(this._header + "exception:" + Environment.NewLine + e.ToString());

                if (s3Ctx != null)
                {
                    s3Ctx.Response.StatusCode = 500;
                    s3Ctx.Response.ContentType = Constants.ContentTypeXml;
                    await s3Ctx.Response.Send(ErrorCode.InternalError).ConfigureAwait(false);
                }

                return;
            }
            finally
            {
                if (s3Ctx != null)
                {
                    s3Ctx.Timestamp.End = DateTime.UtcNow;

                    if (this._settings.PostRequestHandler != null)
                    {
                        try
                        {
                            await this._settings.PostRequestHandler(s3Ctx).ConfigureAwait(false);
                        }
                        catch (Exception e)
                        {
                            this._logger?.LogInformation(this._header + "post-request handler exception:" + Environment.NewLine + e.ToString());
                        }
                    }
                }
            }
        }
    }

    private static void AddRestoreHeader(NameValueCollection headers, RestoreStatus status)
    {
        if (headers == null || status == null) return;

        string headerValue = status.HeaderValue;
        if (!string.IsNullOrEmpty(headerValue))
            headers.Add(Constants.HeaderRestore, headerValue);
    }

    #endregion
}