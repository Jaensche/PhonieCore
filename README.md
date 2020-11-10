# PhonieCore
A simple Raspberry Pi RFID music box implemented in .NET Core 

Inspired by the [Phoniebox project](http://phoniebox.de)

## What it does
- Plays mp3 file(s) when a corresponding RFID tag is recognized (uid)
- When a new tag is recognized a folder is created in a samba share
- When an existing tag is recognized the media files in the folder are played. 
- The folder that corresponds with the current tag is marked with an @ in the end (so that one knows where to add the media files for that tag)

## Hardware
- Raspberry pi 3b running raspbian buster lite
- rc522 RFID reader connected via SPI
- Powerbank with pass through changing (to enable use and charge at the same time)
- Nice box that fits all components

## Installation
### Update Raspbian
```
sudo apt update
sudo apt upgrade
```

### Samba shares
```
sudo apt-get install samba
sudo nano /etc/samba/smb.conf
```
Add these lines at the bottom to get two open shares for the executable and the media:
```
[media]
path = /media/
public = yes
writable = yes
guest ok = yes

[core]
path = /opt/phoniecore
public = yes
writable = yes
guests ok = yes
```

### Zeroconf (use hostname for connections) 
```
sudo apt-get install avahi-daemon
sudo nano /etc/hostname
```

### Mopidy
```
sudo apt-get install mopidy
sudo apt-get install python3-pip
wget -q -O - https://apt.mopidy.com/mopidy.gpg | sudo apt-key add -
sudo wget -q -O /etc/apt/sources.list.d/mopidy.list https://apt.mopidy.com/buster.list
sudo apt-get install python-spotify
sudo python3 -m pip install Mopidy-Spotify
```
mopidy.conf
```
[spotify]
client_id = ...
client_secret = ...
```

### .Net Core
https://dotnet.microsoft.com/download/dotnet-core

### PhonieCore
```
Copy the binaries to the core share
```

### Run PhonieCore using systemd
https://devblogs.microsoft.com/dotnet/net-core-and-systemd/

## ToDo
- [ ] Proper shutdown

