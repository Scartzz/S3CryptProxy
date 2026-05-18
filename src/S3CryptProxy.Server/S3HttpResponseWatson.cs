namespace S3CryptProxy.Server;

using System;
using System.Collections.Specialized;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using S3CryptProxy.Handler.Interfaces;
using WatsonWebserver.Core;

internal class S3HttpResponseWatson : IS3HttpResponse
{
    private readonly HttpResponseBase _httpResponse;

    public S3HttpResponseWatson(HttpResponseBase httpResponse)
    {
        ArgumentNullException.ThrowIfNull(httpResponse);
        this._httpResponse = httpResponse;
    }

    public int StatusCode
    {
        get => this._httpResponse.StatusCode;
        set => this._httpResponse.StatusCode = value;
    }
    public string ContentType
    {
        get => this._httpResponse.ContentType;
        set => this._httpResponse.ContentType = value;
    }
    public NameValueCollection Headers
    {
        get => this._httpResponse.Headers;
        set => this._httpResponse.Headers = value;
    }
    public long ContentLength
    {
        get => this._httpResponse.ContentLength;
        set => this._httpResponse.ContentLength = value;
    }
    public bool ChunkedTransfer
    {
        get => this._httpResponse.ChunkedTransfer;
        set => this._httpResponse.ChunkedTransfer = value;
    }
    public Stream Data
    {
        get => this._httpResponse.Data;
    }
    public string DataAsString
    {
        get => this._httpResponse.DataAsString;
    }
    public byte[] DataAsBytes
    {
        get => this._httpResponse.DataAsBytes;
    }

    public Task<bool> Send(CancellationToken cancellationToken = default)
    {
        return this._httpResponse.Send(cancellationToken);
    }

    public Task<bool> Send(byte[] data, CancellationToken cancellationToken = default)
    {
        return this._httpResponse.Send(data, cancellationToken);
    }

    public Task<bool> Send(long contentLength, Stream stream, CancellationToken cancellationToken = default)
    {
        return this._httpResponse.Send(contentLength, stream, cancellationToken);
    }

    public Task<bool> SendChunk(byte[] chunk, bool isFinal, CancellationToken cancellationToken = default)
    {
        return this._httpResponse.SendChunk(chunk, isFinal, cancellationToken);
    }
}