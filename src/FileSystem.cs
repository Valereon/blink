public class FileSystem
{

    private string _fileSystemRoot = string.Empty;

    private List<string> _path = new();
    
     /// <summary>
    /// reads the entire file. 
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns>the whole file in one string</returns>
    /// <exception cref="BlinkFSException">throws if could not read, or access the file</exception>
    public string ReadFile(string filePath)
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
    public void WriteFile(string filePath, string text)
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
    public void DeleteFile(string filePath)
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
    public void CreateDirectory(string filePath)
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
    public void DeleteDirectory(string filePath)
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


    public void DownloadFile(string url, string pathToSave)
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
                DeleteDirectory(pathToSave);
                throw new BlinkDownloadException($"Url '{url}' has run into an issue, please try again with a different url");
            }
        }
    }


    /// <summary>
    /// checks if program is on the BLINK path not sys path
    /// </summary>
    /// <param name="program"></param>
    /// <returns>Returns true if on path, and false otherwise</returns>
    public bool IsProgramInPath(string program)
    {
        if (program == string.Empty || program == null)
            return false;

        program = RemoveFileExtension(program);
        foreach (string file in _path)
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
    public string GetProgramOnPathsFilePath(string program)
    {
        program = RemoveFileExtension(program);
        foreach (string file in _path)
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
    public void AddProgramToPath(string program)
    {
        if (_path.Contains(program))
        {
            throw new BlinkFSException($"File '{program}' Already exists on the path");
        }
        _path.Add(program);
    }

    public List<string> GetPathList()
    {
        return _path;
    }

    public void SetPathList(List<string> newPath)
    {
        _path = newPath;
    }

    /// <summary>
    /// This will take any path including relative paths. Clean them and turn them absolute including ".\" "\" or "python.exe" (which python.exe would be on the path)
    /// </summary>
    public string MakePathAbsolute(string path)
    {
        if (path == null || path == string.Empty)
            return string.Empty;


        if (path.Contains($@".{Config.PathSeparator}"))
        {
            return Path.GetFullPath(Path.Combine(_fileSystemRoot, path));
        }
        else if (IsProgramInPath(path))
        {
            return GetProgramOnPathsFilePath(path);
        }
        else if (path.Contains(_fileSystemRoot))
        {
            return path;
        }
        else
        {
            return Path.GetFullPath(Path.Combine(_fileSystemRoot, path.TrimStart('\\', '/')));
        }
    }

    /// <summary>
    /// Takes an absolute path and returns a path relative to the root of the project.
    /// used for the paths in config.toml
    /// </summary>
    public string MakePathRelative(string path)
    {
        if (path.Contains(_fileSystemRoot))
        {
            // root\bin\python.exe -> .\bin\python.exe
            return path.Replace(_fileSystemRoot, @$".");
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
    private string RemoveFileExtension(string path)
    {
        return Regex.Replace(path, @"\.[^.]+$", "");
    }
}