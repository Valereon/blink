using System.Linq.Expressions;
using DotMake.CommandLine;


BlinkFS.LoadFileSystem();
Cli.Run<BlinkCLI>(args);
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
            Directory.CreateDirectory(BlinkFS.MakePathAbsolute(@".\.blink"));
            Directory.CreateDirectory(BlinkFS.MakePathAbsolute(@".\.blink\bin"));
            BlinkFS.WriteFile(BlinkFS.MakePathAbsolute(@".\.blink\config.toml"), string.Empty); //TODO: have a base toml file to write to these things
            BlinkFS.WriteFile(BlinkFS.MakePathAbsolute(@".\.blink\build.toml"), string.Empty);
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


        [CliOption(Description = "Lists all of the commands inside of Build.toml", Required = false, Name = "List")]
        public bool list { get; set; }



        public void Run()
        {
            if (list)
            {
                List<string> commands = TOMLHandler.GetAllCommandsInBuildTOML();
                foreach (string command in commands)
                {
                    Console.WriteLine(command);
                }
                return;
            }


            ProgramRunner.SetupEnv();



            Args = ProgramRunner.PrepareArguments(Args);
            // Args = LanguageSupport.GetPackageManagerArgs(Name, Args);

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


            TOMLHandler.GetPathFromTOML();
            if (Path == null)
                return;


            BlinkFS.AddProgramToPath(BlinkFS.MakePathRelative(Path));
            TOMLHandler.PutPathToTOML();
        }
    }

    [CliCommand(Name = "verify")]
    public class Verify
    {
        [CliOption(Name = "Fix", Required = false, Description = "Automatically fixed structure issues in a blink project")]
        public bool Fix { get; set; } = false;
        public void Run()
        {
            BlinkFS.IsBlinkFileStructureValid();
        }
    }


    [CliCommand(Name = "langAdd")]
    public class Add
    {
        [CliArgument(Name = "language", Description = "the name of the language you want to install")]
        public string Language { get; set; } = string.Empty;

        [CliArgument(Name = "version", Description = "you put the version number as denoted by the target language ex 3.1 or 24.12.3")]
        public string Version { get; set; } = string.Empty;

        public void Run()
        {
            LanguageSupport.Language lang = LanguageSupport.StringToEnumLang(Language);
            LanguageInstaller.InstallLanguage(lang, Version);
        }
    }



    public class Export
    {
        public void Run()
        {

        }
    }






}

