using System.Data;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;

/// <summary>
/// This class handles running processes as well as preparing and cleaning arguments
/// </summary>
public static class ProgramRunner
{
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
    private static string[] MakeArgsAbsolute(string[] arguments)
    {
        for (int i = 0; i < arguments.Length; i++)
        {
            //no file extension so skip. its an argument          
            if (!Regex.Match(arguments[i], @"\.[^.]+$").Success)
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
            // does not check if the file exists? but when i was it was messing up with system commands so the user will just have to figure it out if their file doesnt exist. Windows should tell them
            if (TOMLHandler.DoesKeyExistInTOML(program, TOMLHandler.GetBuildTOML()))
                throw new BlinkFSException($"File: '{program}' Does not exist, or is not in build.toml OR the path");

            TryHandleFallback(program, combinedArgs);


            StartProgram(program, combinedArgs);

        }
    }



    /// <summary>
    ///  Starts a program given the args going and pipes the in,out,and errors into the console
    /// </summary>
    public static void StartProgram(string name, string[] args)
    {
        args = PrepareArguments(args);

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
        proc.Dispose();
        Environment.Exit(0);
    }



    /// <summary>
    /// tries to run in the blink environment
    /// </summary>
    /// <param name="name"></param>
    /// <param name="args"></param>
    /// <returns>true if it worked, false if it fails</returns>
    public static bool TryRunBlink(string name, string[] args)
    {

        if (BlinkFS.IsProgramInPath(name))
        {
            StartProgram(name, args);
            return true;
        }
        else if (name.Contains(@$".{Config.PathSeparator}"))
        {
            StartProgram(BlinkFS.MakePathAbsolute(name), args);
            return true;
        }

        return false;
    }


    /// <summary>
    /// Runs the program, and args in the windows shell, bypassing all blink features
    /// </summary>
    /// <param name="name"></param>
    /// <param name="args"></param>
    public static void RunInShell(string name, string[] args)
    {
        string shellExe = (string)TOMLHandler.GetVarFromConfigTOML(Config.ShellExe);
        string ShellExtraArgs = (string)TOMLHandler.GetVarFromConfigTOML(Config.ShellExtraArgs);
        List<string> shellArgs = args.ToList();
        Console.WriteLine(shellExe);
        shellArgs.Insert(0, name);
        shellArgs.Insert(0, ShellExtraArgs);
        StartProgram(shellExe, shellArgs.ToArray());
    }

    /// <summary>
    /// tries to run a command in the build toml
    /// </summary>
    /// <param name="name"></param>
    /// <param name="args"></param>
    /// <returns>True if it ran, false otherwise</returns>
    public static bool TryRunFromBuildTOML(string name, string[] args)
    {
        if (TOMLHandler.DoesKeyExistInTOML(name, TOMLHandler.GetBuildTOML()))
        {
            TOMLArbitraryRun(name, args);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Handles when blink run fails, will either auto run in shell or, ask you if you want to. This is based on Config.FallbackMode taken from the config.toml
    /// </summary>
    /// <param name="name"></param>
    /// <param name="args"></param>
    /// <returns>True if ran, otherwise false</returns>
    public static bool TryHandleFallback(string name, string[] args)
    {
        string fallbackMode = (string)TOMLHandler.GetVarFromConfigTOML(Config.FallbackMode);
        fallbackMode = fallbackMode.ToLower();
        if (fallbackMode.ToLower() == "auto")
        {
            RunInShell(name, args);
            return true;
        }
        else if (fallbackMode.ToLower() == "ask")
        {
            Console.Write("Blink execution failed would you like to run this in the shell instead? y/N: ");
            string? key = Console.ReadLine();
            if (key?.ToLower() == "y")
            {
                RunInShell(name, args);
                return true;
            }

        }

        return false;
    }
}

