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
3) Next, you'll need to make sure 'Hide manager game object' is set to True in the BepInEx config. You may need to run the game with mods once for this to appear. The way you edit mod configs depends on your mod manager:
    * With Thunderstore Mod manager, click "Edit config" on the Profile page, then click edit on "BepInEx/config/BepInEx.cfg". Under the "Chainloader" section, set "HideManagerGameObject" to true and click save.
    * With r2modman, select your profile and click "Config editor" on the sidebar to your left. Select "BepInEx/config/BepInEx.cfg", then "Edit Config". Under the "Chainloader" section, set "HideManagerGameObject" to true and click Save.
    * With Gale, select your profile and click "Edit mod config" on the sidebar to your left. Select "BepInEx", then "ChainLoader", and make sure that the button to the right of "Hide manager game object" has a green checkmark.
4) Click the Run Modded button to launch R.E.P.O. When the main menu loads, you should see a new button at the bottom of the screen that says "ARCHIPELAGO".
5) On the main menu, click on the Archipelago button to open the connection menu. Enter your multiworld's address, port, password (if it has one, otherwise leave the box blank), and your slot name, then click CONNECT. If you entered everything correctly, the text above the address box should change from 'Not connected' to 'Connected'.
6) After a successful connection, start a new save file and begin playing.

#### Manual Client Install Instructions
1) Download version 5.4.21 of BepInEx from https://thunderstore.io/c/repo/p/BepInEx/BepInExPack/
2) Follow the instructions on Thunderstore to manually install BepInEx.
3) Run R.E.P.O. so BepInEx completes its installation.
4) In your R.E.P.O. game folder, open the BepInEx/config folder and open the BepInEx.cfg file with any text editor
5) Under [Chainloader] set HideManagerGameObject to true. Then save and close the file.
3) Download the latest release of the client plugin.
4) Extract the contents of the downloaded zip folder into the BepInEx/Plugins folder.
6) Download MenuLib and REPOLib from Thunderstore and add them to your plugins folder.
7) Launch R.E.P.O.
8) On the main menu, click the Archipelago button and enter the server address, port, server password, and player slot name into the listed fields.
9) After a successful connection, start a new save file and begin playing.

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
- Truck messages will currently be displayed redundantly if there is more than one host (connected) player in the same lobby.
