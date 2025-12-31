using System.CommandLine;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic;
using Tomlyn;
using Tomlyn.Model;
using Tomlyn.Syntax;

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
        // return arguments;
    }

    /// <summary>
    /// contains repetitive logic from PrepareArguments() and makes args absolute while ignoring flags
    /// </summary>
    private static string[] MakeArgsAbsolute(string[] arguments)
    {
        for (int i = 0; i < arguments.Length; i++)
        {
            //no file extension so skip. its an argument or a folder?
            if (!Regex.Match(arguments[i], @"\.[^.]+$").Success)
                continue;
            if (!BlinkFS.IsProgramInPath(arguments[i]))
            {
                arguments[i] = BlinkFS.MakePathAbsolute(arguments[i]);
                arguments[i] = Regex.Replace(arguments[i], @"^['""]|['""]$", "");
            }

        }
        return arguments;
    }

    private static List<string> PrepareTOMLArgsRun(string command, string[] args)
    {
        TomlArray tomlArrCommand = (TomlArray)TOMLHandler.GetVarFromBuildTOML(command);

        List<string> split = TOMLHandler.TOMLArrayToList(tomlArrCommand);

        List<string> newSplit = PrepareArguments(split.ToArray()).ToList();



        newSplit.AddRange(args);
        return newSplit;
    }

    private static void TOMLChainRunCheck(List<string> args)
    {
        List<string> commands = TOMLHandler.GetAllCommandsInBuildTOML();
        //TODO: fix this with linq
        
        // python3.3 = ".bin\python\python.exe"
        // runImp = "src\python\import.py"
        // fullRunImp = "python3.3 runImp"


        
        for (int i = 0; i < args.Count; i++)
        {
            if (commands.Contains(args[i]))
            {
                
            }
        }
        

    }

    /// <summary>
    /// runs the pre-specified command in build.toml returns true if command is run and false if it couldnt find one
    /// </summary>
    public static void TOMLArbitraryRun(string command, string[] args)
    {
        List<string> properArgs = PrepareTOMLArgsRun(command, args);
        string program = properArgs[0];
        properArgs.Remove(program);


        if (BlinkFS.IsProgramInPath(program))
        {
            string programOnPath = BlinkFS.GetProgramOnPathsFilePath(program);
            StartProgram(programOnPath, properArgs.ToArray());
        }
        else
        {
            StartProgram(program, properArgs.ToArray());
        }
    }



    /// <summary>
    ///  Starts a program given the args going and pipes the in,out,and errors into the console
    /// </summary>
    public static void StartProgram(string name, string[] args)
    {
        if (name.Contains("." + Config.PathSeparator))
        {
            name = BlinkFS.MakePathAbsolute(name);
        }
        else if (BlinkFS.IsProgramInPath(name))
        {
            name = BlinkFS.GetProgramOnPathsFilePath(name);
        }


        ProcessStartInfo psi = new ProcessStartInfo();
        psi.FileName = name;
        foreach (string arg in args)
        {
            psi.ArgumentList.Add(arg);
        }
        psi.UseShellExecute = false;


        Process proc = new Process();
        proc.StartInfo = psi;
        proc.Start();
        proc.WaitForExit();
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

