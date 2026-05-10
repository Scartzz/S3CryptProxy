namespace S3ServerLibrary.S3Objects
{
    using System.Runtime.Serialization;
    using System.Text.Json.Serialization;
    using System.Xml.Serialization;

    /// <summary>
    /// Restore tiers used by S3 archive restore operations.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum RestoreTierEnum
    {
        /// <summary>
        /// Standard retrieval.
        /// </summary>
        [EnumMember(Value = "Standard")]
        [XmlEnum(Name = "Standard")]
        Standard,
        /// <summary>
        /// Bulk retrieval.
        /// </summary>
        [EnumMember(Value = "Bulk")]
        [XmlEnum(Name = "Bulk")]
        Bulk,
        /// <summary>
        /// Expedited retrieval.
        /// </summary>
        [EnumMember(Value = "Expedited")]
        [XmlEnum(Name = "Expedited")]
        Expedited
    }
}
