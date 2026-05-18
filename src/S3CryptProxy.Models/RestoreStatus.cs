namespace S3CryptProxy.Models;

using System;
using System.Globalization;
using System.Xml.Serialization;
using S3CryptProxy.Shared;

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
        get => this._expiryDate;
        set
        {
            if (value != null) this._expiryDate = DateTime.SpecifyKind(value.Value, DateTimeKind.Utc);
            else this._expiryDate = null;
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
            if (this.OngoingRequest) return "ongoing-request=\"true\"";
            if (this.ExpiryDate != null)
                return "ongoing-request=\"false\", expiry-date=\"" + this.ExpiryDate.Value.ToString(Constants.AmazonTimestampFormatVerbose, CultureInfo.InvariantCulture) + "\"";
            return "ongoing-request=\"false\"";
        }
    }

    private DateTime? _expiryDate = null;
}