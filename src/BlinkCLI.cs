



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

    [CliCommand(Description = "Opens the blink shell", Name = "Run")]
    // you cannot have them both be run so in the cli it shows up as run but i havbe to name it something different
    public class Runner
    {
        public void Run()
        {
            

            
        }
    }

    public class Export
    {
        public void Run()
        {

        }
    }
    
}

