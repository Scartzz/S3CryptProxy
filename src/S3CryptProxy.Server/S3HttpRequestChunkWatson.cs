namespace S3CryptProxy.Server;

using System;
using S3CryptProxy.Handler.Interfaces;
using WatsonWebserver.Core;

internal class S3HttpRequestChunkWatson : IS3HttpRequestChunk
{
    private readonly Chunk _chunk;

    public S3HttpRequestChunkWatson(Chunk chunk)
    {
        ArgumentNullException.ThrowIfNull(chunk);
        this._chunk = chunk;
    }
    
    public int Length
    {
        get => this._chunk.Length;
        set => this._chunk.Length = value;
    }
    public bool IsFinal
    {
        get => this._chunk.IsFinal;
        set => this._chunk.IsFinal = value;
    }
    public byte[] Data
    {
        get => this._chunk.Data;
        set => this._chunk.Data = value;
    }
}