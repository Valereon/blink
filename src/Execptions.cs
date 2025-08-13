


public class BlinkException : Exception
{
    public BlinkException(string msg) : base(msg)
    {

    }

    

}


public class BlinkTOMLException : BlinkException
{
    public BlinkTOMLException(string msg) : base(msg)
    {

    }
}


public class BlinkFSException : BlinkException
{
    public BlinkFSException(string msg) : base(msg)
    {

    }
}


public class BlinkDownloadException : BlinkException
{
    public BlinkDownloadException(string msg) : base(msg)
    {
        
    }
}


