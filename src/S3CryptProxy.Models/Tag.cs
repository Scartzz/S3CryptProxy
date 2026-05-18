namespace S3CryptProxy.Models;

using System;
using System.Xml.Serialization;

/// <summary>
/// Tag.
/// </summary>
[XmlRoot(ElementName = "Tag")]
public class Tag
{
    // Namespace = "http://s3.amazonaws.com/doc/2006-03-01/"

    #region Public-Members

    /// <summary>
    /// Key.
    /// </summary>
    [XmlElement(ElementName = "Key", IsNullable = true)]
    public string Key
    {
        get
        {
            return this._key;
        }
        set
        {
            if (String.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(this.Key));
            this._key = value;
        }
    }

    /// <summary>
    /// Value.
    /// </summary>
    [XmlElement(ElementName = "Value", IsNullable = true)]
    public string Value { get; set; } = null;

    #endregion

    #region Private-Members

    private string _key = null;

    #endregion

    #region Constructors-and-Factories

    /// <summary>
    /// Instantiate.
    /// </summary>
    public Tag()
    {

    }

    /// <summary>
    /// Instantiate.
    /// </summary>
    /// <param name="key">Key.</param>
    /// <param name="val">Value.</param>
    public Tag(string key, string val)
    {
        this.Key = key;
        this.Value = val;
    }

    #endregion

    #region Public-Methods

    #endregion

    #region Private-Methods

    #endregion
}