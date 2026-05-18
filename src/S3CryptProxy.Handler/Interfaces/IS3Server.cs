namespace S3CryptProxy.Handler.Interfaces;

using System;

public interface IS3Server : IDisposable
{
    public bool IsListening { get; }
    public void Start();
    public void Stop();
}