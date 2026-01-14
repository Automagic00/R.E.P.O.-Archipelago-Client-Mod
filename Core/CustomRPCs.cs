using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Reflection;
using Archipelago.MultiClient.Net.Models;
using HarmonyLib;
using Newtonsoft.Json.Linq;

namespace RepoAP
{
    public class CustomRPCs : MonoBehaviour
    {
        /*public static void AppendMethods()
        {
            MethodInfo updateItemNameRPC = typeof(CustomRPCs).GetMethod("UpdateItemNameRPC");
            if (updateItemNameRPC != null)
            {

            }
        }*/

        public void CallUpdateItemNameRPC(string name, GameObject inst)
        {
            Plugin.Logger.LogInfo("Calling UpdateItemNameRPC");
            PhotonView photonView = inst.GetComponent<PhotonView>();
            object[] p = new object[] { name};
            photonView.RPC(nameof(CustomRPCs.UpdateItemNameRPC), RpcTarget.Others, p);
        }

        public void CallFocusTextRPC(string message, UnityEngine.Color mainCol, UnityEngine.Color flashCol, float lingerTime, GameObject inst)
        {
            if (GameManager.instance.gameMode == 1)
            {
                PhotonView photonView = inst.GetComponent<PhotonView>();
                object[] p = new object[] { message, mainCol, flashCol, lingerTime };
                photonView.RPC(nameof(CustomRPCs.FocusTextRPC), RpcTarget.All, p);
            }
            else
            {
                FocusTextOffline(message, mainCol, flashCol, lingerTime);
            }

        }

        public void CallSyncSlotDataWithClientsRpc(GameObject inst) // I don't even know if this will work. I believe this needs to be called when entering a new level and extracting
        {
            if (GameManager.instance.gameMode != 1 || !PhotonNetwork.IsMasterClient)
                return;
            Plugin.Logger.LogInfo("Syncing ap data with clients");
            PhotonView photonView = inst.GetComponent<PhotonView>(); 
            object[] p = new object[] { APSave.saveData.pellysGathered.ToArray<string>(), APSave.saveData.valuablesGathered.ToArray<string>(), 
                APSave.saveData.monsterSoulsGathered.ToArray<string>(), APSave.saveData.locationsScouted.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToJson(full:true) ), 
                APSave.saveData.pellysRequired.ToString(), APSave.saveData.valuableHunt, APSave.saveData.monsterHunt };  
            photonView.RPC(nameof(CustomRPCs.SyncSlotDataWithClientsRpc), RpcTarget.All, p);
        }


        [PunRPC]
        public void UpdateItemNameRPC(string name, PhotonMessageInfo info)
        {
            Plugin.Logger.LogInfo("UpdateItemNameRPC Called");
            var inst = info.photonView.gameObject.GetComponent<ItemAttributes>();
            //ItemAttributes att = inst.GetComponent<ItemAttributes>();

            FieldInfo field = AccessTools.Field(typeof(ItemAttributes), "itemName");
            field.SetValue(inst, name.Replace("_"," "));

        }
        [PunRPC]
        public void FocusTextRPC(string message, UnityEngine.Color mainCol, UnityEngine.Color flashCol, float lingerTime)
        {
            SemiFunc.UIFocusText(message, mainCol, flashCol, lingerTime);
        }
        public void FocusTextOffline(string message, UnityEngine.Color mainCol, UnityEngine.Color flashCol, float lingerTime)
        {
            SemiFunc.UIFocusText(message, mainCol, flashCol, lingerTime);
        }

        [PunRPC]
        public void SyncSlotDataWithClientsRpc(string[] pellys_gathered, string[] valuables_gathered, string[] monster_souls_gathered, Dictionary<long, string> locations_scouted, string pellys_required, bool valuable_hunt, bool monster_hunt)
        {
            APSave.saveData ??= new APSaveData();
            //APSave.saveData.locationsChecked =                            // not needed by clients
            APSave.saveData.pellysGathered = pellys_gathered.ToList<string>();               // needed for PhysGrabObjectPatch
            APSave.saveData.valuablesGathered = valuables_gathered.ToList<string>();         // needed for PhysGrabObjectPatch
            APSave.saveData.monsterSoulsGathered = monster_souls_gathered.ToList<string>();  // needed for PhysGrabObjectPatch
            //APSave.saveData.shopItemsPurchased =                          // not used at all
            //APSave.saveData.shopStockSlotData = shop_stock;               // not needed by clients
            //APSave.saveData.shopStockReceived =                           // not needed by clients
            //APSave.saveData.itemsReceived =                               // not needed by clients
            //APSave.saveData.levelsUnlocked =                              // not needed by clients
            //APSave.saveData.itemReceivedIndex =                           // not needed by clients
            APSave.saveData.locationsScouted = locations_scouted.ToDictionary(kvp => kvp.Key, kvp => SerializableItemInfo.FromJson(kvp.Value));// needed for PhysGrabObjectPatch
            APSave.saveData.pellysRequired = JArray.Parse(pellys_required); // needed
            //APSave.saveData.pellySpawning = pelly_spawning;               // not needed by clients
            //APSave.saveData.levelQuota = level_quota;                     // not needed by clients
            //APSave.saveData.upgradeLocations = upgrade_locations;         // not used at all anymore
            APSave.saveData.valuableHunt = valuable_hunt;                   // needed
            APSave.saveData.monsterHunt = monster_hunt;                     // needed
            Plugin.Logger.LogInfo("Ap data synced with host");
        }
    }
}
