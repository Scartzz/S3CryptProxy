namespace S3CryptProxy.Models;

using System;
using System.Xml.Serialization;

/// <summary>
/// Object metadata.
/// </summary>
[XmlRoot(ElementName = "Contents")]
public class ObjectMetadata
{
    // Namespace = "http://s3.amazonaws.com/doc/2006-03-01/"

    #region Public-Members

    /// <summary>
    /// Object key.
    /// </summary>
    [XmlElement(ElementName = "Key")]
    public string Key { get; set; } = null;

    /// <summary>
    /// Timestamp from the last modification of the resource.
    /// </summary>
    [XmlElement(ElementName = "LastModified")]
    public DateTime LastModified
    {
        get => this._lastModified;
        set => this._lastModified = DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }

    /// <summary>
    /// ETag of the resource.
    /// </summary>
    [XmlElement(ElementName = "ETag")]
    public string ETag
    {
        get
        {
            return this._eTag;
        }
        set
        {
            if (!String.IsNullOrEmpty(value))
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
    [XmlIgnore]
    public string ContentType { get; set; } = "application/octet-stream";

    /// <summary>
    /// The size in bytes of the resource.
    /// </summary>
    [XmlElement(ElementName = "Size")]
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
    /// </summary>
    [XmlElement(ElementName = "StorageClass")]
    public string StorageClass { get; set; } = "STANDARD";

    /// <summary>
    /// Object owner.
    /// </summary>
    [XmlElement(ElementName = "Owner")]
    public Owner Owner { get; set; } = new Owner();

    /// <summary>
    /// Restore status for archived objects, returned through the x-amz-restore response header.
    /// </summary>
    [XmlIgnore]
    public RestoreStatus RestoreStatus { get; set; } = null;

    #endregion

    #region Private-Members

    private long _size = 0;
    private string _eTag = null;
    private DateTime _lastModified = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);

    #endregion

    #region Constructors-and-Factories

    /// <summary>
    /// Instantiate.
    /// </summary>
    public ObjectMetadata()
    {

    }

    /// <summary>
    /// Instantiate.
    /// </summary>
    /// <param name="key">Key.</param>
    /// <param name="lastModified">Last modified.</param>
    /// <param name="eTag">ETag.</param>
    /// <param name="size">Size.</param>
    /// <param name="owner">Owner.</param>
    /// <param name="storageClass">Storage class.</param>
    public ObjectMetadata(string key, DateTime lastModified, string eTag, long size, Owner owner, string storageClass = "STANDARD")
    {
        this.Key = key;
        this.LastModified = lastModified;
        this.ETag = eTag;
        this.Size = size;
        this.StorageClass = storageClass;
        this.Owner = owner;
    }

    #endregion

    #region Public-Methods

    #endregion

    #region Private-Methods

    #endregion
}