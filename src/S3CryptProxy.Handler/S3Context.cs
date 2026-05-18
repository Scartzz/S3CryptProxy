namespace S3CryptProxy.Handler;

using System;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using S3CryptProxy.Handler.Interfaces;
using Timestamps;

/// <summary>
/// S3 context.
/// </summary>
public class S3Context : IDisposable
{
    #region Public-Members

    /// <summary>
    /// Time information for start, end, and total runtime.
    /// </summary>
    public Timestamp Timestamp { get; set; } = new Timestamp();

    /// <summary>
    /// S3 request.
    /// </summary>
    public S3Request Request { get; private set; } = null;

    /// <summary>
    /// S3 response.
    /// </summary>
    public S3Response Response
    {
        get
        {
            return this._response;
        }
        set
        {
            if (value == null) throw new ArgumentNullException(nameof(this.Response));
            this._response = value;
        }
    }

    /// <summary>
    /// User metadata, supplied by your application.
    /// </summary>
    public object Metadata { get; set; } = null;

    /// <summary>
    /// HTTP context from which the S3 context was created.
    /// </summary>
    [JsonPropertyOrder(999)]
    public IS3HttpContext Http { get; private set; } = null;

    #endregion

    #region Private-Members

    private S3Response _response = null;
    private bool _disposed = false;

    #endregion

    #region Constructors-and-Factories

    /// <summary>
    /// Instantiate.
    /// </summary>
    public S3Context()
    {

    }

    /// <summary>
    /// Instantiate.
    /// </summary>
    /// <param name="ctx">HTTP context.</param>
    /// <param name="baseDomainFinder">Callback to invoke to find a base domain for a given hostname, used with virtual hosted style URLs.</param> 
    /// <param name="metadata">User metadata, provided by your application.</param>
    /// <param name="logger">Method to invoke to send log messages.</param>
    public S3Context(IS3HttpContext ctx, ILoggerFactory loggerFactory = null)
    {
        if (ctx == null) throw new ArgumentNullException(nameof(ctx));

        this.Metadata = null;
        this.Http = ctx;
        this.Request = new S3Request(this, loggerFactory);
        this.Response = new S3Response(this);
    }

    #endregion

    #region Public-Methods

    /// <summary>
    /// Dispose.
    /// </summary>
    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion

    #region Private-Methods

    /// <summary>
    /// Dispose of resources.
    /// </summary>
    /// <param name="disposing">Disposing.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (this._disposed) return;
        if (disposing)
        {
            this.Http?.Dispose();
        }
        this._disposed = true;
    }

    #endregion
}