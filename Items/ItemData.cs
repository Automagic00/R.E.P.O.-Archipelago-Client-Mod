using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RepoAP
{
    class ItemData
    {
        const int baseID = 75912022;
        public const int shopStockID = 10;

        public static Dictionary<long, string> itemIDToName;
        public static Dictionary<string, long> itemNameToID;

        public static void CreateItemDataTable()
        {
            itemIDToName = new Dictionary<long, string>();
            itemNameToID = new Dictionary<string, long>();

            List<string> names = new List<string>();
            List<long> ids = new List<long>();

            int base_shop_offset = shopStockID;


            //0-9 Reserved for Levels
            ids.Add(0);
            names.Add(LocationNames.swiftbroom_academy);

            ids.Add(1);
 			names.Add(LocationNames.headman_manor);

            ids.Add(2);
 			names.Add(LocationNames.mcjannek_station);

            ids.Add(3);
 			names.Add(LocationNames.museum_of_human_art);

            // ---- AP Function Items ----
            ids.Add(base_shop_offset++);
            names.Add(ItemNames.shop_stock);

            // ---- UPGRADES ----
            ids.Add(base_shop_offset++);
 			names.Add(ItemNames.upgrade_health);

            ids.Add(base_shop_offset++);
 			names.Add(ItemNames.upgrade_strength);

            ids.Add(base_shop_offset++);
 			names.Add(ItemNames.upgrade_range);

            ids.Add(base_shop_offset++);
 			names.Add(ItemNames.upgrade_sprint_speed);

            ids.Add(base_shop_offset++);
 			names.Add(ItemNames.upgrade_stamina);

            ids.Add(base_shop_offset++);
 			names.Add(ItemNames.upgrade_player_count);

            ids.Add(base_shop_offset++);
 			names.Add(ItemNames.upgrade_double_jump);

            ids.Add(base_shop_offset++);
 			names.Add(ItemNames.upgrade_tumble_launch);

            ids.Add(base_shop_offset++);
 			names.Add(ItemNames.upgrade_crouch_rest);

            ids.Add(base_shop_offset++);
 			names.Add(ItemNames.upgrade_tumble_wings);

            ids.Add(base_shop_offset++);
            names.Add(ItemNames.upgrade_tumble_climb);

            ids.Add(base_shop_offset++);
            names.Add(ItemNames.upgrade_death_head_battery);

            // ---- SHOP UNLOCKS ----
            ids.Add(base_shop_offset++);
 			names.Add(ItemNames.small_health);

            ids.Add(base_shop_offset++);
 			names.Add(ItemNames.medium_health);

            ids.Add(base_shop_offset++);
 			names.Add(ItemNames.large_health);

            ids.Add(base_shop_offset++);
 			names.Add(ItemNames.progressive_health);

            ids.Add(base_shop_offset++);
 			names.Add(ItemNames.baseball_bat);

            ids.Add(base_shop_offset++);
 			names.Add(ItemNames.frying_pan);

            ids.Add(base_shop_offset++);
 			names.Add(ItemNames.sledge_hammer);

            ids.Add(base_shop_offset++);
 			names.Add(ItemNames.sword);

            ids.Add(base_shop_offset++);
 			names.Add(ItemNames.inflatable_hammer);

            ids.Add(base_shop_offset++);
 			names.Add(ItemNames.prodzap);

            ids.Add(base_shop_offset++);
 			names.Add(ItemNames.gun);

            ids.Add(base_shop_offset++);
 			names.Add(ItemNames.shotgun);

            ids.Add(base_shop_offset++);
 			names.Add(ItemNames.tranq_gun);

            ids.Add(base_shop_offset++);
 			names.Add(ItemNames.pulse_pistol);

            ids.Add(base_shop_offset++);
 			names.Add(ItemNames.photon_blaster);

            ids.Add(base_shop_offset++);
 			names.Add(ItemNames.boltzap);

            ids.Add(base_shop_offset++);
 			names.Add(ItemNames.cart_cannon);

            ids.Add(base_shop_offset++);
 			names.Add(ItemNames.cart_laser);

            ids.Add(base_shop_offset++);
 			names.Add(ItemNames.grenade);

            ids.Add(base_shop_offset++);
 			names.Add(ItemNames.shock_grenade);

            ids.Add(base_shop_offset++);
            names.Add(ItemNames.human_grenade);

            ids.Add(base_shop_offset++);
 			names.Add(ItemNames.stun_grenade);

            ids.Add(base_shop_offset++);
 			names.Add(ItemNames.duct_taped_grenade);

            ids.Add(base_shop_offset++);
 			names.Add(ItemNames.shockwave_mine);

            ids.Add(base_shop_offset++);
 			names.Add(ItemNames.stun_mine);

            ids.Add(base_shop_offset++);
 			names.Add(ItemNames.explosive_mine);

            ids.Add(base_shop_offset++);
 			names.Add(ItemNames.rubber_duck);

            ids.Add(base_shop_offset++);
 			names.Add(ItemNames.recharge_drone);

            ids.Add(base_shop_offset++);
 			names.Add(ItemNames.indestructible_drone);

            ids.Add(base_shop_offset++);
 			names.Add(ItemNames.roll_drone);

            ids.Add(base_shop_offset++);
 			names.Add(ItemNames.feather_drone);

            ids.Add(base_shop_offset++);
 			names.Add(ItemNames.zero_grav_drone);

            ids.Add(base_shop_offset++);
 			names.Add(ItemNames.pocket_cart);

            ids.Add(base_shop_offset++);
 			names.Add(ItemNames.cart);

            ids.Add(base_shop_offset++);
 			names.Add(ItemNames.valuable_detector);

            ids.Add(base_shop_offset++);
 			names.Add(ItemNames.extraction_detector);

            ids.Add(base_shop_offset++);
 			names.Add(ItemNames.energy_crystal);

            ids.Add(base_shop_offset++);
 			names.Add(ItemNames.zero_grav_orb);

            ids.Add(base_shop_offset++);
 			names.Add(ItemNames.duck_bucket);

            ids.Add(base_shop_offset++);
 			names.Add(ItemNames.phase_bridge);

            for (int i = 0; i < ids.Count;i++)
            {
                itemIDToName.Add(ids[i], names[i]);
                itemNameToID.Add(names[i], ids[i]);
            }
        }

        public static void AddItemToInventory(long itemId, bool repeatedAdditions)
        {
            
            string itemName = IdToItemName(RemoveBaseId(itemId));
            Plugin.Logger.LogInfo("Attempting to add item to inventory: " + RemoveBaseId(itemId) + " : " + itemName);

            if (LocationNames.all_levels.Contains(itemName))
            {
                APSave.AddLevelReceived(itemName);
            }
            else if (itemName == ItemNames.shop_stock)
            {
                /*if (repeatedAdditions)
                {
                    return;
                }*/
                APSave.AddStockReceived();
                APSave.UpdateAvailableItems();
            }
            else if (itemName.Contains("Upgrade"))
            {
                // To ensure we don't grant upgrades multiple times, we check how many AP upgrades the save file already knows about and compare it to how many we have now.
                // itemsUpgradesPurchased only tracks non-AP upgrades, which lets the player keep them in addition to the AP ones.
                int upgradesReceived = StatsManager.instance.itemsPurchasedTotal[itemName] - StatsManager.instance.itemsUpgradesPurchased[itemName];
                if (APSave.GetItemsReceived()[itemId] > upgradesReceived)
                    StatsManager.instance.ItemPurchase(itemName);
                else
                    Plugin.Logger.LogInfo("Item " + itemName + " has already been received. Skipping...");
            }


        }

        public static string IdToItemName(long itemId)
        {
            return itemIDToName[itemId];
        }


        public static long RemoveBaseId(long id)
        {
            return id - baseID;
        }
        public static long AddBaseId(long id)
        {
            return id + baseID;
        }
    }
}
