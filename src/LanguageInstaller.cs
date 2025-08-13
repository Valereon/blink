using System.IO.Compression;
using System.Net;

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
    public static void InstallNodeJS(string version)
    {
        string folderPath = BlinkFS.InitLanguageFolder(LanguageSupport.Language.NodeJS, version);
        string fileName = folderPath + @$"\bin\NodeJS-{version}.zip";
        Console.WriteLine(fileName);
        DownloadFile($"https://nodejs.org/dist/v{version}/node-v{version}-win-x64.zip", fileName);
        
        ZipFile.ExtractToDirectory(fileName, @$"{folderPath}\bin\");
        BlinkFS.DeleteFile(fileName);
        BlinkFS.AddProgramToPath(folderPath + @$"\bin\node-v{version}-win-x64\node.exe");
        TOMLHandler.PutPathToTOML();
    }



}