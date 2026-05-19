# R.E.P.O. Archipelago Client Mod

#### What is Archipelago
Archipelago is a multi-game open-source randomizer that combines a variety of games into a single multiplayer experience.
More information at https://archipelago.gg/

#### Multiworld Setup Instructions
1) Download the latest repo.apworld from releases
2) Follow the instructions on https://archipelago.gg/tutorial/Archipelago/setup/en on generating a game. Note that you will have to place the .apworld file in the lib/worlds folder of your Archipelago install and generate locally.
3) Using the Archipelago.gg website or a local machine, host a server using the output file generated.

#### Thunderstore instructions
1) Install a Mod Manager (I personally recommend [r2modman](<https://thunderstore.io/c/repo/p/ebkr/r2modman/>) or [Gale](<https://thunderstore.io/c/repo/p/Kesomannen/GaleModManager/>), but [Thunderstore Mod Manager](https://www.overwolf.com/app/thunderstore-thunderstore_mod_manager) is also an option)
2) Open the mod manager and create a new profile. Select that profile, then open the tab that lets you browse mods and install Archipelago Randomizer for REPO and it's dependencies. Alternatively, go to https://thunderstore.io/c/repo/p/Automagic/Archipelago_Randomizer_for_REPO/ and click the "Install with Mod Manager" button.
3) Click the Run Modded button to launch R.E.P.O. When the main menu loads, you should see a new button at the bottom of the screen that says "ARCHIPELAGO".
4) On the main menu, click on the Archipelago button to open the connection menu. Enter your multiworld's address, port, password (if it has one, otherwise leave the box blank), and your slot name, then click CONNECT. If you entered everything correctly, the text above the address box should change from 'Not connected' to 'Connected'.
5) After a successful connection, start a new save file and begin playing.

#### Manual Client Install Instructions
1) Download version 5.4.21 of BepInEx from https://thunderstore.io/c/repo/p/BepInEx/BepInExPack/
2) Follow the instructions on Thunderstore to manually install BepInEx.
3) Run R.E.P.O. so BepInEx completes its installation.
4) Download the latest release of the client plugin.
5) Extract the contents of the downloaded zip folder into the BepInEx/Plugins folder.
6) Download MenuLib and REPOLib from Thunderstore and add them to your plugins folder.
7) Launch R.E.P.O.
8) On the main menu, click the Archipelago button and enter the server address, port, server password, and player slot name into the listed fields.
9) After a successful connection, start a new save file and begin playing.

#### Downpatching the Game
When REPO receives major updates, this mod may not work correctly until it is updated to support the latest game version. If that happens, you will need to downpatch the game to an older version.
1) Follow the first 6 steps of this guide, but replace the command in step 5 with download_depot 3241660 3241661 180069324351455863
2) Open your REPO folder (in the steamapps/common folder of Steam)
3) Open the Depot folder (in steamapps/content)
4) Drag all the files in the depot folder (including the REPO.exe file) and replace the files in the REPO folder (steamapps/common)
5) Go into your steam library, right click REPO, click Properties -> Updates, and set it to only update when the game is launched (this will cause it to update if it is launched in Vanilla but not Modded)
6) In your mod manager or manual installaton, ensure that the versions of REPOLib and MenuLib you have installed are for the previous game version.
7) If you are using a mod manager, launch the game from it. If you are not using a mod manager, switch steam to Offline mode before launching the game. Once REPO finishes launching, you can switch back online.

#### Randomized Items
- Upgrades
- Levels
- Unlock Items in Shop
- Shop Stock

#### Randomized Locations
- Shop Items
- Pelly Extraction
- Valuable Extraction
- Monster Soul Extraction

#### Known Issues
- Unsecure socket notification in server. This is from an upstream library and causes no gameplay issues currently.
