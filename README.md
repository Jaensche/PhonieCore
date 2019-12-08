# PhonieCore
A simple RFID music box implemented in .NET Core

## What it does
Plays mp3 file(s) when a corresponding RFID tag is recognized (uid). 
When a new tag is recognized a folder is created in the samba share. 
When an existing tag is recognized the mp3 files in the folder are played. 
The folder that corresponds with the current tag is marked with an @ in the end (so that one knows where to add the files for that tag). 

## How it does it

## Installation
Update your raspbian
```
sudo apt update
sudo apt upgrade
```

Get samba
```
sudo apt-get install samba
```

Configure samba
```
TBD
```

Get omxplayer
```
sudo apt-get install omxplayer
```

Install .Net Core
https://dotnet.microsoft.com/download/dotnet-core

Set up systemd
https://devblogs.microsoft.com/dotnet/net-core-and-systemd/

