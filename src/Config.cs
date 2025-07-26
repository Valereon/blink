

/// <summary>
/// Central Class for managing TOML keys and file system paths
/// </summary>
public static class Config
{

    public const string PathKey = "path";
    public const string LangsInProject = "langsInProject";
    public const string FileSystemRoot = "fileSystemRoot";

    public static string PathSeperator = @"\";

    // blink env stuff
    public const string BinFolderPath = @".\.blink\bin";
    public const string ConfigTomlPath = @".\.blink\config.toml";
    public const string BuildTomlPath = @".\.blink\build.toml";

    //python
    public const string PythonHome = "pythonHome";
    public const string PythonEnv = "pythonEnv";
    public const string PythonArgs = "pythonArgs";
    public const string PipArgs = "pipArgs";

    /// <summary>
    /// updates the path seperator for the current operating system
    /// </summary>
    public static void UpdatePathSeparator()
    {
        PathSeperator = Path.DirectorySeparatorChar.ToString();
    }
}
