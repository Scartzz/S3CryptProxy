namespace S3CryptProxy.Handler;

using System;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using S3CryptProxy.Handler.Callbacks;
using S3CryptProxy.Handler.Interfaces;

/// <summary>
/// S3 server settings.
/// </summary>
public class S3ServerSettings
{
    #region Public-Members

    /// <summary>
    /// Method to invoke when sending a log message.  This value can only be changed before the server has been started.  
    /// If you need to change the name after the server has been started, dispose and start again with the correct settings.
    /// </summary>
    [JsonIgnore]
    public ILoggerFactory LoggerFactory { get; set; } = null;

    /// <summary>
    /// Size limits for certain operations.
    /// </summary>
    public OperationLimitsSettings OperationLimits
    {
        get
        {
            return this._limits;
        }
        set
        {
            if (value == null) this._limits = new OperationLimitsSettings();
            else this._limits = value;
        }
    }

    /// <summary>
    /// Webserver settings.
    /// </summary>
    public IS3ServerSettings Webserver
    {
        get
        {
            return this._webserver;
        }
        set
        {
            if (value == null) throw new ArgumentNullException(nameof(this.Webserver));
            this._webserver = value;
        }
    }

    /// <summary>
    /// Callback method to use prior to examining requests for AWS S3 APIs.
    /// Return true if you wish to terminate the request, otherwise, return false, which will further route the request.
    /// </summary>
    [JsonIgnore]
    public Func<S3Context, Task<bool>> PreRequestHandler = null;

    /// <summary>
    /// Callback method to call when no matching AWS S3 API callback could be found. 
    /// </summary>
    [JsonIgnore]
    public Func<S3Context, Task> DefaultRequestHandler = null;

    /// <summary>
    /// Callback method to call after a response has been sent.
    /// </summary>
    [JsonIgnore]
    public Func<S3Context, Task> PostRequestHandler = null;
    
    public IServiceCallbacks ServiceCallbacks { get; set; } = null;

    public IBucketCallbacks BucketCallbacks { get; set; } = null;

    public IObjectCallbacks ObjectCallbacks { get; set; } = null;

    #endregion

    #region Private-Members

    private IS3ServerSettings _webserver;
    private OperationLimitsSettings _limits = new OperationLimitsSettings();

    #endregion

    #region Constructors-and-Factories

    /// <summary>
    /// Instantiate.
    /// </summary>
    public S3ServerSettings(IS3ServerSettings webserverSettings)
    {
        _webserver = webserverSettings;
    }

    #endregion
}