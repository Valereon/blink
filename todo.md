- make any string defined in build.toml executeable as a command
- some form of sandboxing?
- a add python and sets up the env script and the folders needed and fetches the binaries from my github

- fix the incosnisty of relative and absoulte paths i think go for all absoulte adn combine the path fixing so that you can handle relative and normal paths in the same method


- make a custom toml class to handle all the toml shit and make vars for everythign so people can change the stuff

- make a verify method that goes through both TOMls and makes sure the paths are relative to the project root

- find a way to pick a version of a program if its the same name and on the path so
what takes priotiy the path or maybe theres a specail path character so

python.exe $main.py

and then it will force use the path

- path = [".\bin\pro\main.py"]

- src
- main.py