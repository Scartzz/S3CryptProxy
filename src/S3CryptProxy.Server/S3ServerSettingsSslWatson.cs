namespace S3CryptProxy.Server;

using System;
using S3CryptProxy.Handler.Interfaces;
using WatsonWebserver.Core.Settings;

internal class S3ServerSettingsSslWatson : IS3ServerSettingsSsl
{
    internal SslSettings SslSettings { get; }

    public S3ServerSettingsSslWatson(SslSettings sslSettings)
    {
        ArgumentNullException.ThrowIfNull(sslSettings);
        this.SslSettings = sslSettings;
    }

    public bool Enable
    {
        get => this.SslSettings.Enable;
        set => this.SslSettings.Enable = value;
    }
}