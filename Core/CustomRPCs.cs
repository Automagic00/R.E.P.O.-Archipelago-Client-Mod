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
            photonView.RPC(nameof(CustomRPCs.UpdateItemNameRPC), RpcTarget.AllBuffered, p);
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

        public void CallSyncSlotDataWithClientsRpc(GameObject inst)
        {
            if (GameManager.instance.gameMode != 1 || !PhotonNetwork.IsMasterClient)
                return;
            Plugin.Logger.LogInfo("Syncing ap data with clients");
            PhotonView photonView = inst.GetComponent<PhotonView>(); 
            object[] p = new object[] { APSave.saveData.pellysGathered.ToArray<string>(), APSave.saveData.valuablesGathered.ToArray<string>(), 
                APSave.saveData.monsterSoulsGathered.ToArray<string>(), APSave.saveData.locationsScouted.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToJson(full:true) ), 
                APSave.saveData.pellysRequired.ToString(), APSave.saveData.valuableHunt, APSave.saveData.monsterHunt, APSave.saveData.trapsUsed };  
            photonView.RPC(nameof(CustomRPCs.SyncSlotDataWithClientsRpc), RpcTarget.Others, p);    // using RpcTarget.All here may have created a race condition with APSaveData.CheckCompletion when both are called after a level is complete
        }
        public void CallClientChangeMonsterOrbName(GameObject inst, string enemyName)
        {
            Plugin.Logger.LogInfo("Calling ClientChangeMonsterOrbName");
            PhotonView photonView = inst.GetComponent<PhotonView>();
            object[] p = new object[] { enemyName };
            photonView.RPC(nameof(CustomRPCs.ClientChangeMonsterOrbName), RpcTarget.All, p);
        }
        public void CallSendClientDeathLink(GameObject inst, string playerWhoDied, string playerSteamID)
        {
            if (GameManager.instance.gameMode != 1 || !PhotonNetwork.IsMasterClient)
                return;
            Plugin.Logger.LogInfo("Sending death link notification to clients");
            PhotonView photonView = inst.GetComponent<PhotonView>();
            object[] p = new object[] { playerWhoDied, playerSteamID };
            photonView.RPC(nameof(CustomRPCs.SendClientDeathLink), RpcTarget.All, p);
        }
        public void CallClientDeathLinkFinished(GameObject inst, string playerSteamIdWhoWasPosessed)
        {
            Plugin.Logger.LogInfo("Notifying clients that death link processing is finished");
            PhotonView photonView = inst.GetComponent<PhotonView>();
            object[] p = new object[] { playerSteamIdWhoWasPosessed };
            photonView.RPC(nameof(CustomRPCs.ClientDeathLinkFinished), RpcTarget.MasterClient, p);
        }
        public void CallPingClientsWithNoise(GameObject inst, string fieldName, Vector3 position)
        {
            Plugin.Logger.LogInfo("Playing lure trap sound for clients");
            PhotonView photonView = inst.GetComponent<PhotonView>();
            object[] p = new object[] { fieldName, position };
            photonView.RPC(nameof(CustomRPCs.PingClientsWithNoise), RpcTarget.All, p);
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
        public void SyncSlotDataWithClientsRpc(string[] pellys_gathered, string[] valuables_gathered, string[] monster_souls_gathered, Dictionary<long, string> locations_scouted, string pellys_required, bool valuable_hunt, bool monster_hunt, Dictionary<string, int> trapsUsed)
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
            APSave.saveData.trapsUsed = trapsUsed;
            Plugin.Logger.LogInfo("Ap data synced with host");
        }
        [PunRPC]
        public void ClientChangeMonsterOrbName(string enemyName)
        {
            EnemyDespawnPatch.ChangeEnemyOrbNames(enemyName);
        }
        [PunRPC]
        public void SendClientDeathLink(string apPlayerWhoDied, string chosenPlayerSteamID)
        {
            RepoAP.Core.DeathLinkPatch.PosessDeathlink(apPlayerWhoDied, chosenPlayerSteamID);
        }
        [PunRPC]
        public void ClientDeathLinkFinished(string playerSteamIdWhoWasPosessed)
        {
            RepoAP.Core.DeathLinkPatch.DeathLinkFinished(playerSteamIdWhoWasPosessed);
        }
        [PunRPC]
        public void PingClientsWithNoise(string soundFieldName, Vector3 position)   // we can't pass a sound through an rpc
        {
            Sound noise = (Sound)AccessTools.Field(typeof(PlayerAvatar), soundFieldName)?.GetValue(PlayerAvatar.instance);
            if (noise == null) 
            {
                Plugin.Logger.LogError($"Unable to play sound. No field found with the name '{soundFieldName}'");
            }
            else 
                noise.Play(position, falloffMultiplier:2);
        }
    }
}
