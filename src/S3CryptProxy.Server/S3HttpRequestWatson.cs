namespace S3CryptProxy.Server;

using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using S3CryptProxy.Handler.Interfaces;
using WatsonWebserver.Core;

internal class S3HttpRequestWatson : IS3HttpRequest
{
    private readonly HttpRequestBase _httpRequest;

    public S3HttpRequestWatson(HttpRequestBase httpRequest)
    {
        ArgumentNullException.ThrowIfNull(httpRequest);
        this._httpRequest = httpRequest;
    }
    
    public long ContentLength
    {
        get => this._httpRequest.ContentLength;
        set => this._httpRequest.ContentLength = value;
    }
    public bool ChunkedTransfer
    {
        get => this._httpRequest.ChunkedTransfer;
        set => this._httpRequest.ChunkedTransfer = value;
    }
    public string UrlFull
    {
        get => this._httpRequest.Url.Full;
        set => this._httpRequest.Url.Full = value;
    }
    public string UrlRawWithoutQuery
    {
        get => this._httpRequest.Url.RawWithoutQuery;
    }
    public string UrlRawWithQuery
    {
        get => this._httpRequest.Url.RawWithQuery;
    }
    public Stream Data
    {
        get => this._httpRequest.Data;
        set => this._httpRequest.Data = value;
    }
    public NameValueCollection Headers
    {
        get => this._httpRequest.Headers;
        set => this._httpRequest.Headers = value;
    }
    public NameValueCollection QueryElements
    {
        get => this._httpRequest.Query?.Elements;
    }
    public string DataAsString
    {
        get => this._httpRequest.DataAsString;
    }
    public byte[] DataAsBytes
    {
        get => this._httpRequest.DataAsBytes;
    }

    public string DestinationHostname
    {
        get => this._httpRequest.Destination.Hostname;
        set => this._httpRequest.Destination.Hostname = value;
    }

    public System.Net.Http.HttpMethod Method
    {
        get
        {
            if (this._httpRequest is null)
                throw new NullReferenceException("HttpRequest is null.");
            return this._httpRequest.Method switch
            {
                HttpMethod.GET => System.Net.Http.HttpMethod.Get,
                HttpMethod.HEAD => System.Net.Http.HttpMethod.Head,
                HttpMethod.PUT => System.Net.Http.HttpMethod.Put,
                HttpMethod.POST => System.Net.Http.HttpMethod.Post,
                HttpMethod.DELETE => System.Net.Http.HttpMethod.Delete,
                HttpMethod.PATCH => System.Net.Http.HttpMethod.Patch,
                HttpMethod.CONNECT => System.Net.Http.HttpMethod.Connect,
                HttpMethod.OPTIONS => System.Net.Http.HttpMethod.Options,
                HttpMethod.TRACE => System.Net.Http.HttpMethod.Trace,
                HttpMethod.UNKNOWN => new System.Net.Http.HttpMethod("UNKNOWN"),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }

    public bool HeaderExists(string key) => this._httpRequest.HeaderExists(key);
    public bool QuerystringExists(string key) => this._httpRequest.QuerystringExists(key);
    public string RetrieveHeaderValue(string key) => this._httpRequest.RetrieveHeaderValue(key);
    public string RetrieveQueryValue(string key) => this._httpRequest.RetrieveQueryValue(key);
    public async Task<IS3HttpRequestChunk> ReadChunk() => new S3HttpRequestChunkWatson(await this._httpRequest.ReadChunk());
}