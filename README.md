# HoyoVERSE

A miHoYo games launcher

## How to build?

Download & Install Visual Studio

Install .NET Development Environment

Open the project inside Visual Studio, Navigate to Build -> Build Solution.

## Known Issues

Switching to CN may fail

Warp/Wish history may fail to open / crashes the launcher if the game is not installed

Switching Language may cause ArgumentOutOfRange Exception error

## System Requirements

Microsoft Windows 10 and newer

x64 Architecture (arm64 with x64 command translator, 21277+)

Stable Internet Connection

## Having any problems?

Open an issue.

## FAQ

Q: The launcher can not be launched.

A: Path can not contain any non-ASCII characters. Go and check your game path and launcher path.

Q: DLL Not found. 

A: Ensure that ALL the DLLs exists n your launcher directory. If the problem persists, check your .NET runtime installation.

Q: Game cannot be downloaded.

A: Check your internet connection. If your internet is fine, then go and check HoYolab or whatever forums. (this is Hoyo's fault)

Q: Game crashes after launching.

A: Same as 'The launcher can not be launched.'

Q: Game is not launching

A: Check your directory permission, and check whether the game did really exist.


* I have added some comments inside the source since it may be too complicated for noobs to understand the code.
