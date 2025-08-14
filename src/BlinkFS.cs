
using System.IO.Compression;
using System.Text.RegularExpressions;
using Tomlyn.Model;

/// <summary>
/// Manages things related to the .blink folder and the projects structure and integrity 
/// </summary>
static class BlinkFS
{
    public static string fileSystemRoot = string.Empty;

    public static List<string> path = new();

    public static void ZipFileSystem(string zipToPath)
    {
        try
        {
            ZipFile.CreateFromDirectory(fileSystemRoot, zipToPath);
        }
        catch (IOException e)
        {
            throw new BlinkFSException($"Error Zipping '{zipToPath}' : {e}");
        }
    }

    public static string ReadFile(string filePath)
    {

        filePath = MakePathAbsolute(filePath);
        if (!File.Exists(filePath))
            throw new BlinkFSException($"Could not read File, '{filePath}' Does not exist");

        try
        {
            return File.ReadAllText(filePath);
        }
        catch (UnauthorizedAccessException)
        {
            throw new BlinkFSException($"Could not read '{filePath}' Permission denied");
        }
        catch (IOException e)
        {
            throw new BlinkFSException($"Error reading file '{filePath}' : {e}");
        }
    }


    /// <summary>
    /// overwrites entire file with specified text
    /// </summary>
    public static void WriteFile(string filePath, string text)
    {


        filePath = MakePathAbsolute(filePath);

        try
        {
            File.WriteAllText(filePath, text);
        }
        catch (UnauthorizedAccessException)
        {
            throw new BlinkFSException($"Could not write '{filePath}' Permission denied");
        }
        catch (IOException e)
        {
            throw new BlinkFSException($"Error Writing file '{filePath}' : {e}");
        }
    }

    public static void DeleteFile(string filePath)
    {
        filePath = MakePathAbsolute(filePath);

        if (!File.Exists(filePath))
            throw new BlinkFSException($"Could not delete File, '{filePath}' Does not exist");
        try
        {
            File.Delete(filePath);
        }
        catch (UnauthorizedAccessException)
        {
            throw new BlinkFSException($"Could not delete '{filePath}' Permission denied");
        }
        catch (IOException e)
        {
            throw new BlinkFSException($"Error Deleting file '{filePath}' : {e}");
        }
    }

    public static void CreateDirectory(string filePath)
    {
        filePath = MakePathAbsolute(filePath);
        try
        {
            Directory.CreateDirectory(filePath);
        }
        catch (UnauthorizedAccessException)
        {
            throw new BlinkFSException($"Could not create directory at path '{filePath}' , Permission Denied");
        }
        catch (IOException e)
        {
            throw new BlinkFSException($"Could not create directory '{filePath}' : {e} ");
        }
    }
    public static void DeleteDirectory(string filePath)
    {
        filePath = MakePathAbsolute(filePath);
        if (!File.Exists(filePath))
            throw new BlinkFSException($"Could not delete directory, '{filePath}' Does not exist");

        try
        {
            Directory.Delete(filePath);
        }
        catch (UnauthorizedAccessException)
        {
            throw new BlinkFSException($"Could not delete directory at path '{filePath}' , Permission Denied");
        }
        catch (IOException e)
        {
            throw new BlinkFSException($"Could not delete directory '{filePath}' : {e} ");
        }
    }


    public static bool IsProgramInPath(string program)
    {
        foreach (string file in path)
        {
            string[] split = file.Split(Config.PathSeparator);

            program = TryRemoveExeEnding(program);

            if (program.Equals(TryRemoveExeEnding(split[split.Length - 1])))
                return true;
        }
        return false;
    }

    /// <summary>
    /// fetches the system path from the blink Env path of a specified program. returns string.empty if not on path
    /// </summary>
    /// <param name="program"></param>
    /// <returns></returns>
    public static string GetProgramOnPathsFilePath(string program)
    {
        foreach (string file in path)
        {
            string[] split = file.Split(Config.PathSeparator);

            program = TryRemoveExeEnding(program);

            if (program.Equals(TryRemoveExeEnding(split[split.Length - 1])))
                return file;
        }
        return string.Empty;
    }

    /// <summary>
    /// simple add program to the current path and it only adds it to the instance path needs to be manually written to the TOML using TOMLHandler.PutPathTOML()
    /// 
    /// </summary>
    /// <param name="program"></param>
    public static void AddProgramToPath(string program)
    {
        if (path.Contains(program))
        {
            throw new BlinkFSException($"File '{program}' Already exists on the path");
        }
        path.Add(program);
    }


    /// <summary>
    /// This will take any path including relative paths. Clean them and turn them absolute including ".\" "\" or "python.exe"
    /// </summary>
    public static string MakePathAbsolute(string path)
    {
        if (path == null)
            return string.Empty;

        //crude path check
        if (path.Contains(Config.PathSeparator))
        {
            if (path.Contains($@".{Config.PathSeparator}"))
            {
                Path.GetFullPath(Path.Combine(fileSystemRoot, path));
            }
        }
        else
        {
            if (IsProgramInPath(path))
            {
                return GetProgramOnPathsFilePath(path);
            }
        }


        if (path.Contains(fileSystemRoot))
        {
            return path;
        }
        else
        {
            return Path.GetFullPath(Path.Combine(fileSystemRoot, path.TrimStart('\\', '/')));
        }
    }

