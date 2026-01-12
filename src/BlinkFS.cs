using System.Diagnostics;
using System.Text.RegularExpressions;
using Tomlyn.Model;

/// <summary>
/// Manages things related to the .blink folder and the projects structure and integrity 
/// </summary>
class BlinkFS
{
    /// <summary>
    /// Checks wether or not the file system root changed and if it did then to write that to the config.toml
    /// </summary>
    /// <returns>string: updated root. if updated otherwise unchanged root</returns>
    private string DidFileSystemRootChange(string root, string currentDir)
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
    private void MakeDirFileSystemRoot(string newRoot)
    {
        TomlTable configTable = TOMLHandler.GetConfigTOML();
        configTable[Config.FileSystemRoot] = newRoot;
        TOMLHandler.PutConfigTOMLWithTable(configTable);
    }


    /// <summary>
    /// preps the instance of the program for running, required to be called when starting the program. Every terminal command calls this first.
    /// </summary>
    public void LoadBlinkFS()
    {
        string currentDir = Directory.GetCurrentDirectory();
        _fileSystemRoot = currentDir;


        IsValidBlinkEnvironment();

        string root = (string)TOMLHandler.GetVarFromConfigTOML(Config.FileSystemRoot);

        TOMLHandler.GetPathFromTOML();
        MakePathVarsRelative();
        TOMLHandler.PutPathToTOML();

        _fileSystemRoot = DidFileSystemRootChange(root, currentDir);

        Config.UpdatePathSeparator();
        SetupShellEnv();
    }


    /// <summary>
    /// if the current directory is not valid it will throw a custom exception otherwise it will just do nothing
    /// </summary>
    /// <exception cref="InvalidBlinkEnvironment"></exception>
    private void IsValidBlinkEnvironment()
    {
        foreach (string entry in Directory.GetDirectories(_fileSystemRoot))
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

    public string InitLanguageFolder(LanguageSupport.Language lang, string version)
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


    private void MakePathVarsRelative()
    {
        for (int i = 0; i < _path.Count; i++)
        {
            _path[i] = MakePathRelative(_path[i]);
        }
    }

    //TODO: make this better? seems a little fucky wucky with the paths idk just take a look
    private void SetupShellEnv()
    {
        ProcessStartInfo psi = new ProcessStartInfo();
        psi.FileName = Config.pathKeyword;
        long mode = (long)TOMLHandler.GetVarFromConfigTOML(Config.pathMode);

        string final = "";
        foreach (string var in _path)
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