using Tomlyn;

static class TOMLHandler
{
    public static string configTomlPath = @".\.blink\config.toml";
    public static string buildTomlPath = @".\.blink\build.toml";

    public static void GetPathFromTOML()
    {
        Tomlyn.Model.TomlArray path = (Tomlyn.Model.TomlArray)GetVarFromConfigTOML(Config.PathKey);
        List<string> pathVars = TOMLArrayToList(path);
        BlinkFS.path = pathVars;
    }
    public static void PutPathToTOML()
    {
        Tomlyn.Model.TomlTable toml = GetConfigTOML();
        toml[Config.PathKey] = BlinkFS.path;
        PutTOML(toml, configTomlPath);
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
        try
        {
            return tomlTable[var];
        }
        catch (KeyNotFoundException)
        {
            Console.WriteLine($"Key {var} not found in TOML {tomlTable}");
            Environment.Exit(1);
            return null;
        }


    }

    public static object GetVarFromConfigTOML(string var)
    {
        try
        {
            return GetVarFromTOML(GetConfigTOML(), var);
        }
        catch (KeyNotFoundException)
        {
            Console.WriteLine($"Error: Key '{var}' does not exist in the Config TOML");
            Environment.Exit(1);
            return null;
        }
    }

    public static object GetVarFromBuildTOML(string var)
    {
        try
        {
            return GetVarFromTOML(GetBuildTOML(), var);
        }
        catch (KeyNotFoundException)
        {
            Console.WriteLine($"Error: Key '{var}' does not exist in the Build TOML");
            Environment.Exit(1);
            return null;
        }
    }


    public static Tomlyn.Model.TomlTable GetConfigTOML()
    {
        return GetTOML(configTomlPath);
    }

    public static Tomlyn.Model.TomlTable GetBuildTOML()
    {
        return GetTOML(buildTomlPath);
    }

    /// <summary>
    /// Gets the TOML using the absolute path
    /// </summary>
    public static Tomlyn.Model.TomlTable GetTOML(string path)
    {
        return Toml.ToModel(BlinkFS.ReadFile(path));
    }


    public static void PutTOML(Tomlyn.Model.TomlTable tomlTable, string path)
    {
        string textTOML = Toml.FromModel(tomlTable);
        BlinkFS.WriteFile(path, textTOML);
    }

    public static List<string> TOMLArrayToList(Tomlyn.Model.TomlArray array)
    {
        return array.Select(x => x.ToString()).ToList();
    }

}