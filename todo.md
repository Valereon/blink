

## do now
add system path support

add remove from path command


add blink getting added to the project for packaging

add an export command 

add the ability to chain build.toml commands? cause you cant do that currently ( like i might have pip="eringe/ergtretg/ertg.pyz" and i want to run a pip command like pipInf="pip --version" but right now i would have to use pip again in the command instead of just saying pip, the only thing is then when you change stuff like lets say its pip3="eourngoeirng/retghrtgh/rthg.pyz" and i do pipVer="pip3 --version" then idk i have to rename shiz properly idk maybe an interactive command editor?)






## Med priority
change langugae installer to use a toml based language support system to make adding new language support easy as hell



## Low Priority
idk if this is important people can just add to their include path in visual studio
- add linux and mac support by including those binaries when using add if the they are added in the target platform in config.toml

blink verify
    literally does nothing useful right now just loads the filesystem which already does some stuff but need to add fixes for when the tomls are fucked up and can be fixed by the program or more in depth issues about why the system is not running


move the entierty of config.cs into blinkfs.cs i think it makes more sense than using config cause it can be confusingly named and such but move the base files into their own uhhhh like file


## Dreams/ far future
make a command or toggle in config to when exporting it will copy all binaries off your system and into a blink folder but the issue there is stuff relys on more than the binary?? so do we go up a folder and take from there or what?? but that wont even work either cause some stuff is nested deep, oh well cool idea and could be very useful for even deeper path replication but idk.
- maybe just a list of whats on their path


## the finale
Make a shell wrapper that gets generated so that when you run "blink activate cpp" or whatever blink has a list of where all the projects are and their names so that when you activate an enviroment it can either activate in place and will have a prefix like pip venv

(blink-cpp) ~/desktop/ $: 

and then you know what your projecgt loaded is but also you can do stuff like 

(blink-cpp) ~/desktop>: run importThing

and it runs a command from the build.toml

so another thing i think is the figuring out the path thing? i guess it will just be appended to your system path right so it works but also if you want to talk to anything in the blink project of your current directory you would do a prefix like "@" so is just an alias for the blink top directory of the project you are in

(blink-cpp) ~/desktop>: ls @/bin 
(blink-cpp) ~/desktop>: python @/source/main.py 