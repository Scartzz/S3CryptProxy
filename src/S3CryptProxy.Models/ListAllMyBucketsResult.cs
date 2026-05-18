namespace S3CryptProxy.Models;

using System.Xml.Serialization;

/// <summary>
/// Result from a ListAllMyBuckets request.
/// </summary>
[XmlRoot(ElementName = "ListAllMyBucketsResult")]
public class ListAllMyBucketsResult
{
    // Namespace = "http://s3.amazonaws.com/doc/2006-03-01/"

    #region Public-Members

    /// <summary>
    /// Bucket owner.
    /// </summary>
    [XmlElement(ElementName = "Owner")]
    public Owner Owner { get; set; } = new Owner();

    /// <summary>
    /// Buckets owned by the user.
    /// </summary>
    [XmlElement(ElementName = "Buckets")]
    public Buckets Buckets { get; set; } = new Buckets();

    /// <summary>
    /// The next continuation token to supply to continue the query.
    /// </summary>
    [XmlElement(ElementName = "ContinuationToken")]
    public string ContinuationToken { get; set; } = null;

    /// <summary>
    /// Prefix
    /// </summary>
    [XmlElement(ElementName = "Prefix")]
    public string Prefix { get; set; } = null;

    #endregion

    #region Private-Members

    #endregion

    #region Constructors-and-Factories

    /// <summary>
    /// Instantiate.
    /// </summary>
    public ListAllMyBucketsResult()
    {

    }

    /// <summary>
    /// Instantiate.
    /// </summary>
    /// <param name="owner">Owmer/</param>
    /// <param name="buckets">Buckets.</param>
    public ListAllMyBucketsResult(Owner owner, Buckets buckets)
    {
        this.Owner = owner;
        this.Buckets = buckets;
    }

    #endregion

    #region Public-Methods

    #endregion

    #region Private-Methods

    #endregion
}