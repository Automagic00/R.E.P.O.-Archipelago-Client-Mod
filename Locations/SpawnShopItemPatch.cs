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
			APSave.UpdateAvailableItems();
        }
    }


    [HarmonyPatch(typeof(PunManager), "SpawnShopItem")]
    class SpawnShopItemPatch
    {
        [HarmonyPrefix]
        static bool ReplaceItemPatch(ref bool __result, ref ItemVolume itemVolume, ref List<Item> itemList, ref int spawnCount, bool isSecret = false)
        {
			APSave.UpdateAvailableItems();
			FieldInfo field = AccessTools.Field(typeof(ItemAttributes), "itemName");
			//Debug.Log($"AP Upgrades Available {Plugin.ShopItemsAvailable.Count}");
			for (int i = itemList.Count - 1; i >= 0; i--)
			{
				//Debug.Log($"{i}/{itemList.Count - 1}");
				//Debug.Log($"Checking {itemList[i].name}");
				Item item;
				//Replaces upgrades with AP items
				if ((itemList[i].itemName.Contains("Upgrade") && !itemList[i].name.Contains("Counted")) && /*Plugin.LastShopItemChecked <= APSave.saveData.upgradeLocations &&*/ Plugin.ShopItemsAvailable.Count > 0)
				{
					Debug.Log("Replacing  " + itemList[i].itemName);
					item = StatsManager.instance.itemDictionary[ItemNames.apItem];
				}
				else
                {
					//Debug.Log($"Item Spawning {itemList[i].name}");
					item = itemList[i];
					//Debug.Log("item set");
					//itemList.RemoveAt(i);
					//return true;
                }		

				if (item.itemVolume == itemVolume.itemVolume)
				{
					//Remove shop items if not unlocked
					if (itemList[i].name.Contains("Item Cart Cannon") && !APSave.IsItemReceived(ItemData.AddBaseId(ItemData.itemNameToID[ItemNames.cart_cannon])))
					{
						Debug.Log(ItemNames.cart_cannon + " Not Unlocked");
						itemList.RemoveAt(i);
						continue;
					}
					else if (itemList[i].name.Contains("Item Cart Laser") && !APSave.IsItemReceived(ItemData.AddBaseId(ItemData.itemNameToID[ItemNames.cart_laser])))
					{
						Debug.Log(ItemNames.cart_laser + " Not Unlocked");
						itemList.RemoveAt(i);
						continue;
					}
					else if (itemList[i].name.Contains("Item Cart Medium") && !APSave.IsItemReceived(ItemData.AddBaseId(ItemData.itemNameToID[ItemNames.cart])))
					{
						Debug.Log(ItemNames.cart + " Not Unlocked");
						itemList.RemoveAt(i);
						continue;
					}
					else if (itemList[i].name.Contains("Item Cart Small") && !APSave.IsItemReceived(ItemData.AddBaseId(ItemData.itemNameToID[ItemNames.pocket_cart])))
					{
						Debug.Log(ItemNames.pocket_cart + " Not Unlocked");
						itemList.RemoveAt(i);
						continue;
					}
					else if (itemList[i].name.Contains("Item Drone Battery") && !APSave.IsItemReceived(ItemData.AddBaseId(ItemData.itemNameToID[ItemNames.recharge_drone])))
					{
						Debug.Log(ItemNames.recharge_drone + " Not Unlocked");
						itemList.RemoveAt(i);
						continue;
					}
					else if (itemList[i].name.Contains("Item Drone Feather") && !APSave.IsItemReceived(ItemData.AddBaseId(ItemData.itemNameToID[ItemNames.feather_drone])))
					{
						Debug.Log(ItemNames.feather_drone + " Not Unlocked");
						itemList.RemoveAt(i);
						continue;
					}
					else if (itemList[i].name.Contains("Item Drone Indestructible") && !APSave.IsItemReceived(ItemData.AddBaseId(ItemData.itemNameToID[ItemNames.indestructible_drone])))
					{
						Debug.Log(ItemNames.indestructible_drone + " Not Unlocked");
						itemList.RemoveAt(i);
						continue;
					}
					else if (itemList[i].name.Contains("Item Drone Torque") && !APSave.IsItemReceived(ItemData.AddBaseId(ItemData.itemNameToID[ItemNames.roll_drone])))
					{
						Debug.Log(ItemNames.roll_drone + " Not Unlocked");
						itemList.RemoveAt(i);
						continue;
					}
					else if (itemList[i].name.Contains("Item Drone Zero Gravity") && !APSave.IsItemReceived(ItemData.AddBaseId(ItemData.itemNameToID[ItemNames.zero_grav_drone])))
					{
						Debug.Log(ItemNames.zero_grav_drone + " Not Unlocked");
						itemList.RemoveAt(i);
						continue;
					}
					else if (itemList[i].name.Contains("Item Duck Bucket") && !APSave.IsItemReceived(ItemData.AddBaseId(ItemData.itemNameToID[ItemNames.duck_bucket])))
					{
						Debug.Log(ItemNames.duck_bucket + " Not Unlocked");
						itemList.RemoveAt(i);
						continue;
					}
					else if (itemList[i].name.Contains("Item Extraction Tracker") && !APSave.IsItemReceived(ItemData.AddBaseId(ItemData.itemNameToID[ItemNames.extraction_detector])))
					{
						Debug.Log(ItemNames.extraction_detector + " Not Unlocked");
						itemList.RemoveAt(i);
						continue;
					}
					else if (itemList[i].name.Contains("Item Grenade Duct Taped") && !APSave.IsItemReceived(ItemData.AddBaseId(ItemData.itemNameToID[ItemNames.duct_taped_grenade])))
					{
						Debug.Log(ItemNames.duct_taped_grenade + " Not Unlocked");
						itemList.RemoveAt(i);
						continue;
					}
					else if (itemList[i].name.Contains("Item Grenade Explosive") && !APSave.IsItemReceived(ItemData.AddBaseId(ItemData.itemNameToID[ItemNames.grenade])))
					{
						Debug.Log(ItemNames.grenade + " Not Unlocked");
						itemList.RemoveAt(i);
						continue;
					}
					else if (itemList[i].name.Contains("Item Grenade Human") && !APSave.IsItemReceived(ItemData.AddBaseId(ItemData.itemNameToID[ItemNames.human_grenade])))
					{
						Debug.Log(ItemNames.human_grenade + " Not Unlocked");
						itemList.RemoveAt(i);
						continue;
					}
					else if (itemList[i].name.Contains("Item Grenade Shockwave") && !APSave.IsItemReceived(ItemData.AddBaseId(ItemData.itemNameToID[ItemNames.shock_grenade])))
					{
						Debug.Log(ItemNames.shock_grenade + " Not Unlocked");
						itemList.RemoveAt(i);
						continue;
					}
					else if (itemList[i].name.Contains("Item Grenade Stun") && !APSave.IsItemReceived(ItemData.AddBaseId(ItemData.itemNameToID[ItemNames.stun_grenade])))
					{
						Debug.Log(ItemNames.stun_grenade + " Not Unlocked");
						itemList.RemoveAt(i);
						continue;
					}
					else if (itemList[i].name.Contains("Item Gun Handgun") && !APSave.IsItemReceived(ItemData.AddBaseId(ItemData.itemNameToID[ItemNames.gun])))
					{
						Debug.Log(ItemNames.gun + " Not Unlocked");
						itemList.RemoveAt(i);
						continue;
					}
					else if (itemList[i].name.Contains("Item Gun Laser") && !APSave.IsItemReceived(ItemData.AddBaseId(ItemData.itemNameToID[ItemNames.photon_blaster])))
					{
						Debug.Log(ItemNames.photon_blaster + " Not Unlocked");
						itemList.RemoveAt(i);
						continue;
					}
					else if (itemList[i].name.Contains("Item Gun Shockwave") && !APSave.IsItemReceived(ItemData.AddBaseId(ItemData.itemNameToID[ItemNames.pulse_pistol])))
					{
						Debug.Log(ItemNames.pulse_pistol + " Not Unlocked");
						itemList.RemoveAt(i);
						continue;
					}
					else if (itemList[i].name.Contains("Item Gun Shotgun") && !APSave.IsItemReceived(ItemData.AddBaseId(ItemData.itemNameToID[ItemNames.shotgun])))
					{
						Debug.Log(ItemNames.shotgun + " Not Unlocked");
						itemList.RemoveAt(i);
						continue;
					}
					else if (itemList[i].name.Contains("Item Gun Stun") && !APSave.IsItemReceived(ItemData.AddBaseId(ItemData.itemNameToID[ItemNames.boltzap])))
					{
						Debug.Log(ItemNames.boltzap + " Not Unlocked");
						itemList.RemoveAt(i);
						continue;
					}
					else if (itemList[i].name.Contains("Item Gun Tranq") && !APSave.IsItemReceived(ItemData.AddBaseId(ItemData.itemNameToID[ItemNames.tranq_gun])))
					{
						Debug.Log(ItemNames.tranq_gun + " Not Unlocked");
						itemList.RemoveAt(i);
						continue;
					}
					else if (itemList[i].name.Contains("Item Health Pack Large") && !APSave.IsItemReceived(ItemData.AddBaseId(ItemData.itemNameToID[ItemNames.progressive_health]), 3))
					{
						Debug.Log("3 " + ItemNames.progressive_health + " Not Unlocked");
						itemList.RemoveAt(i);
						continue;
					}
					else if (itemList[i].name.Contains("Item Health Pack Medium") && !APSave.IsItemReceived(ItemData.AddBaseId(ItemData.itemNameToID[ItemNames.progressive_health]), 2))
					{
						Debug.Log("2 " + ItemNames.progressive_health + " Not Unlocked");
						itemList.RemoveAt(i);
						continue;
					}
					else if (itemList[i].name.Contains("Item Health Pack Small") && !APSave.IsItemReceived(ItemData.AddBaseId(ItemData.itemNameToID[ItemNames.progressive_health]), 1))
					{
						Debug.Log("1 " + ItemNames.progressive_health + " Not Unlocked");
						itemList.RemoveAt(i);
						continue;
					}
					else if (itemList[i].name.Contains("Item Melee Baseball Bat") && !APSave.IsItemReceived(ItemData.AddBaseId(ItemData.itemNameToID[ItemNames.baseball_bat])))
					{
						Debug.Log(ItemNames.baseball_bat + " Not Unlocked");
						itemList.RemoveAt(i);
						continue;
					}
					else if (itemList[i].name.Contains("Item Melee Frying Pan") && !APSave.IsItemReceived(ItemData.AddBaseId(ItemData.itemNameToID[ItemNames.frying_pan])))
					{
						Debug.Log(ItemNames.frying_pan + " Not Unlocked");
						itemList.RemoveAt(i);
						continue;
					}
					else if (itemList[i].name.Contains("Item Melee Inflatable Hammer") && !APSave.IsItemReceived(ItemData.AddBaseId(ItemData.itemNameToID[ItemNames.inflatable_hammer])))
					{
						Debug.Log(ItemNames.inflatable_hammer + " Not Unlocked");
						itemList.RemoveAt(i);
						continue;
					}
					else if (itemList[i].name.Contains("Item Melee Sledge Hammer") && !APSave.IsItemReceived(ItemData.AddBaseId(ItemData.itemNameToID[ItemNames.sledge_hammer])))
					{
						Debug.Log(ItemNames.sledge_hammer + " Not Unlocked");
						itemList.RemoveAt(i);
						continue;
					}
					else if (itemList[i].name.Contains("Item Melee Stun Baton") && !APSave.IsItemReceived(ItemData.AddBaseId(ItemData.itemNameToID[ItemNames.prodzap])))
					{
						Debug.Log(ItemNames.prodzap + " Not Unlocked");
						itemList.RemoveAt(i);
						continue;
					}
					else if (itemList[i].name.Contains("Item Melee Sword") && !APSave.IsItemReceived(ItemData.AddBaseId(ItemData.itemNameToID[ItemNames.sword])))
					{
						Debug.Log(ItemNames.sword + " Not Unlocked");
						itemList.RemoveAt(i);
						continue;
					}
					else if (itemList[i].name.Contains("Item Mine Explosive") && !APSave.IsItemReceived(ItemData.AddBaseId(ItemData.itemNameToID[ItemNames.explosive_mine])))
					{
						Debug.Log(ItemNames.explosive_mine + " Not Unlocked");
						itemList.RemoveAt(i);
						continue;
					}
					else if (itemList[i].name.Contains("Item Mine Shockwave") && !APSave.IsItemReceived(ItemData.AddBaseId(ItemData.itemNameToID[ItemNames.shockwave_mine])))
					{
						Debug.Log(ItemNames.shockwave_mine + " Not Unlocked");
						itemList.RemoveAt(i);
						continue;
					}
					else if (itemList[i].name.Contains("Item Mine Stun") && !APSave.IsItemReceived(ItemData.AddBaseId(ItemData.itemNameToID[ItemNames.stun_mine])))
					{
						Debug.Log(ItemNames.stun_mine + " Not Unlocked");
						itemList.RemoveAt(i);
						continue;
					}
					else if (itemList[i].name.Contains("Item Orb Zero Gravity") && !APSave.IsItemReceived(ItemData.AddBaseId(ItemData.itemNameToID[ItemNames.zero_grav_orb])))
					{
						Debug.Log(ItemNames.zero_grav_orb + " Not Unlocked");
						itemList.RemoveAt(i);
						continue;
					}
					else if (itemList[i].name.Contains("Item Phase Bridge") && !APSave.IsItemReceived(ItemData.AddBaseId(ItemData.itemNameToID[ItemNames.phase_bridge])))
					{
						Debug.Log(ItemNames.phase_bridge + " Not Unlocked");
						itemList.RemoveAt(i);
						continue;
					}
					else if (itemList[i].name.Contains("Item Power Crystal") && !APSave.IsItemReceived(ItemData.AddBaseId(ItemData.itemNameToID[ItemNames.energy_crystal])))
					{
						Debug.Log(ItemNames.energy_crystal + " Not Unlocked");
						itemList.RemoveAt(i);
						continue;
					}
					else if (itemList[i].name.Contains("Item Rubber Duck") && !APSave.IsItemReceived(ItemData.AddBaseId(ItemData.itemNameToID[ItemNames.rubber_duck])))
					{
						Debug.Log(ItemNames.rubber_duck + " Not Unlocked");
						itemList.RemoveAt(i);
						continue;
					}
					else if (itemList[i].name.Contains("Item Valuable Tracker") && !APSave.IsItemReceived(ItemData.AddBaseId(ItemData.itemNameToID[ItemNames.valuable_detector])))
					{
						Debug.Log(ItemNames.valuable_detector + " Not Unlocked");
						itemList.RemoveAt(i);
						continue;
					}
					else
                    {
						Debug.Log(itemList[i].name + " Unlocked, Spawning");
                    }
					ShopManager.instance.itemRotateHelper.transform.parent = itemVolume.transform;
					ShopManager.instance.itemRotateHelper.transform.localRotation = item.spawnRotationOffset;
					Quaternion rotation = ShopManager.instance.itemRotateHelper.transform.rotation;
					ShopManager.instance.itemRotateHelper.transform.parent = ShopManager.instance.transform;
					string text = "Items/" + item.prefab.name;
					if (SemiFunc.IsMultiplayer())
					{
						var inst = PhotonNetwork.InstantiateRoomObject(text, itemVolume.transform.position, rotation, 0, null);
						Plugin.LastShopItemChecked++;
						/*while (Plugin.ShopItemsBought.Contains(Plugin.LastShopItemChecked))
						{
							Plugin.LastShopItemChecked++;
						}*/
						if (item.itemType == SemiFunc.itemType.item_upgrade && item.name == ItemNames.apItem)
						{
							System.Random rand = new System.Random();
							int randomIndex = rand.Next(Plugin.ShopItemsAvailable.Count);
							int itemID = Plugin.ShopItemsAvailable[randomIndex];
							inst.name += "_Counted_" + itemID;
							Plugin.ShopItemsAvailable.RemoveAt(randomIndex);
						}
					}
					else
					{
                        var inst = UnityEngine.Object.Instantiate<GameObject>(item.prefab, itemVolume.transform.position, rotation);
						Plugin.LastShopItemChecked++;
						/*while (Plugin.ShopItemsBought.Contains(Plugin.LastShopItemChecked))
						{
							Plugin.LastShopItemChecked++;
						}*/
						if (item.itemType == SemiFunc.itemType.item_upgrade && item.name == ItemNames.apItem)
						{
							System.Random rand = new System.Random();
							int randomIndex = rand.Next(Plugin.ShopItemsAvailable.Count);
							int itemID = Plugin.ShopItemsAvailable[randomIndex];
							inst.name += "_Counted_" + itemID;
							Plugin.ShopItemsAvailable.RemoveAt(randomIndex);
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
						ItemInfo itemInfo = APSave.GetScoutedShopItem(LocationData.AddBaseId(Int64.Parse(name)));

						___itemName = $"{itemInfo.Player}'s {itemInfo.ItemName}";

						if (GameManager.instance.gameMode == 1)
						{

							FieldInfo field = AccessTools.Field(typeof(ItemUpgrade), "photonView");
							PhotonView photonView = (PhotonView)field.GetValue(__instance.GetComponent<ItemUpgrade>());
							Plugin.customRPCManager.CallUpdateItemNameRPC(___itemName, __instance.gameObject);
							return;
						}
					}
				}
			}
        }
    }


}
