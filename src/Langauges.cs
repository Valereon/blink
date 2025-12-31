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
        //TODO: make this actually list out the damn languages at what they are supposed to be accept
        throw new BlinkException($"'{langauge}' is not supported check Readme To see supported languages");
    }
}

public class LanguageConfig
{
    public LanguageSupport.Language lang;
    // public 
}