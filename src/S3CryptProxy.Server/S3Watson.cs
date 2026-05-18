namespace S3CryptProxy.Server;

using S3CryptProxy.Handler.Interfaces;
using WatsonWebserver.Core;

public class S3Watson
{
    public static IS3ServerSettings Create(string hostname, int port, bool ssl = false)
    {
        return new S3ServerSettingsWatson(new WebserverSettings(hostname, port, ssl));
    }
}