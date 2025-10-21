using System.IO.Compression;
using System.Net;
using System.Runtime.CompilerServices;
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
    private static void DownloadFile(string url, string pathToSave)
    {
        using (WebClient wc = new WebClient())
        {
            try
            {
                wc.DownloadFile(url, pathToSave);
            }
            catch
            {
                BlinkFS.DeleteDirectory(pathToSave);
                throw new BlinkDownloadException($"Url '{url}' has run into an issue, please try again with a different url");
            }
        }
    }
    //https://www.python.org/ftp/python/3.13.6/python-3.13.6-embed-amd64.zip
    //windows!
    private static void InstallPython(string version)
    {
        string folderPath = BlinkFS.InitLanguageFolder(LanguageSupport.Language.Python, version);
        string fileName = folderPath + @$"{Config.PathSeparator}Python-{version}.zip";


        DownloadFile($"https://www.python.org/ftp/python/{version}/python-{version}-embed-amd64.zip", fileName);
        // python
        ZipFile.ExtractToDirectory(fileName, $"{folderPath}");
        BlinkFS.DeleteFile(fileName);

        //create pip folder and shove pip in it
        //.\.blink\bin\Python-3.13.6\pip\
        DownloadFile($"https://bootstrap.pypa.io/pip/pip.pyz", $"{folderPath}{Config.PathSeparator}pip.pyz");
        BlinkFS.CreateDirectory($"{folderPath}{Config.PathSeparator}pip{Config.PathSeparator}");
        File.Move($"{folderPath}{Config.PathSeparator}pip.pyz", $"{folderPath}{Config.PathSeparator}pip{Config.PathSeparator}pip.pyz");




        //.\.blink\bin\Python-3.13.6\python.exe
        string relPython = BlinkFS.MakePathRelative(folderPath + @$"{Config.PathSeparator}python.exe");
        //.\.blink\bin\Python-3.13.6\pip\pip.pyz
        string relPip = BlinkFS.MakePathRelative(folderPath + @$"{Config.PathSeparator}pip{Config.PathSeparator}pip.pyz");


        //.\.blink\bin\Python-3.13.6\python313._pth
        // take the dumb version and turn it into python313 from 3.13.6
        // this is needed to recognize site packages for python, for per install shit.
        string pthFileVersion = System.Text.RegularExpressions.Regex.Replace(version, @"\b(\d+)\.(\d+)\.\d+\b", "$1$2");
        EditPythonPathFile(folderPath + $"{Config.PathSeparator}python{pthFileVersion}._pth");

        TomlTable table = TOMLHandler.GetBuildTOML();

        if (BlinkFS.IsProgramInPath("python.exe") == false && BlinkFS.IsProgramInPath("pip.pyz") == false)
        {
            BlinkFS.AddProgramToPath(relPip);
            BlinkFS.AddProgramToPath(relPython);


            List<string> buildList = [relPython, relPip];

            table.Add("pip", buildList);

            TOMLHandler.PutTOML(table, Config.BuildTomlPath);
            Console.WriteLine($"Python {version} and Pip {version} were added to the path! you can use them like 'blink run python -a main.py' and 'blink run pip -a install pygame'");
        }
        else
        {
            ResolveAlreadyOnPathConflict("python", version, "pip", relPython, relPip, $"pip{version}", [relPython, relPip]);
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
        DownloadFile($"https://nodejs.org/dist/v{version}/node-v{version}-win-x64.zip", fileName);

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