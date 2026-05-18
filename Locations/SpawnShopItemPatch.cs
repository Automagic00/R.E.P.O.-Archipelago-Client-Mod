using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Archipelago.MultiClient.Net.Models;
using HarmonyLib;
using Photon.Pun;
using RepoAP.Core;
using UnityEngine;

namespace RepoAP
{
	[HarmonyPatch(typeof(PunManager),"Start")]
	class ItemNumberTracker
    {
		[HarmonyPostfix]
		static void SetNumber()
        {
			Plugin.Logger.LogInfo("Current Level: " + (RunManager.instance.levelCurrent.name));
			if (RunManager.instance.levelCurrent.name.Contains("Menu") || RunManager.instance.levelCurrent.name.Contains("Splash"))
            {
				return;
            }
			Plugin.LastShopItemChecked = 0;
			APSave.UpdateAvailableItems();
        }
    }

    [HarmonyPatch(typeof(ShopManager), "GetAllItemsFromStatsManager")]
    class CreateShopItemListPatch
    {
        static bool IsItemUnlockedInMultiworld(Item itemToCheck)
        {
            if (itemToCheck.itemType == SemiFunc.itemType.item_upgrade)
            {
                Plugin.Logger.LogDebug($"Item '{itemToCheck.itemName}' is an upgrade and always able to spawn");
                return true;
            }
            else if (itemToCheck.itemName.Contains("Health Pack"))
            {
                if (itemToCheck.itemName.Contains("Large") && !APSave.IsItemReceived(ItemData.AddBaseId(ItemData.itemNameToID[ItemNames.progressive_health]), 3)) return false;
                if (itemToCheck.itemName.Contains("Medium") && !APSave.IsItemReceived(ItemData.AddBaseId(ItemData.itemNameToID[ItemNames.progressive_health]), 2)) return false;
                if (itemToCheck.itemName.Contains("Small") && !APSave.IsItemReceived(ItemData.AddBaseId(ItemData.itemNameToID[ItemNames.progressive_health]), 1)) return false;
                Plugin.Logger.LogDebug($"Item '{itemToCheck.itemName}' is unlocked and able to spawn");
                return true;
            }
            if (!ItemData.itemNameToID.TryGetValue(itemToCheck.itemName + " Unlock", out long itemID))     // if an item is not in our item data table, we can assume it's not known by AP and should be left alone
            {
                if (itemToCheck.itemName.Equals("Duct Taped Grenades")) itemID = ItemData.itemNameToID[ItemNames.duct_taped_grenade];  // temporary fix for incorrect name
                else if (itemToCheck.itemName.Equals("POCKET C.A.R.T.")) itemID = ItemData.itemNameToID[ItemNames.pocket_cart];  // temporary fix for incorrect name
                else if (itemToCheck.itemName.Equals("Extraction Tracker")) itemID = ItemData.itemNameToID[ItemNames.extraction_detector];  // temporary fix for incorrect name
                else if (itemToCheck.itemName.Equals("Shockwave Grenade")) itemID = ItemData.itemNameToID[ItemNames.shock_grenade];  // temporary fix for incorrect name
                else if (itemToCheck.itemName.Equals("Valuable Tracker")) itemID = ItemData.itemNameToID[ItemNames.valuable_detector];  // temporary fix for incorrect name
                else
                {
                    Plugin.Logger.LogDebug($"Item '{itemToCheck.itemName}' not found in AP data table");
                    return true;
                }
            }
            if (APSave.IsItemReceived(ItemData.AddBaseId(itemID)))
            {
                Plugin.Logger.LogDebug($"Item '{itemToCheck.itemName}' is unlocked and able to spawn");
                return true;
            }
            Plugin.Logger.LogDebug($"Item '{itemToCheck.itemName}' is not unlocked and will not spawn");
            return false;
        }

        /*
         * Goes through every shop item list and removes items that haven't been unlocked yet,
         * then moves all secret room items to the regular shelf (for logic reasons) and let the secret shop have illegal items.
         */
        [HarmonyPostfix]
        static void RemoveLockedItemsFromShopPool(ShopManager __instance)
        {
            if (SemiFunc.IsNotMasterClient())
                return;
            List<Item> allPotentialItems = [.. ShopManager.instance.potentialItems.Where(item => IsItemUnlockedInMultiworld(item))];    
            List<Item> allpotentialItemConsumables = [.. ShopManager.instance.potentialItemConsumables.Where(item => IsItemUnlockedInMultiworld(item))];
            List<Item> allpotentialItemUpgrades = [];
            List<Item> allpotentialItemHealthPacks = [.. ShopManager.instance.potentialItemHealthPacks.Where(item => IsItemUnlockedInMultiworld(item))];
            Dictionary<SemiFunc.itemSecretShopType, List<Item>> allPotentialSecretItems = [];

            foreach (var secretPair in ShopManager.instance.potentialSecretItems)
                allPotentialSecretItems[secretPair.Key] = [.. secretPair.Value.Where(item => IsItemUnlockedInMultiworld(item))];

            if (ShopManager.instance.potentialItemUpgrades.Count() == 0)
            {
                foreach (Item obj in StatsManager.instance.itemDictionary.Values)
                {
                    if (obj.itemType == SemiFunc.itemType.item_upgrade && obj != StatsManager.instance.itemDictionary[ItemNames.ap_item])
                    {
                        allpotentialItemUpgrades.Add(obj);
                    }
                }
                allpotentialItemUpgrades.Shuffle<Item>();
            }
            else allpotentialItemUpgrades = ShopManager.instance.potentialItemUpgrades;

            int shopItemsToReplace = Math.Min(Plugin.ShopItemsAvailable.Count, allpotentialItemUpgrades.Count);
            for (int i = shopItemsToReplace - 1; i >= 0; i--)
            {
                allpotentialItemUpgrades.RemoveAt(i);
                allpotentialItemUpgrades.Add(StatsManager.instance.itemDictionary[ItemNames.ap_item]);
            }

            ShopManager.instance.potentialItems = allPotentialItems;
            ShopManager.instance.potentialItemConsumables = allpotentialItemConsumables;
            ShopManager.instance.potentialItemUpgrades = allpotentialItemUpgrades;
            ShopManager.instance.potentialItemHealthPacks = allpotentialItemHealthPacks;
            ShopManager.instance.potentialSecretItems = allPotentialSecretItems;
        }
    }

