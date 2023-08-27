[![Release Version](https://img.shields.io/github/v/tag/mi5hmash/AutostrongSharp?label=version)](https://github.com/mi5hmash/AutostrongSharp/releases/latest)
[![License: MIT](https://img.shields.io/badge/License-Unlicense-blueviolet.svg)](https://opensource.org/licenses/MIT)
[![Visual Studio 2022](https://img.shields.io/badge/VS%202022-blueviolet?logo=visualstudio&logoColor=white)](https://visualstudio.microsoft.com/)
[![dotNET7](https://img.shields.io/badge/.NET%207-blueviolet)](https://visualstudio.microsoft.com/)

# :cd: AutostrongSharp - What is it :interrobang:
This application can **decrypt and encrypt save files** from various games running on RE Engine. It can also **resign these save files** with your own SteamID to **use any SaveData on your profile**.

## Supported titles*
| Game Title                | App ID  | Tested Version | Platform |
|---------------------------|---------|----------------|----------|
| Devil May Cry 5           | 601150  | 11025947       | PC       |
| Resident Evil 2 Remake    | 883710  | 11636119       | PC       |
| Resident Evil 3 Remake    | 952060  | 11026988       | PC       |
| Resident Evil 7 Biohazard | 418370  | 11026049       | PC       |
| Resident Evil 8 Village   | 1196590 | 11260452       | PC       |

**the latest versions that I have tested and are supported*

# 🤯 Why was it created :interrobang:
I wanted to share my SaveData files with a friend, but they were not compatible.

# :scream: Is it safe?
**No.** You can corrupt your SaveData files and lose your progress or get banned from playing online if you unreasonably modify your save.

Remember to always make a backup of the files that you want to edit, before modifying them.

Also, disable the Steam Cloud before you replace any SaveData files.

You have been warned and now you can move on to the next chapter, fully aware of possible consequences.

# :scroll: How to use this tool

<img src="https://github.com/mi5hmash/AutostrongSharp/blob/main/.resources/images/MainWindow.png" alt="MainWindow"/>

## Setting the Input Directory
There are three ways to achieve this. The first one is to drop the SaveData file or folder it is in on a TextBox **(1)** or a button **(2)**. Alternatively, you may use button **(2)** to open a folder picker and navigate to the directory from it. Also, you can type in the directory path into the text box **(1)**.

> **Note:** The program will extract the Steam32_ID from the "Input Folder Path" TextBox **(1)**, if it ends with *"<steam_id>\\<steam_appid>\remote\win64_save"*, and will fill the TextBox **(3)** for you.

## Decrypting files
Select the Game Profile **(4)** corresponding to the game from which the save file comes, and press the **"Decrypt All"** button **(6)**.

## Encrypting files
Select the Game Profile **(4)** corresponding to the game from which the save file comes, and press the **"Encrypt All"** button **(7)**.

## Resigning files
Type in the Steam32_ID of the user that will use the SaveData file\s, into a TextBox **(3)**.   
Then, type in the Steam32_ID, that you want to pack the SaveData file\s with, into a TextBox **(4)**. You can use [this site](https://www.steamidfinder.com) to find it. Once you have it typed in, select the Game Profile **(4)** corresponding to the game from which the save file comes, and press the **"Resign All"** button **(8)**.

## Backup functionality
By default, the backup option is checked **(5)**. In this state, the application will back up files before each operation to the the new folder inside the ***"AutostrongSharp/_BACKUP/"*** directory. Application can create up to 3 folders.

## Open the Backup Directory
You can open the Backup directory in a new Explorer window by using the button **(9)**.

## Other buttons
Button **(10)** cancels the currently running operation.

# :fire: Issues
All the problems I've encountered during my tests have been fixed on the go. If you find any other issue (hope you won't) then please, feel free to report it [there](https://github.com/mi5hmash/AutostrongSharp/issues).

**IF YOU DO NOT SEE SAVEDATA FILES THAT YOU HAVE RESIGNED THEN PLEASE, READ <a href="https://github.com/mi5hmash/AutostrongSharp/tree/main/.resources/Save%20Files" target="_blank">THIS DOCUMENT</a>.**

# :star: Sources
* https://github.com/tremwil/DS3SaveUnpacker
* https://www.steamgriddb.com - Game Profile icons