namespace S3CryptProxy.Handler.Interfaces;

using System.Collections.Specialized;
using System.IO;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

public interface IS3HttpRequest
{
    public long ContentLength { get; set; }
    public bool ChunkedTransfer { get; set; }
    public NameValueCollection Headers { get; set; }
    public NameValueCollection QueryElements { get; }
    public string UrlFull { get; set; }
    public string UrlRawWithoutQuery { get; }
    public string UrlRawWithQuery { get; }
    [JsonIgnore]
    public Stream Data { get; set; }
    [JsonIgnore]
    public string DataAsString { get; }
    public string DestinationHostname { get; set; }
    public HttpMethod Method { get; }
    [JsonIgnore]
    public byte[] DataAsBytes { get; }
    bool HeaderExists(string key);
    bool QuerystringExists(string key);
    string RetrieveHeaderValue(string key);
    string RetrieveQueryValue(string key);
    Task<IS3HttpRequestChunk> ReadChunk();
}