#!/bin/bash

# Known directories:
case1="$HOME/.steam/steam/steamapps/common/DayZ/"
case2="/run/media/mmcblk0p1/steamapps/common/DayZ"
case3="/run/media/mmcblk0p1/SteamLibrary/steamapps/common/DayZ"

# Set executable:
chmod +x ./bin/DayZLauncher.UnixPatcher

# Patch launcher:
echo "Attempting to patch the DayZ Launcher"
if [-d "$case1"]
then 
    echo "DayZ Launcher found at $case1/Launcher"
    ./DayZLauncher.UnixPatcher "$case1"
    exit 0
else if [-d "$case2"]
then
    echo "DayZ Launcher found at $case2/Launcher"
    ./DayZLauncher.UnixPatcher "$case2"
    exit 0
else if [-d "$case3"]
then
    echo "DayZ Launcher found at $case3/Launcher"
    ./DayZLauncher.UnixPatcher "$case3"
    exit 0
else
    echo "DayZ path not found!"
    echo "Run the patch manually via a terminal by typing:"
    echo "./bin/DayZLauncher.UnixPatcher "/Path/To/DayZ/""
    exit 0
fi
echo "Patch failed!"
exit 1