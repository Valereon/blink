using System.Diagnostics;

/// <summary>
/// This class handles running processes as well as preparing and cleaning arguments
/// </summary>
public static class ProgramRunner
{
    public static void SetupEnv()
    {
        TOMLHandler.GetPathFromTOML();
    }
    /// <summary>
    /// makes arguments absolute paths for sake of robustness takes a string of args split by spaces
    /// </summary>
    public static string[] PrepareArguments(string arguments)
    {
        if (arguments == null)
            return Array.Empty<string>();

        string[] splitArgs = arguments.Split(" ");

        return MakeArgsAbsolute(splitArgs);
    }
    /// <summary>
    /// makes arguments absolute paths for sake of robustness takes an array of args
    /// </summary>
    public static string[] PrepareArguments(string[] arguments)
    {
        if (arguments == null)
            return Array.Empty<string>();


        return MakeArgsAbsolute(arguments);
    }

    /// <summary>
    /// contains repetitive logic from PrepareArguments() and makes args absolute while ignoring flags
    /// </summary>
    static string[] MakeArgsAbsolute(string[] arguments)
    {
        for (int i = 0; i < arguments.Length; i++)
        {
            //no file extension so skip. its an argument
            if (!arguments[i].Contains("."))
                continue;
            arguments[i] = BlinkFS.MakePathAbsolute(arguments[i]);
        }
        return arguments;
    }

    /// <summary>
    /// runs the pre-specified command in build.toml returns true if command is run and false if it couldnt find one
    /// </summary>
    public static void TOMLArbitraryRun(string command, string[] args)
    {
        string commandToRun = (string)TOMLHandler.GetVarFromBuildTOML(command);
        string[] split = commandToRun.Split(" ");
        string program = split[0];


        List<string> newSplit = split.ToList();
        newSplit.Remove(program);
        split = PrepareArguments(newSplit.ToArray());


        // combine the args from the toml specified command and if theres any more args provided add them
        string[] combinedArgs;
        if (args != null)
            combinedArgs = split.Concat(args).ToArray();
        else
            combinedArgs = split;



        if (BlinkFS.IsProgramInPath(program))
        {
            string programOnPath = BlinkFS.GetProgramOnPathsFilePath(program);
            StartProgram(programOnPath, combinedArgs);
        }
        else
        {
            // this if it runs the program will exit the blink app so no need to check and if it doesn't run it will move on
            CheckAndRunFallbackMode(program, combinedArgs);

            if (!File.Exists(program))
                throw new BlinkFSException($"File: '{program}' Does not exist, or is not in build.toml OR the path");


            StartProgram(program, combinedArgs);

        }
    }

    private static void CheckAndRunFallbackMode(string program, string[] args)
    {

        string fallbackMode = (string)TOMLHandler.GetVarFromConfigTOML(Config.FallbackMode);
        fallbackMode = fallbackMode.ToLower();

        string shellExe = (string)TOMLHandler.GetVarFromConfigTOML(Config.ShellExe);
        string ShellExtraArgs = (string)TOMLHandler.GetVarFromConfigTOML(Config.ShellExtraArgs);
        List<string> shellArgs = args.ToList();


        shellArgs.Insert(0, ShellExtraArgs);
        shellArgs.Insert(1, program);


        if (fallbackMode == "auto")
        {
            StartProgram(shellExe, shellArgs.ToArray());
        }
        if (fallbackMode == "ask")
        {
            Console.Write("Blink execution failed would you like to run this in the shell instead? y/N: ");
            string? key = Console.ReadLine();
            if (key?.ToLower() == "y")
            {
                StartProgram(shellExe, shellArgs.ToArray());
            }
        }
    }

    /// <summary>
    ///  Starts a program given the args going and pipes the in,out,and errors into the console
    /// </summary>
    public static void StartProgram(string name, string[] args)
    {

        string combinedArgs = string.Empty;
        if (args != null)
        {
            for (int i = 0; i < args.Length; i++)
            {
                combinedArgs += $" {args[i]}";
            }
        }

        // if a program has .\ or ./ it will run the version specified instead of the path version
        if (name.Contains("." + Config.PathSeparator))
        {
            name = BlinkFS.MakePathAbsolute(name);
        }
        else if (BlinkFS.IsProgramInPath(name))
        {
            name = BlinkFS.GetProgramOnPathsFilePath(name);
        }

        Process proc = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = name,
                Arguments = $"{combinedArgs}",
                UseShellExecute = false,
            }
        };

        proc.Start();
        proc.WaitForExit();
        Environment.Exit(0);
    }

}

