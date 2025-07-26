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