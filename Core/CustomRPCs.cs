using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Reflection;
using HarmonyLib;

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
            photonView.RPC(nameof(CustomRPCs.UpdateItemNameRPC), RpcTarget.All, p);
        }

        public void CallFocusTextRPC(string message, Color mainCol, Color flashCol, float lingerTime, GameObject inst)
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
        public void FocusTextRPC(string message, Color mainCol, Color flashCol, float lingerTime)
        {
            SemiFunc.UIFocusText(message, mainCol, flashCol, lingerTime);
        }
        public void FocusTextOffline(string message, Color mainCol, Color flashCol, float lingerTime)
        {
            SemiFunc.UIFocusText(message, mainCol, flashCol, lingerTime);
        }
    }
}
