namespace S3CryptProxy.Models;

using System;
using System.Collections.Generic;
using System.Xml.Serialization;

/// <summary>
/// Location constraint for a resource.
/// </summary>
[XmlRoot(ElementName = "LocationConstraint")]
public class LocationConstraint
{
    // Namespace = "http://s3.amazonaws.com/doc/2006-03-01/"

    #region Public-Members

    /// <summary>
    /// Text, i.e. the region.
    /// Valid values are valid S3 regions, i.e. us-west-1.
    /// </summary>
    [XmlText]
    public string Text
    {
        get
        {
            return this._text;
        }
        set
        {
            if (String.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(this.Text));
            this._text = value;
        }
    }

    #endregion

    #region Private-Members

    private string _text = "us-west-1";

    #endregion

    #region Constructors-and-Factories

    /// <summary>
    /// Instantiate.
    /// </summary>
    public LocationConstraint()
    {

    }

    /// <summary>
    /// Instantiate.
    /// </summary>
    /// <param name="region">Region.  Valid values are valid S3 regions, i.e. us-west-1.</param>
    public LocationConstraint(string region)
    {
        this.Text = region;
    }

    #endregion

    #region Public-Methods

    #endregion

    #region Private-Methods

    #endregion
}