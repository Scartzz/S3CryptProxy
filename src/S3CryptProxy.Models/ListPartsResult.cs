namespace S3CryptProxy.Models;

using System;
using System.Collections.Generic;
using System.Xml.Serialization;

/// <summary>
/// List parts result.
/// </summary>
[XmlRoot(ElementName = "ListPartsResult")]
public class ListPartsResult
{
    // Namespace = "http://s3.amazonaws.com/doc/2006-03-01/"

    #region Public-Members

    /// <summary>
    /// Bucket.
    /// </summary>
    [XmlElement(ElementName = "Bucket", IsNullable = false)]
    public string Bucket { get; set; } = null;

    /// <summary>
    /// Key.
    /// </summary>
    [XmlElement(ElementName = "Key", IsNullable = false)]
    public string Key { get; set; } = null;

    /// <summary>
    /// Upload ID.
    /// </summary>
    [XmlElement(ElementName = "UploadId", IsNullable = false)]
    public string UploadId { get; set; } = null;

    /// <summary>
    /// Part number marker.
    /// </summary>
    [XmlElement(ElementName = "PartNumberMarker", IsNullable = false)]
    public int PartNumberMarker
    {
        get
        {
            return this._partNumberMarker;
        }
        set
        {
            if (value < 0) throw new ArgumentOutOfRangeException(nameof(this.PartNumberMarker));
            this._partNumberMarker = value;
        }
    }

    /// <summary>
    /// Next part number marker.
    /// </summary>
    [XmlElement(ElementName = "NextPartNumberMarker", IsNullable = false)]
    public int NextPartNumberMarker
    {
        get
        {
            return this._nextPartNumberMarker;
        }
        set
        {
            if (value < 0) throw new ArgumentOutOfRangeException(nameof(this.NextPartNumberMarker));
            this._nextPartNumberMarker = value;
        }
    }

    /// <summary>
    /// Max parts.
    /// </summary>
    [XmlElement(ElementName = "MaxParts", IsNullable = false)]
    public int MaxParts
    {
        get
        {
            return this._maxParts;
        }
        set
        {
            if (value < 0) throw new ArgumentOutOfRangeException(nameof(this.MaxParts));
            this._maxParts = value;
        }
    }

    /// <summary>
    /// Flag to indicate if the results were truncated.
    /// </summary>
    [XmlElement(ElementName = "IsTruncated", IsNullable = false)]
    public bool IsTruncated { get; set; } = false;

    /// <summary>
    /// Initiator of the upload.
    /// </summary>
    [XmlElement(ElementName = "Initiator")]
    public Owner Initiator { get; set; } = new Owner();

    /// <summary>
    /// Owner of the object.
    /// </summary>
    [XmlElement(ElementName = "Owner")]
    public Owner Owner { get; set; } = new Owner();

    /// <summary>
    /// Storage class.
    /// </summary>
    [XmlElement(ElementName = "StorageClass", IsNullable = false)]
    public StorageClassEnum StorageClass { get; set; } = StorageClassEnum.Standard;

    /// <summary>
    /// Checksum algorithm.
    /// </summary>
    [XmlElement(ElementName = "ChecksumAlgorithm", IsNullable = false)]
    public ChecksumAlgorithmEnum ChecksumAlgorithm { get; set; } = ChecksumAlgorithmEnum.Crc32;

    /// <summary>
    /// Flag to control whether ChecksumAlgorithm is serialized.
    /// Default value is false.
    /// </summary>
    [XmlIgnore]
    public bool IncludeChecksumAlgorithm { get; set; } = false;

    /// <summary>
    /// Parts.
    /// </summary>
    [XmlElement(ElementName = "Part")]
    public List<Part> Parts
    {
        get
        {
            return this._parts;
        }
        set
        {
            if (value == null) this._parts = new List<Part>();
            else this._parts = value;
        }
    }

    #endregion

    #region Private-Members

    private int _partNumberMarker = 0;
    private int _nextPartNumberMarker = 0;
    private int _maxParts = 1000;
    private List<Part> _parts = new List<Part>();

    #endregion

    #region Constructors-and-Factories

    /// <summary>
    /// Instantiate.
    /// </summary>
    public ListPartsResult()
    {

    }

    #endregion

    #region Public-Methods

    /// <summary>
    /// Helper method for XML serialization.
    /// </summary>
    /// <returns>Boolean.</returns>
    public bool ShouldSerializeChecksumAlgorithm()
    {
        return this.IncludeChecksumAlgorithm;
    }

    #endregion

    #region Private-Methods

    #endregion
}