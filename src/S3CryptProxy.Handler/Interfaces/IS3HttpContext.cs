namespace S3CryptProxy.Handler.Interfaces;

using System;

public interface IS3HttpContext : IDisposable
{
    public IS3HttpRequest Request { get; }
    public IS3HttpResponse Response { get; }
}