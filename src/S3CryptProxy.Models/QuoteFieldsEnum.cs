namespace S3CryptProxy.Models;

using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

/// <summary>
/// Quote fields options. 
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum QuoteFieldsEnum
{
    /// <summary>
    /// ALWAYS.
    /// </summary>
    [EnumMember(Value = "ALWAYS")]
    [XmlEnum(Name = "ALWAYS")]
    Always,
    /// <summary>
    /// ASNEEDED.
    /// </summary>
    [EnumMember(Value = "ASNEEDED")]
    [XmlEnum(Name = "ASNEEDED")]
    Asneeded,
}