/// <summary>
/// Adds support for various programming language specific things
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
        throw new BlinkException($"'{langauge}' is not supported check Readme To see supported languages");
    }
}