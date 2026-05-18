namespace S3CryptProxy.Handler;

using System;

/// <summary>
/// Boundary conditions for certain operations, e.g. PutObject, GetObject, etc.
/// </summary>
public class OperationLimitsSettings
{
    #region Public-Members

    /// <summary>
    /// Maximum content-length for object write (PutObject) before use of multi-part upload is required.
    /// </summary>
    public long MaxPutObjectSize
    {
        get
        {
            return this._maxPutObjectSize;
        }
        set
        {
            if (value < 1) throw new ArgumentOutOfRangeException(nameof(this.MaxPutObjectSize));
            else this._maxPutObjectSize = value;
        }
    }

    #endregion

    #region Private-Members

    private long _maxPutObjectSize = (1024 * 1024 * 128); // 128MB

    #endregion

    #region Constructors-and-Factories

    /// <summary>
    /// Instantiates the object.
    /// </summary>
    public OperationLimitsSettings()
    {

    }

    #endregion

    #region Public-Methods

    #endregion

    #region Private-Methods

    #endregion
}