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
        static int baseID = 75912022;
        public static void AddItemToInventory(long itemId, bool repeatedAdditions)
        {
            
            string itemName = IdToItemName(RemoveBaseId(itemId));
            Debug.Log("Adding Item To Inventory: " + RemoveBaseId(itemId) + " : " + itemName);

            List<string> levelNames = new List<string> { LocationNames.mcjannek, LocationNames.headman_manor, LocationNames.swiftbroom };
            if (levelNames.Contains(itemName))
            {
                APSave.AddLevelRecieved(itemName);
            }
            else if (itemName == ItemNames.shopStock)
            {
                /*if (repeatedAdditions)
                {
                    return;
                }*/
                APSave.AddStockRecieved();
                APSave.UpdateAvailableItems();
            }
            else
            {
                StatsManager.instance.itemsPurchased[itemName]++;
            }
            
        }

        private static string IdToItemName(long itemId)
        {
            string itemName = "";
            switch (itemId)
            {
                //0-9 Reserved for Levels
                case 0: itemName = LocationNames.swiftbroom; break;
                case 1: itemName = LocationNames.headman_manor; break;
                case 2: itemName = LocationNames.mcjannek; break;
                //Upgrades
                case 10: itemName = ItemNames.upgreadHealth; break;
                case 11: itemName = ItemNames.upgradeStrength; break;
                case 12: itemName = ItemNames.upgradeRange; break;
                case 13: itemName = ItemNames.upgradeSprintSpeed; break;
                case 14: itemName = ItemNames.upgradeStamina; break;
                case 15: itemName = ItemNames.upgradePlayerCount; break;
                case 16: itemName = ItemNames.upgradeDoubleJump; break;
                case 17: itemName = ItemNames.upgradeTumbleLaunch; break;
                case 18: itemName = ItemNames.shopStock; break;
            }

            return itemName;
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
