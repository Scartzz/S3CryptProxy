namespace S3CryptProxy.Handler;

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PrettyId;
using S3CryptProxy.Handler.Interfaces;

/// <summary>
/// S3 request.
/// </summary>
public class S3Request
{
    #region Public-Members

    /// <summary>
    /// Request ID.
    /// </summary>
    public string RequestId { get; set; } = _idGenerator.GenerateUrlSafe("req_", 32);

    /// <summary>
    /// Trace ID.
    /// </summary>
    public string TraceId { get; set; } = _idGenerator.GenerateUrlSafe("trace_", 32);

    /// <summary>
    /// Indicates if the request includes the bucket name in the hostname or not.
    /// </summary>
    public S3RequestStyle RequestStyle { get; private set; } = S3RequestStyle.Unknown;

    /// <summary>
    /// Indicates the type of S3 request.
    /// </summary>
    public S3RequestType RequestType { get; private set; } = S3RequestType.Unknown;

    /// <summary>
    /// Indicates if chunked transfer-encoding is in use.
    /// </summary>
    public bool Chunked { get; set; } = false;

    /// <summary>
    /// AWS region.
    /// </summary>
    public string Region { get; set; } = null;

    /// <summary>
    /// Hostname.
    /// </summary>
    public string Hostname { get; set; } = null;

    /// <summary>
    /// Host header value.
    /// </summary>
    public string Host { get; set; } = null;

    /// <summary>
    /// Base domain identified in the hostname.
    /// </summary>
    public string BaseDomain { get; set; } = null;

    /// <summary>
    /// Bucket.
    /// </summary>
    public string Bucket { get; set; } = null;

    /// <summary>
    /// Object key.
    /// </summary>
    public string Key { get; set; } = null;

    /// <summary>
    /// Object key prefix.
    /// </summary>
    public string Prefix { get; set; } = null;

    /// <summary>
    /// Delimiter.
    /// </summary>
    public string Delimiter { get; set; } = null;

    /// <summary>
    /// Marker.
    /// </summary>
    public string Marker { get; set; } = null;

    /// <summary>
    /// Part number from a multipart upload.
    /// </summary>
    public int PartNumber
    {
        get
        {
            return this._partNumber;
        }
        set
        {
            if (value < 0) throw new ArgumentOutOfRangeException(nameof(this.PartNumber));
            this._partNumber = value;
        }
    }

    /// <summary>
    /// Part number arker.
    /// </summary>
    public int PartNumberMarker
    {
        get
        {
            return this._partNumberMarker;
        }
        set
        {
            if (value < 0) throw new ArgumentOutOfRangeException(nameof(this.PartNumberMarker));
            this._partNumberMarker = value;
        }
    }

    /// <summary>
    /// Maximum number of keys to retrieve in an enumeration.
    /// </summary>
    public int MaxKeys
    {
        get
        {
            return this._maxKeys;
        }
        set
        {
            if (value < 0) throw new ArgumentOutOfRangeException(nameof(this.MaxKeys));
            this._maxKeys = value;
        }
    }

    /// <summary>
    /// Maximum number of parts to retrieve in an enumeration.
    /// </summary>
    public int MaxParts
    {
        get
        {
            return this._maxParts;
        }
        set
        {
            if (value < 0) throw new ArgumentOutOfRangeException(nameof(this.MaxParts));
            this._maxParts = value;
        }
    }

    /// <summary>
    /// Object version ID.
    /// </summary>
    public string VersionId { get; set; } = null;

    /// <summary>
    /// Upload ID.
    /// </summary>
    public string UploadId { get; set; } = null;

    /// <summary>
    /// Authorization header string, in full.
    /// </summary>
    public string Authorization { get; set; } = null;

    /// <summary>
    /// Signature version from authorization header.
    /// </summary>
    public S3SignatureVersion SignatureVersion { get; set; } = S3SignatureVersion.Unknown;

    /// <summary>
    /// Signature from authorization header.
    /// </summary>
    public string Signature { get; set; } = null;

    /// <summary>
    /// Content type.
    /// </summary>
    public string ContentType { get; set; } = null;

    /// <summary>
    /// Content MD5 hash from request headers.
    /// </summary>
    public string ContentMd5 { get; set; } = null;

    /// <summary>
    /// Content SHA256 hash from request headers.
    /// </summary>
    public string ContentSha256 { get; set; } = null;

    /// <summary>
    /// Content length.
    /// </summary>
    public long ContentLength
    {
        get
        {
            if (this._httpRequest != null) return this._httpRequest.ContentLength;
            return 0;
        }
    }

    /// <summary>
    /// Date parameter.
    /// </summary>
    public string Date { get; set; } = null;

