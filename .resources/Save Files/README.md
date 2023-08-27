# :old_key: Unlocking SaveData Slots

If you do not see several or one of the save files after loading the game, you may have never before saved the game manually in the slot where the save file is missing. Slots unlock when the first SaveData is written by a game on a given slot.

## How to locate SaveData directory

Complete the following path pattern with your data to find your SaveData files location: ***<steam_installation_folder>\userdata\\<steam_id>\\<steam_appid>\remote\win64_save\\***.

| Game Title                | App ID  |
|---------------------------|---------|
| Resident Evil 2 Remake    | 883710  |
| Resident Evil 3 Remake    | 952060  |
| Resident Evil 7 Biohazard | 418370  |
| Resident Evil 8 Village   | 1196590 |

## How to unlock locked SaveData slots

First of all, temporarily disable Steam Cloud and enable it after you are done.

<img src="https://github.com/mi5hmash/AutostrongSharp/blob/main/.resources/images/How to%20disable%20steam%20cloud.png" alt="How to disable steam cloud"/>

### data00-1.bin
When you are starting on a new Steam Account the first thing you should do is to run the game and reach the Main Menu. This should unlock the slot for the ***data00-1.bin*** file.

### data000.bin
Next, you should choose an option to start a New Game and reach the point where the game creates its first AutoSave. This should unlock the slot for the ***data000.bin*** file.

### data001Slot.bin - data020Slot.bin
In order to unlock the rest of the slots, you should save the game on each of these slots one by one. For Resident Evil games, you must reach the first typewriter to manually save the game state.

I have prepared an AutoSave in front of the typewriter to make it easier for you. Feel free to download the one corresponding to your game, from the table below:

| Game Title                | AutoSave  |
|---------------------------|---------|
| Resident Evil 2 Remake    | <a href="https://github.com/mi5hmash/AutostrongSharp/raw/main/.resources/Save%20Files/re2.zip" target="_blank">re2.zip</a> |
| Resident Evil 3 Remake    | <a href="https://github.com/mi5hmash/AutostrongSharp/raw/main/.resources/Save%20Files/re3.zip" target="_blank">re3.zip</a> |
| Resident Evil 7 Biohazard | <a href="https://github.com/mi5hmash/AutostrongSharp/raw/main/.resources/Save%20Files/re7.zip" target="_blank">re7.zip</a> |
| Resident Evil 8 Village   | <a href="https://github.com/mi5hmash/AutostrongSharp/raw/main/.resources/Save%20Files/re8.zip" target="_blank">re8.zip</a> |

After you unzip the archive, you have to resign the unpacked SaveData with my tool. Fill in your Steam32_ID and push the **"Resign All"** button. Then, put the resigned ***data000.bin*** in your SaveData files directory, launch the game, and load the AutoSave.

Once you are standing in front of the typewriter, simply use it to create a SaveData file in every slot possible.
  
The next steps are: Exit to the Desktop, Navigate to SaveData files directory, Delete all SaveData files in there, Copy & Paste the resigned SaveData files that you want to use.  
  
Now, when you launch the game again, you should be able to see your shiny SaveData files ready to be loaded.

When everything is done and working, you can enable Steam Cloud.

The same procedure should also work for the DLC SaveData files.

Have fun! :smile:
