using DotMake.CommandLine;



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
            BlinkFS.WriteFile(BlinkFS.MakePathAbsolute(@".\.blink\config.toml"), Config.BaseConfigTOML); //TODO: have a base toml file to write to these things
            BlinkFS.WriteFile(BlinkFS.MakePathAbsolute(@".\.blink\build.toml"), Config.BaseBuildTOML);
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


            ProgramRunner.SetupEnv();
            Args = ProgramRunner.PrepareArguments(Args);

            if (BlinkFS.IsProgramInPath(Name))
            {
                ProgramRunner.StartProgram(Name, Args);
            }
            else if (Name.Contains(@$".{Config.PathSeparator}"))
            {
                ProgramRunner.StartProgram(BlinkFS.MakePathAbsolute(Name), Args);
            }
            else
            {
                ProgramRunner.TOMLArbitraryRun(Name, Args);
            }




        }
    }

    [CliCommand(Description = "Adds a program or file to the blink path", Name = "PathAdd")]
    public class PathAdd
    {
        [CliArgument(Description = "The path you want to add to the blink path")]
        public string Path { get; set; } = string.Empty;

        public void Run()
        {

            BlinkFS.LoadFileSystem();
            TOMLHandler.GetPathFromTOML();
            if (Path == null || Path == string.Empty)
                throw new BlinkException("program path cannot be null or empty");


            BlinkFS.AddProgramToPath(BlinkFS.MakePathRelative(Path));
            TOMLHandler.PutPathToTOML();
            Console.WriteLine($"'{Path}' successfully added to the path. You can now use it as '{System.IO.Path.GetFileName(Path)}' in commands and args");


        }
    }

    [CliCommand(Name = "verify")]
    public class Verify
    {
        [CliOption(Name = "Fix", Required = false, Description = "Automatically fixed structure issues in a blink project")]
        public bool Fix { get; set; } = false;
        public void Run()
        {
            BlinkFS.LoadFileSystem();
            BlinkFS.IsBlinkFileStructureValid();
        }
    }



    [CliCommand(Name = "langAdd")]
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

    [CliCommand(Name = "listPath")]
    public class ListPath
    {
        public void Run()
        {
            BlinkFS.LoadFileSystem();

            foreach (string var in BlinkFS.path)
            {
                Console.WriteLine(var);
            }

        }
    }

    [CliCommand(Name = "listBuildCommands")]
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

