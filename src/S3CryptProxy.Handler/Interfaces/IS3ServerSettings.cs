namespace S3CryptProxy.Handler.Interfaces;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IS3ServerSettings
{
    public string Hostname { get; set; }
    public int Port { get; set; }
    public IS3ServerSettingsSsl Ssl { get; set; }
    public Dictionary<string, string> DefaultHeaders { get; set; }
    public IS3Server CreateServer(Func<IS3HttpContext, Task> handler);
}