<!-- - some form of sand boxing? -->


- a add python and fetches the binaries from python.com and sets it up gracefully inside the proper folder and add the folders and add the binary to the path

- make a verify method that goes through both TOMls and makes sure the paths are relative to the project root


 
- add support to chaNGE SLN files and others in vs and vscode for better inclusion and intellisense
idk if this is important people can just add to their include path in visual studio



- make blank toml to copy for projects that include all the settings

- more error handling

- add a filesystem integrity check on every run to make sure core things like build.toml or config.toml are'nt missing or the .blink folder

aka when you try and run a blink command if there is no .blink file its not a valid blink project or directory and you can just handle the toml's not being there normally and if they are'nt there during a run you just tell them that recreate the file and they can populate it again

- add linux and mac support by including those binaries when using add if the they are added in the target platform in config.toml



- improve ux in the command line and understanding of the tool

- finish the verify command and --fix command


- fix the binaries downloading when folder exists it exits the program, even if the bin folder is empty under lang

- add a list command, command for the build.toml



- MAYBE IN THE FUTURE ADD A TRADEOFF
 - so if teams host on github or whatever you should be able to produce a diff file for packages and binaries so say like idk maybe not binaries cause thats harder to figure out but say like i update numpy to 3.0 and yours is 2.9 and we sync enviroments there should be a way to blink export --packages then it will export a full list of packages and version numbers and then when you get that file you run blink update --packages and thenit will update packages accordingly but binaries is too hard they probably would just do a full project sync