    /*
     * Refreshes available shop items once per visit
     */
    [HarmonyPatch(typeof(PunManager), nameof(PunManager.ShopPopulateItemVolumes))]
    class ApStoreItemsPatch
	{
        [HarmonyPrefix]
        static void RefreshAvailableAPShopItems()	
        {
            Plugin.Logger.LogInfo("Refreshing Available AP Shop Items");
            APSave.UpdateAvailableItems();
        }
    }
    class ApUpgradeStandItemsPatch
    {
        static int apShopItemsAvailable = 0;

        [HarmonyPatch(typeof(UpgradeStand), "SpawnNewUpgrades")]
        [HarmonyPrefix]
        static void RefreshAvailableAPShopItems()
        {
            Plugin.Logger.LogInfo("Refreshing Available AP Shop Items");
            APSave.UpdateAvailableItems();
            apShopItemsAvailable = Plugin.ShopItemsAvailable.Count;
        }
        [HarmonyPatch(typeof(UpgradeStand), "GetWeightedUpgradeExcluding")]
        [HarmonyPostfix]
        static void NormalizeItemWeightForApItems(ref Item __result)
        {
            Plugin.Logger.LogInfo($"Ap Shop items available: {apShopItemsAvailable}");
            if (apShopItemsAvailable > 0 && UnityEngine.Random.Range(0, 2) == 1)
            {
                __result = StatsManager.instance.itemDictionary[ItemNames.ap_item];
                apShopItemsAvailable--;
            }
            else if (apShopItemsAvailable <= 0 && __result == StatsManager.instance.itemDictionary[ItemNames.ap_item])
            {
                List<Item> allUpgrades = [.. StatsManager.instance.itemDictionary.Values.Where(value =>
                        value.itemType == SemiFunc.itemType.item_upgrade && value != StatsManager.instance.itemDictionary[ItemNames.ap_item])];
                __result = allUpgrades[UnityEngine.Random.Range(0, allUpgrades.Count)];
            }
        }
    }

    class APItemNamePatch
    {
        [HarmonyPatch(typeof(ItemAttributes), "Start")]
        [HarmonyPostfix]
        static void NamePatch(ref string ___itemName, ItemAttributes __instance)
        {
            if (___itemName.Contains("Archipelago"))
            {
                if (SemiFunc.IsMasterClientOrSingleplayer() && SemiFunc.RunIsShop())
                {
                    if (Plugin.ShopItemsAvailable.Count <= 0) return;
                    Plugin.Logger.LogDebug($"Assigning {___itemName} a random AP item");
                    System.Random rand = new System.Random();
                    int randomIndex = rand.Next(Plugin.ShopItemsAvailable.Count);
                    int itemID = Plugin.ShopItemsAvailable[randomIndex];
                    __instance.gameObject.name += "_Counted_" + itemID;
                    Plugin.ShopItemsAvailable.RemoveAt(randomIndex);
                    string name = __instance.gameObject.name;

                    if (!name.Any(Char.IsDigit)) return;
                    
                    name = new string(name.Where(x => char.IsDigit(x)).ToArray());

                    SerializableItemInfo itemInfo = APSave.GetScoutedShopItem(LocationData.AddBaseId(Int64.Parse(name)));
                    string newItemName = $"{itemInfo.Player}'s {itemInfo.ItemName}";

                    if (GameManager.instance.gameMode == 1 && __instance.gameObject.GetComponent<ItemRPCs>() != null)
                    {
                        __instance.gameObject.GetComponent<ItemRPCs>().CallUpdateItemNameRPC(newItemName, __instance.gameObject);
                    }
                    else
                    {
                        __instance.itemName = newItemName;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(ItemAttributes), "GetItemNameLocalized")]
        [HarmonyPostfix]
        static void ReplaceLocalizedNameOfAPItems(ref string __result, ref string ___itemName, ItemAttributes __instance)
        {
            if (__instance.item.name.Equals(ItemNames.ap_item)) __result = ___itemName;
        }

    }


}
