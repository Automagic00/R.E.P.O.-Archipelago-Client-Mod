using System;
using System.Collections.Generic;
using System.Reflection;
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
                
                StatsManager.instance.itemsPurchased[ItemNames.upgradeStrength] = 15;
                //StatsManager.instance.
            }

            if (Input.GetKeyDown(KeyCode.F5))
            {
                Debug.Log("Try Connect");
                Plugin.connection.TryConnect(Plugin.apAdress, Int32.Parse(Plugin.apPort), Plugin.apPassword, Plugin.apSlot);
            }
            if (Input.GetKeyDown(KeyCode.F6))
            {
                string completionOutput = "-- Completetion Data --";
                completionOutput += $"\nLevel Quota: {APSave.saveData.levelQuota}";

                completionOutput += $"\nPellys Required: ";

                foreach (var pelly in APSave.saveData.pellysRequired)
                {
                    completionOutput += ($"\n- {pelly.ToString()}");
                }
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
                foreach(var levelValuables in LevelGenerator.Instance.Level.ValuablePresets)
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
                        output += $"\n{val.name} - {LevelGenerator.Instance.Level.name}: {val.GetComponent<ValuableObject>().physAttributePreset}".Replace("PhysAttribute","").Replace("()","");
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
        }
    } 

}
