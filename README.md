# blink



Blink is my attempt to solve "it works on my machine." The goal of blink is to have a one step proccess for installing a development enviroment, the one step? unzip it! Once unzipped you will have all, binaries, libraries, runtimes, path configurations, and custom commands of the person who configured the enviroment It's completely standalone! I am aware some langauges have virtual enviorments like python, or things like Nix and Docker. My solution differs in that the ENTIRE enviroment can be shared and opened with zero setup and offline friendly! no more dependency hell or path variables configuration! everything just works!

# Pros and Cons
## Cons
Now there are some tradeoffs with this apporach of course. I want to mention the cons before the pros so you know them without being too excited.
- Large inital file size
- Hosting issues, harder to share
- Some learning curve and some quirks
- it does flood your git changes when you want to commit so excluding .blink and uploading it sepereatly may be better for large projects. When you want to share you can zip it and send it

## Pros
- One step setup
- Easy new contributor boarding
- Backups of entire Enviroments including interally modified packages
- Backups of packages that could cease to be hosted on package managers
- Offline Friendly
- Git Friendly
- One Download
- Future Proof
- Cross Langauge



## Using the blink enviroment and specific quirks
- Since blink has its own path, there is a caveat with using it. You have to preface every command with `blink run {command}`
- for running commands with args you need to add `--args` e.x `blink run python --args main.py`.
- Adding custom commands is done in Build.toml, any command can be added and will be executed using `blink run {commandName}` commands in build.toml DO NOT NEED --args bc this just executes them normally. The command line tool needs a flag for optional args which is why `--args` is required
- Languge binaries are all the standalone versions that dont need installing
- Package managers MUST USE `blink run` or else it will not use the blink specific manager and install it system wide `blink run pip --args install numnpy`
- Pacakges install were the usually would (most of the time see below)

## Quick Start
```bash
#start project
mkdir myProject
cd myProject
blink init # AT THE TOP LEVEL OF YOUR PROJECT
blink langAdd python 3.10

#install dependancies
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
In build.toml you can define your own commands as easily as adding a new entry 
<br>
```toml
    build = "dotnet build .."
    run = "dotnet run ..\src\\"
    foo = "ls -l"
```
all you do is run `blink run build` or `blink run foo` you can name these commands anything you want.


### The Proccess Behind Blink
It works by packaging all packages, binaries, runtimes, and path configurations into a neat folder. When you run a command like `blink run python --args main.py`. Blink has its own path independant of your system. You can see it in `config.toml`. Blink checks the path in `config.toml` and replaces `python` with `.blink\bin\python\bin\python.exe` and if main.py is on your path it does the same thing. As long as you use `blink run` you will use the packages and binaries you have installed in the .blink folder. I think this is the magic of it all is being able to bundle everything together and keep enviroment variables alive and if you want to add more just add them to `config.toml` and you can freely use that file
