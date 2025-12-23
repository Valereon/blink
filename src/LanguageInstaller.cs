using System.IO.Compression;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Tomlyn.Model;

public static class LanguageInstaller
{


    public static void InstallLanguage(LanguageSupport.Language lang, string version)
    {
        switch (lang)
        {
            case LanguageSupport.Language.Python:
                InstallPython(version);
                break;
            case LanguageSupport.Language.NodeJS:
                InstallNodeJS(version);
                break;
        }
    }

    //https://www.python.org/ftp/python/3.13.6/python-3.13.6-embed-amd64.zip
    //windows!
    private static void InstallPython(string version)
    {
        string folderPath = BlinkFS.InitLanguageFolder(LanguageSupport.Language.Python, version);
        string pythonFilePath = folderPath + @$"{Config.PathSeparator}Python-{version}.zip";
        string pipFileName = "pip.pyz";
        string pythonFileName = "python.exe";

        BlinkFS.DownloadFile($"https://www.python.org/ftp/python/{version}/python-{version}-embed-amd64.zip", pythonFilePath);
        // python
        ZipFile.ExtractToDirectory(pythonFilePath, $"{folderPath}");
        BlinkFS.DeleteFile(pythonFilePath);


        //create pip folder and shove pip in it
        //.\.blink\bin\Python-3.13.6\pip.pyz
        string pipDownloadLocation = Path.Join(folderPath, pipFileName);
        BlinkFS.DownloadFile($"https://bootstrap.pypa.io/pip/pip.pyz", pipDownloadLocation);

        //shove pip in a folder
        // .\.blink\bin\Python-3.13.6\pip\pip.pyz
        string pipDirectory = Path.Join(folderPath, "pip");
        BlinkFS.CreateDirectory(pipDirectory);

        string pipPath = Path.Join(pipDirectory, pipFileName);
        File.Move(pipDownloadLocation, pipPath);

        //.\.blink\bin\Python-3.13.6\python.exe
        string pythonPath = Path.Join(folderPath, pythonFileName);
        //.\.blink\bin\Python-3.13.6\pip\pip.pyz



        //.\.blink\bin\Python-3.13.6\python313._pth
        // take the dumb version and turn it into python313 from 3.13.6
        // this is needed to recognize site packages for python, for per install shit.
        string pthFileVersion = Regex.Replace(version, @"\b(\d+)\.(\d+)\.\d+\b", "$1$2");
        string pthFilePath = Path.Join(folderPath, "python", $"{pthFileVersion}._pth");
        EditPythonPathFile(folderPath + pthFilePath);


        if (BlinkFS.IsProgramInPath(pythonFileName) == false && BlinkFS.IsProgramInPath(pipFileName) == false)
        {
            BlinkFS.AddProgramToPath(pipPath);
            BlinkFS.AddProgramToPath(pythonPath);


            List<string> buildList = [pythonPath, pipPath];

            TOMLHandler.AddKeyToBuildTOML("pip", buildList);

            Console.WriteLine($"Python {version} and Pip {version} were added to the path! you can use them like 'blink run python -a main.py' and 'blink run pip -a install pygame'");
        }
        else
        {
            ResolveAlreadyOnPathConflict("python", version, "pip", pythonPath, pipPath, $"pip{version}", [pythonPath, pipPath]);
        }
        TOMLHandler.PutPathToTOML();
    }

    //https://nodejs.org/dist/v22.17.1/node-v22.17.1-win-x64.zip
    // windows!
    private static void InstallNodeJS(string version)
    {
        string folderPath = BlinkFS.InitLanguageFolder(LanguageSupport.Language.NodeJS, version);
        string fileName = folderPath + @$"{Config.PathSeparator}NodeJS-{version}.zip";
        Console.WriteLine(fileName);
        BlinkFS.DownloadFile($"https://nodejs.org/dist/v{version}/node-v{version}-win-x64.zip", fileName);

        ZipFile.ExtractToDirectory(fileName, $"{folderPath}");
        BlinkFS.DeleteFile(fileName);

        string relNode = BlinkFS.MakePathRelative(folderPath + @$"{Config.PathSeparator}node-v{version}-win-x64{Config.PathSeparator}node.exe");
        string relNPM = BlinkFS.MakePathRelative(folderPath + @$"{Config.PathSeparator}node-v{version}-win-x64{Config.PathSeparator}npm.cmd");

        if (BlinkFS.IsProgramInPath("node.exe") == false && BlinkFS.IsProgramInPath("npm.cmd") == false)
        {
            BlinkFS.AddProgramToPath(relNode);
            BlinkFS.AddProgramToPath(relNPM);
        }
        else
        {
            ResolveAlreadyOnPathConflict("node", version, "npm", relNode, relNPM);
        }
        TOMLHandler.PutPathToTOML();
    }


    /// <summary>
    /// resolves errors on the path IF PASSING CUSTOMPACKGEMANAGER OR CUSTOMSTRINGKEY PLEASE MAKE SURE BOTH OF THEM ARE FILLED OUT even if just passing one of them
    /// </summary>
    /// <param name="programName"></param>
    /// <param name="version"></param>
    /// <param name="packageManagerName"></param>
    /// <param name="relProgram"></param>
    /// <param name="relPackageManager"></param>
    /// <param name="customPackageManagerStringKey"></param>
    /// <param name="customPackageManagerStringObject"></param>
    private static void ResolveAlreadyOnPathConflict(string programName, string version, string packageManagerName, string relProgram, string relPackageManager, string customPackageManagerStringKey = "", params List<string> customPackageManagerStringObject)
    {

        Console.WriteLine($"Since {programName}.exe Is on the path an alias will be made of '{programName}{version}' inside of build.toml so use '{programName}{version} main.yourLangExtension' you can change the name of the alias in build.toml");

        TomlTable table = TOMLHandler.GetBuildTOML();

        if (!table.ContainsKey($"{programName}{version}"))
            table.Add($"{programName}{version}", new string[] { relProgram });



        if (!table.ContainsKey($"{packageManagerName}{version}") || !table.ContainsKey(packageManagerName))
            if (customPackageManagerStringKey == string.Empty && customPackageManagerStringObject == null)
                table.Add($"{packageManagerName}{version}", new string[] { relPackageManager });
            else
                table.Add(customPackageManagerStringKey, customPackageManagerStringObject);




        TOMLHandler.PutTOML(table, Config.BuildTomlPath);
        Console.WriteLine($"build.toml written, you can now use '{programName}{version} or '{packageManagerName}{version}' for this specific version. for the default {programName} install please use '{programName}'");
    }

    private static void EditPythonPathFile(string filePath)
    {
        string file = BlinkFS.ReadFile(filePath);

        file = file.Replace("#import site", "import site");

        BlinkFS.WriteFile(filePath, file);
    }


}