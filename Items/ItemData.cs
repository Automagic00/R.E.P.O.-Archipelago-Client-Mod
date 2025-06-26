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
        public const int shopStockID = 20;

        public static Dictionary<long, string> itemIDToName;
        public static Dictionary<string, long> itemNameToID;

        public static void CreateItemDataTable()
        {
            itemIDToName = new Dictionary<long, string>();
            itemNameToID = new Dictionary<string, long>();

            List<string> names = new List<string>();
            List<long> ids = new List<long>();



            //0-9 Reserved for Levels
            ids.Add(0);
            names.Add(LocationNames.swiftbroom);

            ids.Add(1);
 			names.Add(LocationNames.headman_manor);

            ids.Add(2);
 			names.Add(LocationNames.mcjannek);

            ids.Add(3);
 			names.Add(LocationNames.museum);

            //Upgrades
            ids.Add(10);
 			names.Add(ItemNames.upgradeHealth);

            ids.Add(11);
 			names.Add(ItemNames.upgradeStrength);

            ids.Add(12);
 			names.Add(ItemNames.upgradeRange);

            ids.Add(13);
 			names.Add(ItemNames.upgradeSprintSpeed);

            ids.Add(14);
 			names.Add(ItemNames.upgradeStamina);

            ids.Add(15);
 			names.Add(ItemNames.upgradePlayerCount);

            ids.Add(16);
 			names.Add(ItemNames.upgradeDoubleJump);

            ids.Add(17);
 			names.Add(ItemNames.upgradeTumbleLaunch);

            ids.Add(18);
 			names.Add(ItemNames.upgradeCrouchRest);

            ids.Add(19);
 			names.Add(ItemNames.upgradeTumbleWings);

            ids.Add(shopStockID);
 			names.Add(ItemNames.shopStock);

            ids.Add(21);
 			names.Add(ItemNames.small_health);

            ids.Add(22);
 			names.Add(ItemNames.medium_health);

            ids.Add(23);
 			names.Add(ItemNames.large_health);

            ids.Add(24);
 			names.Add(ItemNames.progressive_health);

            ids.Add(25);
 			names.Add(ItemNames.baseball_bat);

            ids.Add(26);
 			names.Add(ItemNames.frying_pan);

            ids.Add(27);
 			names.Add(ItemNames.sledge_hammer);

            ids.Add(28);
 			names.Add(ItemNames.sword);

            ids.Add(29);
 			names.Add(ItemNames.inflatable_hammer);

            ids.Add(30);
 			names.Add(ItemNames.prodzap);

            ids.Add(31);
 			names.Add(ItemNames.gun);

            ids.Add(32);
 			names.Add(ItemNames.shotgun);

            ids.Add(33);
 			names.Add(ItemNames.tranq_gun);

            ids.Add(34);
 			names.Add(ItemNames.pulse_pistol);

            ids.Add(35);
 			names.Add(ItemNames.photon_blaster);

            ids.Add(36);
 			names.Add(ItemNames.boltzap);

            ids.Add(37);
 			names.Add(ItemNames.cart_cannon);

            ids.Add(38);
 			names.Add(ItemNames.cart_laser);

            ids.Add(39);
 			names.Add(ItemNames.grenade);

            ids.Add(40);
 			names.Add(ItemNames.shock_grenade);

            ids.Add(41);
 			names.Add(ItemNames.stun_grenade);

            ids.Add(42);
 			names.Add(ItemNames.duct_taped_grenade);

            ids.Add(43);
 			names.Add(ItemNames.shockwave_mine);

            ids.Add(44);
 			names.Add(ItemNames.stun_mine);

            ids.Add(45);
 			names.Add(ItemNames.explosive_mine);

            ids.Add(46);
 			names.Add(ItemNames.rubber_duck);

            ids.Add(47);
 			names.Add(ItemNames.recharge_drone);

            ids.Add(48);
 			names.Add(ItemNames.indestructible_drone);

            ids.Add(49);
 			names.Add(ItemNames.roll_drone);

            ids.Add(50);
 			names.Add(ItemNames.feather_drone);

            ids.Add(51);
 			names.Add(ItemNames.zero_grav_drone);

            ids.Add(52);
 			names.Add(ItemNames.pocket_cart);

            ids.Add(53);
 			names.Add(ItemNames.cart);

            ids.Add(54);
 			names.Add(ItemNames.valuable_detector);

            ids.Add(55);
 			names.Add(ItemNames.extraction_detector);

            ids.Add(56);
 			names.Add(ItemNames.energy_crystal);

            ids.Add(57);
 			names.Add(ItemNames.zero_grav_orb);

            ids.Add(58);
 			names.Add(ItemNames.duck_bucket);

            ids.Add(59);
 			names.Add(ItemNames.phase_bridge);

            ids.Add(60);
            names.Add(ItemNames.human_grenade);

            for (int i = 0; i < ids.Count;i++)
            {
                itemIDToName.Add(ids[i], names[i]);
                itemNameToID.Add(names[i], ids[i]);
            }
        }

        public static void AddItemToInventory(long itemId, bool repeatedAdditions)
        {
            
            string itemName = IdToItemName(RemoveBaseId(itemId));
            Debug.Log("Adding Item To Inventory: " + RemoveBaseId(itemId) + " : " + itemName);

            if (LocationNames.all_levels.Contains(itemName))
            {
                APSave.AddLevelReceived(itemName);
            }
            else if (itemName == ItemNames.shopStock)
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
                StatsManager.instance.itemsPurchased[itemName]++;
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
