namespace S3CryptProxy.Shared;

internal class Constants
{
    internal const string AmazonTimestampFormatVerbose = "ddd, dd MMM yyy HH:mm:ss 'GMT'";
    internal const string AmazonTimestampFormatCompact = "yyyyMMddTHHmmssZ";
    internal const string AmazonDatestampFormat = "yyyyMMdd";

    internal const string HeaderStorageClass = "x-amz-storage-class";
    internal const string HeaderLastModified = "Last-Modified";
    internal const string HeaderRequestId = "x-amz-request-id";
    internal const string HeaderTraceId = "x-amz-id-2";
    internal const string HeaderBucketRegion = "x-amz-bucket-region";
    internal const string HeaderETag = "ETag";
    internal const string HeaderConnection = "Connection";
    internal const string HeaderAcceptRanges = "Accept-Ranges";
    internal const string HeaderRestore = "x-amz-restore";
    internal const string HeaderRestoreOutputPath = "x-amz-restore-output-path";

    internal const string ContentTypeXml = "application/xml";
    internal const string ContentTypeText = "text/plain";
    internal const string ContentTypeOctetStream = "application/octet-stream";
}