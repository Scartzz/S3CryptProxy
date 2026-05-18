namespace S3CryptProxy.Handler;

using System;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using S3CryptProxy.Handler.Interfaces;
using S3CryptProxy.Models;
using S3CryptProxy.Shared;

/// <summary>
/// S3 response.
/// </summary>
public class S3Response
{
    #region Public-Members

    /// <summary>
    /// The HTTP status code to return to the requestor (client).
    /// </summary>
    public int StatusCode
    {
        get
        {
            return this._httpResponse.StatusCode;
        }
        set
        {
            this._httpResponse.StatusCode = value;
        }
    }

    /// <summary>
    /// User-supplied headers to include in the response.
    /// </summary>
    public NameValueCollection Headers
    {
        get
        {
            return this._httpResponse.Headers;
        }
        set
        {
            if (value == null) this._httpResponse.Headers = new NameValueCollection(StringComparer.InvariantCultureIgnoreCase);
            else this._httpResponse.Headers = value;
        }
    }

    /// <summary>
    /// User-supplied content-type to include in the response.
    /// </summary>
    public string ContentType
    {
        get
        {
            return this._httpResponse.ContentType;
        }
        set
        {
            this._httpResponse.ContentType = value;
        }
    }

    /// <summary>
    /// The length of the data in the response stream.  This value must be set before assigning the stream.
    /// </summary>
    public long ContentLength
    {
        get
        {
            return this._httpResponse.ContentLength;
        }
        set
        {
            if (value < 0) throw new ArgumentException("Content length must be zero or greater.");
            this._httpResponse.ContentLength = value;
        }
    }

    /// <summary>
    /// Enable or disable chunked transfer-encoding.
    /// By default this parameter is set to the value of Chunked in the S3Request object.
    /// If Chunked is false, use Send() APIs.
    /// If Chunked is true, use SendChunk() or SendFinalChunk() APIs.
    /// The Send(ErrorCode) API is valid for both conditions.
    /// </summary>
    public bool ChunkedTransfer
    {
        get
        {
            return this._httpResponse.ChunkedTransfer;
        }
        set
        {
            this._httpResponse.ChunkedTransfer = value;
        }
    }

    /// <summary>
    /// The data to return to the requestor.  Set ContentLength before assigning the stream.
    /// </summary>
    [JsonIgnore]
    public Stream Data
    {
        get
        {
            return this._httpResponse.Data;
        }
    }

    /// <summary>
    /// Data stream as a string.  Fully reads the data stream.
    /// </summary>
    [JsonIgnore]
    public string DataAsString
    {
        get
        {
            return this._httpResponse.DataAsString;
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
            return this._httpResponse.DataAsBytes;
        }
    }

    #endregion

    #region Private-Members

    private IS3HttpResponse _httpResponse = null;
    private S3Request _s3Request = null;

    #endregion

    #region Constructors-and-Factories

    /// <summary>
    /// Instantiate.
    /// </summary>
    public S3Response()
    {

    }

    /// <summary>
    /// Instantiate the object without supplying a stream.  Useful for HEAD responses.
    /// </summary>
    /// <param name="ctx">S3 context.</param>
    public S3Response(S3Context ctx)
    {
        if (ctx == null) throw new ArgumentNullException(nameof(ctx));

        this._httpResponse = ctx.Http.Response;
        this._s3Request = ctx.Request;
    }

    #endregion

    #region Public-Methods

    /// <summary>
    /// Send the response with no data to the requestor and close the connection.
    /// </summary>
    /// <returns>True if successful.</returns>
    public async Task<bool> Send()
    {
        if (this.ChunkedTransfer) throw new IOException("Responses with chunked transfer-encoding enabled require use of SendChunk() and SendFinalChunk().");

        if (this.ContentLength > 0) this._httpResponse.ContentLength = this.ContentLength;

        this.SetResponseHeaders();

        return await this._httpResponse.Send().ConfigureAwait(false);
    }

    /// <summary>
    /// Send the response with the supplied data to the requestor and close the connection.
    /// </summary>
    /// <param name="data">Data.</param>
    /// <returns>True if successful.</returns>
    public async Task<bool> Send(string data)
    {
        if (this.ChunkedTransfer) throw new IOException("Responses with chunked transfer-encoding enabled require use of SendChunk() and SendFinalChunk().");

        byte[] bytes = Array.Empty<byte>();
        if (!string.IsNullOrEmpty(data))
        {
            bytes = Encoding.UTF8.GetBytes(data);
            this.ContentLength = bytes.Length;
        }
        else
        {
            this.ContentLength = 0;
        }

        this.SetResponseHeaders();

        return await this._httpResponse.Send(bytes).ConfigureAwait(false);
    }

