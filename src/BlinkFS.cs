using System.CommandLine;
using System.Diagnostics;
using System.IO.Compression;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic;
using Tomlyn;


static class BlinkFS
{
    public static string fileSystemRoot;

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
        }
        catch (UnauthorizedAccessException)
        {
            Console.WriteLine($"Could not read {zipPath} Permission denied");
        }
        catch (IOException e)
        {
            Console.WriteLine($"Error IOException {e}");
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
        }
    }


    public static string ReadFile(string filePath)
    {
        try
        {
            return File.ReadAllText(MakePathAbsoulute(filePath));
        }
        catch (FileNotFoundException)
        {
            throw new FileNotFoundException($"Could not read File, {filePath} Does not exist");
        }
        catch (UnauthorizedAccessException)
        {
            throw new UnauthorizedAccessException($"Could not read {filePath} Permission denied");
        }
        catch (IOException e)
        {
            throw new IOException($"Error IOException {e}");
        }
    }

    public static void WriteFile(string filePath, string text)
    {
        try
        {
            File.WriteAllText(MakePathAbsoulute(filePath), text);

        }
        catch (FileNotFoundException)
        {
            throw new FileNotFoundException($"could not write to File, {filePath} Does not exist");
        }
        catch (UnauthorizedAccessException)
        {
            throw new UnauthorizedAccessException($"Could not write {filePath} Permission denied");
        }
        catch (IOException e)
        {
            throw new IOException($"Error IOException {e}");
        }
    }

    public static void DeleteFile(string filePath)
    {
        try
        {
            File.Delete(MakePathAbsoulute(filePath));
        }
        catch (FileNotFoundException)
        {
            throw new FileNotFoundException($"could not delete File, {filePath} Does not exist");
        }
        catch (UnauthorizedAccessException)
        {
            throw new UnauthorizedAccessException($"Could not delete {filePath} Permission denied");
        }
        catch (IOException e)
        {
            throw new IOException($"Error IOException {e}");
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


    public static string GetProgramOnPathsFilePath(string program)
    {
        foreach (string file in path)
        {
            string[] split = file.Split(Config.PathSeperator);

            program = TryRemoveExeEnding(program);

            if (program.Equals(TryRemoveExeEnding(split[split.Length - 1])))
                return file;
        }
        return null;
    }

    public static void ReloadPath()
    {
        TOMLHandler.PutPathToTOML();
        TOMLHandler.GetPathFromTOML();
    }

    public static void AddProgramToPath(string program)
    {
        path.Add(program);
    }


    /// <summary>
    /// This will take any path including relative paths. Clean them and turn them absoulute including ".\" "\" or "python.exe"
    /// </summary>
    public static string MakePathAbsoulute(string path)
    {
        if (path == null)
            throw new Exception("Path is null in MakePathAbsolute");

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
            return path.Replace(fileSystemRoot, @$".{Config.PathSeperator}");
        }
        else
        {
            return $@".{Config.PathSeperator}{path}";
        }
    }

    /// <summary>
    /// Trys to remove the exe ending from the path and if it cannot then it returns the original path
    /// </summary>
    public static string TryRemoveExeEnding(string path)
    {
        if (path.EndsWith(".exe"))
        {
            return path.Replace(".exe", "");
        }

        return path;
    }

}