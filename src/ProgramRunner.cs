using System.Collections;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection.Metadata.Ecma335;
using System.Text.RegularExpressions;
using Tomlyn;

public static class ProgramRunner
{
    public static void SetupEnv()
    {
        BlinkFS.GetPathFromTOML();
        LanguageSupport.EnableEnvVarsForIncludedLangs();
    }
    public static string[] PrepareArguments(string commands)
    {
        if (commands == null)
            return null;

        string[] arguments = commands.Split(" ");

        for (int i = 0; i < arguments.Length; i++)
        {
            arguments[i] = BlinkFS.MakePathAbsoulute(arguments[i]);
        }
        return arguments;
    }

    /// <summary>
    /// runs the pre-specified command in build.toml returns true if command is run and false if it couldnt find one
    /// </summary>
    public static bool TOMLArbitraryRun(string command, string[] args)
    {
        Tomlyn.Model.TomlTable toml = BlinkFS.GetTOML(BlinkFS.MakePathAbsoulute(@".\.blink\build.toml"));
        string commandToRun = (string)toml[command];
        string[] split = commandToRun.Split(" ");
        string program = split[0];


        List<string> newSplit = split.ToList();
        newSplit.Remove(program);
        split = newSplit.ToArray();

        for (int i = 0; i < split.Length; i++)
        {
            split[i] = BlinkFS.MakePathAbsoulute(split[i]);
        }

        // combine the args from the toml specified command and if theres any more args provided add them
        string[] combinedArgs;
        if (args != null)
            combinedArgs = (string[])split.Concat(args);
        else
            combinedArgs = split;



        if (BlinkFS.IsProgramInPath(program))
        {
            string programOnPath = BlinkFS.GetProgramOnPathsPath(program);
            StartProgram(programOnPath, combinedArgs);
            return true;
        }
        else
        {
            try
            {
                StartProgram(program, combinedArgs);
                return true;

            }
            catch (System.IO.FileNotFoundException e)
            {
                Console.WriteLine($"{program} Is not in the build.toml or on the path!");
                return false;
            }
        }        
    }


    /// <summary>
    ///  Starts a program given the args going [nameOfProgram, arg1,arg2,arg3] and pipes the in,out,and errors into the Blinkshell
    /// </summary>
    public static void StartProgram(string name, string[] args = null)
    {

        string combinedArgs = null;
        if (args != null)
        {
            for (int i = 0; i < args.Length; i++)
            {
                combinedArgs += $" {args[i]}";
            }

        }

        if (BlinkFS.IsProgramInPath(name))
        {
            name = BlinkFS.GetProgramOnPathsPath(name);
        }

        Process proc = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = name,
                Arguments = $"{combinedArgs}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                // RedirectStandardInput = true,
                RedirectStandardError = true,
                CreateNoWindow = true

            }
        };

        proc.Start();
        Console.WriteLine("proc has started");

        proc.OutputDataReceived += (sender, e) =>
        {
            if (e.Data != null)
                Console.WriteLine(e.Data);
        };
        proc.ErrorDataReceived += (sender, e) =>
        {
            if (e.Data != null)
                Console.WriteLine(e.Data);
        };

        proc.BeginOutputReadLine();
        proc.BeginErrorReadLine();


        while (!proc.HasExited)
        {
            // string input = Console.ReadLine();
            if (!proc.HasExited)
            {
                // proc.StandardInput.WriteLine(input);
            }
            // if (input == null)
                // continue;
        }
    }
}