    /// <summary>
    /// Send the response with the supplied data to the requestor and close the connection.
    /// </summary>
    /// <param name="data">Data.</param>
    /// <returns>True if successful.</returns>
    public async Task<bool> Send(byte[] data)
    {
        if (this.ChunkedTransfer) throw new IOException("Responses with chunked transfer-encoding enabled require use of SendChunk() and SendFinalChunk().");

        this.ContentLength = 0;

        using (MemoryStream ms = new MemoryStream())
        {
            if (data != null && data.Length > 0)
            {
                ms.Write(data, 0, data.Length);
                this.ContentLength = data.Length;
            }

            ms.Seek(0, SeekOrigin.Begin);

            this.SetResponseHeaders();

            return await this._httpResponse.Send(this.ContentLength, ms).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Send the response with the supplied stream to the requestor and close the connection.
    /// </summary>
    /// <param name="contentLength">Content length.</param>
    /// <param name="stream">Stream containing data.</param>
    /// <returns>True if successful.</returns>
    public async Task<bool> Send(long contentLength, Stream stream)
    {
        if (this.ChunkedTransfer) throw new IOException("Responses with chunked transfer-encoding enabled require use of SendChunk() and SendFinalChunk().");

        this.ContentLength = contentLength;

        this.SetResponseHeaders();

        if (stream != null && this.ContentLength > 0)
        {
            return await this._httpResponse.Send(this.ContentLength, stream).ConfigureAwait(false);
        }
        else
        {
            return await this._httpResponse.Send().ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Send an error response to the requestor and close the connection.
    /// </summary>
    /// <param name="error">Error.</param>
    /// <returns>True if successful.</returns>
    public async Task<bool> Send(Error error)
    {
        this.ChunkedTransfer = false;

        byte[] bytes = Encoding.UTF8.GetBytes(SerializationHelper.SerializeXml(error));

        using (MemoryStream ms = new MemoryStream(bytes))
        {
            ms.Seek(0, SeekOrigin.Begin);

            this.ContentLength = bytes.Length;
            this.StatusCode = error.HttpStatusCode;
            this.ContentType = Constants.ContentTypeXml;

            this.SetResponseHeaders();

            return await this._httpResponse.Send(this.ContentLength, ms).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Send an error response to the requestor and close the connection.
    /// </summary>
    /// <param name="error">ErrorCode.</param>
    /// <returns>True if successful.</returns>
    public async Task<bool> Send(ErrorCode error)
    {
        this.ChunkedTransfer = false;

        Error errorBody = new Error(error);

        byte[] bytes = Encoding.UTF8.GetBytes(SerializationHelper.SerializeXml(errorBody));

        using (MemoryStream ms = new MemoryStream(bytes))
        {
            ms.Seek(0, SeekOrigin.Begin);

            this.ContentLength = bytes.Length;
            this.StatusCode = errorBody.HttpStatusCode;
            this.ContentType = Constants.ContentTypeXml;

            this.SetResponseHeaders();

            return await this._httpResponse.Send(this.ContentLength, ms).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Send a chunk of data using chunked transfer-encoding to the requestor.
    /// </summary>
    /// <param name="data">Chunk of data.</param>
    /// <param name="isFinal">Boolean indicating if the chunk is the final chunk.</param>
    /// <returns>True if successful.</returns>
    public async Task<bool> SendChunk(byte[] data, bool isFinal)
    {
        if (!this.ChunkedTransfer) throw new IOException("Responses with chunked transfer-encoding disabled require use of Send().");

        this.SetResponseHeaders();

        if (data == null) data = Array.Empty<byte>();
        return await this._httpResponse.SendChunk(data, isFinal).ConfigureAwait(false);
    }

    #endregion

    #region Private-Methods

    private void SetResponseHeaders()
    {
        if (this.Headers == null) this.Headers = new NameValueCollection(StringComparer.InvariantCultureIgnoreCase);

        if (this.Headers.Get("Server") == null)
            this.Headers.Add("Server", "AmazonS3");

        if (this.Headers.Get("Date") != null) this.Headers.Remove("Date");

        this.Headers.Add("Date", DateTime.UtcNow.ToString(Constants.AmazonTimestampFormatVerbose, CultureInfo.InvariantCulture));
    }

    #endregion
}