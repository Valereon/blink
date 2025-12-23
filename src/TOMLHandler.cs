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
    public static void GetPathFromTOML()
    {
        TomlArray path = (TomlArray)GetVarFromConfigTOML(Config.PathKey);
        BlinkFS.GetPathList().Clear();
        BlinkFS.GetPathList().AddRange(TOMLArrayToList(path));
    }

    /// <summary>
    /// Change the config.toml path to have the current path of the running instance
    /// </summary>
    public static void PutPathToTOML()
    {
        TomlTable toml = GetConfigTOML();
        toml[Config.PathKey] = BlinkFS.GetPathList();
        PutTOML(toml, Config.ConfigTomlPath);
    }

    /// <summary>
    /// Gets the specified var from the provided TOML
    /// </summary>
    /// <param name="tomlPath"></param>
    /// <param name="var"></param>
    /// <returns>Returns an object and you cast the type onto it based on what it is in the TOML</returns>
    /// <exception cref="BlinkTOMLException"></exception>
    public static object GetVarFromTOML(string tomlPath, string var)
    {
        TomlTable table = GetTOML(tomlPath);

        if (!table.ContainsKey(var))
            throw new BlinkTOMLException($"Key '{var}' Does not exist in {Path.GetFileName(tomlPath)}");

        return table[var];
    }




    /// <summary>
    /// Gets the specified var from the provided TOML
    /// </summary>
    /// <param name="tomlTable"></param>
    /// <param name="var"></param>
    /// <param name="tomlName"></param>
    /// <returns>Returns an object and you cast the type onto it based on what it is in the TOML</returns>
    /// <exception cref="BlinkTOMLException"></exception>
    public static object GetVarFromTOML(TomlTable tomlTable, string var, string tomlName)
    {
        if (!tomlTable.ContainsKey(var))
            throw new BlinkTOMLException($"Key: '{var}' does not exist in {tomlName}.toml");

        return tomlTable[var];
    }

    /// <summary>
    /// Gets the specified var from the config TOML
    /// </summary>
    /// <param name="var"></param>
    /// <returns>Returns an object and you cast the type onto it based on what it is in the TOML</returns>
    public static object GetVarFromConfigTOML(string var)
    {
        return GetVarFromTOML(GetConfigTOML(), var, "config");
    }

    /// <summary>
    /// Gets the specified var from the build TOML
    /// </summary>
    /// <param name="var"></param>
    /// <returns>Returns an object and you cast the type onto it based on what it is in the TOML</returns>
    public static object GetVarFromBuildTOML(string var)
    {
        return GetVarFromTOML(GetBuildTOML(), var, "build");
    }

    /// <summary>
    /// Does the provided key exist in the provided TOML
    /// </summary>
    /// <param name="key"></param>
    /// <param name="table"></param>
    /// <returns>true if it exists, and false if it doesn't</returns>
    public static bool DoesKeyExistInTOML(string key, TomlTable table)
    {
        return table.ContainsKey(key);
    }

    /// <summary>
    /// Gets the config TOML inside \blink
    /// </summary>
    /// <returns>TomlTable, the config TOML</returns>
    public static TomlTable GetConfigTOML()
    {
        return GetTOML(Config.ConfigTomlPath);
    }
    /// <summary>
    /// Gets the config TOML inside \blink
    /// </summary>
    /// <returns>TomlTable, the config TOML</returns>
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
            throw new BlinkTOMLException($"TOML: '{path}' is invalid, it most likely One of these reasons.\nIt has an invalid character in one of its strings, probably an escape character in a path. Please always use \\\\ in paths.\nParams are seperated by list indices in the build.toml Like\n\tcommand = [\"ls\", \"-l\"]");
        }
    }

    /// <summary>
    /// Takes the given TOML and puts it to a file on the specified path, TOML name must be included in path such as "\home\user\build.toml"
    /// </summary>
    /// <param name="tomlTable"></param>
    /// <param name="path"></param>
    public static void PutTOML(TomlTable tomlTable, string path)
    {
        string textTOML = Toml.FromModel(tomlTable);
        BlinkFS.WriteFile(path, textTOML);
    }

    public static void PutConfigTOMLWithTable(TomlTable configTable)
    {
        string textTOML = Toml.FromModel(configTable);
        BlinkFS.WriteFile(Config.ConfigTomlPath, textTOML);
    }
    public static void PutBuildTOMLWithTable(TomlTable buildTable)
    {
        string textTOML = Toml.FromModel(buildTable);
        BlinkFS.WriteFile(Config.BuildTomlPath, textTOML);
    }

    public static void AddKeyToBuildTOML<T>(string key, T value)
    {
        TomlTable build = GetBuildTOML();
        build[key] = value!;
        PutBuildTOMLWithTable(build);
    }


    /// <summary>
    /// Takes a TOML array and converts to csharp list
    /// </summary>
    /// <param name="array"></param>
    /// <returns>csharp list of TOML array</returns>
    /// <exception cref="BlinkTOMLException"></exception>
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


    /// <summary>
    /// one off method that returns the name of all custom commands inside of Build TOML
    /// </summary>
    /// <returns>List<String> the name of all commands in build toml</returns>
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
}