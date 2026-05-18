namespace S3CryptProxy.Server;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using S3CryptProxy.Handler.Interfaces;
using WatsonWebserver.Core;
using WatsonWebserver.Core.Settings;

internal class S3ServerSettingsWatson : IS3ServerSettings
{
    internal WebserverSettings Settings { get; }

    public S3ServerSettingsWatson(WebserverSettings webserverSettings)
    {
        ArgumentNullException.ThrowIfNull(webserverSettings);
        this.Settings = webserverSettings;
    }

    public IS3Server CreateServer(Func<IS3HttpContext,Task> handler)
    {
        return new S3ServerWatson(this, handler);
    }

    public string Hostname 
    {
        get => this.Settings.Hostname;
        set => this.Settings.Hostname = value;
    }
    public int Port
    {
        get => this.Settings.Port;
        set => this.Settings.Port = value;
    }
    public IS3ServerSettingsSsl Ssl
    {
        get => new S3ServerSettingsSslWatson(this.Settings.Ssl);
        set => this.Settings.Ssl = ((S3ServerSettingsSslWatson)value).SslSettings;
    }
    public Dictionary<string, string> DefaultHeaders
    {
        get => this.Settings.Headers?.DefaultHeaders;
        set
        {
            this.Settings.Headers ??= new HeaderSettings();
            this.Settings.Headers.DefaultHeaders = value;
        }
    }
}