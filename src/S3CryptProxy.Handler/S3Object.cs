namespace S3CryptProxy.Handler;

using System;
using System.IO;
using System.Text;
using S3CryptProxy.Models;
using S3CryptProxy.Shared;

/// <summary>
/// S3 object.
/// </summary>
public class S3Object : IDisposable
{
    #region Public-Members

    /// <summary>
    /// Object key.
    /// </summary>
    public string Key { get; set; } = null;

    /// <summary>
    /// Version ID.
    /// </summary>
    public string VersionId { get; set; } = null;

    /// <summary>
    /// Indicates if this version is the latest version for the object.
    /// </summary>
    public bool IsLatest { get; set; } = true;

    /// <summary>
    /// Timestamp from the last modification of the resource.
    /// </summary>
    public DateTime LastModified
    {
        get => this._lastModified;
        set => this._lastModified = DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }

    /// <summary>
    /// ETag of the resource.
    /// </summary>
    public string ETag
    {
        get
        {
            return this._eTag;
        }
        set
        {
            if (!string.IsNullOrEmpty(value))
            {
                value = value.Trim();
                if (!value.StartsWith("\"")) value = "\"" + value;
                if (!value.EndsWith("\"")) value = value + "\"";
            }

            this._eTag = value;
        }
    }

    /// <summary>
    /// Content type.
    /// </summary>
    public string ContentType { get; set; } = Constants.ContentTypeOctetStream;

    /// <summary>
    /// The size in bytes of the resource.
    /// </summary>
    public long Size
    {
        get
        {
            return this._size;
        }
        set
        {
            if (value < 0) throw new ArgumentOutOfRangeException(nameof(this.Size));
            this._size = value;
        }
    }

    /// <summary>
    /// The class of storage where the resource resides.
    /// Valid values are STANDARD, REDUCED_REDUNDANCY, GLACIER, STANDARD_IA, ONEZONE_IA, INTELLIGENT_TIERING, DEEP_ARCHIVE, OUTPOSTS.
    /// </summary>
    public StorageClassEnum? StorageClass { get; set; } = StorageClassEnum.Standard;

    /// <summary>
    /// Object owner.
    /// </summary>
    public Owner Owner { get; set; } = new Owner();

    /// <summary>
    /// Restore status for archived objects, returned through the x-amz-restore response header.
    /// </summary>
    public RestoreStatus RestoreStatus { get; set; } = null;

    /// <summary>
    /// Stream containing data.
    /// </summary>
    public Stream Data
    {
        get
        {
            if (this._dataStream != null) return this._dataStream;
            if (this._dataBytes != null)
            {
                this._dataStream = new MemoryStream();
                this._dataStream.Write(this._dataBytes, 0, this._dataBytes.Length);
                this._dataStream.Seek(0, SeekOrigin.Begin);
                return this._dataStream;
            }
            return null;
        }
        set
        {
            this._dataStream = null;
            this._dataBytes = null;

            if (value != null) this._dataStream = value;
        }
    }

    /// <summary>
    /// Data in byte array form.  If the data was supplied as a stream, the stream will be fully read.  If the stream supports seeking, it will automatically seek to the beginning.
    /// </summary>
    public byte[] DataBytes
    {
        get
        {
            if (this._dataBytes != null) return this._dataBytes;
            if (this._dataStream != null)
            {
                this._dataBytes = ReadFromStream(this._dataStream, this.Size, 65536);
                if (this._dataStream.CanSeek) this._dataStream.Seek(0, SeekOrigin.Begin);
                return this._dataBytes;
            }
            return null;
        }
        set
        {
            this._dataStream = null;
            this._dataBytes = null;

            if (value != null)
            {
                this._dataBytes = new byte[value.Length];
                Buffer.BlockCopy(value, 0, this._dataBytes, 0, value.Length);

                this._dataStream = new MemoryStream();
                this._dataStream.Write(this._dataBytes, 0, this._dataBytes.Length);
                this._dataStream.Seek(0, SeekOrigin.Begin);
            }
        }
    }

    /// <summary>
    /// Data in string form.  If the data was supplied as a stream, the stream will be fully read.  If the stream supports seeking, it will automatically seek to the beginning.
    /// </summary>
    public string DataString
    {
        get
        {
            if (this.DataBytes != null) return Encoding.UTF8.GetString(this.DataBytes);
            return null;
        }
        set
        {
            this._dataStream = null;
            this._dataBytes = null;

            if (!string.IsNullOrEmpty(value))
            {
                byte[] bytes = Encoding.UTF8.GetBytes(value);
                this._dataBytes = new byte[bytes.Length];
                Buffer.BlockCopy(bytes, 0, this._dataBytes, 0, value.Length);

                this._dataStream = new MemoryStream();
                this._dataStream.Write(this._dataBytes, 0, this._dataBytes.Length);
                this._dataStream.Seek(0, SeekOrigin.Begin);
            }
        }
    }

    #endregion

