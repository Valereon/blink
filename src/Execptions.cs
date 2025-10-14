


/// <summary>
/// Base exception
/// </summary>
public class BlinkException : Exception
{
    public BlinkException(string msg) : base(msg)
    {

    }



}

/// <summary>
/// An exception for anything related to TOML operations 
/// </summary>
public class BlinkTOMLException : BlinkException
{
    public BlinkTOMLException(string msg) : base(msg)
    {

    }
}

/// <summary>
/// An exception for anything related to the FS but not TOML operations
/// </summary>
public class BlinkFSException : BlinkException
{
    public BlinkFSException(string msg) : base(msg)
    {

    }
}

/// <summary>
/// For things related to downloading
/// </summary>
public class BlinkDownloadException : BlinkException
{
    public BlinkDownloadException(string msg) : base(msg)
    {
        
    }
}


