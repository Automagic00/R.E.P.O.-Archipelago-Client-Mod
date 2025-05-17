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
            Debug.Log("Calling RPC");
            PhotonView photonView = inst.GetComponent<PhotonView>();
            object[] p = new object[] { name};
            photonView.RPC(nameof(CustomRPCs.UpdateItemNameRPC), RpcTarget.All, p);
        }

        public void CallFocusTextRPC(string message, GameObject inst)
        {
            if (GameManager.instance.gameMode == 1)
            {
                PhotonView photonView = inst.GetComponent<PhotonView>();
                object[] p = new object[] { message };
                photonView.RPC(nameof(CustomRPCs.FocusTextRPC), RpcTarget.All, p);
            }
            else
            {
                FocusTextOffline(message);
            }

        }


        [PunRPC]
        public void UpdateItemNameRPC(string name, PhotonMessageInfo info)
        {
            Debug.Log("RPC Called");
            var inst = info.photonView.gameObject.GetComponent<ItemAttributes>();
            //ItemAttributes att = inst.GetComponent<ItemAttributes>();

            FieldInfo field = AccessTools.Field(typeof(ItemAttributes), "itemName");
            field.SetValue(inst, name.Replace("_"," "));

        }
        [PunRPC]
        public void FocusTextRPC(string message)
        {
            SemiFunc.UIFocusText(message, Color.white, Color.green, 3f);
        }
        public void FocusTextOffline(string message)
        {
            SemiFunc.UIFocusText(message, Color.white, Color.green, 3f);
        }
    }
}
