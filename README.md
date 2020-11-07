SteamyNG

MirrorNG docs and the official community Discord.

SteamyNG brings together Steam and MirrorNG utilising Async of a Steam P2P network transport layer for MirrorNG.
Dependencies

Both of these projects need to be installed and working before you can use this transport.

    SteamWorks.NET SteamyNG relies on Steamworks.NET to communicate with the Steamworks API. Requires .Net 4.x
    Mirror FizzySteamworks is also obviously dependant on MirrorNG which is a streamline, bug fixed, maintained version of UNET for Unity.
    
FizzySteam is only for 64bit version. If you require 32bit you will need to find the dlls yourself.

## Installation
The preferred installation method is Unity Package manager.

If you are using unity 2019.3 or later: 

1) Open your project in unity
2) Install [MirrorNG](https://github.com/MirrorNG/MirrorNG)
3) Click on Windows -> Package Manager
4) Click on the plus sign on the left and click on "Add package from git URL..."
5) enter https://github.com/MirrorNG/FizzySteamyMirror.git?path=/Assets/Mirror/Runtime/Transport/FizzySteamyMirror
6) Unity will download and install MirrorNG SteamyNG

Note: The default 480(Spacewar) appid is a very grey area, technically, it's not allowed but they don't really do anything about it. When you have your own appid from steam then replace the 480 with your own game appid. If you know a better way around this please make a Issue ticket.
Host

To be able to have your game working you need to make sure you have Steam running in the background. SteamManager will print a Debug Message if it initializes correctly.
Client

Before sending your game to your buddy make sure you have your steamID64 ready. To find your steamID64 the transport prints the hosts steamID64 in the console when the server has started.

    Send the game to your buddy. The transport shows your Steam User ID after you have started a server.
    Your buddy needs your steamID64 to be able to connect.
    Place the steamID64 into "localhost" then click "Client"
    Then they will be connected to you.

Testing your game locally

You cant connect to yourself locally while using SteamyNG since it's using steams P2P. If you want to test your game locally you'll have to use "Telepathy Transport" instead of "SteamyNG".

Thanks to all developers of the original code for this work. I have just made it work for MirrorNG the original creators deserve the thanks
