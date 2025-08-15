using System.IO.Compression;
using System.Net;
using Tomlyn.Model;

public static class LanguageInstaller
{

    public static void DownloadFile(string url, string pathToSave)
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
    public static void InstallPython(string version)
    {
        string folderPath = BlinkFS.InitLanguageFolder(LanguageSupport.Language.Python, version);
        string fileName = folderPath + @$"{Config.PathSeparator}Python-{version}.zip";
        Console.WriteLine(fileName);
        DownloadFile($"https://www.python.org/ftp/python/{version}/python-{version}-embed-amd64.zip", fileName);
        DownloadFile($"https://bootstrap.pypa.io/pip/pip.pyz", $"{folderPath}{Config.PathSeparator}pip.pyz");


        //create pip folder and shove pip in it
        //.\.blink\bin\Python-3.13.6\pip\
        BlinkFS.CreateDirectory($"{folderPath}{Config.PathSeparator}pip{Config.PathSeparator}");
        File.Move(folderPath + Config.PathSeparator + "pip.pyz", $"{folderPath}{Config.PathSeparator}pip{Config.PathSeparator}pip.pyz");

        ZipFile.ExtractToDirectory(fileName, $"{folderPath}");
        BlinkFS.DeleteFile(fileName);
        TOMLHandler.GetPathFromTOML();


        //.\.blink\bin\Python-3.13.6\python.exe
        string relPython = BlinkFS.MakePathRelative(folderPath + @$"{Config.PathSeparator}python.exe");
        //.\.blink\bin\Python-3.13.6\pip\pip.pyz
        string relPip = BlinkFS.MakePathRelative(folderPath + @$"{Config.PathSeparator}pip{Config.PathSeparator}pip.pyz");


        //.\.blink\bin\Python-3.13.6\python313._pth
        // take the dumb version and turn it into python313 from 3.13.6
        // this is needed to recognize site packages for python, for per install shit.
        string pthFileVersion = System.Text.RegularExpressions.Regex.Replace(version, @"\b(\d+)\.(\d+)\.\d+\b", "$1$2");
        EditPythonPathFile(folderPath + $"{Config.PathSeparator}python{pthFileVersion}._pth");



        if (BlinkFS.IsProgramInPath("python.exe") == false && BlinkFS.IsProgramInPath("pip.pyz") == false)
        {
            BlinkFS.AddProgramToPath(relPip);
            BlinkFS.AddProgramToPath(relPython);
            Console.WriteLine($"Python {version} and Pip {version} were added to the path! you can use them like 'blink run python -a main.py' and 'blink run pip -a install pygame'");
        } 
        else
        {
            Console.WriteLine($"Since python.exe and pip are on the path an alias will be made of 'python{version}' and 'pip{version}' inside of build.toml so use 'python{version} main.py' or 'pip{version} install numpy' you can change the name of the alias in build.toml");

            TomlTable table = TOMLHandler.GetBuildTOML();


            if (!table.ContainsKey($"python{version}"))
                table.Add($"python{version}", relPython);
            if (!table.ContainsKey($"pip{version}"))
                // this pip needs to be run by python so we do that in the build.toml
                table.Add($"pip{version}", $"{relPython} {relPip}");


            TOMLHandler.PutTOML(table, Config.BuildTomlPath);
            Console.WriteLine($"build.toml written, you can now use 'python{version}' or 'pip{version}' for this specific version. for the default python install please use 'python'");
        }

        TOMLHandler.PutPathToTOML();
    }

    //https://nodejs.org/dist/v22.17.1/node-v22.17.1-win-x64.zip
    // windows!
    public static void InstallNodeJS(string version)
    {
        string folderPath = BlinkFS.InitLanguageFolder(LanguageSupport.Language.NodeJS, version);
        string fileName = folderPath + @$"{Config.PathSeparator}NodeJS-{version}.zip";
        Console.WriteLine(fileName);
        DownloadFile($"https://nodejs.org/dist/v{version}/node-v{version}-win-x64.zip", fileName);

        ZipFile.ExtractToDirectory(fileName, $"{folderPath}");
        BlinkFS.DeleteFile(fileName);
        TOMLHandler.GetPathFromTOML();

        string relNode = BlinkFS.MakePathRelative(folderPath + @$"{Config.PathSeparator}node-v{version}-win-x64{Config.PathSeparator}node.exe");
        string relNPM = BlinkFS.MakePathRelative(folderPath + @$"{Config.PathSeparator}node-v{version}-win-x64{Config.PathSeparator}npm.cmd");
        
        if (BlinkFS.IsProgramInPath("node.exe") == false && BlinkFS.IsProgramInPath("npm.cmd") == false)
        {


            BlinkFS.AddProgramToPath(relNode);
            BlinkFS.AddProgramToPath(relNPM);

        }
        else
        {
            Console.WriteLine($"Since Node.exe Is on the path an alias will be made of 'node{version}' inside of build.toml so use 'node{version} main.js' you can change the name of the alias in build.toml");

            TomlTable table = TOMLHandler.GetBuildTOML();




            if (!table.ContainsKey($"node{version}"))
                table.Add($"node{version}", relNode);
            if (!table.ContainsKey($"npm{version}"))
                table.Add($"npm{version}", relNPM);


            TOMLHandler.PutTOML(table, Config.BuildTomlPath);
            Console.WriteLine($"build.toml written, you can now use 'node{version} or 'npm{version}' for this specific version. for the default node install please use 'node'");
        }

        TOMLHandler.PutPathToTOML();
    }


    public static void EditPythonPathFile(string filePath)
    {
        string file = BlinkFS.ReadFile(filePath);

        file = file.Replace("#import site", "import site");

        BlinkFS.WriteFile(filePath, file);
    }


}