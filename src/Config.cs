

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

    public const string PathModWindows = @"set PATH=";

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
    

    # execution and fallback
    #AUTO: falls back to shell when blink command does not work
    #SHELL: runs always in the shell 
    #ASK:  when blink run fails asks if should run in the shell
    #BLINK: runs in the blink and if it fails then throws
    # IF NONE OF THIS ARE CHOSEN, or if one is spelled incorrectly it will automatically assume "AUTO"
    mode = "auto" # auto | shell | ask | blink     

    shellExecutable = "cmd.exe"
    extraShellArgs = "/c"

    #Path Modification
    # this will allow you to append the ENTIRE blink path to the front or end of your current system path.
    # KEEP IN MIND!!!! this will only affect the current shell session none of this is permanent.
    # If you wish to get your original path back please restart your shell
    # THIS WILL ALSO NOT TAKE INTO ACCOUNT YOUR BUILD.TOML!
    #
    # 0: NONE will not append to your path at all
    # 1: PREPEND to your path so it will "blinkPath;%PATH%"
    # 2: POSTPEND to your path "%PATH%;blinkPath;"
    """;


    public const string BaseBuildTOML = """
    # YOU MUST put every arg into its own string in a list, because of how the paths and stuff work this is the best way i found to do this. Also it looks nicer
    test = ["echo", "hello"]


    """;
}
