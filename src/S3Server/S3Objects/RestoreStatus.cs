namespace S3ServerLibrary.S3Objects
{
    using System;
    using System.Globalization;
    using System.Xml.Serialization;

    /// <summary>
    /// Restore status of an archived object.
    /// </summary>
    public class RestoreStatus
    {
        /// <summary>
        /// True if restore is still in progress.
        /// </summary>
        [XmlIgnore]
        public bool OngoingRequest { get; set; } = false;

        /// <summary>
        /// Expiration of the active restored copy, if available.
        /// </summary>
        [XmlIgnore]
        public DateTime? ExpiryDate
        {
            get => _ExpiryDate;
            set
            {
                if (value != null) _ExpiryDate = DateTime.SpecifyKind(value.Value, DateTimeKind.Utc);
                else _ExpiryDate = null;
            }
        }

        /// <summary>
        /// Format for the x-amz-restore response header.
        /// </summary>
        [XmlIgnore]
        public string HeaderValue
        {
            get
            {
                if (OngoingRequest) return "ongoing-request=\"true\"";
                if (ExpiryDate != null)
                    return "ongoing-request=\"false\", expiry-date=\"" + ExpiryDate.Value.ToString(Constants.AmazonTimestampFormatVerbose, CultureInfo.InvariantCulture) + "\"";
                return "ongoing-request=\"false\"";
            }
        }

        private DateTime? _ExpiryDate = null;
    }
}
