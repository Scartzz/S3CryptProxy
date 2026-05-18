namespace S3CryptProxy.Handler.Interfaces;

public interface IS3HttpRequestChunk
{
    public int Length { get; set; }
    public bool IsFinal { get; set; }
    public byte[] Data { get; set; }
}