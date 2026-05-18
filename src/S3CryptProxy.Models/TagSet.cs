namespace S3CryptProxy.Models;

using System.Collections.Generic;
using System.Xml.Serialization;

/// <summary>
/// Tag set.
/// </summary>
[XmlRoot(ElementName = "TagSet")]
public class TagSet
{
    // Namespace = "http://s3.amazonaws.com/doc/2006-03-01/"

    #region Public-Members

    /// <summary>
    /// Tag.
    /// </summary>
    [XmlElement(ElementName = "Tag", IsNullable = true)]
    public List<Tag> Tags
    {
        get
        {
            return this._tagList;
        }
        set
        {
            if (value == null) this._tagList = new List<Tag>();
            else this._tagList = value;
        }
    }

    #endregion

    #region Private-Members

    private List<Tag> _tagList = new List<Tag>();

    #endregion

    #region Constructors-and-Factories

    /// <summary>
    /// Instantiate.
    /// </summary>
    public TagSet()
    {

    }

    /// <summary>
    /// Instantiate.
    /// </summary>
    /// <param name="tags">Tags.</param>
    public TagSet(List<Tag> tags)
    {
        this.Tags = tags;
    }

    #endregion

    #region Public-Methods

    #endregion

    #region Private-Methods

    #endregion
}