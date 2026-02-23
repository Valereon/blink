# blink

blink is my attempt to solve "it works on my machine." and long setup in a bit of an unorthodox way. The goal of blink is the same as Docker or Nix, to create an easy way to reproduce dev enviroments. In my opinion both of these are pretty overkill for most sitiuations, you do not need an exact replica of the targets setup. Most of the time, a snapshot of all the libraries, the codebase, and the path can be enough.


Lets walk through what its like to be on the reciving end of a blink project.

1. Obtain the zip file
2. unzip it
3. run the code!

It really is that simple! I have taken a different approach to this problem, what blink does is try and make everything self contained and able to be zipped into a single file, how it does this is, it installs a portable language runtime for your project, then as you add packages to it, the packages are isolated to soley be in that blink project. Furthermore blink has its own path seperate from the system! this helps isolate projects but also allows for sharability as peopel dont have to mess with their path with your custom stuff. Finally blink has a custom command file! so you can make commands that act as normal commands if they are valid bash programs.




# Pros and Cons
- There are some notable trade offs to this compared to other methods
# Cons
- Large file size
    - since you are storing every library, and every language runtime inside the project it adds up quickly.
- Some learning curve and quirks.


# Pros
- One step setup
- easy new contributor onboarding
- backups of entire enviroments
- backups of packages that could cease to exist or could break
- offline setup
- single download
- future proof
- cross language





