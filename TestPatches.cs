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
            //var logEnemiesCommand = new DebugCommandHandler.ChatCommand("logenemies", "Prints the names and mass of all enemies in the output log.", LogEnemyMass, Suggest, SemiFunc.RunIsLevel);

            REPOLib.Modules.Commands.RegisterCommand(logStoreItemsCommand);
            REPOLib.Modules.Commands.RegisterCommand(tryConnectCommand);
            REPOLib.Modules.Commands.RegisterCommand(logCompletionStatusCommand);
            REPOLib.Modules.Commands.RegisterCommand(logCurrentLevelCommand);
            REPOLib.Modules.Commands.RegisterCommand(logShopStockCommand);
            REPOLib.Modules.Commands.RegisterCommand(logValuableWeightsCommand);
            REPOLib.Modules.Commands.RegisterCommand(logReceivedItemsCommand);
            //REPOLib.Modules.Commands.RegisterCommand(logEnemiesCommand);
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

        private static void LogEnemyMass(bool isDebugConsole, string[] args)
        {
            // need a way to get the enemy's default strength threshold and the threshold when stunned
            string output = "--- Enemy Weights ---";
            foreach(var enemy in REPOLib.Modules.Enemies.AllEnemies)
            {
                if (enemy.spawnObjects.Count == 0 || enemy.spawnObjects.Count > 1) continue;
                EnemyParent parent = enemy.spawnObjects[0].Prefab.GetComponent<EnemyParent>();
                if (parent != null)
                {
                    output += $"\n{parent.enemyName}: ";
                    Enemy realEnemy = parent.GetComponentInChildren<Enemy>();
                    if (realEnemy != null)
                    {
                        Plugin.Logger.LogInfo("got Enemy");
                        EnemyRigidbody body = realEnemy.Rigidbody;
                        if (body != null)
                        {
                            Plugin.Logger.LogInfo("got EnemyRigidBody");
                            PhysGrabObject grabObject = body.GetComponentInChildren<PhysGrabObject>();
                            if (grabObject != null)
                            {
                                Plugin.Logger.LogInfo(grabObject.rb.mass);
                                Plugin.Logger.LogInfo("got PhysGrabObject");
                                float mass = grabObject.massOriginal;
                                Plugin.Logger.LogInfo($"Mass: {mass}"); // looking for stunMassOverrideMultiplier as well
                            }
                        }
                    }
                }

                // call GrabStrengthGet on EnemyParent.Enemy.Rigidbody.PhysGrabObject


                // what that method looks like:
                /*double mass = 8.0;
                int strength = 1;
                int dividerLimit = 30;

                float num1 = (double)mass < 2.0 ? 7f : 20f;
                float b1 = (float)(1.0 + (double)num1 * 0.5);
                for (int index = 0; index < 3; ++index)
                {
                    float num2 = (float)(1.0 / (1.0 + (double)b1));
                    float num3 = (float)((double)b1 - 1.0 + (double)num2 * (double)num2 - (double)num1 * 0.5);
                    b1 -= num3 / (float)(1.0 - 2.0 * (double)num2 * (double)num2 * (double)num2);
                }
                float a1 = Mathf.Min(strength, b1);
                float t1 = Mathf.Min((a1 - 1f) / num1, 0.9f);
                double a2 = (double)Mathf.Lerp(a1, a1 / (1f + a1), t1);
                float t2 = Mathf.Min((strength - 1f) / num1, 0.9f);
                double b2 = (double)Mathf.Lerp(strength, strength / (1f + Mathf.Min(strength, dividerLimit)), t2);
                Log(Mathf.Max((float)a2, (float)b2));*/



                //"{parent.GetComponentInChildren<Enemy>()?.Rigidbody.physGrabObject.massOriginal}";    // the Enemy field is null in the prefabs
            }
            Plugin.Logger.LogInfo("Output");
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