    #region Private-Members

    private bool _disposed = false;
    private Stream _dataStream = null;
    private byte[] _dataBytes = null;
    private long _size = 0;
    private string _eTag = null;
    private DateTime _lastModified = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);

    #endregion

    #region Constructors-and-Factories

    /// <summary>
    /// Instantiate.
    /// </summary>
    public S3Object()
    {

    }

    /// <summary>
    /// Instantiate.
    /// </summary>
    /// <param name="key">Key.</param>
    /// <param name="versionId">Version ID.</param>
    /// <param name="isLatest">Is latest.</param>
    /// <param name="lastModified">Last modified.</param>
    /// <param name="etag">ETag.</param>
    /// <param name="size">Size.</param>
    /// <param name="owner">Owner.</param>
    /// <param name="data">Stream containing data.</param>
    /// <param name="contentType">Content type.</param>
    /// <param name="storageClass">Storage class.</param>
    public S3Object(string key, string versionId, bool isLatest, DateTime lastModified, string etag, long size, Owner owner, Stream data, string contentType = "application/octet-stream", StorageClassEnum storageClass = StorageClassEnum.Standard)
    {
        this.Key = key;
        this.VersionId = versionId;
        this.IsLatest = isLatest;
        this.LastModified = lastModified;
        this.ETag = etag;
        this.ContentType = contentType;
        this.Size = size;
        this.StorageClass = storageClass;
        this.Owner = owner;
        this.Data = data;
    }

    /// <summary>
    /// Instantiate.
    /// </summary>
    /// <param name="key">Key.</param>
    /// <param name="versionId">Version ID.</param>
    /// <param name="isLatest">Is latest.</param>
    /// <param name="lastModified">Last modified.</param>
    /// <param name="etag">ETag.</param>
    /// <param name="size">Size.</param>
    /// <param name="owner">Owner.</param>
    /// <param name="data">Stream containing data.</param>
    /// <param name="contentType">Content type.</param>
    /// <param name="storageClass">Storage class.</param>
    public S3Object(string key, string versionId, bool isLatest, DateTime lastModified, string etag, long size, Owner owner, byte[] data, string contentType = "application/octet-stream", StorageClassEnum storageClass = StorageClassEnum.Standard)
    {
        this.Key = key;
        this.VersionId = versionId;
        this.IsLatest = isLatest;
        this.LastModified = lastModified;
        this.ETag = etag;
        this.ContentType = contentType;
        this.Size = size;
        this.StorageClass = storageClass;
        this.Owner = owner;
        this.DataBytes = data;
    }

    /// <summary>
    /// Instantiate.
    /// </summary>
    /// <param name="key">Key.</param>
    /// <param name="versionId">Version ID.</param>
    /// <param name="isLatest">Is latest.</param>
    /// <param name="lastModified">Last modified.</param>
    /// <param name="etag">ETag.</param>
    /// <param name="size">Size.</param>
    /// <param name="owner">Owner.</param>
    /// <param name="data">Stream containing data.</param>
    /// <param name="contentType">Content type.</param>
    /// <param name="storageClass">Storage class.</param>
    public S3Object(string key, string versionId, bool isLatest, DateTime lastModified, string etag, long size, Owner owner, string data, string contentType = "application/octet-stream", StorageClassEnum storageClass = StorageClassEnum.Standard)
    {
        this.Key = key;
        this.VersionId = versionId;
        this.IsLatest = isLatest;
        this.LastModified = lastModified;
        this.ETag = etag;
        this.ContentType = contentType;
        this.Size = size;
        this.StorageClass = storageClass;
        this.Owner = owner;
        this.DataString = data;
    }

    #endregion

    #region Public-Methods

    /// <summary>
    /// Dispose.
    /// </summary>
    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion

    #region Private-Methods

    /// <summary>
    /// Dispose of resources.
    /// </summary>
    /// <param name="disposing">Disposing.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (this._disposed) return;

        if (disposing)
        {
            if (this._dataStream != null)
            {
                this._dataStream.Dispose();
                this._dataStream = null;
            }
        }

        this._disposed = true;
    }

    private static byte[] ReadFromStream(Stream stream, long count, int bufferLen)
    {
        if (count <= 0) return null;
        if (bufferLen <= 0) throw new ArgumentException("Buffer must be greater than zero bytes.");
        byte[] buffer = new byte[bufferLen];

        int read = 0;
        long bytesRemaining = count;

        using (MemoryStream ms = new MemoryStream())
        {
            while (bytesRemaining > 0)
            {
                if (bufferLen > bytesRemaining) buffer = new byte[bytesRemaining];

                read = stream.Read(buffer, 0, buffer.Length);
                if (read > 0)
                {
                    ms.Write(buffer, 0, read);
                    bytesRemaining -= read;
                }
                else
                {
                    throw new IOException("Could not read from supplied stream.");
                }
            }

            return ms.ToArray();
        }
    }

    #endregion
}