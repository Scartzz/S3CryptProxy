namespace S3CryptProxy.Models;

using System;
using System.Collections.Generic;
using System.Xml.Serialization;

/// <summary>
/// Result from a ListBucket operation.
/// </summary>
[XmlRoot(ElementName = "ListBucketResult", IsNullable = true)]
public class ListBucketResult
{
    // Namespace = "http://s3.amazonaws.com/doc/2006-03-01/"

    #region Public-Members

    /// <summary>
    /// Name of the bucket.
    /// </summary>
    [XmlElement(ElementName = "Name")]
    public string Name { get; set; } = null;

    /// <summary>
    /// Prefix specified in the request.
    /// </summary>
    [XmlElement(ElementName = "Prefix")]
    public string Prefix
    {
        get
        {
            return this._prefix;
        }
        set
        {
            if (String.IsNullOrEmpty(value)) this._prefix = "";
            else this._prefix = value;
        }
    }

    /// <summary>
    /// Marker.
    /// </summary>
    [XmlElement(ElementName = "Marker")]
    public string Marker { get; set; } = null;

    /// <summary>
    /// Number of keys.
    /// </summary>
    [XmlElement(ElementName = "KeyCount")]
    public int KeyCount
    {
        get
        {
            return this._keyCount;
        }
        set
        {
            if (value < 0) throw new ArgumentOutOfRangeException(nameof(this.KeyCount));
            this._keyCount = value;
        }
    }

    /// <summary>
    /// Maximum number of keys.
    /// </summary>
    [XmlElement(ElementName = "MaxKeys")]
    public int MaxKeys
    {
        get
        {
            return this._maxKeys;
        }
        set
        {
            if (value < 0) throw new ArgumentOutOfRangeException(nameof(this.MaxKeys));
            this._maxKeys = value;
        }
    }

    /// <summary>
    /// Delimiter.
    /// </summary>
    [XmlElement(ElementName = "Delimiter")]
    public string Delimiter
    {
        get
        {
            return this._delimiter;
        }
        set
        {
            this._delimiter = value;
        }
    }

    /// <summary>
    /// Encoding type.
    /// </summary>
    [XmlElement(ElementName = "EncodingType")]
    public string EncodingType { get; set; } = null;

    /// <summary>
    /// Indicates if the response is truncated.
    /// </summary>
    [XmlElement(ElementName = "IsTruncated")]
    public bool IsTruncated { get; set; } = false;

    /// <summary>
    /// The next continuation token to supply to continue the query.
    /// </summary>
    [XmlElement(ElementName = "NextContinuationToken")]
    public string NextContinuationToken { get; set; } = null;

    /// <summary>
    /// The continuation token to supply to continue the query.
    /// </summary>
    [XmlElement(ElementName = "ContinuationToken")]
    public string ContinuationToken { get; set; } = null;

    /// <summary>
    /// The next marker to supply to continue the query.
    /// </summary>
    [XmlElement(ElementName = "NextMarker")]
    public string NextMarker { get; set; } = null;

    /// <summary>
    /// Bucket contents.
    /// </summary>
    [XmlElement(ElementName = "Contents")]
    public List<ObjectMetadata> Contents { get; set; } = new List<ObjectMetadata>();

    /// <summary>
    /// Common prefixes.
    /// </summary>
    [XmlElement(ElementName = "CommonPrefixes")]
    public List<CommonPrefixes> CommonPrefixes { get; set; } = new List<CommonPrefixes>();

    /// <summary>
    /// Bucket region string.  Not included in the XML, but rather as the HTTP header x-amz-bucket-region.
    /// </summary>
    [XmlIgnore]
    public string BucketRegion { get; set; } = "us-west-1";

    #endregion

    #region Private-Members

    private int _keyCount = 0;
    private int _maxKeys = 1000;
    private string _prefix = "";
    private string _delimiter = null;

    #endregion

    #region Constructors-and-Factories

    /// <summary>
    /// Instantiate.
    /// </summary>
    public ListBucketResult()
    {

    }

    /// <summary>
    /// Instantiate.
    /// </summary>
    /// <param name="name">Name.</param>
    /// <param name="contents">Contents.</param>
    /// <param name="keyCount">Key count.</param>
    /// <param name="maxKeys">Max keys.</param>
    /// <param name="prefix">Prefix.</param>
    /// <param name="marker">Marker.</param>
    /// <param name="delimiter">Delimiter.</param>
    /// <param name="isTruncated">Is truncated.</param>
    /// <param name="nextToken">Next continuation token.</param>
    /// <param name="prefixes">Prefixes</param>
    /// <param name="bucketRegion">Bucket region.</param>
    public ListBucketResult(
        string name,
        List<ObjectMetadata> contents,
        int keyCount,
        int maxKeys,
        string prefix = null,
        string marker = null,
        string delimiter = null,
        bool isTruncated = false,
        string nextToken = null,
        List<CommonPrefixes> prefixes = null,
        string bucketRegion = "us-west-1")
    {
        if (String.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
        this.Name = name;
        this.Prefix = prefix;
        this.Marker = marker;
        this.KeyCount = keyCount;
        this.MaxKeys = maxKeys;
        this.Delimiter = delimiter;
        this.IsTruncated = isTruncated;
        this.NextContinuationToken = nextToken;
        if (contents != null) this.Contents = contents;
        if (prefixes != null) this.CommonPrefixes = prefixes;
        this.BucketRegion = bucketRegion;
    }

    #endregion

    #region Public-Methods

    /*
     * See https://stackoverflow.com/a/51440611 for information on ShouldSerialize methods
     * and how they are used by XmlSerializer
     */

    /// <summary>
    /// Helper method for XML serialization.
    /// </summary>
    /// <returns>Boolean</returns>
    public bool ShouldSerializeMarker()
    {
        return !String.IsNullOrEmpty(this.Marker);
    }

    /// <summary>
    /// Helper method for XML serialization.
    /// </summary>
    /// <returns>Boolean</returns>
    public bool ShouldSerializeNextContinuationToken()
    {
        return !String.IsNullOrEmpty(this.NextContinuationToken);
    }

    /// <summary>
    /// Helper method for XML serialization.
    /// </summary>
    /// <returns>Boolean</returns>
    public bool ShouldSerializeCommonPrefixes()
    {
        return this.CommonPrefixes != null && this.CommonPrefixes.Count > 0;
    }

    /// <summary>
    /// Helper method for XML serialization.
    /// </summary>
    /// <returns>Boolean</returns>
    public bool ShouldSerializeDelimiter()
    {
        return !String.IsNullOrEmpty(this._delimiter);
    }

    /// <summary>
    /// Helper method for XML serialization.
    /// </summary>
    /// <returns>Boolean</returns>
    public bool ShouldSerializeEncodingType()
    {
        return !String.IsNullOrEmpty(this.EncodingType);
    }

    #endregion

    #region Private-Methods

    #endregion
}