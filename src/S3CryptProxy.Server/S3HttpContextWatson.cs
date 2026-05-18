namespace S3CryptProxy.Server;

using System;
using S3CryptProxy.Handler.Interfaces;
using WatsonWebserver.Core;

internal class S3HttpContextWatson : IS3HttpContext
{
    private readonly HttpContextBase _ctx;

    public S3HttpContextWatson(HttpContextBase ctx)
    {
        ArgumentNullException.ThrowIfNull(ctx);
        this._ctx = ctx;
    }

    public void Dispose()
    {
        this._ctx?.Dispose();
    }

    public IS3HttpRequest Request => this._ctx.Request is null ? null : new S3HttpRequestWatson(this._ctx.Request);
    public IS3HttpResponse Response => this._ctx.Response is null ? null : new S3HttpResponseWatson(this._ctx.Response);
}