
using System.ComponentModel;
using System.Diagnostics;
using System.IO.Compression;
using System.Text.RegularExpressions;
using Tomlyn.Model;

/// <summary>
/// Manages things related to the .blink folder and the projects structure and integrity 
/// </summary>
static class BlinkFS
{
    private static string fileSystemRoot = string.Empty;

    // the blink path
    private static List<string> path = new();

    /// <summary>
    /// reads the entire file. 
    /// 
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns>the whole file in one string</returns>
    /// <exception cref="BlinkFSException">throws if could not read, or access the file</exception>
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

    /// <summary>
    /// Deletes a file given a path.
    /// </summary>
    /// <param name="filePath"></param>
    /// <exception cref="BlinkFSException"></exception>
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

    /// <summary>
    /// Creates a directory based on the filepath, new directory should be appeneded at the end so like c:\\user\\newDir\\
    /// </summary>
    /// <param name="filePath"></param>
    /// <exception cref="BlinkFSException"></exception>
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

    /// <summary>
    /// Deletes a directory given a path
    /// </summary>
    /// <param name="filePath"></param>
    /// <exception cref="BlinkFSException"></exception>
    public static void DeleteDirectory(string filePath)
    {
        filePath = MakePathAbsolute(filePath);
        if (!Directory.Exists(filePath))
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


    public static void DownloadFile(string url, string pathToSave)
    {
        using (HttpClient wc = new HttpClient())
        {
            try
            {
                // stack overflow !! https://stackoverflow.com/a/71949994
                using (var client = new HttpClient())
                {
                    using (var s = client.GetStreamAsync(url))
                    {
                        using (var fs = new FileStream(pathToSave, FileMode.OpenOrCreate))
                        {
                            s.Result.CopyTo(fs);
                        }
                    }
                }
            }
            catch
            {
                BlinkFS.DeleteDirectory(pathToSave);
                throw new BlinkDownloadException($"Url '{url}' has run into an issue, please try again with a different url");
            }
        }
    }


    /// <summary>
    /// checks if program is on the BLINK path not sys path
    /// </summary>
    /// <param name="program"></param>
    /// <returns>Returns true if on path, and false otherwise</returns>
    public static bool IsProgramInPath(string program)
    {
        if (program == string.Empty || program == null)
            return false;

        program = RemoveFileExtension(program);
        foreach (string file in path)
        {
            string[] split = file.Split(Config.PathSeparator);

            string fileOnPath = RemoveFileExtension(split[split.Length - 1]);
            if (program.Equals(fileOnPath))
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
        program = RemoveFileExtension(program);
        foreach (string file in path)
        {
            string[] split = file.Split(Config.PathSeparator);
            string fileOnPath = RemoveFileExtension(split[split.Length - 1]);

            if (program.Equals(fileOnPath))
                return file;
        }
        return string.Empty;
    }

    /// <summary>
    /// simple add program to the current path and it only adds it to the instance path needs to be manually written to the TOML using TOMLHandler.PutPathTOML()
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

    public static List<string> GetPathList()
    {
        return path;
    }

    public static void SetPathList(List<string> newPath)
    {
        path = newPath;
    }

    /// <summary>
    /// This will take any path including relative paths. Clean them and turn them absolute including ".\" "\" or "python.exe" (which python.exe would be on the path)
    /// </summary>
    public static string MakePathAbsolute(string path)
    {
        if (path == null || path == string.Empty)
            return string.Empty;


        if (path.Contains($@".{Config.PathSeparator}"))
        {
            return Path.GetFullPath(Path.Combine(fileSystemRoot, path));
        }
        else if (IsProgramInPath(path))
        {
            return GetProgramOnPathsFilePath(path);
        }
        else if (path.Contains(fileSystemRoot))
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
            // root\bin\python.exe -> .\bin\python.exe
            return path.Replace(fileSystemRoot, @$".");
        }
        else
        {
            if (path.Contains(@$".{Config.PathSeparator}"))
            {
                // .\bin\python.exe
                return path;
            }
            return $@".{Config.PathSeparator}{path}";
        }
    }

    /// <summary>
    /// Removes any file extension
    /// </summary>
    private static string RemoveFileExtension(string path)
    {
        return Regex.Replace(path, @"\.[^.]+$", "");
    }

    /// <summary>
    /// Checks wether or not the file system root changed and if it did then to write that to the config.toml
    /// </summary>
    /// <returns>string: updated root. if updated otherwise unchanged root</returns>
    private static string DidFileSystemRootChange(string root, string currentDir)
    {
        if (root != currentDir)
        {
            MakeDirFileSystemRoot(currentDir);

            return currentDir;
        }
        return root;
    }

    /// <summary>
    /// Edits the config.toml config.FileSystemRoot to be the current folder
    /// </summary>
    /// <param name="newRoot"></param>
    private static void MakeDirFileSystemRoot(string newRoot)
    {
        TomlTable configTable = TOMLHandler.GetConfigTOML();
        configTable[Config.FileSystemRoot] = newRoot;
        TOMLHandler.PutConfigTOMLWithTable(configTable);
    }


    /// <summary>
    /// preps the instance of the program for running, required to be called when starting the program. Every terminal command calls this first.
    /// </summary>
    public static void LoadFileSystem()
    {
        string currentDir = Directory.GetCurrentDirectory();
        fileSystemRoot = currentDir;


        IsValidBlinkEnvironment();

        string root = (string)TOMLHandler.GetVarFromConfigTOML(Config.FileSystemRoot);

        TOMLHandler.GetPathFromTOML();
        MakePathVarsRelative();
        TOMLHandler.PutPathToTOML();

        fileSystemRoot = DidFileSystemRootChange(root, currentDir);

        Config.UpdatePathSeparator();
        SetupShellEnv();
    }


    /// <summary>
    /// if the current directory is not valid it will throw a custom exception otherwise it will just do nothing
    /// </summary>
    /// <exception cref="InvalidBlinkEnvironment"></exception>
    private static void IsValidBlinkEnvironment()
    {
        foreach (string entry in Directory.GetDirectories(fileSystemRoot))
        {
            // Console.WriteLine(entry);
            if (entry.Contains(".blink"))
            {
                break;
            }
            throw new BlinkFSException($"The Directory {Directory.GetCurrentDirectory()}, does not contain a valid .blink folder. Please blink init or restore the .blink folder");
        }

        bool configExists = File.Exists(MakePathAbsolute(Config.ConfigTomlPath));
        bool buildExists = File.Exists(MakePathAbsolute(Config.BuildTomlPath));
        bool binExists = Directory.Exists(MakePathAbsolute(Config.BinFolderPath));

        if (configExists && buildExists && binExists)
            return;

        if (!configExists)
            throw new BlinkFSException($"The Config TOML does not exist in the .blink folder for blink to work you need to restore it or run blink verify for a pre configured TOML");
        if (!buildExists)
            throw new BlinkFSException($"The Build TOML does not exist in the .blink folder for blink to work properly you need to restore it or run blink verify for a new build TOML");
        if (!binExists)
            throw new BlinkFSException($"the .blink folder does not contain a bin folder this is essential for language support so please revert it or create a bin folder");

    }

    public static string InitLanguageFolder(LanguageSupport.Language lang, string version)
    {
        //EX: ".\.blink\bin\python-2.12.2"
        string versionFolderPath = Path.Join(Config.BinFolderPath, $"{lang}-{version}");
        string absoluteVersionFolderPath = MakePathAbsolute(versionFolderPath);

        if (Directory.Exists(absoluteVersionFolderPath) && Directory.EnumerateFiles($@"{absoluteVersionFolderPath}{Config.PathSeparator}").Count() > 0)
        {
            throw new BlinkFSException($"'{lang}' with version '{version}' is already installed in the blink environment if the binaries are not installed please delete the '{lang}-{version}' folder in .blink\\bin");
        }

        string absoluteFolderPath = MakePathAbsolute(absoluteVersionFolderPath);

        CreateDirectory(absoluteFolderPath);

        return versionFolderPath;
    }


    private static void MakePathVarsRelative()
    {
        for (int i = 0; i < path.Count; i++)
        {
            path[i] = MakePathRelative(path[i]);
        }
    }


    private static void SetupShellEnv()
    {
        ProcessStartInfo psi = new ProcessStartInfo();
        psi.FileName = Config.pathKeyword;
        long mode = (long)TOMLHandler.GetVarFromConfigTOML(Config.pathMode);

        string final = "";
        foreach (string var in path)
        {
            final += var + ";";
        }


        switch (mode)
        {
            case 1:
                psi.ArgumentList.Add($"{Config.pathKeyword}{final}");
                break;
            case 2:
                psi.ArgumentList.Add($"{Config.pathKeyword}%PATH%;{final}");
                break;
            default:
                return;
        }


        Process proc = new Process();
        proc.StartInfo = psi;
        proc.Start();
        proc.WaitForExit();

    }

}