using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;
using Photon.Pun;
using System.Reflection;
using System;
using Archipelago.MultiClient.Net.Models;

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
                return true;
            }
            else if (itemToCheck.itemName.Contains("Health Pack"))
            {
                if (itemToCheck.itemName.Contains("Large") && !APSave.IsItemReceived(ItemData.AddBaseId(ItemData.itemNameToID[ItemNames.progressive_health]), 3)) return false;
                if (itemToCheck.itemName.Contains("Medium") && !APSave.IsItemReceived(ItemData.AddBaseId(ItemData.itemNameToID[ItemNames.progressive_health]), 2)) return false;
                if (itemToCheck.itemName.Contains("Small") && !APSave.IsItemReceived(ItemData.AddBaseId(ItemData.itemNameToID[ItemNames.progressive_health]), 1)) return false;
                Plugin.Logger.LogInfo($"Item '{itemToCheck.itemName}' is unlocked and able to spawn");
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
                    Plugin.Logger.LogInfo($"Item '{itemToCheck.itemName}' not found in AP data table");
                    return true;
                }
            }
            if (APSave.IsItemReceived(ItemData.AddBaseId(itemID)))
            {
                Plugin.Logger.LogInfo($"Item '{itemToCheck.itemName}' is unlocked and able to spawn");
                return true;
            }
            Plugin.Logger.LogInfo($"Item '{itemToCheck.itemName}' is not unlocked and will not spawn");
            return false;
        }

        // search for StatsManager.instance.itemDictionary.Values and replace it with our own edited list
        [HarmonyPrefix]
		static bool GetAllAPItemsFromStatsManager()
		{
            if (SemiFunc.IsNotMasterClient())
                return true;
            ShopManager.instance.potentialItems.Clear();
            ShopManager.instance.potentialItemConsumables.Clear();
            ShopManager.instance.potentialItemUpgrades.Clear();
            ShopManager.instance.potentialItemHealthPacks.Clear();
            ShopManager.instance.potentialSecretItems.Clear();
            ShopManager.instance.itemConsumablesAmount = UnityEngine.Random.Range(4, 6);
            foreach (Item obj in StatsManager.instance.itemDictionary.Values)
            {
                int itemsPurchased = SemiFunc.StatGetItemsPurchased(obj.name);
                float max = obj.value.valueMax / 1000f * (float)AccessTools.Field(typeof(ShopManager), "itemValueMultiplier").GetValue(ShopManager.instance);//ShopManager.instance.itemValueMultiplier;
                if (obj.itemType == SemiFunc.itemType.item_upgrade)
                    max = ShopManager.instance.UpgradeValueGet(max, obj);
                else if (obj.itemType == SemiFunc.itemType.healthPack)
                    max = ShopManager.instance.HealthPackValueGet(max);
                else if (obj.itemType == SemiFunc.itemType.power_crystal)
                    max = ShopManager.instance.CrystalValueGet(max);
                float num = Mathf.Clamp(max, 1f, max);
                bool flag1 = obj.itemType == SemiFunc.itemType.power_crystal;
                bool flag2 = obj.itemType == SemiFunc.itemType.item_upgrade;
                bool flag3 = obj.itemType == SemiFunc.itemType.healthPack;
                int maxAmountInShop = obj.maxAmountInShop;
                if (itemsPurchased < maxAmountInShop && (!obj.maxPurchase || StatsManager.instance.GetItemsUpgradesPurchasedTotal(obj.name) < obj.maxPurchaseAmount) && ((double)num <= (double)ShopManager.instance.totalCurrency || UnityEngine.Random.Range(0, 100) < 25) && IsItemUnlockedInMultiworld(obj))
                {
                    for (int index = 0; index < maxAmountInShop - itemsPurchased; ++index)
                    {
                        if (flag2)
                            ShopManager.instance.potentialItemUpgrades.Add(obj);
                        else if (flag3)
                            ShopManager.instance.potentialItemHealthPacks.Add(obj);
                        else if (flag1)
                            ShopManager.instance.potentialItemConsumables.Add(obj);
                        else //if (obj.itemSecretShopType == SemiFunc.itemSecretShopType.none)    // for fun
                             //{
                            ShopManager.instance.potentialItems.Add(obj);
                        /*}
                        else
                        {
                            if (!ShopManager.instance.potentialSecretItems.ContainsKey(obj.itemSecretShopType))
                                ShopManager.instance.potentialSecretItems.Add(obj.itemSecretShopType, new List<Item>());
                            ShopManager.instance.potentialSecretItems[obj.itemSecretShopType].Add(obj);
                        }*/
                    }
                }
                // this is just a test for fun
                if (obj.itemType != SemiFunc.itemType.item_upgrade && obj.itemType != SemiFunc.itemType.cart && obj.itemType != SemiFunc.itemType.pocket_cart)
                {
                    if (!ShopManager.instance.potentialSecretItems.ContainsKey(SemiFunc.itemSecretShopType.shop_attic))
                        ShopManager.instance.potentialSecretItems.Add(SemiFunc.itemSecretShopType.shop_attic, new List<Item>());
                    ShopManager.instance.potentialSecretItems[SemiFunc.itemSecretShopType.shop_attic].Add(obj);
                }
            }
            ShopManager.instance.potentialItems.Shuffle<Item>();
            ShopManager.instance.potentialItemConsumables.Shuffle<Item>();
            ShopManager.instance.potentialItemUpgrades.Shuffle<Item>();
            ShopManager.instance.potentialItemHealthPacks.Shuffle<Item>();
            foreach (IList<Item> list in ShopManager.instance.potentialSecretItems.Values)
                list.Shuffle<Item>();
            return false;
        }
    }

    // it would be much easier to do this with a transpiler patch and less costly in terms of performance
    [HarmonyPatch(typeof(PunManager), "SpawnShopItem")]
    class SpawnShopItemPatch
	{
        /*[HarmonyTranspiler]
		static IEnumerable<CodeInstruction> ModifySpawnableShopItems(IEnumerable<CodeInstruction> instructions)
		{
			var codeMatcher = new CodeMatcher(instructions);
			codeMatcher.MatchStartForward(
				CodeMatch.Calls(() => default(PunManager).SpawnShopItem(default, ShopManager.instance.potentialItems, default))
				)
				.ThrowIfInvalid("Could not find the target method call to PunManager.SpawnShopItem")
				.RemoveInstruction()
				.InsertAndAdvance(
				CodeInstruction.Call
				)
        }*/


        [HarmonyPrefix]
		static bool ReplaceItemPatch(ref bool __result, ref ItemVolume itemVolume, ref List<Item> itemList, ref int spawnCount, bool isSecret = false)
		{
            //APSave.UpdateAvailableItems();
            //Plugin.Logger.LogInfo($"AP Upgrades Available {Plugin.ShopItemsAvailable.Count}");
            if (itemList.Count <= 0 || itemList[0].itemType != SemiFunc.itemType.item_upgrade)
			{
				return true;
            }
            for (int i = itemList.Count - 1; i >= 0; i--)
			{
                //Debug.Log($"{i}/{itemList.Count - 1}");
                //Debug.Log($"Checking {itemList[i].name}");
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
                // handle upgrade spawning ourself (need to check if the itemVolume is for upgrades) SemiFunc.ItemVolume.upgrade
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
                            Plugin.Logger.LogInfo($"Replacing {itemList[i].itemName} with a random AP item");
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
                            Plugin.Logger.LogInfo($"Replacing {itemList[i].itemName} with a random AP item");
                            System.Random rand = new System.Random();
							int randomIndex = rand.Next(Plugin.ShopItemsAvailable.Count);
							int itemID = Plugin.ShopItemsAvailable[randomIndex];
							inst.name += "_Counted_" + itemID;
							Plugin.ShopItemsAvailable.RemoveAt(randomIndex);
							Plugin.Logger.LogDebug($"Spawned AP Item with ID: {itemID}");
							//inst.name += "_Counted_" + Plugin.LastShopItemChecked;
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

    [HarmonyPatch(typeof(PunManager), nameof(PunManager.ShopPopulateItemVolumes))]
	class ApStoreItemsPatch
	{
        [HarmonyPrefix]
        static void RefreshAvailableAPShopItems()	// refreshes available shop items once per visit
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
						//Debug.Log(LocationData.AddBaseId(Int64.Parse(name)));
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
