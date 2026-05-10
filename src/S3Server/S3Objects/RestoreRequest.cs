namespace S3ServerLibrary.S3Objects
{
    using System;
    using System.Linq;
    using System.Xml;
    using System.Xml.Serialization;

    /// <summary>
    /// Restore object request.
    /// </summary>
    [XmlRoot(ElementName = "RestoreRequest")]
    public class RestoreRequest
    {
        /// <summary>
        /// Lifetime of the restored copy in days.
        /// </summary>
        [XmlElement(ElementName = "Days", IsNullable = true)]
        public int? Days
        {
            get
            {
                return _Days;
            }
            set
            {
                if (value != null && value.Value < 1) throw new ArgumentOutOfRangeException(nameof(Days));
                _Days = value;
            }
        }

        /// <summary>
        /// Glacier job parameters.
        /// </summary>
        [XmlElement(ElementName = "GlacierJobParameters")]
        public GlacierJobParameters GlacierJobParameters { get; set; } = null;

        /// <summary>
        /// Top-level retrieval tier.
        /// </summary>
        [XmlElement(ElementName = "Tier")]
        public RestoreTierEnum? Tier { get; set; } = null;

        /// <summary>
        /// Optional description.
        /// </summary>
        [XmlElement(ElementName = "Description")]
        public string Description { get; set; } = null;

        /// <summary>
        /// Any additional request elements not modeled by this class.
        /// Used to detect unsupported restore-select payloads.
        /// </summary>
        [XmlAnyElement]
        public XmlElement[] AdditionalElements { get; set; } = null;

        /// <summary>
        /// Effective restore tier.
        /// </summary>
        [XmlIgnore]
        public RestoreTierEnum? EffectiveTier
        {
            get
            {
                if (Tier != null) return Tier;
                return GlacierJobParameters?.Tier;
            }
        }

        /// <summary>
        /// True if the request contains unsupported restore-select fields.
        /// </summary>
        [XmlIgnore]
        public bool HasUnsupportedRestoreSelectFields
        {
            get
            {
                if (AdditionalElements == null || AdditionalElements.Length < 1) return false;

                return AdditionalElements.Any(e =>
                    e != null
                    && (e.LocalName.Equals("OutputLocation", StringComparison.OrdinalIgnoreCase)
                    || e.LocalName.Equals("SelectParameters", StringComparison.OrdinalIgnoreCase)
                    || e.LocalName.Equals("Type", StringComparison.OrdinalIgnoreCase)));
            }
        }

        private int? _Days = null;
    }
}
