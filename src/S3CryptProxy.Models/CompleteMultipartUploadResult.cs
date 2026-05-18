namespace S3CryptProxy.Models;

using System;
using System.Xml.Serialization;

/// <summary>
/// Complete multipart upload result.
/// </summary>
[XmlRoot(ElementName = "CompleteMultipartUploadResult")]
public class CompleteMultipartUploadResult
{
    // Namespace = "http://s3.amazonaws.com/doc/2006-03-01/"

    #region Public-Members

    /// <summary>
    /// Location.
    /// </summary>
    [XmlElement(ElementName = "Location")]
    public string Location { get; set; } = null;

    /// <summary>
    /// Bucket.
    /// </summary>
    [XmlElement(ElementName = "Bucket")]
    public string Bucket { get; set; } = null;

    /// <summary>
    /// Key.
    /// </summary>
    [XmlElement(ElementName = "Key")]
    public string Key { get; set; } = null;

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

    #endregion

    #region Private-Members

    private string _eTag = null;

    #endregion

    #region Constructors-and-Factories

    /// <summary>
    /// Instantiate.
    /// </summary>
    public CompleteMultipartUploadResult()
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