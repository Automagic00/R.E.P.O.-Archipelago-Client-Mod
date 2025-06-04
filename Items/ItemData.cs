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
                case 3: itemName = LocationNames.museum; break;
                //Upgrades
                case 10: itemName = ItemNames.upgradeHealth; break;
                case 11: itemName = ItemNames.upgradeStrength; break;
                case 12: itemName = ItemNames.upgradeRange; break;
                case 13: itemName = ItemNames.upgradeSprintSpeed; break;
                case 14: itemName = ItemNames.upgradeStamina; break;
                case 15: itemName = ItemNames.upgradePlayerCount; break;
                case 16: itemName = ItemNames.upgradeDoubleJump; break;
                case 17: itemName = ItemNames.upgradeTumbleLaunch; break;
                case 18: itemName = ItemNames.upgradeCrouchRest; break;
                case 19: itemName = ItemNames.upgradeTumbleWings; break;
                case shopStockID: itemName = ItemNames.shopStock; break;
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
