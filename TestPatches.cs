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
    [HarmonyPatch(typeof(ExtractionPoint), "HaulGoalSet")]
    class TestPatches
    {
        [HarmonyPrefix]
        private static void Prefix(ref int value)
        {
            value = 1;
        }
    }

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

    /*[HarmonyPatch(typeof(PlayerController), "Update")]
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
                Debug.Log("Pelly Req Count");
                Debug.Log(APSave.saveData.pellysRequired.Count);
            }
        }
    }  */ 

}
