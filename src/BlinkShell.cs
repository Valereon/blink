using System.Collections;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using System.Text.RegularExpressions;
using Tomlyn;

static class ProgramRunner
{
    public static void Start()
    {
        BlinkFS.GetPathFromTOML();
        LanguageSupport.EnableEnvVarsForIncludedLangs();
    }
    static void InterpretCommands(string commands)
    {
        if (commands == null)
            return;


        string[] arguments = commands.Split(" ");

        for (int i = 0; i < arguments.Length; i++)
        {
            arguments[i] = BlinkFS.MakePathAbsoulute(arguments[i]);
        }

        string command = arguments[0];

        if (BlinkFS.IsProgramInPath(command))
            StartProgram(command, arguments);
            if (!TOMLArbitraryRun(command, arguments))
            {
                StartProgram(command, arguments);
            }


    }

    /// <summary>
    /// runs the pre-specified command in build.toml returns true if command is run and false if it couldnt find one
    /// </summary>
    /// 
    static bool TOMLArbitraryRun(string command, string[] args)
    {
        Tomlyn.Model.TomlTable toml = BlinkFS.GetTOML(BlinkFS.MakePathAbsoulute(@".\.blink\build.toml"));
        string commandToRun = (string)toml[command];
        string[] split = commandToRun.Split(" ");

        if (BlinkFS.IsProgramInPath(split[0]))
        {
            for (int i = 0; i < split.Length; i++)
            {
                split[i] = BlinkFS.MakePathAbsoulute(split[i]);
            }
            string[] combinedArgs = (string[])split.Concat(args);
            StartProgram(split[0], combinedArgs);
            return true;
        }
        else
        {
            Console.WriteLine($"The Configured {command} command is not in the path and cannot be run");
            return false;
        }

    }


    //BUG: when a program finishes you HAVE to press enter to exit it need non blocking console reading to fix this cause console.readline blocks and cannot check if the proc has already exited so it has to check AFTER you press enter
    /// <summary>
    ///  Starts a program given the args going [nameOfProgram, arg1,arg2,arg3] and pipes the in,out,and errors into the Blinkshell
    /// </summary>
    public static void StartProgram(string name, string[] args = null)
    {
        Console.WriteLine("yo started the program");

        string combinedArgs = null;
        if (args != null)
        {
            for (int i = 1; i < args.Length; i++)
            {
                combinedArgs += $" {args[i]}";
            }
    
        }

        if (BlinkFS.IsProgramInPath(name))
        {
            name = BlinkFS.MakePathAbsoulute(name);
            Console.WriteLine(name);
        }

        Process proc = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = name,
                Arguments = $"{combinedArgs}",
                UseShellExecute = false,
                // RedirectStandardOutput = true,
                // RedirectStandardInput = true,
                // RedirectStandardError = true,
                CreateNoWindow = true

            }
        };

        proc.Start();

        // proc.OutputDataReceived += (sender, e) =>
        // {
        //     if (e.Data != null)
        //         Console.WriteLine(e.Data);
        // };
        // proc.ErrorDataReceived += (sender, e) =>
        // {
        //     if (e.Data != null)
        //         Console.WriteLine(e.Data);
        // };

        // proc.BeginOutputReadLine();
        // proc.BeginErrorReadLine();


        // while (!proc.HasExited)
        // {
        //     string input = Console.ReadLine();
        //     if (input == null)
        //         continue;
        //     if (!proc.HasExited)
        //     {
        //         proc.StandardInput.WriteLine(input);
        //     }
        // }
    }

    static string TryAutoComplete()
    {
        return "";
    }

}

