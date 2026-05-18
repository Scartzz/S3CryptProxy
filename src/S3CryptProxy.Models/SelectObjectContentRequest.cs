namespace S3CryptProxy.Models;

using System.Xml.Serialization;

/// <summary>
/// Select object content request.
/// </summary>
[XmlRoot(ElementName = "SelectObjectContentRequest")]
public class SelectObjectContentRequest
{
    // Namespace = "http://s3.amazonaws.com/doc/2006-03-01/"

    #region Public-Members

    /// <summary>
    /// Expression.
    /// </summary>
    [XmlElement(ElementName = "Expression", IsNullable = false)]
    public string Expression { get; set; } = null;

    /// <summary>
    /// Expression type.
    /// </summary>
    [XmlElement(ElementName = "ExpressionType", IsNullable = false)]
    public ExpressionTypeEnum ExpressionType { get; set; } = ExpressionTypeEnum.Sql;

    /// <summary>
    /// Request progress.
    /// </summary>
    [XmlElement(ElementName = "RequestProgress", IsNullable = true)]
    public RequestProgress RequestProgress
    {
        get
        {
            return this._requestProgress;
        }
        set
        {
            if (value == null) this._requestProgress = new RequestProgress();
            else this._requestProgress = value;
        }
    }

    /// <summary>
    /// Input serialization.
    /// </summary>
    [XmlElement(ElementName = "InputSerialization", IsNullable = false)]
    public InputSerialization InputSerialization
    {
        get
        {
            return this._inputSerialization;
        }
        set
        {
            if (value == null) this._inputSerialization = new InputSerialization();
            else this._inputSerialization = value;
        }
    }

    /// <summary>
    /// Output serialization.
    /// </summary>
    [XmlElement(ElementName = "OutputSerialization", IsNullable = false)]
    public OutputSerialization OutputSerialization
    {
        get
        {
            return this._outputSerialization;
        }
        set
        {
            if (value == null) this._outputSerialization = new OutputSerialization();
            else this._outputSerialization = value;
        }
    }

    /// <summary>
    /// Scan range.
    /// </summary>
    [XmlElement(ElementName = "ScanRange", IsNullable = true)]
    public ScanRange ScanRange { get; set; } = new ScanRange();

    #endregion

    #region Private-Members

    private RequestProgress _requestProgress = new RequestProgress();
    private InputSerialization _inputSerialization = new InputSerialization();
    private OutputSerialization _outputSerialization = new OutputSerialization();
    private ScanRange _scanRange = new ScanRange();

    #endregion

    #region Constructors-and-Factories

    /// <summary>
    /// Instantiate.
    /// </summary>
    public SelectObjectContentRequest()
    {

    }

    #endregion

    #region Public-Methods

    #endregion

    #region Private-Methods

    #endregion
}