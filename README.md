# music-prank
This application runs secretely on background and every 'X' minutes plays a random song to all sound outputs.

## Why it was created?
Whole purpose of this project is to tease somebody. This program still runs and every specified interval it plays a random song to all sound outputs. So victim's computer starts playing a song and the victim wonder why its computer does that :)

## Features
* Plays to all sound ouputs - headphones plugged don't help to mute the sound
* Still unmutes and sets volume level so victim can't turn it off
* On the start up sets program itself to run on every Windows start up

## How it works
Whole magic is done in [Controller.cs](src/Mcpk/Controller.cs). As you can see, program first copy itself to *C:\Users\\<\<user\>\>\AppData\Local\NETBrains\Mcpk*. Then it creates an entry in the registry to run copied program on every Windows start up. So far very simple.

After that it starts two loops:
1. Controls playing songs
2. Unmutes volume and set volume level if song is playing

Let's describe the first loop. It waits 'X' minutes within specified time interval. The 'X' is chosen by random. So if interval 20-40 minutes is specified, it can wait 21 minutes, 25 minutes, 35 minutes, ... Then a random song si chosen. Songs are in the same directory as exe file and their name must starts with MST prefix. As a part of the solution there are 4 songs. One of them is in the file [MST_1.dll](src/Mcpk/MST_1.dll). It's just an mp3 file with extension changed to dll. After song is played the application waits again...

## Usage
Compile the project and run the program on the victim's computer. That's all. Enjoy!