namespace S3CryptProxy.Models;

using System;
using System.Collections.Generic;
using System.Xml.Serialization;

/// <summary>
/// List multipart uploads.
/// </summary>
[XmlRoot(ElementName = "ListMultipartUploadsResult")]
public class ListMultipartUploadsResult
{
    // Namespace = "http://s3.amazonaws.com/doc/2006-03-01/"

    #region Public-Members

    /// <summary>
    /// Bucket.
    /// </summary>
    [XmlElement(ElementName = "Bucket", IsNullable = false)]
    public string Bucket { get; set; } = null;

    /// <summary>
    /// Key marker.
    /// </summary>
    [XmlElement(ElementName = "KeyMarker")]
    public string KeyMarker { get; set; } = null;

    /// <summary>
    /// Upload ID marker.
    /// </summary>
    [XmlElement(ElementName = "UploadIdMarker")]
    public string UploadIdMarker { get; set; } = null;

    /// <summary>
    /// Next key marker.
    /// </summary>
    [XmlElement(ElementName = "NextKeyMarker")]
    public string NextKeyMarker { get; set; } = null;

    /// <summary>
    /// Prefix.
    /// </summary>
    [XmlElement(ElementName = "Prefix")]
    public string Prefix { get; set; } = null;

    /// <summary>
    /// Delimiter.
    /// </summary>
    [XmlElement(ElementName = "Delimiter")]
    public string Delimiter { get; set; } = null;

    /// <summary>
    /// Next upload ID marker.
    /// </summary>
    [XmlElement(ElementName = "NextUploadIdMarker")]
    public string NextUploadIdMarker { get; set; } = null;

    /// <summary>
    /// Max uploads.
    /// </summary>
    [XmlElement(ElementName = "MaxUploads", IsNullable = false)]
    public int MaxUploads
    {
        get
        {
            return this._maxUploads;
        }
        set
        {
            if (value < 1) throw new ArgumentOutOfRangeException(nameof(this.MaxUploads));
            this._maxUploads = value;
        }
    }

    /// <summary>
    /// Flag indicating if the results are truncated.
    /// </summary>
    [XmlElement(ElementName = "IsTruncated", IsNullable = false)]
    public bool IsTruncated { get; set; } = false;

    /// <summary>
    /// Uploads.
    /// </summary>
    [XmlElement(ElementName = "Upload")]
    public List<Upload> Uploads
    {
        get
        {
            return this._uploads;
        }
        set
        {
            if (value == null) this._uploads = new List<Upload>();
            else this._uploads = value;
        }
    }

    /// <summary>
    /// Common prefixes.
    /// </summary>
    [XmlElement(ElementName = "CommonPrefixes")]
    public List<CommonPrefixes> CommonPrefixes
    {
        get
        {
            return this._commonPrefixes;
        }
        set
        {
            this._commonPrefixes = value;
        }
    }

    /// <summary>
    /// Encoding type used to encode object key names in the XML response.
    /// Valid value is "url" for URL encoding.
    /// </summary>
    [XmlElement(ElementName = "EncodingType")]
    public string EncodingType { get; set; } = null;

    #endregion

    #region Private-Members

    private int _maxUploads = 1000;
    private List<Upload> _uploads = new List<Upload>();
    private List<CommonPrefixes> _commonPrefixes = new List<CommonPrefixes>();

    #endregion

    #region Constructors-and-Factories

    /// <summary>
    /// Instantiate.
    /// </summary>
    public ListMultipartUploadsResult()
    {

    }

    /// <summary>
    /// Instantiate.
    /// </summary>
    /// <param name="bucket">Bucket.</param>
    /// <param name="key">Key.</param>
    /// <param name="uploadId">Upload ID.</param>
    public ListMultipartUploadsResult(string bucket, string key, string uploadId)
    {
        if (String.IsNullOrEmpty(bucket)) throw new ArgumentNullException(nameof(bucket));
        if (String.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));
        if (String.IsNullOrEmpty(uploadId)) throw new ArgumentNullException(nameof(uploadId));
    }

    #endregion

    #region Public-Methods

    /// <summary>
    /// Helper method for XML serialization.
    /// </summary>
    /// <returns>Boolean.</returns>
    public bool ShouldSerializeUploads()
    {
        return this._uploads != null && this._uploads.Count > 0;
    }

    /// <summary>
    /// Helper method for XML serialization.
    /// </summary>
    /// <returns>Boolean.</returns>
    public bool ShouldSerializeCommonPrefixes()
    {
        return this._commonPrefixes != null && this._commonPrefixes.Count > 0;
    }

    /// <summary>
    /// Helper method for XML serialization.
    /// </summary>
    /// <returns>Boolean.</returns>
    public bool ShouldSerializeEncodingType()
    {
        return !String.IsNullOrEmpty(this.EncodingType);
    }

    #endregion

    #region Private-Methods

    #endregion
}