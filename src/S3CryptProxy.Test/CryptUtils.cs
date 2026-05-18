namespace S3CryptProxy.Test;

public class CryptUtils
{
    public CryptUtils()
    {
        
    }

    public string GetRealKey(string storageKey)
    {
        return storageKey;
    }
    public string GetStorageKey(string realKey)
    {
        return realKey;
    }
    
    public long GetRealSize(long storageSize)
    {
        return storageSize;
    }
    public long GetStorageSize(long realSize)
    {
        return realSize;
    }
}