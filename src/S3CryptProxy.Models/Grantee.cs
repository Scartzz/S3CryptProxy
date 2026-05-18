namespace S3CryptProxy.Models;

using System;
using System.Collections.Generic;
using System.Xml.Serialization;

/// <summary>
/// A permission recipient.
/// </summary>
[XmlInclude(typeof(CanonicalUser))]
[XmlInclude(typeof(Group))]
[XmlRoot(ElementName = "Grantee")]
public class Grantee
{
    // Namespace = "http://s3.amazonaws.com/doc/2006-03-01/"

    #region Public-Members

    /// <summary>
    /// ID of the grantee.
    /// </summary>
    [XmlElement(ElementName = "ID")]
    public string Id { get; set; } = null;

    /// <summary>
    /// Display name.
    /// </summary>
    [XmlElement(ElementName = "DisplayName")]
    public string DisplayName { get; set; } = null;

    /// <summary>
    /// For a group, the URI of the group.
    /// </summary>
    [XmlElement(ElementName = "URI")]
    public string Uri { get; set; } = null;

    /// <summary>
    /// Type of grantee.
    /// Valid values are CanonicalUser, AmazonCustomerByEmail, Group.
    /// </summary>
    [XmlIgnore]
    public string GranteeType
    {
        get
        {
            return this._type;
        }
        set
        {
            if (String.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(this.GranteeType));
            if (!this._typeValidValues.Contains(value)) throw new ArgumentException("Unknown Type '" + value + "'.");
            this._type = value;
        }
    }

    /// <summary>
    /// Email address of the grantee.
    /// </summary>
    [XmlElement(ElementName = "EmailAddress")]
    public string EmailAddress { get; set; } = null;

    #endregion

    #region Private-Members

    private string _type = "CanonicalUser";
    private List<string> _typeValidValues = new List<string>
    {
        "CanonicalUser",
        "AmazonCustomerByEmail",
        "Group"
    };

    #endregion

    #region Constructors-and-Factories

    /// <summary>
    /// Instantiate.
    /// </summary>
    public Grantee()
    {

    }

    /// <summary>
    /// Instantiate.
    /// </summary>
    /// <param name="id">ID.</param>
    /// <param name="displayName">Display name.</param>
    /// <param name="uri">URI.</param>
    /// <param name="granteeType">Grantee type.  Valid values are CanonicalUser, AmazonCustomerByEmail, Group.</param>
    /// <param name="email">Email.</param>
    public Grantee(string id, string displayName, string uri, string granteeType, string email)
    {
        this.Id = id;
        this.DisplayName = displayName;
        this.Uri = uri;
        this.GranteeType = granteeType;
        this.EmailAddress = email;
    }

    #endregion

    #region Public-Methods

    /// <summary>
    /// Helper method for XML serialization.
    /// </summary>
    /// <returns>Boolean.</returns>
    public bool ShouldSerializeId()
    {
        return !String.IsNullOrEmpty(this.Id);
    }

    /// <summary>
    /// Helper method for XML serialization.
    /// </summary>
    /// <returns>Boolean.</returns>
    public bool ShouldSerializeDisplayName()
    {
        return !String.IsNullOrEmpty(this.DisplayName);
    }

    /// <summary>
    /// Helper method for XML serialization.
    /// </summary>
    /// <returns>Boolean.</returns>
    public bool ShouldSerializeUri()
    {
        return !String.IsNullOrEmpty(this.Uri);
    }

    /// <summary>
    /// Helper method for XML serialization.
    /// </summary>
    /// <returns>Boolean.</returns>
    public bool ShouldSerializeEmailAddress()
    {
        return !String.IsNullOrEmpty(this.EmailAddress);
    }

    #endregion

    #region Private-Methods

    #endregion
}

/// <summary>
/// Instantiate.
/// </summary>
[XmlType(TypeName = "CanonicalUser")]
public class CanonicalUser : Grantee
{
    /// <summary>
    /// Instantiate.
    /// </summary>
    public CanonicalUser()
    {
        base.GranteeType = "CanonicalUser";
    }
}

/// <summary>
/// Instantiate.
/// </summary>
[XmlType(TypeName = "Group")]
public class Group : Grantee
{
    /// <summary>
    /// Instantiate.
    /// </summary>
    public Group()
    {
        base.GranteeType = "Group";
    }
}

/// <summary>
/// Instantiate.
/// </summary>
[XmlType(TypeName = "Group")]
public class AmazonCustomerByEmail : Grantee
{
    /// <summary>
    /// Instantiate.
    /// </summary>
    public AmazonCustomerByEmail()
    {
        base.GranteeType = "AmazonCustomerByEmail";
    }
}