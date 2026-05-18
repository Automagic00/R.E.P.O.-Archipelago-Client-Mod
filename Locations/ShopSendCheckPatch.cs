using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace RepoAP
{
	//test
	[HarmonyPatch(typeof(ExtractionPoint), "DestroyAllPhysObjectsInShoppingList")]
    class ShopSendCheckPatch
    {
        [HarmonyPrefix]
        static bool ShopCheckPatch(ExtractionPoint __instance)
        {
			Plugin.Logger.LogInfo("Connected in shop check");
			if (SemiFunc.IsMasterClientOrSingleplayer())
			{
				//Exit if not connected to server
				if (Plugin.connection == null)
				{
					Plugin.Logger.LogInfo("Connection Null");
					return true;
				}
				foreach (PlayerAvatar player in GameDirector.instance.PlayerList)
                {
					PlayerDeathHead playerDeathHead = player.playerDeathHead;
                    if (playerDeathHead.inExtractionPoint)
                    {
                        float localPlayerHaulScore = playerDeathHead.GetLocalPlayerHaulScore(__instance.gameObject);
                        if (localPlayerHaulScore > 0f)
                        {
                            ProgressionManager.instance.roundPointsSupport += 100f * localPlayerHaulScore;
                        }
                    }

                    // playerDeathHead is now internal
                    playerDeathHead.Revive();
                }
				List<ItemAttributes> list = new List<ItemAttributes>();


				foreach (ItemAttributes shopping in ShopManager.instance.shoppingList)
				{
                    if (!shopping || !shopping.GetComponent<PhysGrabObject>() || SemiFunc.StatGetRunCurrency() - shopping.value < 0)
                    {
                        continue;
                    }
                    SemiFunc.StatSetRunCurrency(SemiFunc.StatGetRunCurrency() - shopping.value);

                    if (shopping.item.name == ItemNames.ap_item)
                    {
                        Plugin.Logger.LogInfo("AP ITEM PURCHASED " + shopping.name);
                        //Send Check Here

                        long id = LocationData.ShopItemToID(shopping.name);
                        if (id != 0)
                        {
                            Plugin.connection.ActivateCheck(id);
                        }
                        //StatsManager.instance.ItemPurchase(itemAttributes.item.itemAssetName);
                    }
                    //Otherwise purchase as normal
                    else
                    {
                        StatsManager.instance.ItemPurchase(shopping.item.name);
                    }

                    if (shopping.item.itemType == SemiFunc.itemType.item_upgrade && shopping.item.name != ItemNames.ap_item)
                    {
                        StatsManager.instance.AddItemsUpgradesPurchased(shopping.item.name);
                    }
                    if (shopping.item.itemType == SemiFunc.itemType.power_crystal)
                    {
                        StatsManager.instance.runStats["chargingStationChargeTotal"] += 17;
                        if (StatsManager.instance.runStats["chargingStationChargeTotal"] > 100)
                        {
                            StatsManager.instance.runStats["chargingStationChargeTotal"] = 100;
                        }
                    }
                    shopping.GetComponent<PhysGrabObject>().DestroyPhysGrabObject();
                    list.Add(shopping);
                }
				foreach (ItemAttributes itemAttributes2 in list)
				{
                    ShopManager.instance.shoppingList.Remove(itemAttributes2);
				}
				SemiFunc.ShopUpdateCost();
			}
			return false;
		}
    }
}
