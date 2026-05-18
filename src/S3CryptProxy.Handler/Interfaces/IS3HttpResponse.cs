namespace S3CryptProxy.Handler.Interfaces;

using System.Collections.Specialized;
using System.IO;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

public interface IS3HttpResponse
{
    public int StatusCode { get; set; }
    public string ContentType { get; set; }
    public NameValueCollection Headers { get; set; }
    public long ContentLength { get; set; }
    public bool ChunkedTransfer { get; set; }
    [JsonIgnore]
    public Stream Data { get; }
    [JsonIgnore]
    public string DataAsString { get; }
    [JsonIgnore]
    public byte[] DataAsBytes { get; }
    Task<bool> Send(CancellationToken cancellationToken = default);
    Task<bool> Send(byte[] data, CancellationToken cancellationToken = default);
    Task<bool> Send(long contentLength, Stream stream, CancellationToken cancellationToken = default);
    Task<bool> SendChunk(byte[] chunk, bool isFinal, CancellationToken cancellationToken = default);
}