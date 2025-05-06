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
			if (RunManager.instance.levelCurrent.name.Contains("Menu"))
            {
				return;
            }
			Plugin.LastShopItemChecked = 0;
			Plugin.ShopItemsBought = APSave.GetShopLocationsChecked();
        }
    }


    [HarmonyPatch(typeof(PunManager), "SpawnShopItem")]
    class SpawnShopItemPatch
    {
        [HarmonyPrefix]
        static bool ReplaceItemPatch(ref bool __result, ref ItemVolume itemVolume, ref List<Item> itemList, ref int spawnCount, bool isSecret = false)
        {
			
			FieldInfo field = AccessTools.Field(typeof(ItemAttributes), "itemName");

			for (int i = itemList.Count - 1; i >= 0; i--)
			{
				Item item;
				if ((itemList[i].itemName.Contains("Upgrade") && !itemList[i].name.Contains("Counted")) && Plugin.LastShopItemChecked <= APSave.saveData.upgradeLocations)
				{
					Debug.Log("Replacing  " + itemList[i].itemName);
					item = StatsManager.instance.itemDictionary[ItemNames.apItem];
				}
				else
                {
					item = itemList[i];
					return true;
                }		

				if (item.itemVolume == itemVolume.itemVolume)
				{
					ShopManager.instance.itemRotateHelper.transform.parent = itemVolume.transform;
					ShopManager.instance.itemRotateHelper.transform.localRotation = item.spawnRotationOffset;
					Quaternion rotation = ShopManager.instance.itemRotateHelper.transform.rotation;
					ShopManager.instance.itemRotateHelper.transform.parent = ShopManager.instance.transform;
					string text = "Items/" + item.prefab.name;
					if (SemiFunc.IsMultiplayer())
					{
						var inst = PhotonNetwork.InstantiateRoomObject(text, itemVolume.transform.position, rotation, 0, null);
						Plugin.LastShopItemChecked++;
						while (Plugin.ShopItemsBought.Contains(Plugin.LastShopItemChecked))
						{
							Plugin.LastShopItemChecked++;
						}
						inst.name += "_Counted_" + Plugin.LastShopItemChecked;
					}
					else
					{
                        var inst = UnityEngine.Object.Instantiate<GameObject>(item.prefab, itemVolume.transform.position, rotation);
						Plugin.LastShopItemChecked++;
						while (Plugin.ShopItemsBought.Contains(Plugin.LastShopItemChecked))
						{
							Plugin.LastShopItemChecked++;
						}
						inst.name += "_Counted_" + Plugin.LastShopItemChecked;
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
						Debug.Log(LocationData.AddBaseId(Int64.Parse(name)));
						ItemInfo itemInfo = APSave.GetScoutedShopItem(LocationData.AddBaseId(Int64.Parse(name)));
						Debug.Log("2");
						___itemName = $"{itemInfo.Player}'s {itemInfo.ItemName}";
						Debug.Log("3");
						if (GameManager.instance.gameMode == 1)
						{
							Debug.Log("4");
							FieldInfo field = AccessTools.Field(typeof(ItemUpgrade), "photonView");
							Debug.Log("5");
							PhotonView photonView = (PhotonView)field.GetValue(__instance.GetComponent<ItemUpgrade>());
							Debug.Log("6");
							CustomRPCs.CallUpdateItemNameRPC(___itemName, __instance.gameObject);
							Debug.Log("7");
							
							Debug.Log("8");
							return;
						}
					}
				}
			}
        }
    }


}
