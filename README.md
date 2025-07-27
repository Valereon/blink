# blink

Blink is my attempt to solve "it works on my machine." I know many languages have their own version of a dev environment like pythons .venv but those require setup and are not universal. My goal was to make a universal format and command line tool to eliminate complex setup processes. Everything you will need to work on a blink project is inside of the .blink included with it.


standalone enviroment 


### How it works
blink works by packaging all packages, binares and runtimes required by the source project into a neat little .blink folder. It also adds the runtimes to the blink path. So when you send someone your zip of your project they unzip it and have the entire environment with the EXACT same things you had because everything is saved in that .blink folder. When you execute <code>blink run python --args .\src\main.py</code> blink has its own path independent of your system so it looks at this command and says "hey do i have python on blinkPath" and if it does it replaces "python" with <code>"C:\project\\.blink\bin\python\bin\python.exe"</code> and so it runs <code>C:\project\\.blink\bin\python\bin\python.exe C:\project\src\main.py</code> this is the magic of blink. It will use the .blinks python.exe and the python packages so that your systems packages and state DO NOT matter and it will just run!


### The Drawbacks
Since blink includes all packages and binaries inside your project the file size will be significantly increased. Costing more bandwidth to download and harder to share across the internet
### The benefits
Since blink includes all necessary files for a project, if any of the packages are no longer available, or you modify them internally for your own use, blink will still have a copy of that file and you wont be left stranded without it. Not only that but blink is a one download and the entire environment is there meaning its offline friendly, one click setup, and easy onboarding. There is no fiddling with anything to onboard someone, you just send it to them and they are up and running!


## Quick Start
```bash
#start project
mkdir myProject
cd myProject
blink init
blink lang add python

#install depedancies
blink run pip install http


#create and run it
echo "import http; print('Http is working!')" > main.py
blink run python main.py


# share it
zip -r myProject.zip
```



<br>
<br>

## How to Setup a blink environment
It is preferable to set up your blink environment when you are starting your project but it is not necessary the steps are the same either way.

1. run <code>blink init</code> inside the topmost level of your project because it cannot use higher folders
2. run <code>blink lang add</code> and one of the following 
    - rust
    - python
    - c
    - cpp
    - java
    - csharp

this will add the language runtime and the package manager to the blink environment



## Using the blink environment
Since blink has its own path, there is a caveat with using it. You have to preface every command with `blink run (command)` dont worry! thats the only change! once you have the path setup you just do that and your golden!

THIS INCLUDES PACKAGE MANAGERS `blink run pip install requests` IS PROPER
this is because the blink environment needs to redirect the install location of these packages
<br>
This also happens with running the program it tells the runtime where the packages are so you HAVE to use `blink run` for everything that requires use of the environment.


## Defining your own commands
In build.toml you can define your own commands as easily as adding a new entry 
<br>
```toml
    build = "dotnet build .."
    run = "dotnet run ..\src\\"
    foo = "ls -l"
```
and you just run `blink run build` or `blink run run` you can name these commands anything you want. All you have to do is save the build.toml and they will work.


## Getting Specific Language Version runtimes
this will add that specified languages latest runtime and latest package manager to the blink environment
    - if you want to add a specified version of the language do <code>blink lang add python@3.9 </code>





# Langauge Specific Quirks
## Python
so in python you can use pip normally with `blink r pip install numpy` and you can do normal pip still as well as uninstall but you have to use the pip provided by blink for blink to work. So the structure of the blink files i tried to keep the same such as 

- .blink\
    - bin\
        - python\
            - bin\
            - cache\

but because of how this standalone pip version works i cannot change where the packages install without loosing major functionality so the cache folder will remain which is normally where the packages would go but for python it will be more like

 
- python\
    - bin\
        - lib\
            - sitePackages\
                - packages
        - scripts\
            - scripts
    - cache\
        - empty
<br>
<br>




the rest will be the python runtime binary in this folder


this doesnt really affect the end user as they are not likely to be doing stuff in this folder manually but its good to know