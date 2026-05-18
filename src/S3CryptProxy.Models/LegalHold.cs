namespace S3CryptProxy.Models;

using System;
using System.Collections.Generic;
using System.Xml.Serialization;

/// <summary>
/// Legal hold status of a resource.
/// </summary>
[XmlRoot(ElementName = "LegalHold", IsNullable = true)]
public class LegalHold
{
    // Namespace = "http://s3.amazonaws.com/doc/2006-03-01/"

    #region Public-Members

    /// <summary>
    /// Legal hold status.
    /// Valid values are null, ON, OFF.
    /// </summary>
    [XmlElement(ElementName = "Status", IsNullable = true)]
    public string Status
    {
        get
        {
            return this._status;
        }
        set
        {
            if (String.IsNullOrEmpty(value)) this._status = value;
            else
            {
                if (!this._statusValidValues.Contains(value)) throw new ArgumentException("Unknown Status '" + value + "'.");
                this._status = value;
            }
        }
    }

    #endregion

    #region Private-Members

    private string _status = "OFF";
    private List<string> _statusValidValues = new List<string>
    {
        "ON",
        "OFF"
    };

    #endregion

    #region Constructors-and-Factories

    /// <summary>
    /// Instantiate.
    /// </summary>
    public LegalHold()
    {

    }

    /// <summary>
    /// Instantiate.
    /// </summary>
    /// <param name="status">Status.  Valid values are null, ON, OFF.</param>
    public LegalHold(string status)
    {
        this.Status = status;
    }

    #endregion

    #region Public-Methods

    #endregion

    #region Private-Methods

    #endregion
}