using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace RepoAP
{
    /*[HarmonyPatch(typeof(ExtractionPoint), "HaulGoalSet")]
    class TestPatches
    {
        [HarmonyPrefix]
        private static void Prefix(ref int value)
        {
            value = 1;
        }
    }*/

    /*[HarmonyPatch(typeof(PunManager), "SpawnShopItem")]
    class ShopPatch
    {
        public static FieldInfo shopManager = AccessTools.Field(typeof(PunManager), "shopManager");
        [HarmonyPrefix]
        static void ShopPre(PunManager __instance, ref ItemVolume itemVolume, ref List<Item> itemList, ref int spawnCount, bool isSecret = false)
        {

            //Check if item in shop is an upgrade
            if (itemList == ((ShopManager)shopManager.GetValue(__instance)).potentialItemUpgrades)
            {
                foreach (Item item in itemList)
                {
                    item.itemName = "Archipelago Item";
                    item.maxAmountInShop = 10;
                    //item.prefab = Resources.Load<GameObject>("Items/);
                }
            }
        }
    }*/

    [HarmonyPatch(typeof(PlayerController), "Update")]
    class DebugKeys
    {
        [HarmonyPrefix]
        static void Prefix()
        {
            //MenuToggle
#if DEBUG
            if (Input.GetKeyDown(KeyCode.Insert))
            {
                Plugin.showMenu = !Plugin.showMenu;
                Debug.Log("Toggle Menu");
            }

            if (Input.GetKeyDown(KeyCode.F1))
            {
                Debug.Log("F1 Pressed");
                RunManager.instance.ChangeLevel(true, false, _changeLevelType: RunManager.ChangeLevelType.Shop);
            }
            if (Input.GetKeyDown(KeyCode.F2))
            {
                Debug.Log("F2 Pressed");

                SemiFunc.StatSetRunCurrency(100000);
            }
            if (Input.GetKeyDown(KeyCode.F3))
            {
                Debug.Log("F3 Pressed");
                foreach (var item in StatsManager.instance.itemDictionary.Keys)
                {
                    Debug.Log($"{item}");
                }

            }

            if (Input.GetKeyDown(KeyCode.F4))
            {
                Debug.Log("F4 Pressed");

                StatsManager.instance.itemsPurchased[ItemNames.upgrade_strength] = 15;
                //StatsManager.instance.
            }

            if (Input.GetKeyDown(KeyCode.F5))
            {
                Debug.Log("Try Connect");
                Plugin.connection.TryConnect(Plugin.apAddress, Int32.Parse(Plugin.apPort), Plugin.apPassword, Plugin.apSlot);    // despite what the compiler is telling us, we don't want or need to
                                                                                                                                 // await this call
            }
            if (Input.GetKeyDown(KeyCode.F6))
            {
                string completionOutput = "-- Completetion Data --";
                completionOutput += $"\nLevel Quota: {APSave.saveData.levelQuota}";

                completionOutput += $"\nPellys Required: {APSave.saveData.pellysRequired}";

                completionOutput += ("\n\nPellys Gathered: ");
                foreach (string pelly in APSave.saveData.pellysGathered)
                {
                    completionOutput += ($"\n- {pelly}");
                }
                completionOutput += "\n\nValuables Gathered: ";
                foreach (string valuable in APSave.saveData.valuablesGathered)
                {
                    completionOutput += $"\n- {valuable}";
                }
                completionOutput += "\n\nValuables Missing: ";
                foreach (string valuable in LocationNames.all_valuables)
                {
                    if (!APSave.saveData.valuablesGathered.Contains(valuable))
                    {
                        completionOutput += $"\n- Missing {valuable}";
                    }
                }
                completionOutput += "\n\nMonster Souls Missing: ";
                foreach (string soul in LocationNames.all_monster_souls)
                {
                    if (!APSave.saveData.monsterSoulsGathered.Contains(soul))
                    {
                        completionOutput += $"\n- Missing {soul}";
                    }
                }
                completionOutput += "\n\nMonster Souls Gathered: ";
                foreach (string soul in APSave.saveData.monsterSoulsGathered)
                {
                    completionOutput += $"\n- {soul}";
                }

                Debug.Log(completionOutput);
            }
            if (Input.GetKeyDown(KeyCode.F8))
            {
                Debug.Log(RunManager.instance.levelCurrent.name);
            }
            if (Input.GetKeyDown(KeyCode.F9))
            {
                Debug.Log(APSave.saveData.shopStockReceived);
            }

            if (Input.GetKeyDown(KeyCode.F10))
            {
                    string output = "--- Valuable Weights ---";
                    foreach (var levelValuables in LevelGenerator.Instance.Level.ValuablePresets)
                    {
                        var allValuables = levelValuables.tiny;
                        allValuables.AddRange(levelValuables.small);
                        allValuables.AddRange(levelValuables.medium);
                        allValuables.AddRange(levelValuables.big);
                        allValuables.AddRange(levelValuables.wide);
                        allValuables.AddRange(levelValuables.tall);
                        allValuables.AddRange(levelValuables.veryTall);


                        foreach (var val in allValuables)
                        {
                            output += $"\n{val.PrefabName} - {LevelGenerator.Instance.Level.name}: {val.Prefab.GetComponent<ValuableObject>().physAttributePreset}".Replace("PhysAttribute", "").Replace("()", "");
                        }
                    }
                Debug.Log(output);
            }

            if (Input.GetKeyDown(KeyCode.F11))
            {
                var items = APSave.GetItemsReceived();

                foreach (var item in items)
                {
                    Debug.Log(ItemData.itemIDToName[ItemData.RemoveBaseId(item.Key)]);
                }
            }
#endif
        }
    } 

    class DebugCommands
    {
        internal static void RegisterDebugCommands()
        {
            var logStoreItemsCommand = new DebugCommandHandler.ChatCommand("logstoreitems", "Prints the names of all store items in the output log.", LogAllStoreItems, Suggest, IsEnabled);
            var tryConnectCommand = new DebugCommandHandler.ChatCommand("tryconnect", "Attempts to connect to a multiworld server using the default arguments from the plugin config.", 
                TryConnect, Suggest, () => { return true; });
            var logCompletionStatusCommand = new DebugCommandHandler.ChatCommand("logcompletionstatus", "Prints a detailed report on the multiworld slot completion status in the output log.", 
                LogCompletionData, Suggest, delegate { return Plugin.connection.connected; });
            var logCurrentLevelCommand = new DebugCommandHandler.ChatCommand("logcurrentlevelname", "Prints the name of the current level in the output log.", (isDebugConsole, args) =>
            {
                Plugin.Logger.LogInfo($"Current Level: {RunManager.instance.levelCurrent.name}");
                DebugCommandHandler.instance?.CommandSuccessEffect();
            }, Suggest, IsEnabled);
            var logShopStockCommand = new DebugCommandHandler.ChatCommand("logshopstockreceived", "Prints the total shop stock items received in the output log.", (isDebugConsole, args) =>
            {
                Plugin.Logger.LogInfo($"Shop stock received: {APSave.saveData.shopStockReceived}");
                DebugCommandHandler.instance?.CommandSuccessEffect();
            }, Suggest, IsEnabled);
            var logValuableWeightsCommand = new DebugCommandHandler.ChatCommand("logvaluableweights", "Prints the size categories of the current level's valuables in the output log.", 
                LogValuableWeightInfo, Suggest, SemiFunc.RunIsLevel);
            var logReceivedItemsCommand = new DebugCommandHandler.ChatCommand("logreceiveditems", "Prints the names of all items received from the multiworld in the output log.",
                LogAPItemsReceived, Suggest, delegate { return Plugin.connection.connected; });

            REPOLib.Modules.Commands.RegisterCommand(logStoreItemsCommand);
            REPOLib.Modules.Commands.RegisterCommand(tryConnectCommand);
            REPOLib.Modules.Commands.RegisterCommand(logCompletionStatusCommand);
            REPOLib.Modules.Commands.RegisterCommand(logCurrentLevelCommand);
            REPOLib.Modules.Commands.RegisterCommand(logShopStockCommand);
            REPOLib.Modules.Commands.RegisterCommand(logValuableWeightsCommand);
            REPOLib.Modules.Commands.RegisterCommand(logReceivedItemsCommand);
        }


        private static void LogAllStoreItems(bool isDebugConsole, string[] args)
        {
            foreach (var item in StatsManager.instance.itemDictionary.Keys)
            {
                Plugin.Logger.LogInfo($"{item}");
            }
            DebugCommandHandler.instance?.CommandSuccessEffect();
        }
        private static void TryConnect(bool isDebugConsole, string[] args)
        {
            _ = Plugin.connection.TryConnect(Plugin.apAddress, Int32.Parse(Plugin.apPort), Plugin.apPassword, Plugin.apSlot);
            DebugCommandHandler.instance?.CommandSuccessEffect();
        }
        private static void LogCompletionData(bool isDebugConsole, string[] args)
        {
            if (!Plugin.connection.connected)
            {
                DebugCommandHandler.instance?.CommandFailedEffect();
                return;
            }
            string completionOutput = "-- Completetion Data --";
            completionOutput += $"\nLevel Quota: {APSave.saveData.levelQuota}";

            completionOutput += $"\nPellys Required: {APSave.saveData.pellysRequired}";

            completionOutput += ("\n\nPellys Gathered: ");
            if (APSave.saveData.pellysGathered.Count == 0) completionOutput += "\nnone";
            foreach (string pelly in APSave.saveData.pellysGathered)
            {
                completionOutput += ($"\n- {pelly}");
            }
            completionOutput += "\n\nValuables Gathered: ";
            if (APSave.saveData.valuablesGathered.Count == 0) completionOutput += "\nnone";
            foreach (string valuable in APSave.saveData.valuablesGathered)
            {
                completionOutput += $"\n- {valuable}";
            }
            completionOutput += "\n\nValuables Missing: ";
            foreach (string valuable in LocationNames.all_valuables)
            {
                if (!APSave.saveData.valuablesGathered.Contains(valuable))
                {
                    completionOutput += $"\n- Missing {valuable}";
                }
            }
            completionOutput += "\n\nMonster Souls Gathered: ";
            if (APSave.saveData.monsterSoulsGathered.Count == 0) completionOutput += "\nnone";
            foreach (string soul in APSave.saveData.monsterSoulsGathered)
            {
                completionOutput += $"\n- {soul}";
            }
            completionOutput += "\n\nMonster Souls Missing: ";
            foreach (string soul in LocationNames.all_monster_souls)
            {
                if (!APSave.saveData.monsterSoulsGathered.Contains(soul))
                {
                    completionOutput += $"\n- Missing {soul}";
                }
            }

            Plugin.Logger.LogInfo(completionOutput);
            DebugCommandHandler.instance?.CommandSuccessEffect();
        }
        private static void LogValuableWeightInfo(bool isDebugConsole, string[] args)
        {
            string output = "--- Valuable Weights ---";
            foreach (var levelValuables in LevelGenerator.Instance.Level.ValuablePresets)
            {
                var allValuables = levelValuables.tiny;
                allValuables.AddRange(levelValuables.small);
                allValuables.AddRange(levelValuables.medium);
                allValuables.AddRange(levelValuables.big);
                allValuables.AddRange(levelValuables.wide);
                allValuables.AddRange(levelValuables.tall);
                allValuables.AddRange(levelValuables.veryTall);


                foreach (var val in allValuables)
                {
                    output += $"\n{val.PrefabName} - {LevelGenerator.Instance.Level.name}: {val.Prefab.GetComponent<ValuableObject>().physAttributePreset}".Replace("PhysAttribute", "").Replace("()", "");
                }
            }
            Plugin.Logger.LogInfo(output);
            DebugCommandHandler.instance?.CommandSuccessEffect();
        }
        private static void LogAPItemsReceived(bool isDebugConsole, string[] args)
        {
            if (!Plugin.connection.connected)
            {
                DebugCommandHandler.instance?.CommandFailedEffect();
                return;
            }
            var items = APSave.GetItemsReceived();
            string output = "--- Items Received ---";
            foreach (var item in items)
            {
                output += $"\n\t{ItemData.itemIDToName[ItemData.RemoveBaseId(item.Key)]}";
            }
            Plugin.Logger.LogInfo(output);
            DebugCommandHandler.instance?.CommandSuccessEffect();
        }


        // partial is the latest argument string from args.
        // args is the full list of arguments.
        private static List<string> Suggest(bool isDebugConsole, string partial, string[] args)
        {
            // Return a list of possible arguments based on the current partial and args.
            return [];
        }

        private static bool IsEnabled()
        {
            // Add logic here if you want your command to be conditionally enabled.

            // Disables your command in the main menu.
            if (SemiFunc.IsSplashScreen() || SemiFunc.IsMainMenu())
            {
                return false;
            }

            // Disables your command in the lobby menu.
            if (SemiFunc.RunIsLobbyMenu())
            {
                return false;
            }

            // Disables your command in the tutorial.
            if (SemiFunc.RunIsTutorial())
            {
                return false;
            }

            // Disables your command if you are not the host.
            if (!SemiFunc.IsMasterClientOrSingleplayer())
            {
                return false;
            }

            return true;
        }
    }

}
