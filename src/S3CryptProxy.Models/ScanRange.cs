namespace S3CryptProxy.Models;

using System;
using System.Xml.Serialization;

/// <summary>
/// Scan range.
/// </summary>
[XmlRoot(ElementName = "ScanRange", IsNullable = true)]
public class ScanRange
{
    // Namespace = "http://s3.amazonaws.com/doc/2006-03-01/"

    #region Public-Members

    /// <summary>
    /// End.
    /// </summary>
    [XmlElement(ElementName = "End", IsNullable = true)]
    public long End
    {
        get
        {
            return this._end;
        }
        set
        {
            if (value < 0) throw new ArgumentOutOfRangeException(nameof(this.End));
            this._end = value;
        }
    }

    /// <summary>
    /// Start.
    /// </summary>
    [XmlElement(ElementName = "Start", IsNullable = true)]
    public long Start
    {
        get
        {
            return this._start;
        }
        set
        {
            if (value < 0) throw new ArgumentOutOfRangeException(nameof(this.Start));
            this._start = value;
        }
    }

    #endregion

    #region Private-Members

    private long _end = 0;
    private long _start = 0;

    #endregion

    #region Constructors-and-Factories

    /// <summary>
    /// Instantiate.
    /// </summary>
    public ScanRange()
    {

    }

    #endregion

    #region Public-Methods

    #endregion

    #region Private-Methods

    #endregion
}