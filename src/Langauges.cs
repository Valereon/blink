/// <summary>
/// Adds support for various programming language specific things
/// </summary>
public static class LanguageSupport
{
    public enum Language
    {
        Python,
        NodeJS
    }
    public static Language StringToEnumLang(string langauge)
    {
        switch (langauge.ToLower())
        {
            case "python":
                return Language.Python;
            case "nodejs":
                return Language.NodeJS;
        }
        throw new BlinkException($"'{langauge}' is not supported check Readme To see supported languages");
    }
}