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
        [HarmonyPatch(typeof(PunManager), nameof(PunManager.TruckPopulateItemVolumes))]
        [HarmonyPrefix]
        internal static void PopulateItemVolumesPrefix(PunManager __instance, ItemManager ___itemManager, out List<Vector3> __state)
        {
            //ItemManager itemManager = (ItemManager)AccessTools.Field(typeof(PunManager), "itemManager").GetValue(__instance);
            //__state = new List<Vector3>(itemManager.itemVolumes.Select(volume => volume.transform.position));
            //Plugin.Logger.LogInfo($"Original Item Volumes Count: {__state.Count}");
            //Plugin.Logger.LogInfo($"Original objList Count: {itemManager.purchasedItems.Count}");
            
            List<Vector3> validSpawnSpots = new();
            /*List<Item> objList = [.. itemManager.purchasedItems];
            for (int i = 0; i < objList.Count; ++i)
            {
                Item item = objList[i];
                ItemVolume matchingVol = itemManager.itemVolumes.Find(v => v.itemVolume == item.itemVolume);
                if (matchingVol != null)
                {
                    validSpawnSpots.Add(matchingVol);
                    objList.RemoveAt(i);
                }
            }*/
            List<ItemVolume> itemVolumeList = [..___itemManager.itemVolumes];
            List<Item> objList = [.. ___itemManager.purchasedItems];

            while (itemVolumeList.Count > 0 && objList.Count > 0)
            {
                bool flag = false;
                for (int index = 0; index < objList.Count; ++index)
                {
                    Item item = objList[index];
                    ItemVolume volume = itemVolumeList.Find(v => v.itemVolume == item.itemVolume && v.itemVolume == SemiFunc.itemVolume.upgrade);
                    if (volume)
                    {
                        validSpawnSpots.Add(volume.transform.position);
                        itemVolumeList.Remove(volume);
                        objList.RemoveAt(index);
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                    break;
            }
            Plugin.Logger.LogInfo($"validSpawnSpots Count: {validSpawnSpots.Count}");
            __state = validSpawnSpots;
        }
        [HarmonyPatch(typeof(PunManager), nameof(PunManager.TruckPopulateItemVolumes))] // this doesn't work. We need to do a transpiler patch because purchasedUpgrades is copied into another list inside the method
        [HarmonyPostfix]
        internal static void PopulateItemVolumesPostfix(PunManager __instance, ref ItemManager ___itemManager, List<Vector3> __state)
        {
            Plugin.Logger.LogInfo($"purchasedItems Count before: {___itemManager.purchasedItems.Count}");
            if (__state.Count < 2) return;
            Quaternion rotation = ShopManager.instance.itemRotateHelper.transform.rotation;
            Vector3 directionToSpawnUpgrades = ShopManager.instance.itemRotateHelper.transform.rotation * Vector3.Cross(__state[0], __state[1]).normalized;
            Plugin.Logger.LogInfo($"Direction to spawn upgrades: {directionToSpawnUpgrades}. Position of first shop slot: {__state[0]}. Position of new shop slot: {__state[0] + directionToSpawnUpgrades}");
            //ItemManager ___itemManager = (ItemManager)AccessTools.Field(typeof(PunManager), "itemManager").GetValue(__instance);
            List<Item> purchasedUpgrades = [..___itemManager.purchasedItems.Where(i => i.itemVolume == SemiFunc.itemVolume.upgrade)];
            int upgradesToPlace = Math.Min(purchasedUpgrades.Count, __state.Count);

            for (int i = 0; i < upgradesToPlace; i++)
            {
                SpawnItemAtPosition(purchasedUpgrades[i], __state[i] + directionToSpawnUpgrades);
                ___itemManager.purchasedItems.Remove(purchasedUpgrades[i]);
            }
            Plugin.Logger.LogInfo($"purchasedItems Count after: {___itemManager.purchasedItems.Count}");
        }

        public static void SpawnItemAtPosition(Item item, Vector3 position)    // borrowed from NoItemSpawnLimit
        {
            ShopManager.instance.itemRotateHelper.transform.parent = ShopManager.instance.transform;
            ShopManager.instance.itemRotateHelper.transform.localRotation = item.spawnRotationOffset;
            Quaternion rotation = ShopManager.instance.itemRotateHelper.transform.rotation;

            if (SemiFunc.IsMasterClient())
            {
                PhotonNetwork.InstantiateRoomObject(item.prefab.ResourcePath, position, rotation, 0);
            }
            else if (!SemiFunc.IsMultiplayer())
            {
                UnityEngine.Object.Instantiate(item.prefab.Prefab, position, rotation);
            }
        }

    }
}
