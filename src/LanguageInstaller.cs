using System.IO.Compression;
using System.Net;
using DotMake.CommandLine;
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

    public static void InstallPython(string version)
    {
        // DownloadFile();
    }

    //https://nodejs.org/dist/v22.17.1/node-v22.17.1-win-x64.zip
    // windows!
    public static void InstallNodeJS(string version)
    {
        string folderPath = BlinkFS.InitLanguageFolder(LanguageSupport.Language.NodeJS, version);
        string fileName = folderPath + @$"{Config.PathSeparator}NodeJS-{version}.zip";
        Console.WriteLine(fileName);
        DownloadFile($"https://nodejs.org/dist/v{version}/node-v{version}-win-x64.zip", fileName);

        ZipFile.ExtractToDirectory(fileName, @$"{folderPath}");
        BlinkFS.DeleteFile(fileName);
        TOMLHandler.GetPathFromTOML();

        if (BlinkFS.IsProgramInPath("node.exe") == false && BlinkFS.IsProgramInPath("npm.cmd") == false)
        {
            string relNode = BlinkFS.MakePathRelative(folderPath + @$"{Config.PathSeparator}node-v{version}-win-x64{Config.PathSeparator}node.exe");
            string relNPM = BlinkFS.MakePathRelative(folderPath + @$"{Config.PathSeparator}node-v{version}-win-x64{Config.PathSeparator}npm.cmd");
            BlinkFS.AddProgramToPath(relNode);
            BlinkFS.AddProgramToPath(relNPM);

        }
        else
        {
            Console.WriteLine($"Since Node.exe Is on the path an alias will be made of 'node{version}' inside of build.toml so use 'node{version} main.js' you can change the name of the alias in build.toml");

            TomlTable table = TOMLHandler.GetBuildTOML();

            string relNode = BlinkFS.MakePathRelative(folderPath + @$"{Config.PathSeparator}node-v{version}-win-x64{Config.PathSeparator}node.exe");
            string relNPM = BlinkFS.MakePathRelative(folderPath + @$"{Config.PathSeparator}node-v{version}-win-x64{Config.PathSeparator}npm.cmd");

            if(!table.ContainsKey($"node{version}"))
                table.Add($"node{version}", relNode);
            if(!table.ContainsKey($"npm{version}"))
                table.Add($"npm{version}", relNPM);


            TOMLHandler.PutTOML(table, Config.BuildTomlPath);
            Console.WriteLine($"build.toml written, you can now use 'node{version} or 'npm{version}' for this specific version. for the default node install please use 'node'");
        }

        TOMLHandler.PutPathToTOML();
    }



}