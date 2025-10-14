

/// <summary>
/// Central Class for managing TOML keys and file system paths
/// </summary>
public static class Config
{

    public const string PathKey = "path";
    public const string FileSystemRoot = "fileSystemRoot";
    public const string FallbackMode = "mode";

    public const string ShellExe = "shellExecutable";
    public const string ShellExtraArgs = "extraShellArgs";

    public static string PathSeparator = @"\";

    // blink env stuff
    public const string BinFolderPath = @".\.blink\bin";
    public const string ConfigTomlPath = @".\.blink\config.toml";
    public const string BuildTomlPath = @".\.blink\build.toml";

    /// <summary>
    /// updates the path seperator for the current operating system
    /// </summary>
    public static void UpdatePathSeparator()
    {
        PathSeparator = Path.DirectorySeparatorChar.ToString();
    }

    public const string BaseConfigTOML = """
    path = [] # NEVER SHOULD BE ABSOLUTE
    fileSystemRoot = "" # ABSOLUTE
    targetPlatforms = []    
    

    # execution and fallback
    #AUTO: falls back to shell when blink command does not work
    #SHELL: runs always in the shell 
    #ASK:  when blink run fails asks if should run in the shell
    #BLINK: runs in the blink and if it fails then throws
    mode = "auto" # auto | shell | ask | blink     

    shellExecutable = "cmd.exe"
    extraShellArgs = "/c"
    """;


    public const string BaseBuildTOML = """
    test = "echo 'hello'"


    """;
}