    /// <summary>
    /// Takes an absolute path and returns a path relative to the root of the project.
    /// used for the paths in config.toml
    /// </summary>
    public static string MakePathRelative(string path)
    {
        if (path.Contains(fileSystemRoot))
        {
            return path.Replace(fileSystemRoot, @$".{Config.PathSeparator}");
        }
        else
        {
            if (path.Contains(@$".{Config.PathSeparator}"))
            {
                return path;
            }
            return $@".{Config.PathSeparator}{path}";
        }
    }

    /// <summary>
    /// Tries to remove the exe ending from the path and if it cannot then it returns the original path
    /// </summary>
    //TODO : CHANGE THIS TO BE FILE ENDING NOT JUST EXE
    public static string TryRemoveExeEnding(string path)
    {
        if (path.EndsWith(".exe"))
        {
            return path.Replace(".exe", string.Empty);
        }

        return path;
    }

    /// <summary>
    /// Checks wether or not the file system root changed and if it did then to write that to the config.toml
    /// </summary>
    /// <returns>string: updated root. if updated otherwise unchanged root</returns>
    public static string DidFileSystemRootChange(string root, string currentDir)
    {
        if (root != currentDir)
        {
            IsValidBlinkEnvironment();
            MakeDirFileSystemRoot(currentDir);

            return currentDir;
        }
        return root;
    }

    public static void MakeDirFileSystemRoot(string newRoot)
    {
        TomlTable config = TOMLHandler.GetConfigTOML();
        config[Config.FileSystemRoot] = newRoot;
        TOMLHandler.PutTOML(config, Config.ConfigTomlPath);
    }


    /// <summary>
    /// preps the instance of the program for running, required to be called when starting the program. Every terminal command calls this first.
    /// </summary>
    public static void LoadFileSystem()
    {
        string currentDir = Directory.GetCurrentDirectory();
        fileSystemRoot = currentDir;

        IsValidBlinkEnvironment();
        IsBlinkFileStructureValid();

        string root = (string)TOMLHandler.GetVarFromConfigTOML(Config.FileSystemRoot);

        root = DidFileSystemRootChange(root, currentDir);
        fileSystemRoot = root;

        Config.UpdatePathSeparator();
    }


    /// <summary>
    /// if the current directory is not valid it will throw a custom exception otherwise it will just do nothing
    /// </summary>
    /// <exception cref="InvalidBlinkEnvironment"></exception>
    static void IsValidBlinkEnvironment()
    {
        foreach (string entry in Directory.GetDirectories(fileSystemRoot))
        {
            if (entry.Contains(".blink"))
            {
                return;
            }
        }

        throw new BlinkFSException($"The Directory {Directory.GetCurrentDirectory()}, does not contain a valid .blink folder. Please blink init or restore the .blink folder");
    }

    /// <summary>
    /// makes sure that the config.toml, build.toml, and bin folder exist, otherwise exits program and suggests fixes
    /// </summary>
    public static void IsBlinkFileStructureValid()
    {
        bool configExists = File.Exists(MakePathAbsolute(Config.ConfigTomlPath));
        bool buildExists = File.Exists(MakePathAbsolute(Config.BuildTomlPath));
        bool binExists = Directory.Exists(MakePathAbsolute(Config.BinFolderPath));


        if (configExists && buildExists && binExists)
            return;

        if (!configExists)
            throw new BlinkFSException($"The Config TOML does not exist in the .blink folder for blink to work you need to restore it or run blink verify --fix for a pre configured TOML");
        if (!buildExists)
            throw new BlinkFSException($"The Build TOML does not exist in the .blink folder for blink to work properly you need to restore it or run blink verify --fix for a new build TOML");
        if (!binExists)
            throw new BlinkFSException($"the .blink folder does not contain a bin folder this is essential for language support so please revert it or run blink verify --fix to fix");

    }

    public static string InitLanguageFolder(LanguageSupport.Language lang, string version)
    {
        string versionFolderPath = $".{Config.PathSeparator}.blink{Config.PathSeparator}bin{Config.PathSeparator}{lang}-{version}";
        versionFolderPath = MakePathAbsolute(versionFolderPath);

        if (Directory.Exists(versionFolderPath) && Directory.EnumerateFiles($@"{versionFolderPath}{Config.PathSeparator}").Count() > 0)
        {
            throw new BlinkFSException($"'{lang}' with version '{version}' is already installed in the blink environment if the binaries are not installed please delete the '{lang}-{version}' folder in .blink\\bin");
        }

        string absoluteFolderPath = MakePathAbsolute(versionFolderPath);

        CreateDirectory(absoluteFolderPath);
        
        return absoluteFolderPath;
    }

    public static void FixFileStructure()
    {
        return;
    }
}