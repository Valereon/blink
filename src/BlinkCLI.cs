using System.Linq.Expressions;
using DotMake.CommandLine;
using Microsoft.VisualBasic.FileIO;


Cli.Run<BlinkCLI>(args);
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
            TOMLHandler.PutTOML(root, TOMLHandler.configTomlPath);


            BlinkFS.fileSystemRoot = currentPath;

            Directory.CreateDirectory(currentPath + @"\.blink");
            Directory.CreateDirectory(currentPath + @"\.blink\bin");
            BlinkFS.WriteFile(currentPath + @"\.blink\config.toml", ""); //TODO: have a base toml file to write to these things
            BlinkFS.WriteFile(currentPath + @"\.blink\build.toml", "");
        }

    }

    [CliCommand(Description = "Runs a command inside the blink enviroment", Name = "Run")]
    // you cannot have them both be run so in the cli it shows up as run but i havbe to name it something different
    public class Runner
    {
        [CliArgument(Description = "The program you want to run in the blink environment")]
        public string Name { get; set; }
        [CliOption(Description = "The args you want to run the program with", Required = false, AllowMultipleArgumentsPerToken =true)]
        public string[] Args { get; set; }
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
        public string Path { get; set; }

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

    public class Export
    {
        public void Run()
        {

        }
    }




    

}

