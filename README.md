# blink



Blink is my attempt to solve "it works on my machine." The goal of blink is to have a one step process for installing a development environment, the one step? unzip it! Once unzipped you will have all, binaries, libraries, runtimes, path configurations, and custom commands of the person who configured the environment It's completely standalone! I am aware some languages have virtual environments like python, or things like Nix and Docker. My solution differs in that the ENTIRE environment can be shared and opened with zero setup and offline friendly! no more dependency hell or path variables configuration! everything just works!


# DISCLAIMER
this DOES NOT work for things that need hardware configurations or need to be installed such as custom drivers, or other things you need to install manually onto your system, and require specific hardware level things, or edit registry or things like that. For those types of things you can make a script that will install them. THIS only works with things that can be considered portable. So included are standalone language binaries, and the packages don't need to be installed  they are just referenced so this works. Still a lot of things SHOULD work its just programs or apps that need configurations that cannot be transferred via a file or directory.





If the program can be installed into a specific directory such as `.\.blink\custom\` and all its required files are inside that you CAN make it apart of your installation


# Pros and Cons
## Cons
Now there are some tradeoffs with this approach of course. I want to mention the cons before the pros so you know them without being too excited.
- Large initial file size.
- Hosting issues, harder to share.
- Some learning curve and some quirks.
- [Kind of?] may require hosting the .blink folder separately if you don't want your git changes flooded with package file changes, or want to separate code changes and blink changes.

## Pros
- One step setup
- Easy new contributor boarding
- Backups of entire Environments including internally modified packages
- Backups of packages that could cease to be hosted on package managers
- Offline Friendly
- Git Friendly
- One Download
- Future Proof
- Cross Language


## Using the blink environment and specific quirks
- Since blink has its own path, there is a caveat with using it. You have to preface every command with `blink run {command}`
- for running commands with args you need to add `--args` e.x `blink run python --args main.py`.
- Adding custom commands is done in Build.toml, any command can be added and will be executed using `blink run {commandName}` commands in build.toml DO NOT NEED --args bc this just executes them normally. The command line tool needs a flag for optional args which is why `--args` is required
- Language binaries are all the standalone versions that don't need installing
- Package managers MUST USE `blink run` or else it will not use the blink specific manager and install it system wide e.x `blink run pip --args install numpy`
- Packages install were the usually would (most of the time see below)




## Quick Start
```bash
The Blink CLI

Usage:
  blink [command] [options]

Options:
  -h, -?, --help  Show help and usage information
  -v, --version   Show version information

Commands:
  i, init                 Inits a blink project in the current directory
  r, run                  Runs a command inside the blink environment
  pa, PathAdd             Adds a program or file to the blink path
  v, verify               Automatically fixes fixable issues in blink
  la, langAdd             Installs a standalone runtime of a specified supported language and version 
  lp, listPath            lists all elements on the blink path
  lbc, listBuildCommands  lists the name of all commands in the build.toml
```

```bash
#start project
mkdir myProject
cd myProject
blink init # AT THE TOP LEVEL OF YOUR PROJECT
blink langAdd python 3.10

#install dependencies
blink run pip --args install http


#create and run it
echo "import http; print('Http is working!')" > main.py
blink run python --args main.py


# share it
zip -r myProject.zip
```

<br>
<br>

# Package Managers
I have learned through numerous trial and error that it is not worth fighting package managers. I have tried to redirect, pip, and node into specific folders but I have found this messes with more things than it helps and causes more work than its worth. So here is a list of where packages install inside of blink.

- Nodejs/NPM
  - Normal, top level of project
- Python
    - `.blink\bin\python\bin\Lib\site-packages` and `.blink\bin\python\bin\Lib\scripts`



## Defining your own commands
In build.toml you can define your own commands as easily as adding a new entry.
<br>
you CAN use things on the path inside of the build.toml and also you DON'T NEED TO use -a/--args for this just write the command normally
<br>
```toml
    build = "dotnet build .."
    run = "dotnet run ..\\src\\"
    foo = "ls -l"
```
all you do is run `blink run build` or `blink run foo` you can name these commands anything you want.


## Other Langauge Support
Any langauge is technically automatically supported. All you need to do is get the standalone binaries from the website and set it up in `\.blink\bin\yourLang` and then do a `blink pa .\blink\bin\yourLang\languageExecutable` and the same with the package manager. Only issue is you have to figure out how to make the package manager behave so that it installs inside of the blink folder. Then you can do as you would!



### The Process Behind Blink
It works by packaging all packages, binaries, runtimes, and path configurations into a neat folder. When you run a command like `blink run python --args main.py`. Blink has its own path independent of your system. You can see it in `config.toml`. Blink checks the path in `config.toml` and replaces `python` with `.blink\bin\python\bin\python.exe` and if main.py is on your path it does the same thing. As long as you use `blink run` you will use the packages and binaries you have installed in the .blink folder. I think this is the magic of it all is being able to bundle everything together and keep environment variables alive and if you want to add more just add them to `config.toml` and you can freely use that file
