using DotMake.CommandLine;


// This CLI library which handles most of the hard parts, has to be run like this. The BlinkCLI is a holder class and each internal class is a command.
try
{
    Cli.Run<BlinkCLI>(args);
}
catch (BlinkException ex)
{
    Console.WriteLine(ex.Message);
    Environment.Exit(1);
}
/// <summary>
/// The Class that contains all command line commands using dot makeCLI
/// </summary>
[CliCommand(Description = "The Blink CLI")]
class BlinkCLI
{


    [CliCommand(Description = "Inits a blink project in the current directory")]
    public class Init
    {
        public void Run()
        {

            BlinkFS.CreateDirectory(BlinkFS.MakePathAbsolute(@".\.blink"));
            BlinkFS.CreateDirectory(BlinkFS.MakePathAbsolute(@".\.blink\bin"));
            BlinkFS.CreateDirectory(BlinkFS.MakePathAbsolute(@".\.blink\custom"));
            BlinkFS.WriteFile(BlinkFS.MakePathAbsolute(@".\.blink\config.toml"), Config.BaseConfigTOML);
            BlinkFS.WriteFile(BlinkFS.MakePathAbsolute(@".\.blink\build.toml"), Config.BaseBuildTOML);
            Console.WriteLine("Project Init successful");
        }

    }

    [CliCommand(Description = "Runs a command inside the blink environment", Name = "run")]
    // you cannot have them both be run so in the cli it shows up as run but i have to name it something different
    public class Runner
    {
        [CliArgument(Description = "The program you want to run in the blink environment")]
        public string Name { get; set; } = string.Empty;
        [CliOption(Description = "The args you want to run the program with", Required = false, AllowMultipleArgumentsPerToken = true)]
        public string[] Args { get; set; } = Array.Empty<string>();



        public void Run()
        {
            BlinkFS.LoadFileSystem();


            Args = ProgramRunner.PrepareArguments(Args);

            string fallbackMode = (string)TOMLHandler.GetVarFromConfigTOML(Config.FallbackMode);
            fallbackMode = fallbackMode.ToLower();

            // setup for shell fallback with custom shell exe and args


            if (fallbackMode == "shell")
            {
                ProgramRunner.RunInShell(Name, Args);
                return;
            }

            if (ProgramRunner.TryRunFromBuildTOML(Name, Args))
                return;
                
            if (ProgramRunner.TryRunBlink(Name, Args))
                return;


            if (ProgramRunner.TryHandleFallback(Name, Args))
                return;


            throw new BlinkException($"running  '{Name} {Args}' has failed in blink");
        }
    }

    [CliCommand(Description = "Adds a program or file to the blink path", Name = "PathAdd")]
    public class PathAdd
    {
        [CliArgument(Description = "The path you want to add to the blink path")]
        public string newPath { get; set; } = string.Empty;

        public void Run()
        {

            BlinkFS.LoadFileSystem();
            if (newPath == null || newPath == string.Empty)
                throw new BlinkException("program path cannot be null or empty");


            BlinkFS.AddProgramToPath(Path.GetRelativePath(Config.FileSystemRoot, newPath));
            TOMLHandler.PutPathToTOML();
            Console.WriteLine($"'{newPath}' successfully added to the path. You can now use it as '{Path.GetFileName(newPath)}' in commands and args");


        }
    }

    [CliCommand(Name = "verify", Description ="Automatically fixes fixable issues in blink")]
    public class Verify
    {
        // [CliOption(Name = "Fix", Required = false, Description = "Automatically fixes fixable issues in blink")]
        // public bool Fix { get; set; } = false;
        public void Run()
        {
            BlinkFS.LoadFileSystem();
            Console.WriteLine("Everything is verified!");
        }
    }



    [CliCommand(Name = "langAdd", Description ="Installs a standalone runtime of a specified supported language and version")]
    public class LangAdd
    {
        [CliArgument(Name = "language", Description = "the name of the language you want to install")]
        public string Language { get; set; } = string.Empty;

        [CliArgument(Name = "version", Description = "you put the version number as denoted by the target language ex '3.1' or '24.12.3'")]
        public string Version { get; set; } = string.Empty;

        public void Run()
        {
            BlinkFS.LoadFileSystem();
            if (Language == null || Language == string.Empty)
                throw new BlinkException("LangAdd: Language Cannot be null");

            if (Version == null || Version == string.Empty)
                throw new BlinkException("LangAdd: Version cannot be null");

            LanguageSupport.Language lang = LanguageSupport.StringToEnumLang(Language);
            LanguageInstaller.InstallLanguage(lang, Version);
        }
    }

    [CliCommand(Name = "listPath", Description ="lists all elements on the blink path")]
    public class ListPath
    {
        public void Run()
        {
            BlinkFS.LoadFileSystem();

            foreach (string var in BlinkFS.GetPathList())
            {
                Console.WriteLine(var);
            }

        }
    }

    [CliCommand(Name = "listBuildCommands", Description ="lists the name of all commands in the build.toml")]
    public class ListBuildCommands
    {
        public void Run()
        {
            BlinkFS.LoadFileSystem();
            List<string> commands = TOMLHandler.GetAllCommandsInBuildTOML();
            foreach (string command in commands)
            {
                Console.WriteLine(command);
            }
        }


    }
}

