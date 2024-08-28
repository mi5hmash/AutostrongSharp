[![License: Unlicense](https://img.shields.io/badge/License-Unlicense-blueviolet.svg)](https://opensource.org/licenses/Unlicense)
[![Release Version](https://img.shields.io/github/v/tag/mi5hmash/AutostrongSharp?label=Version)](https://github.com/mi5hmash/AutostrongSharp/releases/latest)
[![Visual Studio 2022](https://custom-icon-badges.demolab.com/badge/Visual%20Studio%202022-5C2D91.svg?&logo=visual-studio&logoColor=white)](https://visualstudio.microsoft.com/)
[![.NET8](https://img.shields.io/badge/.NET%208-512BD4?logo=dotnet&logoColor=fff)](#)

> [!IMPORTANT]
> **This software is free and open source. If someone asks you to pay for it, it's likely a scam.**

# :cd: AutostrongSharp - What is it :interrobang:
This application can **decrypt and encrypt SaveData files** from various games running on RE Engine. It can also **resign these SaveData files** with your own SteamID to **use any SaveData on your Steam Account**.

## Supported titles*
| Game Title                | App ID  | Tested Version | Platform |
|---------------------------|---------|----------------|----------|
| Devil May Cry 5           | 601150  | 11025947       | PC       |
| Resident Evil 2 Remake    | 883710  | 11636119       | PC       |
| Resident Evil 3 Remake    | 952060  | 11960962       | PC       |
| Resident Evil 7 Biohazard | 418370  | 11026049       | PC       |
| Resident Evil 8 Village   | 1196590 | 11260452       | PC       |

**the most recent versions that I have tested and are supported.*

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

<img src="https://github.com/mi5hmash/AutostrongSharp/blob/main/.resources/images/MainWindow.png" alt="MainWindow"/>

## Setting the Input Directory
There are three ways to achieve this. The first one is to drop the SaveData file or a folder that contains it, onto a TextBox **(1)** or a button **(2)**. Alternatively, you may use a button **(2)** to open a folder-picker window and navigate to the directory with it. You can also type the path to the folder in the **(1)** TextBox.

> [!TIP]
> The program will extract the Steam32_ID from the "Input Folder Path" TextBox **(1)**, if it ends with *"<steam_id>\\<steam_appid>\remote\win64_save"*, and will fill the TextBox **(3)** for you.

## About Steam32 ID
It is a 32-bit representation of your 64-bit SteamID.

##### Example:
| 64-bit SteamID    | 32-bit SteamID |
|-------------------|----------------|
| 76561197960265729 | 1              |

> [!NOTE]
> Steam32 ID is also known as AccountID or Friend Code. 

> [!TIP]
You can use the calculator on [steamdb.info](https://steamdb.info/calculator/) to find your SteamID.

## Resigning files
If you want to resign your SaveData files so you can use them on another Steam Account, type in the Steam32_ID of that Steam Account into a TextBox **(3)**. Once you have it typed in, select the Game Profile **(4)** corresponding to the game from which the save file comes, and press the **"Resign All"** button **(10)**.

## Enabling SuperUser Mode

> [!WARNING]
> This mode is for advanced users only.

If you really need it, you can enable SuperUser mode by triple-clicking the version number label **(11)**.

## Decrypting files

> [!IMPORTANT]  
> This button is visible only when the SuperUser Mode is Enabled. 

If you want to decrypt SaveData file\s to read its content, select the Game Profile **(4)** corresponding to the game from which the SaveData file comes, and press the **"Decrypt All"** button **(7)**.

## Encrypting files

> [!IMPORTANT]  
> This button is visible only when the SuperUser Mode is Enabled. 

If you want to encrypt the decrypted SaveData file\s, select the Game Profile **(4)** corresponding to the game from which the SaveData file comes, and press the **"Encrypt All"** button **(8)**.

## Backup functionality
By default, the backup option is checked **(5)**. In this state, the application will back up files before each operation to the new folder inside the ***"AutostrongSharp/_BACKUP/"*** directory. This app can create up to 3 zip archives.

## Open the Backup Directory
You may open the ***"AutostrongSharp/_BACKUP/"*** directory in a new File Explorer window by pressing the button **(6)**.

## Other buttons
Button **(9)** cancels the currently running operation.

# :fire: Issues
All the problems I've encountered during my tests have been fixed on the go. If you find any other issues (which I hope you won't) feel free to report them [there](https://github.com/mi5hmash/AutostrongSharp/issues).

> [!TIP]
> This application creates a log file that may be helpful in troubleshooting.  
It can be found in the same directory as the executable file.

**IF YOU DO NOT SEE SAVEDATA FILES THAT YOU HAVE RESIGNED, IN THE GAME MENU, THEN PLEASE, READ <a href="https://github.com/mi5hmash/AutostrongSharp/tree/main/.resources/Save%20Files" target="_blank">THIS DOCUMENT</a>.**

# :star: Sources
* https://github.com/tremwil/DS3SaveUnpacker
* https://www.steamgriddb.com - Game Profile icons
