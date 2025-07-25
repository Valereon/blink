using System.ComponentModel;
using System.Diagnostics;

public static class ProgramRunner
{
    public static void SetupEnv()
    {
        TOMLHandler.GetPathFromTOML();
        LanguageSupport.EnableEnvVarsForIncludedLangs();
    }
    /// <summary>
    /// makes arguments absoulute paths for sake of robustness can either take a string of args or an array of args
    /// </summary>
    public static string[] PrepareArguments(string arguments)
    {
        if (arguments == null)
            return null;

        string[] splitArgs = arguments.Split(" ");

        return MakeArgsAbsoulute(splitArgs);
    }

    public static string[] PrepareArguments(string[] arguments)
    {
        if (arguments == null)
            return null;


        return MakeArgsAbsoulute(arguments);
    }


    static string[] MakeArgsAbsoulute(string[] arguments)
    {
        for (int i = 0; i < arguments.Length; i++)
        {
            //no file extension so skip. its an argument
            if (!arguments[i].Contains("."))
                continue;
            arguments[i] = BlinkFS.MakePathAbsoulute(arguments[i]);
        }
        return arguments;
    }

    /// <summary>
    /// runs the pre-specified command in build.toml returns true if command is run and false if it couldnt find one
    /// </summary>
    public static bool TOMLArbitraryRun(string command, string[] args)
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
    ///  Starts a program given the args going and pipes the in,out,and errors into the console
    /// </summary>
    public static void StartProgram(string name, string[] args)
    {

        string combinedArgs = "";
        if (args != null)
        {
            for (int i = 0; i < args.Length; i++)
            {
                combinedArgs += $" {args[i]}";
            }
        }

        // if a program has .\ or ./ it will run the version specified instead of the path version
        if (name.Contains("." + Config.PathSeperator))
        {
            name = BlinkFS.MakePathAbsoulute(name);
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
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                RedirectStandardError = true,
                CreateNoWindow = true

            }
        };

        proc.Start();

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

        var inputTask = Task.Run(() =>
        {
            while (!proc.HasExited)
            {
                try
                {
                    string? input = Console.ReadLine();
                    if (!proc.HasExited && input != null)
                    {
                        proc.StandardInput.WriteLine(input);
                        proc.StandardInput.Flush();
                    }
                }
                catch (InvalidOperationException)
                {
                    break;
                }
            }
        });

        proc.WaitForExit();

        inputTask.Wait(1000);

    }
}

