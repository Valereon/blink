using System.IO.Compression;
using System.Text.RegularExpressions;

/// <summary>
/// Manages things releated to the .blink folder and the projects structure and integrity 
/// </summary>
static class BlinkFS
{
    public static string fileSystemRoot = string.Empty;

    public static List<string> path = new();


    public static void ExtractFileSystem(string zipPath, string extractPath)
    {
        try
        {
            ZipFile.ExtractToDirectory(zipPath, extractPath);
            fileSystemRoot = extractPath;
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine($"File {zipPath} Does not exist");
            Environment.Exit(1);
        }
        catch (UnauthorizedAccessException)
        {
            Console.WriteLine($"Could not read {zipPath} Permission denied");
            Environment.Exit(1);
        }
        catch (IOException e)
        {
            Console.WriteLine($"Error IOException {e}");
            Environment.Exit(1);
        }
    }

    public static void ZipFileSystem(string zipToPath)
    {
        try
        {
            ZipFile.CreateFromDirectory(fileSystemRoot, zipToPath);
        }
        catch (IOException e)
        {
            Console.WriteLine($"Error IOException {e}");
            Environment.Exit(1);
        }
    }

    /// <summary>
    /// reads a file tile the end, takes a path
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static string ReadFile(string filePath)
    {
        try
        {
            return File.ReadAllText(MakePathAbsolute(filePath));
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine($"Could not read File, {filePath} Does not exist");
        }
        catch (UnauthorizedAccessException)
        {
            Console.WriteLine($"Could not read {filePath} Permission denied");
        }
        catch (IOException e)
        {
            Console.WriteLine($"Error IOException {e}");
        }
        Environment.Exit(1);
        return null;
    }


    /// <summary>
    /// overwrites entire file with specified text
    /// </summary>
    public static void WriteFile(string filePath, string text)
    {
        try
        {
            File.WriteAllText(MakePathAbsolute(filePath), text);
            return;
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine($"could not write to File, {filePath} Does not exist");
            Environment.Exit(1);
        }
        catch (UnauthorizedAccessException)
        {
            Console.WriteLine($"Could not write {filePath} Permission denied");
            Environment.Exit(1);
        }
        catch (IOException e)
        {
            Console.WriteLine($"Error IOException {e}");
            Environment.Exit(1);
        }
    }

    public static void DeleteFile(string filePath)
    {
        try
        {
            File.Delete(MakePathAbsolute(filePath));
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine($"could not delete File, {filePath} Does not exist");
            Environment.Exit(1);
        }
        catch (UnauthorizedAccessException)
        {
            Console.WriteLine($"Could not delete {filePath} Permission denied");
            Environment.Exit(1);
        }
        catch (IOException e)
        {
            Console.WriteLine($"Error IOException {e}");
            Environment.Exit(1);
        }
    }

    public static bool IsProgramInPath(string program)
    {
        foreach (string file in path)
        {
            string[] split = file.Split(Config.PathSeperator);

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
            string[] split = file.Split(Config.PathSeperator);

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
        path.Add(program);
    }


    /// <summary>
    /// This will take any path including relative paths. Clean them and turn them absoulute including ".\" "\" or "python.exe"
    /// </summary>
    public static string MakePathAbsolute(string path)
    {
        if (path == null)
            return string.Empty;

        //crude path check
        if (path.Contains(Config.PathSeperator))
        {
            if (path.Contains($@".{Config.PathSeperator}"))
            {
                path = Regex.Replace(path, @"\.[/\\]", Config.PathSeperator);
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
            // ??????? I DONT KNOW WHY THE FUCK Path.Combine() fucks this up so hard but this works great! i think it has to do with my logic above
            // i BELIEVE its because path is blah\blah\blah and the root is \inerg\ergner\gin so the path is missing the \ making it just a string and not a path
            return fileSystemRoot + path;
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
            return path.Replace(fileSystemRoot, @$".{Config.PathSeperator}");
        }
        else
        {
            return $@".{Config.PathSeperator}{path}";
        }
    }

    /// <summary>
    /// Tries to remove the exe ending from the path and if it cannot then it returns the original path
    /// </summary>
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
    /// <returns>string updated root if updated otherwise root</returns>
    public static string DidFileSystemRootChange(string root, string currentDir)
    {
        if (root != currentDir)
        {
            IsValidBlinkEnvironment();
            root = currentDir;
            Tomlyn.Model.TomlTable config = TOMLHandler.GetConfigTOML();
            config[Config.FileSystemRoot] = root;
            TOMLHandler.PutTOML(config, Config.ConfigTomlPath);
            return root;

        }
        return root;
    }


    /// <summary>
    /// preps the instance of the program for running, required to be called when starting the program. Every terminal command calls this first.
    /// </summary>
    public static void LoadFileSystemRoot()
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
            Console.WriteLine(entry);
        }

        Console.WriteLine($"The Directory {Directory.GetCurrentDirectory()}, does not contain a valid .blink folder. Please blink init or restore the .blink folder");
        Environment.Exit(1);
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
            Console.WriteLine($"The Config TOML does not exist in the .blink folder for blink to work you need to restore it or run blink verify --fix for a pre configured TOML");
        if (!buildExists)
            Console.WriteLine($"The Build TOML does not exist in the .blink folder for blink to work properly you need to restore it or run blink verify --fix for a new build TOML");
        if (!binExists)
            Console.WriteLine($"the .blink folder does not contain a bin folder this is essential for language support so please revert it or run blink verify --fix to fix");

        Environment.Exit(1);
    }


    public static void FixFileStructure()
    {
        return;
    }
}