    /// <summary>
    /// Expiration parameter from authorization header.
    /// </summary>
    public string Expires { get; set; } = null;

    /// <summary>
    /// Access key, parsed from authorization header.
    /// </summary>
    public string AccessKey { get; set; } = null;

    /// <summary>
    /// Start value from the Range header.
    /// </summary>
    public long? RangeStart
    {
        get
        {
            return this._rangeStart;
        }
        set
        {
            if (value != null && value.Value < 0)
                throw new ArgumentOutOfRangeException(nameof(this.RangeStart));
            this._rangeStart = value;
        }
    }

    /// <summary>
    /// End value from the Range header.
    /// </summary>
    public long? RangeEnd
    {
        get
        {
            return this._rangeEnd;
        }
        set
        {
            if (value != null && value.Value < 0)
                throw new ArgumentOutOfRangeException(nameof(this.RangeEnd));
            this._rangeEnd = value;
        }
    }

    /// <summary>
    /// Continuation token.
    /// </summary>
    public string ContinuationToken { get; set; } = null;

    /// <summary>
    /// Indicates if the request is a service request.
    /// </summary>
    public bool IsServiceRequest
    {
        get
        {
            if (this.RequestType == S3RequestType.ListBuckets)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Indicates if the request is a bucket request.
    /// </summary>
    public bool IsBucketRequest
    {
        get
        {
            if (this.RequestType == S3RequestType.BucketDelete
                || this.RequestType == S3RequestType.BucketDeleteTags
                || this.RequestType == S3RequestType.BucketDeleteWebsite
                || this.RequestType == S3RequestType.BucketExists
                || this.RequestType == S3RequestType.BucketRead
                || this.RequestType == S3RequestType.BucketReadAcl
                || this.RequestType == S3RequestType.BucketReadLocation
                || this.RequestType == S3RequestType.BucketReadLogging
                || this.RequestType == S3RequestType.BucketReadTags
                || this.RequestType == S3RequestType.BucketReadVersioning
                || this.RequestType == S3RequestType.BucketReadVersions
                || this.RequestType == S3RequestType.BucketReadWebsite
                || this.RequestType == S3RequestType.BucketWrite
                || this.RequestType == S3RequestType.BucketWriteAcl
                || this.RequestType == S3RequestType.BucketWriteLogging
                || this.RequestType == S3RequestType.BucketWriteTags
                || this.RequestType == S3RequestType.BucketWriteVersioning
                || this.RequestType == S3RequestType.BucketWriteWebsite)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Indicates if the request is an object request.
    /// </summary>
    public bool IsObjectRequest
    {
        get
        {
            if (this.RequestType == S3RequestType.ObjectDelete
                || this.RequestType == S3RequestType.ObjectDeleteMultiple
                || this.RequestType == S3RequestType.ObjectDeleteTags
                || this.RequestType == S3RequestType.ObjectExists
                || this.RequestType == S3RequestType.ObjectRead
                || this.RequestType == S3RequestType.ObjectReadAcl
                || this.RequestType == S3RequestType.ObjectReadLegalHold
                || this.RequestType == S3RequestType.ObjectReadRange
                || this.RequestType == S3RequestType.ObjectReadRetention
                || this.RequestType == S3RequestType.ObjectReadTags
                || this.RequestType == S3RequestType.ObjectRestore
                || this.RequestType == S3RequestType.ObjectWrite
                || this.RequestType == S3RequestType.ObjectWriteAcl
                || this.RequestType == S3RequestType.ObjectWriteLegalHold
                || this.RequestType == S3RequestType.ObjectWriteRetention
                || this.RequestType == S3RequestType.ObjectWriteTags)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Indicates if the request is a multipart upload request.
    /// </summary>
    public bool IsMultipartUploadRequest
    {
        get
        {
            if (this.RequestType == S3RequestType.BucketReadMultipartUploads
                || this.RequestType == S3RequestType.ObjectAbortMultipartUpload
                || this.RequestType == S3RequestType.ObjectCompleteMultipartUpload
                || this.RequestType == S3RequestType.ObjectCreateMultipartUpload
                || this.RequestType == S3RequestType.ObjectDeleteMultiple
                || this.RequestType == S3RequestType.ObjectReadParts
                || this.RequestType == S3RequestType.ObjectUploadPart)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Lists the permission typically required for this type of request.
    /// See https://docs.aws.amazon.com/AmazonS3/latest/dev/acl-overview.html for details.
    /// </summary>
    public S3PermissionType PermissionsRequired
    {
        get
        {
            switch (this.RequestType)
            {
                case S3RequestType.BucketDelete:
                case S3RequestType.BucketDeleteTags:
                case S3RequestType.BucketDeleteWebsite:
                case S3RequestType.BucketWrite:
                case S3RequestType.BucketWriteLogging:
                case S3RequestType.BucketWriteTags:
                case S3RequestType.BucketWriteVersioning:
                case S3RequestType.BucketWriteWebsite:
                    return S3PermissionType.BucketWrite;

                case S3RequestType.BucketExists:
                case S3RequestType.BucketRead:
                case S3RequestType.BucketReadLocation:
                case S3RequestType.BucketReadLogging:
                case S3RequestType.BucketReadTags:
                case S3RequestType.BucketReadVersioning:
                case S3RequestType.BucketReadVersions:
                case S3RequestType.BucketReadWebsite:
                    return S3PermissionType.BucketRead;

                case S3RequestType.BucketReadAcl:
                    return S3PermissionType.BucketReadAcp;

                case S3RequestType.BucketWriteAcl:
                    return S3PermissionType.BucketWriteAcp;

                case S3RequestType.ObjectExists:
                case S3RequestType.ObjectRead:
                case S3RequestType.ObjectReadLegalHold:
                case S3RequestType.ObjectReadRange:
                case S3RequestType.ObjectReadRetention:
                case S3RequestType.ObjectReadTags:
                    return S3PermissionType.ObjectRead;

                case S3RequestType.ObjectDelete:
                case S3RequestType.ObjectDeleteMultiple:
                case S3RequestType.ObjectDeleteTags:
                case S3RequestType.ObjectRestore:
                case S3RequestType.ObjectWrite:
                case S3RequestType.ObjectWriteLegalHold:
                case S3RequestType.ObjectWriteRetention:
                case S3RequestType.ObjectWriteTags:
                    return S3PermissionType.BucketWrite;

                case S3RequestType.ObjectReadAcl:
                    return S3PermissionType.ObjectReadAcp;

                case S3RequestType.ObjectWriteAcl:
                    return S3PermissionType.ObjectWriteAcp;

                case S3RequestType.ListBuckets:
                case S3RequestType.Unknown:
                default:
                    return S3PermissionType.NotApplicable;
            }
        }
    }

    /// <summary>
    /// List of signed headers.
    /// </summary>
    [JsonPropertyOrder(998)]
    public List<string> SignedHeaders
    {
        get
        {
            return this._signedHeaders;
        }
        set
        {
            if (value == null) this._signedHeaders = new List<string>();
            else this._signedHeaders = value;
        }
    }

    /// <summary>
    /// Stream containing the request body.
    /// </summary>
    [JsonIgnore]
    public Stream Data { get; private set; } = null;

    /// <summary>
    /// Data stream as a string.  Fully reads the data stream.
    /// </summary>
    [JsonIgnore]
    public string DataAsString
    {
        get
        {
            if (this._httpRequest != null) return this._httpRequest.DataAsString;
            return null;
        }
    }

    /// <summary>
    /// Data stream as a byte array.  Fully reads the data stream.
    /// </summary>
    [JsonIgnore]
    public byte[] DataAsBytes
    {
        get
        {
            if (this._httpRequest != null) return this._httpRequest.DataAsBytes;
            return null;
        }
    }

    #endregion

    #region Private-Members

    private static IdGenerator _idGenerator = new IdGenerator();

    private string _header = "[S3Request] ";
    private IS3HttpRequest _httpRequest = null;
    private ILoggerFactory _loggerFactory = null;
    private ILogger<S3Request> _logger = null;
    private List<string> _signedHeaders = new List<string>();

    private Dictionary<object, object> _userMetadata = new Dictionary<object, object>();

    private int _maxKeys = 1000;
    private int _maxParts = 1000;
    private int _partNumber = 1;
    private int _partNumberMarker = 1;
    private long? _rangeStart = null;
    private long? _rangeEnd = null;

    #endregion

    #region Constructors-and-Factories

    /// <summary>
    /// Instantiate.
    /// </summary>
    public S3Request()
    {
    }

    /// <summary>
    /// Instantiates the object.
    /// </summary>
    /// <param name="ctx">S3 context.</param>
    /// <param name="baseDomainFinder">Callback to invoke to find a base domain for a given hostname, used with virtual hosted style URLs.</param> 
    /// <param name="logger">Method to invoke to send log messages.</param> 
    public S3Request(S3Context ctx, ILoggerFactory loggerFactory = null)
    {
        if (ctx == null) throw new ArgumentNullException(nameof(ctx));
        if (ctx.Http == null) throw new ArgumentNullException(nameof(ctx.Http));

        this._httpRequest = ctx.Http.Request;
        this._loggerFactory = loggerFactory;
        if (loggerFactory is not null)
            this._logger = loggerFactory.CreateLogger<S3Request>();

        this.ParseHttpContext();
    }

    #endregion

    #region Public-Methods

    /// <summary>
    /// Determine if a header exists.
    /// </summary>
    /// <param name="key">Header key.</param>
    /// <returns>True if exists.</returns>
    public bool HeaderExists(string key)
    {
        if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));

        if (this._httpRequest != null)
        {
            return this._httpRequest.HeaderExists(key);
        }

        return false;
    }

    /// <summary>
    /// Determine if a querystring entry exists.
    /// </summary>
    /// <param name="key">Querystring key.</param>
    /// <returns>True if exists.</returns>
    public bool QuerystringExists(string key)
    {
        if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));

        if (this._httpRequest != null)
        {
            return this._httpRequest.QuerystringExists(key);
        }

        return false;
    }

    /// <summary>
    /// Retrieve a header (or querystring) value.
    /// </summary>
    /// <param name="key">Key.</param>
    /// <returns>Value.</returns>
    public string RetrieveHeaderValue(string key)
    {
        if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));

        if (this._httpRequest != null)
        {
            return this._httpRequest.RetrieveHeaderValue(key);
        }

        return null;
    }

    /// <summary>
    /// Retrieve a querystring value.
    /// </summary>
    /// <param name="key">Key.</param>
    /// <returns>Value.</returns>
    public string RetrieveQueryValue(string key)
    {
        if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));

        if (this._httpRequest != null)
        {
            return this._httpRequest.RetrieveQueryValue(key);
        }

        return null;
    }

