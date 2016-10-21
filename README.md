# YRC-for-Windows
A simple command line tool to use some of the remote control features of Yamaha's network receivers. 

## Usage

YRC can be used with command line arguments only, if you want to have a graphical interface, simply use the AV-Controller app by Yamaha or the webinterface of your receiver.

### Current Arguments

* The first argument (required): __must__ be the IP address of your receiver: xxx.xxx.xxx.xxx

* __-power_on__: Turns the receiver on.

* __-power_off__: Turn the receiver off.

* __-setvol__ xxx: The volume of your receiver. This is a double represented as an integer. You can use only values between 0 and 805.
While 0 is -0.00 dB (which is pretty loud) and 805 is mute. 800 is the lowest volume possible. Only values with .00 or .50 are shown within the apps, so I am not sure if 0.34 is valid or not.

* __-input__ xxx: The input of your receiver. Example values: AV1, AV2, HDMI1, SERVER, ...

* __-fadevol__ startvolume finalvolume stepsize sleepduration: Sets the volume to your finalvolume over time.


|    Argument   | Datatype | Suggested Range |                                Explanation                                |
|:-------------:|:--------:|:---------------:|:-------------------------------------------------------------------------:|
|  startvolume  |   Int32  |     [0-805]     | The volume the fade starts with (can be higher or lower than finalvolume) |
|  finalvolume  |   Int32  |     [0-805]     |   The target volume to fade to (can be higher or lower than startvolume)  |
|    stepsize   |   Int32  |      [1-50]     |         The volume is increasing/decreasing by each stepsize/cycle        |
| sleepduration |   Int32  |      [1-∞[      |                          The target sleepduration                         |


I am using: `-fadevol 600 350 25 350` or `-fadevol 600 300 1 1` to fade in, to fade out, simply replace the startvolume with the finalvolume.

* __-directcommand__: Executes any custom command - keep in mind that you have to escape the string properly. 

* __-disable_sleep__: Disables the sleeping for the server functions´.

* __-set_server__ arg[1];arg[2];arg[n]: Use the server function with your custom arguments.

This is acutally highly tricky because the Yamaha Receivers are using locally cached pointers (at least I haven't figured out how to access this otherwise).

On startup, you can use it this way:

`.\Yamaha_Remote_Console.exe 192.168.178.54 -power_on -set_server "Zeus;Music Library;Titel;The Hanging Tree"`

This is accessing the Server "Zeus", entering the "Music Library", entering the list "Titel" and finally plays the title "The Hanging Tree".

Since the pointer is stored on the receiver's cache, you now can simply use `-set_server "Titlename"` to switch the Title. 

If you want to use another server, you have to reset the input first `-input AV1` (for example). I highly recommend to get the names of the arguments you need here from the webinterface.

This function is always setting the input to SERVER and mutes the autoplay, if you want the receiver to autoplay from your last used server, simply use `-input SERVER`.

This function also freezes the program for some seconds (one second per executed argument) in order to work properly.

### Example Usage

.\Yamaha_Remote_Console.exe -power_on -input AV1 -setvol 450

Turns the receiver with the ip address "192.168.178.54" on, sets the input to AV1 and sets the volume to -45.00 dB. 

## Why shall I use YRC?

Well, you don't have to. It was just written in order to automate the receiver.

## Misc

### Planned features

* Increasing & Decreasing Volume: Start at value xx and move to value xx with xx iterations. -> done!
* Sleep-Function: Use the built-in sleep function.
