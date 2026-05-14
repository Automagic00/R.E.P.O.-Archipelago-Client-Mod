using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace RepoAP.Core
{
    internal class UpgradeSpawningPatch
    {
        const int MAX_UPGRADE_ROWS = 3;
        const int MAX_UPGRADES_PER_ROW = 32;

        /**
         * The goal here is to create new spots for upgrades to spawn along the shelf on the side of the truck.
         */
        [HarmonyPatch(typeof(PunManager), nameof(PunManager.TruckPopulateItemVolumes))]
        [HarmonyPrefix]
        internal static void PrepareNewTruckItemVolumes(PunManager __instance, ItemManager ___itemManager)
        {
            List<ItemVolume> upgradeItemVolumeList = [.. ___itemManager.itemVolumes.Where(volume => volume.itemVolume == SemiFunc.itemVolume.upgrade)];
            if (upgradeItemVolumeList.Count < 16) return;    // just a precaution
            Plugin.Logger.LogDebug("Creating new upgrades spawn positions in the truck");
            Vector3 directionToSpawnUpgrades = Vector3.Cross(upgradeItemVolumeList[7].transform.position, upgradeItemVolumeList[8].transform.position).normalized;   // this is originally (0.00, 1.00, 0.09) at game start. after, it is (0.00, 1.00, -0.09) in levels and (0.09, 1.00, 0.00) on the road
            directionToSpawnUpgrades.y = 0;
            if (directionToSpawnUpgrades.z > 0) directionToSpawnUpgrades.z *= -1;
            if (directionToSpawnUpgrades.x > 0) directionToSpawnUpgrades.x *= -1;
            Vector3 distBetweenUpgrades = upgradeItemVolumeList[7].transform.position - upgradeItemVolumeList[8].transform.position;    // these two are right next to one another, which is needed
            // when you load a save, the upgrade volumes are at z=-14.02. all other times, they are either at -19.28 or -16.78
            Vector3 newRowStartPosition = (upgradeItemVolumeList[7].transform.position + directionToSpawnUpgrades * 26f) - distBetweenUpgrades;
            newRowStartPosition.y += 1;

            List<ItemVolume> newUpgradeItemVolumes = [];
            for (int i = 0; i < MAX_UPGRADE_ROWS; i++)
            {
                Vector3 newRowPosition = newRowStartPosition;
                for (int j = 0; j < MAX_UPGRADES_PER_ROW; j++)
                {
                    ItemVolume newVolume = GameObject.Instantiate(upgradeItemVolumeList[7], newRowPosition += directionToSpawnUpgrades, upgradeItemVolumeList[8].transform.rotation);
                    newUpgradeItemVolumes.Add(newVolume);
                }
                newRowStartPosition += distBetweenUpgrades;
            }
            ___itemManager.itemVolumes.AddRange(newUpgradeItemVolumes);
            Plugin.Logger.LogDebug($"Created {newUpgradeItemVolumes.Count} new spots for upgrades in truck");
        }

        [HarmonyPatch(typeof(StatsManager), "LoadItemsFromFolder")]
        [HarmonyPostfix]
        internal static void AllowMoreOfUpgradeTypeToSpawnPatch(StatsManager __instance)
        {
            foreach (Item item in __instance.itemDictionary.Values)
            {
                if (item.itemType == SemiFunc.itemType.item_upgrade && item.maxAmount < 20) item.maxAmount = 20;
            }
        }

    }
}