    /// <summary>
    /// Read a chunk from the request body.
    /// </summary>
    /// <returns>Chunk.</returns>
    public async Task<IS3HttpRequestChunk> ReadChunk()
    {
        return await this._httpRequest.ReadChunk().ConfigureAwait(false);
    }

    #endregion

    #region Private-Methods

    private void ParseHttpContext()
    {
        if (this._httpRequest == null) throw new InvalidOperationException("HTTP context not supplied in the S3 request object.");

        #region Initialize

        this.Chunked = this._httpRequest.ChunkedTransfer;
        this.Region = null;
        this.Hostname = this._httpRequest.DestinationHostname;
        this.RequestType = S3RequestType.Unknown;
        this.RequestStyle = S3RequestStyle.Unknown;
        this.Bucket = null;
        this.Key = null;
        this.Authorization = null;
        this.AccessKey = null;
        this.Data = this._httpRequest.Data;

        #endregion

        #region Set-Parameters-from-Querystring

        if (this._httpRequest.QueryElements != null && this._httpRequest.QueryElements != null && this._httpRequest.QueryElements.Count > 0)
        {
            this.AccessKey = this.RetrieveQueryValue("awsaccesskeyid");
            this.ContinuationToken = this.RetrieveQueryValue("continuation-token");
            this.Delimiter = this.RetrieveQueryValue("delimiter");
            this.Expires = this.RetrieveQueryValue("expires");
            this.Marker = this.RetrieveQueryValue("marker");
            this.Prefix = this.RetrieveQueryValue("prefix");
            this.Signature = this.RetrieveQueryValue("signature");
            this.UploadId = this.RetrieveQueryValue("uploadid");
            this.VersionId = this.RetrieveQueryValue("versionid");

            if (this.QuerystringExists("max-keys"))
            {
                int maxKeys = 0;
                string maxKeysStr = this.RetrieveQueryValue("max-keys");
                if (!string.IsNullOrEmpty(maxKeysStr))
                {
                    if (int.TryParse(this._httpRequest.QueryElements["max-keys"], out maxKeys))
                    {
                        this.MaxKeys = maxKeys;
                    }
                }
            }

            if (this.QuerystringExists("max-parts"))
            {
                int maxParts = 0;
                string maxPartsStr = this.RetrieveQueryValue("max-parts");
                if (!string.IsNullOrEmpty(maxPartsStr))
                {
                    if (int.TryParse(this._httpRequest.QueryElements["max-parts"], out maxParts))
                    {
                        this.MaxParts = maxParts;
                    }
                }
            }

            if (this.QuerystringExists("partnumber"))
            {
                int partNum = 0;
                string partNumStr = this.RetrieveQueryValue("partnumber");
                if (!string.IsNullOrEmpty(partNumStr))
                {
                    if (int.TryParse(this._httpRequest.QueryElements["partnumber"], out partNum))
                    {
                        this.PartNumber = partNum;
                    }
                }
            }

            if (this.QuerystringExists("part-number-marker"))
            {
                int partNumMarker = 0;
                string partNumMarkerStr = this.RetrieveQueryValue("part-number-marker");
                if (!string.IsNullOrEmpty(partNumMarkerStr))
                {
                    if (int.TryParse(this._httpRequest.QueryElements["part-number-marker"], out partNumMarker))
                    {
                        this.PartNumberMarker = partNumMarker;
                    }
                }
            }
        }

        #endregion

        #region Set-Values-From-Headers

        if (this._httpRequest.Headers != null && this._httpRequest.Headers.Count > 0)
        {
            if (this.HeaderExists("authorization"))
            {
                this._logger?.LogInformation(this._header + "processing Authorization header");
                this.Authorization = this.RetrieveHeaderValue("authorization");
                this.ParseAuthorizationHeader();
            }

            if (this.HeaderExists("range"))
            {
                string rangeHeaderValue = this.RetrieveHeaderValue("range");

                if (!string.IsNullOrEmpty(rangeHeaderValue))
                {
                    long? start = null;
                    long? end = null;
                    this.ParseRangeHeader(rangeHeaderValue, out start, out end);

                    this.RangeStart = start;
                    this.RangeEnd = end;
                }
            }

            if (this.HeaderExists("content-md5")) this.ContentMd5 = this.RetrieveHeaderValue("content-md5");
            if (this.HeaderExists("content-type")) this.ContentType = this.RetrieveHeaderValue("content-type");
            if (this.HeaderExists("host")) this.Host = this.RetrieveHeaderValue("host");

            if (this.HeaderExists("x-amz-content-sha256"))
            {
                this.ContentSha256 = this.RetrieveHeaderValue("x-amz-content-sha256");
                if (!string.IsNullOrEmpty(this.ContentSha256))
                {
                    if (this.ContentSha256.IndexOf("streaming", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        this.Chunked = true;
                        this._httpRequest.ChunkedTransfer = true;
                    }
                }
            }

            if (this.HeaderExists("date"))
                this.Date = this.RetrieveHeaderValue("date");

            if (this.HeaderExists("x-amz-date"))
                this.Date = this.RetrieveHeaderValue("x-amz-date");

            if (this.HeaderExists("x-amz-request-id"))
                this.RequestId = this.RetrieveHeaderValue("x-amz-request-id");

            if (this.HeaderExists("x-amz-id-2"))
                this.TraceId = this.RetrieveHeaderValue("x-amz-id-2");
        }

        #endregion

        #region Set-Region-Bucket-Style-and-Key

        if (!string.IsNullOrEmpty(this.Hostname)
            && !string.IsNullOrEmpty(this._httpRequest.UrlRawWithoutQuery))
        {
            this.ParseHostnameAndUrl();
        }

        #endregion

        #region Set-RequestType

        this.SetRequestType();

        #endregion 
    }

    private void ParseAuthorizationHeader()
    {
        string logMessage = "";
        if (string.IsNullOrEmpty(this.Authorization)) return;
        string exceptionMsg = "Invalid authorization header format: " + this.Authorization;

        try
        {
            #region Retrieve-Outer-Values

            // [encryption] [values]
            string[] valsOuter = this.Authorization.Split(new[] { ' ' }, 2);
            if (valsOuter == null || valsOuter.Length < 2) throw new ArgumentException(exceptionMsg);

            logMessage += this._header + "Authorization header : " + this.Authorization + Environment.NewLine;
            logMessage += this._header + "Outer header values  :" + Environment.NewLine;
            for (int i = 0; i < valsOuter.Length; i++)
            {
                logMessage += "  " + i + ": " + valsOuter[i].Trim() + Environment.NewLine;
            }

            #endregion

            if (valsOuter[0].Equals("AWS"))
            {
                #region Signature-V2

                // see https://docs.aws.amazon.com/AmazonS3/latest/dev/RESTAuthentication.html#ConstructingTheAuthenticationHeader
                // Authorization: AWS AWSAccessKeyId:Signature

                string[] valsInner = valsOuter[1].Split(':');

                logMessage += this._header + "Inner header values" + Environment.NewLine;
                for (int i = 0; i < valsInner.Length; i++)
                {
                    logMessage += "  " + i + ": " + valsInner[i].Trim() + Environment.NewLine;
                }

                if (valsInner.Length != 2) throw new ArgumentException(exceptionMsg);
                this.SignatureVersion = S3SignatureVersion.Version2;
                this.AccessKey = valsInner[0].Trim();
                this.Signature = valsInner[1].Trim();

                logMessage +=
                    this._header + "Signature version    : " + this.SignatureVersion.ToString() + Environment.NewLine +
                    this._header + "Access key           : " + this.AccessKey + Environment.NewLine +
                    this._header + "Signature            : " + this.Signature;

                return;

                #endregion
            }
            else if (valsOuter[0].Equals("AWS4-HMAC-SHA256"))
            {
                #region Signature-V4

                // see https://docs.aws.amazon.com/AmazonS3/latest/API/sigv4-auth-using-authorization-header.html
                // 
                // AWS4-HMAC-SHA256 Credential=access/20190418/us-east-1/s3/aws4_request, SignedHeaders=content-length;content-type;host;user-agent;x-amz-content-sha256;x-amz-date;x-amz-decoded-content-length, Signature=66946e06895806f4e32d32217c1a02313b9d9235b759f3a690742c8f9971daa0
                //
                // valsOuter[0] AWS4-HMAC-SHA256
                // valsOuter[1] everything else...

                this.SignatureVersion = S3SignatureVersion.Version4;

                string[] keyValuePairs = valsOuter[1].Split(',');
                List<string> keyValuePairsTrimmed = new List<string>();

                logMessage += this._header + "Inner header values" + Environment.NewLine;

                for (int i = 0; i < keyValuePairs.Length; i++)
                {
                    string currKey = keyValuePairs[i];
                    if (string.IsNullOrEmpty(currKey)) continue;

                    currKey = currKey.Trim();
                    keyValuePairsTrimmed.Add(currKey);

                    logMessage += i + ": " + keyValuePairs[i].Trim() + Environment.NewLine;
                }

                foreach (string currKey in keyValuePairsTrimmed)
                {
                    if (currKey.StartsWith("Credential="))
                    {
                        #region Credentials

                        string credentialString = currKey.Replace("Credential=", "").Trim();
                        string[] credentialVals = credentialString.Split('/');
                        if (credentialVals.Length < 5) throw new ArgumentException(exceptionMsg);
                        this.AccessKey = credentialVals[0].Trim();
                        this.Region = credentialVals[2].Trim();

                        #endregion
                    }
                    else if (currKey.StartsWith("SignedHeaders="))
                    {
                        #region Signed-Headers

                        string signedHeadersString = currKey.Replace("SignedHeaders=", "").Trim();
                        string[] signedHeaderVals = signedHeadersString.Split(';');
                        if (signedHeaderVals != null && signedHeaderVals.Length > 0)
                        {
                            foreach (string currSignedHeader in signedHeaderVals)
                            {
                                this.SignedHeaders.Add(currSignedHeader.Trim());
                            }

                            this.SignedHeaders.Sort();
                        }

                        #endregion
                    }
                    else if (currKey.StartsWith("Signature="))
                    {
                        #region Signature

                        this.Signature = currKey.Replace("Signature=", "").Trim();

                        #endregion
                    }
                    else if (currKey.StartsWith("Expires="))
                    {
                        #region Expires

                        this.Expires = currKey.Replace("Expires=", "").Trim();

                        #endregion
                    }
                }

                logMessage +=
                    this._header + "Signature version    : " + this.SignatureVersion.ToString() + Environment.NewLine +
                    this._header + "Access key           : " + this.AccessKey + Environment.NewLine +
                    this._header + "Region               : " + this.Region + Environment.NewLine +
                    this._header + "Signature            : " + this.Signature;

                return;

                #endregion
            }
            else
            {
                throw new ArgumentException(exceptionMsg + this.Authorization);
            }
        }
        finally
        {
            this._logger?.LogInformation(logMessage);
        }
    }

    private void ParseHostnameAndUrl()
    {
        string fullUrl = this._httpRequest.UrlFull;

        // When the server is bound to a wildcard hostname (*, +, 0.0.0.0),
        // the URL will contain the wildcard which is not a valid URI hostname.
        // Replace it with the Host header value from the actual HTTP request.
        if (!string.IsNullOrEmpty(fullUrl) && !string.IsNullOrEmpty(this.Host))
        {
            Uri tempUri;
            if (!Uri.TryCreate(fullUrl, UriKind.Absolute, out tempUri))
            {
                string hostValue = this.Host.Contains(":") ? this.Host.Split(':')[0] : this.Host;
                fullUrl = ReplaceWildcardHostname(fullUrl, hostValue);
            }
        }

        Uri uri = new Uri(fullUrl);
        this.Hostname = uri.Host;

        this._logger?.LogInformation(this._header + "parsing URL " + this._httpRequest.UrlFull);

        if (IsIpAddress(this.Hostname))
        {
            this.RequestStyle = S3RequestStyle.PathStyle;
            this._logger?.LogDebug(this._header + "supplied hostname is an IP address");
        }
        else
        {
            // assume path style
            this.RequestStyle = S3RequestStyle.PathStyle;
            this._logger?.LogDebug(this._header + "no base domain finder specified, request assumed to have bucket in URL");
        }

        string rawUrl = this._httpRequest.UrlRawWithoutQuery;

        rawUrl = rawUrl.TrimStart('/');

        switch (this.RequestStyle)
        {
            case S3RequestStyle.VirtualHostedStyle:
                this.Key = WebUtility.UrlDecode(rawUrl);
                break;

            case S3RequestStyle.PathStyle:
                string[] valsInner = rawUrl.Split(new[] { '/' }, 2);
                if (valsInner.Length > 0) this.Bucket = WebUtility.UrlDecode(valsInner[0]);
                if (valsInner.Length > 1) this.Key = WebUtility.UrlDecode(valsInner[1]);
                break;
        }

        this._logger?.LogInformation($"{this._header}parsed URL: Full URL: {this._httpRequest.UrlFull}, Raw URL: {this._httpRequest.UrlRawWithoutQuery}, Hostname: {this.Hostname}, Base domain: {this.BaseDomain}, Bucket name: {this.Bucket}, Style: {this.RequestStyle}, Object key: {this.Key}, Region: {this.Region}");

        return;
    }

    private void ParseRangeHeader(string header, out long? start, out long? end)
    {
        start = null;
        end = null;

        if (string.IsNullOrEmpty(header)) throw new ArgumentNullException(nameof(header));
        header = header.ToLower();
        if (header.StartsWith("bytes=")) header = header.Substring(6);
        string[] vals = header.Split('-');
        if (vals.Length != 2) throw new ArgumentException("Invalid range header: " + header);

        if (!string.IsNullOrEmpty(vals[0])) start = Convert.ToInt64(vals[0]);
        if (!string.IsNullOrEmpty(vals[1])) end = Convert.ToInt64(vals[1]);
    }

    private void SetRequestType()
    {
        if (this._httpRequest.Method == HttpMethod.Head)
        {
            if (string.IsNullOrEmpty(this.Bucket) && string.IsNullOrEmpty(this.Key))
                this.RequestType = S3RequestType.ServiceExists;
            if (!string.IsNullOrEmpty(this.Bucket) && string.IsNullOrEmpty(this.Key))
                this.RequestType = S3RequestType.BucketExists;
            else if (!string.IsNullOrEmpty(this.Bucket) && !string.IsNullOrEmpty(this.Key))
                this.RequestType = S3RequestType.ObjectExists;
        }
        else if (this._httpRequest.Method == HttpMethod.Get)
        {
            if (string.IsNullOrEmpty(this.Bucket) && string.IsNullOrEmpty(this.Key))
            {
                this.RequestType = this.RequestType = S3RequestType.ListBuckets;
            }
            else if (!string.IsNullOrEmpty(this.Bucket) && string.IsNullOrEmpty(this.Key))
            {
                if (this.QuerystringExists("acl"))
                    this.RequestType = S3RequestType.BucketReadAcl;
                else if (this.QuerystringExists("location"))
                    this.RequestType = S3RequestType.BucketReadLocation;
                else if (this.QuerystringExists("logging"))
                    this.RequestType = S3RequestType.BucketReadLogging;
                else if (this.QuerystringExists("tagging"))
                    this.RequestType = S3RequestType.BucketReadTags;
                else if (this.QuerystringExists("uploads"))
                    this.RequestType = S3RequestType.BucketReadMultipartUploads;
                else if (this.QuerystringExists("versions"))
                    this.RequestType = S3RequestType.BucketReadVersions;
                else if (this.QuerystringExists("versioning"))
                    this.RequestType = S3RequestType.BucketReadVersioning;
                else if (this.QuerystringExists("website"))
                    this.RequestType = S3RequestType.BucketReadWebsite;
                else
                    this.RequestType = S3RequestType.BucketRead;
            }
            else if (!string.IsNullOrEmpty(this.Bucket) && !string.IsNullOrEmpty(this.Key))
            {
                if (this.HeaderExists("range") && this._rangeStart != null)
                    this.RequestType = S3RequestType.ObjectReadRange;
                else if (this.QuerystringExists("acl"))
                    this.RequestType = S3RequestType.ObjectReadAcl;
                else if (this.QuerystringExists("legal-hold"))
                    this.RequestType = S3RequestType.ObjectReadLegalHold;
                else if (this.QuerystringExists("uploadid"))
                    this.RequestType = S3RequestType.ObjectReadParts;
                else if (this.QuerystringExists("retention"))
                    this.RequestType = S3RequestType.ObjectReadRetention;
                else if (this.QuerystringExists("tagging"))
                    this.RequestType = S3RequestType.ObjectReadTags;
                else
                    this.RequestType = S3RequestType.ObjectRead;
            }
        }
        else if (this._httpRequest.Method == HttpMethod.Put)
        {
            if (!string.IsNullOrEmpty(this.Bucket) && string.IsNullOrEmpty(this.Key))
            {
                if (this.QuerystringExists("acl"))
                    this.RequestType = S3RequestType.BucketWriteAcl;
                else if (this.QuerystringExists("logging"))
                    this.RequestType = S3RequestType.BucketWriteLogging;
                else if (this.QuerystringExists("tagging"))
                    this.RequestType = S3RequestType.BucketWriteTags;
                else if (this.QuerystringExists("versioning"))
                    this.RequestType = S3RequestType.BucketWriteVersioning;
                else if (this.QuerystringExists("website"))
                    this.RequestType = S3RequestType.BucketWriteWebsite;
                else
                    this.RequestType = S3RequestType.BucketWrite;
            }
            else if (!string.IsNullOrEmpty(this.Bucket) && !string.IsNullOrEmpty(this.Key))
            {
                if (this.QuerystringExists("tagging"))
                    this.RequestType = S3RequestType.ObjectWriteTags;
                else if (this.QuerystringExists("acl"))
                    this.RequestType = S3RequestType.ObjectWriteAcl;
                else if (this.QuerystringExists("legal-hold"))
                    this.RequestType = S3RequestType.ObjectWriteLegalHold;
                else if (this.QuerystringExists("retention"))
                    this.RequestType = S3RequestType.ObjectWriteRetention;
                else if (this.QuerystringExists("partnumber") && this.QuerystringExists("uploadid"))
                    this.RequestType = S3RequestType.ObjectUploadPart;
                else
                    this.RequestType = S3RequestType.ObjectWrite;
            }
        }
        else if (this._httpRequest.Method == HttpMethod.Post)
        {
            if (!string.IsNullOrEmpty(this.Bucket))
            {
                if (this.QuerystringExists("delete"))
                    this.RequestType = S3RequestType.ObjectDeleteMultiple;

                if (!string.IsNullOrEmpty(this.Key))
                {
                    if (this.QuerystringExists("restore"))
                        this.RequestType = S3RequestType.ObjectRestore;
                    if (this.QuerystringExists("select")
                        && this.QuerystringExists("select-type")
                        && this.RetrieveQueryValue("select-type").Equals("2"))
                    {
                        this.RequestType = S3RequestType.ObjectSelectContent;
                    }

                    if (this.QuerystringExists("uploadid"))
                        this.RequestType = S3RequestType.ObjectCompleteMultipartUpload;
                    if (this.QuerystringExists("uploads"))
                        this.RequestType = S3RequestType.ObjectCreateMultipartUpload;
                }
            }
        }
        else if (this._httpRequest.Method == HttpMethod.Delete)
        {
            if (!string.IsNullOrEmpty(this.Bucket) && string.IsNullOrEmpty(this.Key))
            {
                if (this.QuerystringExists("acl"))
                    this.RequestType = S3RequestType.BucketDeleteAcl;
                else if (this.QuerystringExists("tagging"))
                    this.RequestType = S3RequestType.BucketDeleteTags;
                else if (this.QuerystringExists("website"))
                    this.RequestType = S3RequestType.BucketDeleteWebsite;
                else
                    this.RequestType = S3RequestType.BucketDelete;
            }
            else if (!string.IsNullOrEmpty(this.Bucket) && !string.IsNullOrEmpty(this.Key))
            {
                if (this.QuerystringExists("acl"))
                    this.RequestType = S3RequestType.ObjectDeleteAcl;
                else if (this.QuerystringExists("tagging"))
                    this.RequestType = S3RequestType.ObjectDeleteTags;
                else if (this.QuerystringExists("uploadid"))
                    this.RequestType = S3RequestType.ObjectAbortMultipartUpload;
                else
                    this.RequestType = S3RequestType.ObjectDelete;
            }
        }
    }

    private static bool IsIpAddress(string val)
    {
        if (string.IsNullOrEmpty(val)) throw new ArgumentNullException(nameof(val));
        return IPAddress.TryParse(val, out _);
    }

    internal static string ReplaceWildcardHostname(string url, string replacement)
    {
        // Match protocol prefix then wildcard hostname
        foreach (string wildcard in new[] { "*", "+", "0.0.0.0" })
        {
            string pattern = "://" + wildcard;
            int idx = url.IndexOf(pattern);
            if (idx >= 0)
            {
                return url.Substring(0, idx + 3) + replacement + url.Substring(idx + 3 + wildcard.Length);
            }
        }
        return url;
    }

    private static string ReplaceLastOccurrence(string src, string find, string replace)
    {
        int place = src.LastIndexOf(find);

        if (place == -1)
            return src;

        string result = src.Remove(place, find.Length).Insert(place, replace);
        return result;
    }

    #endregion
}