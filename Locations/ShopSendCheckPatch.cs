using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace RepoAP
{
	//test
	[HarmonyPatch(typeof(ExtractionPoint), "DestroyAllPhysObjectsInShoppingList")]
    class ShopSendCheckPatch
    {
		static FieldInfo field = AccessTools.Field(typeof(ShopManager), "shoppingList");
		static List<ItemAttributes> shoppingList;

		static FieldInfo field2 = AccessTools.Field(typeof(ItemAttributes), "value");
		static int value;

        [HarmonyPrefix]
        static bool ShopCheckPatch(ExtractionPoint __instance)
        {
			shoppingList = (List<ItemAttributes>)field.GetValue(ShopManager.instance);
			Plugin.Logger.LogInfo("Connected in shop check");
			if (SemiFunc.IsMasterClientOrSingleplayer())
			{
				//Exit if not connected to server
				if (Plugin.connection == null)
				{
					Plugin.Logger.LogInfo("Connection Null");
					return true;
				}
				foreach (PlayerAvatar playerAvatar in GameDirector.instance.PlayerList)
				{
					playerAvatar.playerDeathHead.Revive();
				}
				List<ItemAttributes> list = new List<ItemAttributes>();


				foreach (ItemAttributes itemAttributes in shoppingList)
				{
					value = (int)field2.GetValue(itemAttributes);
					if (itemAttributes && itemAttributes.GetComponent<PhysGrabObject>() && SemiFunc.StatGetRunCurrency() - value >= 0)
					{
						SemiFunc.StatSetRunCurrency(SemiFunc.StatGetRunCurrency() - value);


						if (itemAttributes.item.prefab.PrefabName == ItemNames.ap_item)
						{
							Plugin.Logger.LogInfo("AP ITEM PURCHASED " + itemAttributes.name);
							//Send Check Here

							long id = LocationData.ShopItemToID(itemAttributes.name);
							if (id != 0)
							{
								Plugin.connection.ActivateCheck(id);
							}
							//StatsManager.instance.ItemPurchase(itemAttributes.item.itemAssetName);
						}
						//Otherwise purchase as normal
						else
						{
							Plugin.Logger.LogInfo("Not AP Item\n" + itemAttributes.item.prefab.PrefabName + " != " + ItemNames.ap_item);
							StatsManager.instance.ItemPurchase(itemAttributes.item.prefab.PrefabName);
						}

						if (itemAttributes.item.itemType == SemiFunc.itemType.item_upgrade && itemAttributes.item.prefab.PrefabName != ItemNames.ap_item)
						{
							StatsManager.instance.AddItemsUpgradesPurchased(itemAttributes.item.prefab.PrefabName);
						}
						if (itemAttributes.item.itemType == SemiFunc.itemType.power_crystal)
						{
							Dictionary<string, int> runStats = StatsManager.instance.runStats;
							runStats["chargingStationChargeTotal"] = runStats["chargingStationChargeTotal"] + 17;
							if (StatsManager.instance.runStats["chargingStationChargeTotal"] > 100)
							{
								StatsManager.instance.runStats["chargingStationChargeTotal"] = 100;
							}
						}
						itemAttributes.GetComponent<PhysGrabObject>().DestroyPhysGrabObject();
						list.Add(itemAttributes);
					}
				}
				foreach (ItemAttributes itemAttributes2 in list)
				{
					List<ItemAttributes> newValue = (List<ItemAttributes>)field.GetValue(ShopManager.instance);
					newValue.Remove(itemAttributes2);
					field.SetValue(ShopManager.instance,newValue);
				}
				SemiFunc.ShopUpdateCost();
			}
			return false;
		}
    }
}
