public static class Config
{

    public const string PathKey = "path";
    public const string LangsInProject = "langsInProject";
    public const string FileSystemRoot = "fileSystemRoot";

    public static string PathSeperator = @"\";


    //python
    public const string PythonHome = "pythonHome";
    public const string PythonEnv = "pythonEnv";
    public const string PythonArgs = "pythonArgs";
    public const string PipArgs = "pipArgs";


    public static void UpdatePathSeperator()
    {
        PathSeperator = Path.DirectorySeparatorChar.ToString();
    }
}
