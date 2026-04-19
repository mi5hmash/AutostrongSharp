[![License: MIT](https://img.shields.io/badge/License-MIT-blueviolet.svg)](https://opensource.org/license/mit)
[![Release Version](https://img.shields.io/github/v/tag/mi5hmash/AutostrongSharp?label=Version)](https://github.com/mi5hmash/AutostrongSharp/releases/latest)
[![Visual Studio 2026](https://custom-icon-badges.demolab.com/badge/Visual%20Studio%202026-F0ECF8.svg?&logo=visual-studio-26)](https://visualstudio.microsoft.com/)
[![.NET10](https://img.shields.io/badge/.NET%2010-512BD4?logo=dotnet&logoColor=fff)](#)

> [!IMPORTANT]
> **This software is free and open source. If someone asks you to pay for it, it's likely a scam.**

# :cd: AutostrongSharp - What is it :interrobang:
This application can **decrypt and encrypt SaveData files** from various games running on RE Engine. It can also **re-sign these SaveData files** with your own SteamID to **use anyone’s SaveData on your Steam Account**.

> [!NOTE]
The Autostrong encryption scheme is built on the Blowfish algorithm.

## Supported game titles
|Game Title|App ID|Platform|
|---|---|---|
|Apollo Justice Ace Attorney Trilogy|2187220|Steam|
|Capcom Arcade 2nd Stadium|1755910|Steam|
|Capcom Arcade Stadium|1515950|Steam|
|Devil May Cry 5|601150|Steam|
|Ghost Trick Phantom Detective|1967430|Steam|
|Ghosts 'n Goblins Resurrection|1375400|Steam|
|Onimusha 2 Samurai's Destiny|3046600|Steam|
|Resident Evil 2 Remake|883710|Steam|
|Resident Evil 3 Remake|952060|Steam|
|Resident Evil 7 Biohazard|418370|Steam|
|Resident Evil 8 Village|1196590|Steam|

# 🤯 Why was it created :interrobang:
I wanted to share my SaveData files with a friend, but they were incompatible with his Steam Account.

# :scream: Is it safe?
The short answer is: **No.** 
> [!CAUTION]
> If you unreasonably edit your SaveData files, you risk corrupting them or getting banned from playing online. In both cases, you will lose your progress.

> [!IMPORTANT]
> Always back up the files you intend to edit before editing them.

> [!IMPORTANT]
> Disable the Steam Cloud before you replace any SaveData files.

You have been warned, and now that you are completely aware of what might happen, you may proceed to the next chapter.

# :scroll: How to use this tool
## [GUI] - 🪟 Windows 
> [!IMPORTANT]
> If you’re working on Linux or macOS, skip this chapter and move on to the next one.

On Windows, you can use either the CLI or the GUI version, but in this chapter I’ll describe the latter.

<img src="https://github.com/mi5hmash/AutostrongSharp/blob/main/.resources/images/MainWindow-v2.png" alt="MainWindow-v2"/>

### BASIC OPERATIONS

#### 1. Setting the Input Directory
You can set the input folder in whichever way feels most convenient:
- **Drag & drop:** Drop SaveData file - or the folder containing it - onto the TextBox **(1)**.
- **Pick a folder manually:** Click the button **(2)** to open a folder‑picker window and browse to the directory where SaveData file is.
- **Type it in:** If you already know the path, simply enter it directly into the TextBox **(1)**.

#### 2. Entering the User ID
In the case of Steam, your User ID is [your Friend Code](https://steamcommunity.com/friends/add).  

#### 3. Selecting the Game Profile
Game Profile is a configuration file that stores the settings for a specific game.
In plain terms, it tells my application how it should behave for that particular game.
I include a package with ready‑to‑use Game Profile files (**profiles.zip**) in the release section.
The ***"_profiles"*** folder inside that package, containing the Game Profile files, should be placed in the same directory as the program’s executable.
Button **(4)** opens the local ***"_profiles"*** folder.

#### 4. Re-signing SaveData files
If you want to re-sign your SaveData files so you can use them on another Steam Account, type in the User ID of that Steam Account into a TextBox **(3)**. Once you have it typed in, select the Game Profile **(5)** corresponding to the game from which the save file comes, and press the **"Re-sign All"** button **(9)**.

> [!NOTE]
> The re‑signed files will be placed in a newly created folder within the ***"AutostrongSharp/_OUTPUT/"*** folder.

#### 5. Accessing modified files
Modified files are being placed in a newly created folder within the ***"AutostrongSharp/_OUTPUT/"*** folder. You may open this directory in a new File Explorer window by pressing the button **(10)**.

> [!NOTE]
> After you locate the modified files, you can copy them into your save‑game folder.
> For Steam, the path looks like this:
> ***"<STEAM_INSTALL_DIRECTORY>/userdata/<USER_ID>/<APP_ID>/remote/win64_save/"***

### ADVANCED OPERATIONS

#### Enabling SuperUser Mode

> [!WARNING]
> This mode is for advanced users only.

If you really need it, you can enable SuperUser mode by triple-clicking the version number label **(11)**.

#### Decrypting SaveData files

> [!IMPORTANT]  
> This button is visible only when the SuperUser Mode is Enabled. 

If you want to decrypt SaveData file\s to read its content, select the Game Profile **(5)** corresponding to the game from which the SaveData file comes, and press the **"Decrypt All"** button **(6)**.

#### Encrypting SaveData files

> [!IMPORTANT]  
> This button is visible only when the SuperUser Mode is Enabled. 

If you want to encrypt the decrypted SaveData file\s, select the Game Profile **(5)** corresponding to the game from which the SaveData file comes, and press the **"Encrypt All"** button **(7)**.

### OTHER BUTTONS
Button **(8)** cancels the currently running operation.
Button **(12)** tries to get the current owner of the first SaveData file in the Input Directory.

## [CLI] - 🪟 Windows | 🐧 Linux | 🍎 macOS

```plaintext
Usage: .\autostrong-sharp-cli.exe -m <mode> [options]

Modes:
  -m d  Decrypt SaveData files
  -m e  Encrypt SaveData files
  -m r  Re-sign SaveData files
  -m o  Get current owner of the first SaveData file

Options:
  -g <game_profile_path>  Path to the Game Profile file
  -p <input_folder_path>  Path to folder containing SaveData files
  -u <user_id>            User ID (used in re-sign mode)
  -q                      Don't wait for user input to exit after operation completes (auto-close)
  -h                      Show this help message
```

### Examples
#### Decrypt
```bash
.\autostrong-sharp-cli.exe -m d -g ".\game_profile.bin" -p ".\InputDirectory"
```
#### Encrypt
```bash
.\autostrong-sharp-cli.exe -m e -g ".\game_profile.bin" -p ".\InputDirectory"
```
#### Re-sign
```bash
.\autostrong-sharp-cli.exe -m r -g ".\game_profile.bin" -p ".\InputDirectory" -u 1
```

#### Get current owner of the first SaveData file
```bash
.\autostrong-sharp-cli.exe -m o -g ".\game_profile.bin" -p ".\InputDirectory"
```

> [!NOTE]
> Modified files are being placed in a newly created folder within the ***"AutostrongSharp/_OUTPUT/"*** folder.

# :fire: Issues
All the problems I've encountered during my tests have been fixed on the go. If you find any other issues (which I hope you won't) feel free to report them [there](https://github.com/mi5hmash/AutostrongSharp/issues).

> [!TIP]
> This application creates a log file that may be helpful in troubleshooting.  
It can be found in the same directory as the executable file.  
Application stores up to two log files from the most recent sessions.

## [ISSUE] Game doesn’t detect SaveData file\s on Steam
If you purchased the game on Steam and it doesn’t detect your re‑signed SaveData files after you’ve placed them in the correct folder, the issue is usually that a valid `remotecache.vdf` file also needs to be generated.

Instructions on how to generate a proper `remotecache.vdf` can be found in [this gist](https://gist.github.com/mi5hmash/47f1be53d213be9b00f2c7e0aa151b11).

## [ISSUE] Not all controls are visible in the WPF application on Windows
You probably have your Windows system font size set higher than the default.
Set the font size back to the default value, or press **`CTRL + SHIFT + J`** to unlock window resizing in the application.