namespace S3CryptProxy.Models;

using System;
using System.Xml.Serialization;

/// <summary>
/// A part from a multipart upload.
/// </summary>
[XmlRoot(ElementName = "Part")]
public class Part
{
    // Namespace = "http://s3.amazonaws.com/doc/2006-03-01/"

    #region Public-Members

    /// <summary>
    /// Checksum from CRC32.
    /// </summary>
    [XmlElement(ElementName = "ChecksumCRC32")]
    public string ChecksumCrc32 { get; set; } = null;

    /// <summary>
    /// Checksum from CRC32C.
    /// </summary>
    [XmlElement(ElementName = "ChecksumCRC32C")]
    public string ChecksumCrc32C { get; set; } = null;

    /// <summary>
    /// Checksum from SHA1.
    /// </summary>
    [XmlElement(ElementName = "ChecksumSHA1")]
    public string ChecksumSha1 { get; set; } = null;

    /// <summary>
    /// Checksum from SHA256.
    /// </summary>
    [XmlElement(ElementName = "ChecksumSHA256")]
    public string ChecksumSha256 { get; set; } = null;

    /// <summary>
    /// ETag.
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
    /// Timestamp from the last modification of the resource.
    /// </summary>
    [XmlElement(ElementName = "LastModified")]
    public DateTime LastModified
    {
        get => this._lastModified;
        set => this._lastModified = DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }

    /// <summary>
    /// Part number.
    /// </summary>
    [XmlElement(ElementName = "PartNumber", IsNullable = false)]
    public int PartNumber
    {
        get
        {
            return this._partNumber;
        }
        set
        {
            if (value < 0) throw new ArgumentOutOfRangeException(nameof(this.PartNumber));
            this._partNumber = value;
        }
    }

    /// <summary>
    /// Size.
    /// </summary>
    [XmlElement(ElementName = "Size", IsNullable = false)]
    public int Size
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

    #endregion

    #region Private-Members

    private string _eTag = null;
    private int _partNumber = 0;
    private int _size = 0;
    private DateTime _lastModified = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);

    #endregion

    #region Constructors-and-Factories

    /// <summary>
    /// Instantiate.
    /// </summary>
    public Part()
    {

    }

    #endregion

    #region Public-Methods

    /// <summary>
    /// Helper method for XML serialization.
    /// </summary>
    /// <returns>Boolean.</returns>
    public bool ShouldSerializeChecksumCrc32()
    {
        return !String.IsNullOrEmpty(this.ChecksumCrc32);
    }

    /// <summary>
    /// Helper method for XML serialization.
    /// </summary>
    /// <returns>Boolean.</returns>
    public bool ShouldSerializeChecksumCrc32C()
    {
        return !String.IsNullOrEmpty(this.ChecksumCrc32C);
    }

    /// <summary>
    /// Helper method for XML serialization.
    /// </summary>
    /// <returns>Boolean.</returns>
    public bool ShouldSerializeChecksumSha1()
    {
        return !String.IsNullOrEmpty(this.ChecksumSha1);
    }

    /// <summary>
    /// Helper method for XML serialization.
    /// </summary>
    /// <returns>Boolean.</returns>
    public bool ShouldSerializeChecksumSha256()
    {
        return !String.IsNullOrEmpty(this.ChecksumSha256);
    }

    #endregion

    #region Private-Methods

    #endregion
}