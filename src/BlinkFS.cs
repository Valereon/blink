using System.IO.Compression;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic;
using Tomlyn;


static class BlinkFS
{
    public static string fileSystemRoot = @"D:\Github\GitHub\BlinkOS\";
    public static string configToml = @".\.blink\config.toml";
    public static string buildToml = @".\.blink\build.toml";

    public static List<string> filesOnPath = new();

    static Tomlyn.Model.TomlTable pathToml;




    public static void ExtractFileSystem(string zipPath, string extractPath)
    {
        ZipFile.ExtractToDirectory(zipPath, extractPath);
        fileSystemRoot = extractPath;
    }

    public static void ZipFileSystem(string zipToPath)
    {
        ZipFile.CreateFromDirectory(fileSystemRoot, zipToPath);
    }


    public static string ReadFile(string filePath)
    {
        return File.ReadAllText(MakePathAbsoulute(filePath));
    }

    public static void WriteFile(string filepath, string text)
    {
        File.WriteAllText(MakePathAbsoulute(filepath), text);
    }

    public static void DeleteFile(string filePath)
    {
        File.Delete(MakePathAbsoulute(filePath));
    }



    /// <summary>
    /// Gets the files in the directory and returns their full filepaths and will NOT return directories
    /// </summary>
    public static string[] GetFilesInDirectory(string directoryPath)
    {
        string[] filePaths = Directory.GetFiles(directoryPath);
        filePaths = SortFilesAndFolders(filePaths);
        return filePaths;
    }

    /// <summary>
    /// Gets the files in the directory and returns their full filepaths and WILL return directories
    /// </summary>
    public static string[] GetFilesAndFoldersInDirectory(string directoryPath)
    {
        string[] filePaths = Directory.GetFileSystemEntries(directoryPath, "*", SearchOption.TopDirectoryOnly);
        filePaths = SortFilesAndFolders(filePaths);
        return filePaths;
    }

    /// <summary>
    /// Gets the files in the directory and returns their names i.e. Sample.txt this will NOT return directories
    /// </summary>
    public static string[] GetFilesInDirectoryWithoutPaths(string directoryPath)
    {
        string[] filePaths = Directory.GetFiles(directoryPath);
        filePaths = SortFilesAndFolders(filePaths);
        for (int i = 0; i < filePaths.Length; i++)
        {
            filePaths[i] = filePaths[i].Replace(directoryPath, "");
        }
        return filePaths;
    }

    /// <summary>
    /// Gets the files in the directory and returns their full filepaths and WILL return directories
    /// </summary>
    public static string[] GetFilesAndFoldersInDirectoryWithoutPaths(string directoryPath)
    {
        string[] filePaths = Directory.GetFileSystemEntries(directoryPath, "*", SearchOption.TopDirectoryOnly);
        filePaths = SortFilesAndFolders(filePaths);

        for (int i = 0; i < filePaths.Length; i++)
        {
            filePaths[i] = filePaths[i].Replace(directoryPath, "");
        }
        return filePaths;
    }

    /// <summary>
    /// Takes a list of files and directories and sorts them so the folders are on top and files are alphabetical
    /// </summary>
    public static string[] SortFilesAndFolders(string[] files)
    {
        List<string> sortedfolders = new();
        List<string> sortedFiles = new();

        for (int i = 0; i < files.Length; i++)
        {
            FileAttributes attributes = File.GetAttributes(files[i]);

            if (attributes.HasFlag(FileAttributes.Directory))
            {
                sortedfolders.Add(files[i]);

            }
            else
            {
                sortedFiles.Add(files[i]);
            }
        }

        if (sortedfolders.Count == 0)
        {
            sortedFiles.Sort();
            return sortedFiles.ToArray();
        }
        if (sortedFiles.Count == 0)
        {
            sortedfolders.Sort();
            return sortedfolders.ToArray();
        }

        sortedfolders.Sort();
        sortedFiles.Sort();

        sortedfolders.AddRange(sortedFiles);

        return sortedfolders.ToArray();
    }

    public static bool IsProgramInPath(string program)
    {

        foreach (string file in filesOnPath)
        {
            string[] split = file.Split(@"\");

            program = TryRemoveExeEnding(program);

            if (program.Equals(TryRemoveExeEnding(split[split.Length - 1])))
                return true;
        }
        return false;
    }

    //TODO: make it so that you dont have to put file extensions for exes
    public static string GetProgramOnPathsPath(string program)
    {
        foreach (string file in filesOnPath)
        {
            string[] split = file.Split(@"\");

            program = TryRemoveExeEnding(program);

            if (program.Equals(TryRemoveExeEnding(split[split.Length - 1])))
                return file;
        }
        return null;
    }

    public static void AddProgramToPath(string program)
    {
        filesOnPath.Add(program);
    }

    public static void GetPathFromTOML()
    {
        //TODO: FIX THIS there is a type
        // i hate to use var here but they did not provide a type??? in the TOML lib
        var model = GetTOML(configToml);
        pathToml = model;

        Tomlyn.Model.TomlArray path = (Tomlyn.Model.TomlArray)model["path"];
        List<string> pathVars = path.Select(x => x.ToString()).ToList();
        filesOnPath = pathVars;
    }
    public static void PutPathToTOML()
    {
        pathToml["path"] = filesOnPath;
        PutTOML(pathToml, configToml);
    }




    /// <summary>
    /// Gets the TOML using the absolute path
    /// </summary>
    public static Tomlyn.Model.TomlTable GetTOML(string path)
    {
        return Toml.ToModel(ReadFile(MakePathAbsoulute(path)));
    }


    public static void PutTOML(Tomlyn.Model.TomlTable tomlTable, string path)
    {
        string textTOML = Toml.FromModel(tomlTable);
        WriteFile(MakePathAbsoulute(path), textTOML);
    }

    public static List<string> TOMLArrayToList(Tomlyn.Model.TomlArray array)
    {
        return array.Select(x => x.ToString()).ToList();
    }



    /// <summary>
    /// This will take any path including relative paths clean them and turn them absoulute including ".\" "\" or "erg"
    /// </summary>
    public static string MakePathAbsoulute(string path)
    {

        //crude path check
        if (path.Contains(@"\"))
        {
            if (path.Contains(@".\"))
            {
                path = Regex.Replace(path, @"\.\\", @"\");
            }
        }
        else
        {
            return path;
        }

        if (path.Contains(fileSystemRoot))
        {
            return path;
        }
        else
        {
            return fileSystemRoot + path;
        }
    }

    /// <summary>
    /// Takes an absoulute path and returns a path releative to the root of the project.
    /// used for the paths in config.toml
    /// </summary>
    public static string MakePathRelative(string path)
    {
        if (path.Contains(fileSystemRoot))
        {

            path.Replace(fileSystemRoot, @".\");
            return path;

        }
        else
        {
            return @".\" + path;
        }
    }

    public static string TryRemoveExeEnding(string path)
    {
        if (path.EndsWith(".exe"))
        {
            return path.Replace(".exe", "");
        }


        return path;        
    }

}