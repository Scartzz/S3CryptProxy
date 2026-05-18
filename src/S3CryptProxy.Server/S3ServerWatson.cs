namespace S3CryptProxy.Server;

using System;
using System.Threading.Tasks;
using S3CryptProxy.Handler.Interfaces;
using WatsonWebserver;
using WatsonWebserver.Core;

internal class S3ServerWatson : IS3Server
{
    private readonly IS3ServerSettings _settings;
    private readonly Func<IS3HttpContext, Task> _handler;
    private readonly WebserverBase _webserver;
    
    public S3ServerWatson(IS3ServerSettings settings, Func<IS3HttpContext,Task> handler)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(handler);
        this._settings = settings;
        this._handler = handler;
        this._webserver = new Webserver(((S3ServerSettingsWatson)settings).Settings, this.DefaultRoute);
    }

    private async Task DefaultRoute(HttpContextBase arg)
    {
        await this._handler(new S3HttpContextWatson(arg));
    }

    public bool IsListening => this._webserver?.IsListening ?? false;

    public void Start()
    {
        this._webserver.Start();
    }
    public void Stop()
    {
        this._webserver.Stop();
    }

    public void Dispose()
    {
        this._webserver.Dispose();
    }
}