using System.Runtime.InteropServices;
using Tomlyn;

/// <summary>
/// handles almost everything related to Build.toml and Config.toml
/// </summary>
static class TOMLHandler
{
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
        PutTOML(toml, Config.ConfigTomlPath);
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
    public static object GetVarFromTOML(Tomlyn.Model.TomlTable tomlTable, string var, bool bubbleUp = false)
    {
        try
        {
            return tomlTable[var];
        }
        catch (KeyNotFoundException) when (bubbleUp == false)
        {
            Console.WriteLine($"Error: Key '{var}' does not exits in the TOML. (program doesn't know which toml based on this call)");
            Environment.Exit(1);
            return null;
        }
    }

    public static object GetVarFromConfigTOML(string var)
    {
        try
        {
            return GetVarFromTOML(GetConfigTOML(), var, true);
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
            return GetVarFromTOML(GetBuildTOML(), var, true);
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
        return GetTOML(Config.ConfigTomlPath);
    }

    public static Tomlyn.Model.TomlTable GetBuildTOML()
    {
        return GetTOML(Config.BuildTomlPath);
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
                Console.WriteLine($"Array {array} has a null value at element {i}");
                Environment.Exit(1);
            }


        }
        return fixedTOMLArray;
    }

    public static List<string> GetAllCommandsInBuildTOML()
    {
        Tomlyn.Model.TomlTable buildTOML = GetBuildTOML();
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