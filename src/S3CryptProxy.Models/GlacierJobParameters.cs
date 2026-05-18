namespace S3CryptProxy.Models;

using System.Xml.Serialization;

/// <summary>
/// Glacier restore job parameters.
/// </summary>
public class GlacierJobParameters
{
    /// <summary>
    /// Restore tier.
    /// </summary>
    [XmlElement(ElementName = "Tier")]
    public RestoreTierEnum? Tier { get; set; } = null;
}