using Tomlyn;

static class TOMLHandler
{
    public static string configToml = @".\.blink\config.toml";
    public static string buildToml = @".\.blink\build.toml";

    public static void GetPathFromTOML()
    {
        Tomlyn.Model.TomlArray path = (Tomlyn.Model.TomlArray)GetVarFromConfigTOML(Config.Path);
        List<string> pathVars = path.Select(x => x.ToString()).ToList();
        BlinkFS.path = pathVars;
    }
    public static void PutPathToTOML()
    {
        Tomlyn.Model.TomlTable toml = GetConfigTOML();
        toml[Config.Path] = BlinkFS.path;
        PutTOML(toml, configToml);
    }
    /// <summary>
    /// Returns an object and you cast the type onto it based on what it is in the TOML
    /// </summary>
    public static object GetVarFromTOML(string tomlPath, string var)
    {
        Tomlyn.Model.TomlTable table = GetTOML(tomlPath);
        return table[var];
        
    }
    
    /// <summary>
    /// Returns an object and you cast the type onto it based on what it is in the TOML
    /// </summary>
    public static object GetVarFromTOML(Tomlyn.Model.TomlTable tomlTable, string var)
    {
        return tomlTable[var];
    }

    public static object GetVarFromConfigTOML(string var)
    {
        return GetVarFromTOML(GetConfigTOML(), var);
    }

    public static object GetVarFromBuildTOML(string var)
    {
        return GetVarFromTOML(GetBuildTOML(), var);
    }


    public static Tomlyn.Model.TomlTable GetConfigTOML()
    {
        return GetTOML(configToml);
    }

    public static Tomlyn.Model.TomlTable GetBuildTOML()
    {
        return GetTOML(buildToml);
    }

    /// <summary>
    /// Gets the TOML using the absolute path
    /// </summary>
    public static Tomlyn.Model.TomlTable GetTOML(string path)
    {
        return Toml.ToModel(BlinkFS.ReadFile(BlinkFS.MakePathAbsoulute(path)));
    }


    public static void PutTOML(Tomlyn.Model.TomlTable tomlTable, string path)
    {
        string textTOML = Toml.FromModel(tomlTable);
        BlinkFS.WriteFile(BlinkFS.MakePathAbsoulute(path), textTOML);
    }

    public static List<string> TOMLArrayToList(Tomlyn.Model.TomlArray array)
    {
        return array.Select(x => x.ToString()).ToList();
    }

}