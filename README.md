# R.E.P.O. Archipelago Client Mod

#### What is Archipelago
Archipelago is a multi-game open-source randomizer that combines a variety of games into a single multiplayer experience.
More information at https://archipelago.gg/

#### Multiworld Setup Instructions
1) Download the latest repo.apworld from releases
2) Follow the instructions on https://archipelago.gg/tutorial/Archipelago/setup/en on generating a game. Note that you will have to place the .apworld file in the lib/worlds folder of your Archipelago install and generate locally.
3) Using the Archipelago.gg website or a local machine, host a server using the output file generated.

#### Thunderstore instructions
1) Install the Thunderstore Mod Manager at https://www.overwolf.com/app/thunderstore-thunderstore_mod_manager
2) Install the R.E.P.O. Archipelago mod and all dependencies by clicking on "Install with Mod Manager" at https://thunderstore.io/c/repo/p/Automagic/Archipelago_Randomizer_for_REPO/
3) On the Profile page of the Thunderstore Mod manager, click "Edit config" then click edit on "BepInEx/config/BepInEx.cfg". You may need to run the game modded first for this to appear.
4) Under the "Chainloader" section, set "HideManagerGameObject" to true and click save.
5) Click the Run Modded button to launch R.E.P.O.
6) On the main menu click the Archipelago button and enter the server address, port, server password, and player slot name into the listed fields.
7) After a successful connection, start a new save file and begin playing.

#### Manual Client Install Instructions
1) Download version 5.4.21 of BepInEx from https://thunderstore.io/c/repo/p/BepInEx/BepInExPack/
2) Follow the instructions on Thunderstore to manually install BepInEx.
3) Run R.E.P.O. so BepInEx completes its installation.
4) In your R.E.P.O. game folder, open the BepInEx/config folder and open the BepInEx.cfg file with any text editor
5) Under [Chainloader] set HideManagerGameObject to true. Then save and close the file.
3) Download the latest release of the client plugin.
4) Extract the contents of the downloaded zip folder into the BepInEx/Plugins folder.
6) Download MenuLib and REPOLib from Thunderstore and add it into  your plugins folder.
7) Launch R.E.P.O.
8) On the main menu click the Archipelago button and enter the server address, port, server password, and player slot name into the listed fields.
9) After a successful connection, start a new save file and begin playing.

#### Randomized Items
- Upgrades
- Levels

#### Randomized Locations
- Upgrades
- Pelly Extraction

#### Known Issues
- Pellys can get stuck in the enviroment
- Non-host players dont see what the AP shop items contain