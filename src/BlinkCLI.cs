



using DotMake.CommandLine;

// BlinkFS.BlinkInit();


Cli.Run<BlinkCLI>(args);
[CliCommand(Description = "The Blink CLI")]
class BlinkCLI
{
    [CliCommand(Description = "Inits a blink project in the current directory")]
    public class Init
    {
        public void Run()
        {
            string currentPath = Directory.GetCurrentDirectory();
            BlinkFS.fileSystemRoot = currentPath;
            Directory.CreateDirectory(currentPath + @"\.blink");
            Directory.CreateDirectory(currentPath + @"\.blink\cache");
            Directory.CreateDirectory(currentPath + @"\.blink\bin");
            BlinkFS.WriteFile(currentPath + @"\.blink\config.toml", "");
            BlinkFS.WriteFile(currentPath + @"\.blink\build.toml", "");
        }

    }

    [CliCommand(Description = "Runs a command inside the blink enviroment", Name = "Run")]
    // you cannot have them both be run so in the cli it shows up as run but i havbe to name it something different
    public class Runner
    {
        [CliArgument(Description = "The program you want to run in the blink environment")]
        public string Name { get; set; }
        [CliOption(Description = "The args you want to run the program with", Required =false)]
        public string Args { get; set; }
        public void Run()
        {
            InitDir();
            ProgramRunner.SetupEnv();
            string[] seperatedArgs = null;
            if (Args != null)
            {
                seperatedArgs = ProgramRunner.PrepareArguments(Args);
            }

            if (BlinkFS.IsProgramInPath(Name))
            {
                ProgramRunner.StartProgram(Name, seperatedArgs);
            }
            else
            {
                // this will try and see if there is a command in build.toml and run it if there is
                ProgramRunner.TOMLArbitraryRun(Name, seperatedArgs);
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
            InitDir();

            BlinkFS.GetPathFromTOML();
            if (Path == null)
                return;


            BlinkFS.AddProgramToPath(BlinkFS.MakePathRelative(Path));
            BlinkFS.PutPathToTOML();
        }
    }

    public class Export
    {
        public void Run()
        {

        }
    }


    public static void InitDir()
    {
        string currentPath = Directory.GetCurrentDirectory();
        BlinkFS.fileSystemRoot = currentPath;
    }

}

