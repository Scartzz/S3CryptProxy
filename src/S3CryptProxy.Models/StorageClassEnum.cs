namespace S3CryptProxy.Models;

using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

/// <summary>
/// Storage classes used by S3. 
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum StorageClassEnum
{
    /// <summary>
    /// Standard.
    /// </summary>
    [EnumMember(Value = "STANDARD")]
    [XmlEnum(Name = "STANDARD")]
    Standard,
    /// <summary>
    /// Reduced redundancy.
    /// </summary>
    [EnumMember(Value = "REDUCED_REDUNDANCY")]
    [XmlEnum(Name = "REDUCED_REDUNDANCY")]
    ReducedRedundancy,
    /// <summary>
    /// Glacier.
    /// </summary>
    [EnumMember(Value = "GLACIER")]
    [XmlEnum(Name = "GLACIER")]
    Glacier,
    /// <summary>
    /// Standard IA.
    /// </summary>
    [EnumMember(Value = "STANDARD_IA")]
    [XmlEnum(Name = "STANDARD_IA")]
    StandardIa,
    /// <summary>
    /// One zone IA.
    /// </summary>
    [EnumMember(Value = "ONEZONE_IA")]
    [XmlEnum(Name = "ONEZONE_IA")]
    OnezoneIa,
    /// <summary>
    /// Intelligent tiering.
    /// </summary>
    [EnumMember(Value = "INTELLIGENT_TIERING")]
    [XmlEnum(Name = "INTELLIGENT_TIERING")]
    IntelligentTiering,
    /// <summary>
    /// Deep archive.
    /// </summary>
    [EnumMember(Value = "DEEP_ARCHIVE")]
    [XmlEnum(Name = "DEEP_ARCHIVE")]
    DeepArchive,
    /// <summary>
    /// Outposts.
    /// </summary>
    [EnumMember(Value = "OUTPOSTS")]
    [XmlEnum(Name = "OUTPOSTS")]
    Outposts
}