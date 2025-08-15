using Tomlyn;
using Tomlyn.Model;

/// <summary>
/// handles almost everything related to Build.toml and Config.toml
/// </summary>
static class TOMLHandler
{
    /// <summary>
    /// Gets the path from the toml and sets the blinkfs.path
    /// </summary>
    /// <returns></returns>
    public static void GetPathFromTOML()
    {
        TomlArray path = (TomlArray)GetVarFromConfigTOML(Config.PathKey);
        List<string> pathVars = TOMLArrayToList(path);
        BlinkFS.path = pathVars;

    }

    public static void PutPathToTOML()
    {
        TomlTable toml = GetConfigTOML();
        toml[Config.PathKey] = BlinkFS.path;
        PutTOML(toml, Config.ConfigTomlPath);
    }
    /// <summary>
    /// Returns an object and you cast the type onto it based on what it is in the TOML
    /// </summary>
    public static object GetVarFromTOML(string tomlPath, string var)
    {
        TomlTable table = GetTOML(tomlPath);

        if (!table.ContainsKey(var))
            throw new BlinkTOMLException($"Key '{var}' Does not exist in {Path.GetFileName(tomlPath)}");

        return table[var];
    }

    /// <summary>
    /// Returns an object and you cast the type onto it based on what it is in the TOML
    /// </summary>
    public static object GetVarFromTOML(TomlTable tomlTable, string var, string tomlName)
    {
        if (!tomlTable.ContainsKey(var))
            throw new BlinkTOMLException($"Key: '{var}' does not exist in {tomlName}.toml");

        return tomlTable[var];
    }

    public static object GetVarFromConfigTOML(string var)
    {
        return GetVarFromTOML(GetConfigTOML(), var, "config");
    }

    public static object GetVarFromBuildTOML(string var)
    {

        return GetVarFromTOML(GetBuildTOML(), var, "build");


    }


    public static TomlTable GetConfigTOML()
    {
        return GetTOML(Config.ConfigTomlPath);
    }

    public static TomlTable GetBuildTOML()
    {
        return GetTOML(Config.BuildTomlPath);
    }

    /// <summary>
    /// Gets the TOML using the absolute path
    /// </summary>
    public static TomlTable GetTOML(string path)
    {
        try
        {
            return Toml.ToModel(BlinkFS.ReadFile(path));
        }
        catch (TomlException)
        {
            throw new BlinkTOMLException(@$"TOML: '{path}' is invalid, it most likely has an invalid character in one of its strings, probably an escape character in a path. Please always use \\ in paths");
        }
    }


    public static void PutTOML(TomlTable tomlTable, string path)
    {
        string textTOML = Toml.FromModel(tomlTable);
        BlinkFS.WriteFile(path, textTOML);
    }

    public static List<string> TOMLArrayToList(TomlArray array)
    {
        List<string> fixedTOMLArray = new();
        for (int i = 0; i < array.Count; i++)
        {
            string? element = (string?)array[i];
            if (element != null)
            {
                fixedTOMLArray.Add(element);
            }
            else
            {
                throw new BlinkTOMLException($"Array: '{array}' has a null value at element {i}");
            }


        }
        return fixedTOMLArray;
    }

    public static List<string> GetAllCommandsInBuildTOML()
    {
        TomlTable buildTOML = GetBuildTOML();
        List<string> commands = new();

        foreach (object command in buildTOML.Keys)
        {
            string castedString;
            try
            {
                castedString = (string)command;
            }
            catch (InvalidCastException)
            {
                continue;
            }

            commands.Add(castedString);
        }

        return commands;
    }


    public static List<string> GetAllPathVarsInConfigTOML()
    {
        TomlArray path = (TomlArray)GetVarFromConfigTOML(Config.PathKey);
        List<string> pathVars = TOMLArrayToList(path);
        return pathVars;
    }

}