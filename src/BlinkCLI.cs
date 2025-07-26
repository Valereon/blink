using System.Linq.Expressions;
using DotMake.CommandLine;


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
        public void Run(CliContext context)
        {

            string currentPath = Directory.GetCurrentDirectory();
            Tomlyn.Model.TomlTable root = TOMLHandler.GetConfigTOML();
            root[Config.FileSystemRoot] = currentPath;
            TOMLHandler.PutTOML(root, Config.ConfigTomlPath);


            BlinkFS.fileSystemRoot = currentPath;

            Directory.CreateDirectory(currentPath + @"\.blink");
            Directory.CreateDirectory(currentPath + @"\.blink\bin");
            BlinkFS.WriteFile(currentPath + @"\.blink\config.toml", string.Empty); //TODO: have a base toml file to write to these things
            BlinkFS.WriteFile(currentPath + @"\.blink\build.toml", string.Empty);
        }

    }

    [CliCommand(Description = "Runs a command inside the blink environment", Name = "Run")]
    // you cannot have them both be run so in the cli it shows up as run but i have to name it something different
    public class Runner
    {
        [CliArgument(Description = "The program you want to run in the blink environment")]
        public string Name { get; set; } = string.Empty;
        [CliOption(Description = "The args you want to run the program with", Required = false, AllowMultipleArgumentsPerToken = true)]
        public string[] Args { get; set; } = Array.Empty<string>();
        public void Run()
        {
            BlinkFS.LoadFileSystemRoot();
            ProgramRunner.SetupEnv();


            Args = ProgramRunner.PrepareArguments(Args);
            Args = LanguageSupport.GetPackageManagerArgs(Name, Args);

            foreach (string item in Args)
            {
                Console.WriteLine(item);
            }
            if (BlinkFS.IsProgramInPath(Name))
            {
                ProgramRunner.StartProgram(Name, Args);
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
            BlinkFS.LoadFileSystemRoot();

            TOMLHandler.GetPathFromTOML();
            if (Path == null)
                return;


            BlinkFS.AddProgramToPath(BlinkFS.MakePathRelative(Path));
            TOMLHandler.PutPathToTOML();
        }
    }

    [CliCommand(Name ="Verify")]
    public class Verify
    {
        [CliOption(Name = "Fix", Required = false, Description ="Automatically fixed structure issues in a blink project")]
        public bool Fix { get; set; } = false;
        public void Run()
        {
            BlinkFS.LoadFileSystemRoot();
            BlinkFS.IsBlinkFileStructureValid();
        }
    }


    public class Export
    {
        public void Run()
        {

        }
    }




    

}

