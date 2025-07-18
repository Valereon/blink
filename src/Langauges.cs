public enum Language
{
    Python,
    Rust,
    C,
    Cpp,
    Csharp,
}


public static class LanguageSupport
{
    public static Language StringToEnumLang(string langauge)
    {
        switch (langauge.ToLower())
        {
            case "python":
                return Language.Python;
            case "rust":
                return Language.Rust;
            case "c":
                return Language.C;
            case "cpp":
                return Language.Cpp;
            case "csharp":
                return Language.Csharp;


        }
        throw new Exception($"{langauge} is not supported check Languages Enum to see supported languages");
    }



    public static void EnableEnvVarsForIncludedLangs()
    {
        Tomlyn.Model.TomlTable toml = BlinkFS.GetTOML(@".\.blink\config.toml");
        Tomlyn.Model.TomlArray langs = (Tomlyn.Model.TomlArray)toml["langsInProject"];
        List<string> listOfLangs = BlinkFS.TOMLArrayToList(langs);
        
        foreach (string lang in listOfLangs)
        {
            Language enumLang = StringToEnumLang(lang);
            switch (enumLang)
            {
                case Language.Python:
                    ProgramRunner.StartProgram((string)toml["pythonEnv"]);
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


}