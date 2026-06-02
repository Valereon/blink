

## do now
add system path support

add remove from path command

i do not need to be fetching shit from the build and config toml all the time just load them as objects into memory and pass them as context objects and write them on the exit of the command

add blink getting added to the project for packaging

add an export command 

add the ability to chain build.toml commands? cause you cant do that currently ( like i might have pip="eringe/ergtretg/ertg.pyz" and i want to run a pip command like pipInf="pip --version" but right now i would have to use pip again in the command instead of just saying pip, the only thing is then when you change stuff like lets say its pip3="eourngoeirng/retghrtgh/rthg.pyz" and i do pipVer="pip3 --version" then idk i have to rename shiz properly idk maybe an interactive command editor?)






## Med priority




## Low Priority
idk if this is important people can just add to their include path in visual studio
- add linux and mac support by including those binaries when using add if the they are added in the target platform in config.toml



## Dreams/ far future
make a command or toggle in config to when exporting it will copy all binaries off your system and into a blink folder but the issue there is stuff relys on more than the binary?? so do we go up a folder and take from there or what?? but that wont even work either cause some stuff is nested deep, oh well cool idea and could be very useful for even deeper path replication but idk.

turn it into a interactive shell so that you dont have to blink run but the thing is its hard plus theres so much i would have to change and such and tab completion and stuff idk




okay so when i start to implement multi platform (if i do ) all i need to do is make blinkfs non static and make a filesystem interface so that its like that so that all i have to do is change the actual implmenmtation but not the logic so its easy to swap them back and forth between windows, linux, and mac, but actually idk if the Path class will handle most of it?

yes but some stuff is differnetg like running programs and stufdf on linux you need chmod and other things that are system specifgic and not io specifgic



public interface IFileSystem(){
    writefile()
    readfile()
    whatever()
}



public class LinuxFS implements IFileSystem(){
    0[ieaqrnmge
    rg
    eworgewrgopew
    org
    werg
    ew
    rg
    erg
    erg
    erg
    erg
    ]
}
