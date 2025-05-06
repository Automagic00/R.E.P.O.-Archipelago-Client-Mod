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
    class CustomRPCs : MonoBehaviour
    {
        /*public static void AppendMethods()
        {
            MethodInfo updateItemNameRPC = typeof(CustomRPCs).GetMethod("UpdateItemNameRPC");
            if (updateItemNameRPC != null)
            {

            }
        }*/

        public static void CallUpdateItemNameRPC(string name, GameObject inst)
        {
            Debug.Log("Calling RPC");
            PhotonView photonView = inst.GetComponent<PhotonView>();
            object[] p = new object[] { name};
            photonView.RPC(nameof(CustomRPCs.UpdateItemNameRPC), RpcTarget.All, p);
        }


        [PunRPC]
        public void UpdateItemNameRPC(string name, PhotonMessageInfo info)
        {
            Debug.Log("RPC Called");
            var inst = info.photonView.gameObject.GetComponent<ItemAttributes>();
            //ItemAttributes att = inst.GetComponent<ItemAttributes>();

            FieldInfo field = AccessTools.Field(typeof(ItemAttributes), "itemName");
            field.SetValue(inst, name);

        }
    }
}
