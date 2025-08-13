/// <summary>
/// Adds support for various programming language specific things like enalbing the env or using the package manger
/// </summary>
public static class LanguageSupport
{
    public enum Language
    {
        Python,
        NodeJS,
        Rust,
        C,
        Cpp,
        Csharp,
        None // this is kind of dumb maybe ill fix it later
    }
    public static Language StringToEnumLang(string langauge)
    {
        switch (langauge.ToLower())
        {
            case "python":
                return Language.Python;
            case "nodejs":
                return Language.NodeJS;
            case "rust":
                return Language.Rust;
            case "c":
                return Language.C;
            case "cpp":
                return Language.Cpp;
            case "csharp":
                return Language.Csharp;


        }
        throw new BlinkException($"{langauge} is not supported check Languages Enum to see supported languages");
    }



    public static void EnableEnvVarsForIncludedLangs()
    {

        Tomlyn.Model.TomlArray langs = (Tomlyn.Model.TomlArray)TOMLHandler.GetVarFromConfigTOML(Config.LangsInProject);
        List<string> listOfLangs = TOMLHandler.TOMLArrayToList(langs);

        foreach (string lang in listOfLangs)
        {
            Language enumLang = StringToEnumLang(lang);
            switch (enumLang)
            {
                case Language.Python:
                    string[] args = { (string)TOMLHandler.GetVarFromConfigTOML(Config.PythonHome) };
                    string command = (string)TOMLHandler.GetVarFromConfigTOML(Config.PythonEnv);
                    ProgramRunner.StartProgram(command, args);
                    break;
                case Language.Rust:
                    return;
                case Language.C:
                    return;
                case Language.Cpp:
                    return;
                case Language.Csharp:
                    return;
            }

        }


    }


    public static string[] GetSpecialArgsForPackageManagers(Language lang, string[] args)
    {
        switch (lang)
        {
            case Language.Python:
                return PythonArgs(args);
            case Language.Rust:
                break;
            case Language.C:
                break;
            case Language.Cpp:
                break;
            case Language.Csharp:
                break;

        }

        return Array.Empty<string>();
    }

    public static Language IsPackageManager(string command)
    {
        switch (command.ToLower())
        {
            case "pip":
                return Language.Python;
            case "cargo":
                return Language.Rust;
            case "nuget":
                return Language.Csharp;
            case "dotnet add":
                return Language.Csharp;
        }
        return Language.None;
    }



    public static string[] GetPackageManagerArgs(string name, string[] args)
    {
        Language packageManager = IsPackageManager(name);
        if (packageManager != Language.None)
        {
            return GetSpecialArgsForPackageManagers(packageManager, args);
        }
        return args;
    }


    static string[] PythonArgs(string[] args)
    {
        if (!args.Contains("install"))
            return args;

        // this inserts the pipArgs key after install so it installs in the proper folder "pip install --user numpy"
        string specialArgs = (string)TOMLHandler.GetVarFromConfigTOML(Config.PipArgs);
        List<string> listArgs = args.ToList();
        listArgs.Insert(1, specialArgs);
        return listArgs.ToArray();
    }
}