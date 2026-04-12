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
            List<Item> allpotentialItemUpgrades = [.. ShopManager.instance.potentialItemUpgrades.Where(item => item != StatsManager.instance.itemDictionary[ItemNames.ap_item])];
            List<Item> allpotentialItemHealthPacks = [.. ShopManager.instance.potentialItemHealthPacks.Where(item => IsItemUnlockedInMultiworld(item))];
            Dictionary<SemiFunc.itemSecretShopType, List<Item>> allPotentialSecretItems = [];

            foreach (List<Item> secretList in ShopManager.instance.potentialSecretItems.Values) allPotentialItems.AddRange(secretList.Where(secretItem => IsItemUnlockedInMultiworld(secretItem)));

            bool oldUpgradeListIsEmpty = allpotentialItemUpgrades.Count() == 0;
            foreach (Item obj in StatsManager.instance.itemDictionary.Values)
            {
                if (obj.itemType != SemiFunc.itemType.cart && obj.itemType != SemiFunc.itemType.pocket_cart && obj.itemType != SemiFunc.itemType.item_upgrade &&
                    obj.itemType != SemiFunc.itemType.power_crystal && obj.itemType != SemiFunc.itemType.healthPack && (IsItemUnlockedInMultiworld(obj) || UnityEngine.Random.Range(0, 100) < 10))
                {
                    if (!allPotentialSecretItems.ContainsKey(SemiFunc.itemSecretShopType.shop_attic))
                        allPotentialSecretItems.Add(SemiFunc.itemSecretShopType.shop_attic, []);
                    allPotentialSecretItems[SemiFunc.itemSecretShopType.shop_attic].Add(obj);
                }
                else if (oldUpgradeListIsEmpty && obj.itemType == SemiFunc.itemType.item_upgrade && obj != StatsManager.instance.itemDictionary[ItemNames.ap_item])
                {
                    allpotentialItemUpgrades.Add(obj);
                }
            }

            if (oldUpgradeListIsEmpty)
            {
                allpotentialItemUpgrades.Shuffle<Item>();
            }
            foreach (IList<Item> list in allPotentialSecretItems.Values)
                list.Shuffle<Item>();

            ShopManager.instance.potentialItems = allPotentialItems;
            ShopManager.instance.potentialItemConsumables = allpotentialItemConsumables;
            ShopManager.instance.potentialItemUpgrades = allpotentialItemUpgrades;
            ShopManager.instance.potentialItemHealthPacks = allpotentialItemHealthPacks;
            ShopManager.instance.potentialSecretItems = allPotentialSecretItems;
        }
    }


    /*
     * If potentialItems and potentialItemConsumables are both empty, nothing else can spawn. This is stupid because the method only gets called if the level currently being generated
     * is the shop. We change that here.
     */
    class ShopPopulateItemVolumesPatch
    {
        private static readonly FieldInfo potentialItemsInfo = AccessTools.Field(typeof(ShopManager), nameof(ShopManager.potentialItems));
        private static readonly FieldInfo potentialItemConsumablesInfo = AccessTools.Field(typeof(ShopManager), nameof(ShopManager.potentialItemConsumables));
        private static readonly FieldInfo itemSpawnTargetAmountInfo = AccessTools.Field(typeof(ShopManager), nameof(ShopManager.itemSpawnTargetAmount));

        [HarmonyPatch(typeof(PunManager), nameof(PunManager.ShopPopulateItemVolumes))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> PreventStupidZeroUpgradesPatch(IEnumerable<CodeInstruction> instructions)
        {
            Plugin.Logger.LogInfo("Patching ShopPopulateItemVolumes with a transpiler to let upgrades spawn...");
            bool found1 = false;
            bool found2 = false;
            int breakIndex = -1;

            // find the load instructions for a sequence of potentialItems, potentialItemConsumables, and the break instruction that all happen before 
            // a >=.If we do, remove the break (leave) instruction.
            var codes = new List<CodeInstruction>(instructions);
            for (var i = 0; i < codes.Count; i++)
            {
                if (codes[i].LoadsField(potentialItemsInfo)) found1 = true;                            // Found first list
                else if (found1 && codes[i].LoadsField(potentialItemConsumablesInfo)) found2 = true;   // Found second list
                else if (found1 && found2 && codes[i].LoadsField(itemSpawnTargetAmountInfo)) break;    // Too far
                else if (found1 && found2 && codes[i].opcode == OpCodes.Leave)                         // Found break. This is the right place
                {
                    breakIndex = i; // mark it for destruction
                    break;          // irony
                }
            }
            if (breakIndex != -1)
            {
                codes[breakIndex].opcode = OpCodes.Nop;
                Plugin.Logger.LogInfo($"Successfully patched ShopPopulateItemVolumes.");
                return codes;
            }
            Plugin.Logger.LogInfo("Failed to patch ShopPopulateItemVolumes. Target instruction not found!");
            return instructions;
        }
    }

    [HarmonyPatch(typeof(PunManager), "SpawnShopItem")]
    class SpawnShopItemPatch
	{
        [HarmonyPrefix]
		static bool ReplaceItemPatch(ref bool __result, ref ItemVolume itemVolume, ref List<Item> itemList, ref int spawnCount, bool isSecret = false)
		{
            // handle upgrade spawning ourself (need to check if the itemVolume is for upgrades) SemiFunc.ItemVolume.upgrade
            if (itemList.Count <= 0 || itemVolume.itemVolume != SemiFunc.itemVolume.upgrade)
			{
                return true;
            }
            for (int i = itemList.Count - 1; i >= 0; i--)
			{
                Item item;
				//Replaces upgrades with AP items
				if ((itemList[i].itemName.Contains("Upgrade") && !itemList[i].name.Contains("Counted")) && Plugin.ShopItemsAvailable.Count > 0)
				{
                    item = StatsManager.instance.itemDictionary[ItemNames.ap_item];
				}
				else
				{
                    item = itemList[i];
                }
                
                if (item.itemVolume == itemVolume.itemVolume)
				{
                    ShopManager.instance.itemRotateHelper.transform.parent = itemVolume.transform;
					ShopManager.instance.itemRotateHelper.transform.localRotation = item.spawnRotationOffset;
					Quaternion rotation = ShopManager.instance.itemRotateHelper.transform.rotation;
					ShopManager.instance.itemRotateHelper.transform.parent = ShopManager.instance.transform;
					if (SemiFunc.IsMultiplayer())
					{
						var inst = PhotonNetwork.InstantiateRoomObject(item.prefab.ResourcePath, itemVolume.transform.position, rotation, 0, null);
						Plugin.LastShopItemChecked++;
						if (item.itemType == SemiFunc.itemType.item_upgrade && item.name == ItemNames.ap_item)
						{
                            Plugin.Logger.LogDebug($"Replacing {itemList[i].itemName} with a random AP item");
                            System.Random rand = new System.Random();
							int randomIndex = rand.Next(Plugin.ShopItemsAvailable.Count);
							int itemID = Plugin.ShopItemsAvailable[randomIndex];
							inst.name += "_Counted_" + itemID;
							Plugin.ShopItemsAvailable.RemoveAt(randomIndex);
						}
					}
					else
					{
						var inst = UnityEngine.Object.Instantiate<GameObject>(item.prefab.Prefab, itemVolume.transform.position, rotation);
						Plugin.LastShopItemChecked++;
						if (item.itemType == SemiFunc.itemType.item_upgrade && item.name == ItemNames.ap_item)
						{
                            Plugin.Logger.LogDebug($"Replacing {itemList[i].itemName} with a random AP item");
                            System.Random rand = new System.Random();
							int randomIndex = rand.Next(Plugin.ShopItemsAvailable.Count);
							int itemID = Plugin.ShopItemsAvailable[randomIndex];
							inst.name += "_Counted_" + itemID;
							Plugin.ShopItemsAvailable.RemoveAt(randomIndex);
							Plugin.Logger.LogDebug($"Spawned AP Item with ID: {itemID}");
						}
					}
					itemList.RemoveAt(i);
					if (!isSecret)
					{
						spawnCount++;
					}
					__result = true;
					return false;
				}
			}
			__result = false;
			return false;
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

    [HarmonyPatch(typeof(ItemAttributes),"Start")]
	class APItemNamePatch
    {
		[HarmonyPostfix]
		static void NamePatch(ref string ___itemName, ItemAttributes __instance)
        {
			if (___itemName.Contains("Archipelago"))
            {
				__instance.gameObject.AddComponent<CustomRPCs>();
				if (SemiFunc.IsMasterClientOrSingleplayer())
				{
					if (RunManager.instance.levelCurrent.name.Contains("Shop"))
					{
						string name = __instance.name;
						if (name.Any(Char.IsDigit))
						{
							name = new string(name.Where(x => char.IsDigit(x)).ToArray());
						}
						___itemName += " " + name;
						SerializableItemInfo itemInfo = APSave.GetScoutedShopItem(LocationData.AddBaseId(Int64.Parse(name)));

                        ___itemName = $"{itemInfo.Player}'s {itemInfo.ItemName}";

						if (GameManager.instance.gameMode == 1)
						{

							FieldInfo field = AccessTools.Field(typeof(ItemUpgrade), "photonView");
							//PhotonView photonView = (PhotonView)field.GetValue(__instance.GetComponent<ItemUpgrade>());	// this is unused
							Plugin.customRPCManager.CallUpdateItemNameRPC(___itemName, __instance.gameObject);
							return;
						}
					}
				}
			}
        }
    }


}
