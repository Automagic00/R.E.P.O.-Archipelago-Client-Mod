using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Photon.Pun;
using UnityEngine;

namespace RepoAP.Core
{
    internal class ItemRPCs : MonoBehaviour
    {

        public void CallUpdateItemNameRPC(string name, GameObject inst)
        {
            Plugin.Logger.LogInfo("Calling UpdateItemNameRPC");
            PhotonView photonView = inst.GetComponent<PhotonView>();
            object[] p = new object[] { name };
            photonView.RPC(nameof(UpdateItemNameRPC), RpcTarget.AllBuffered, p);
        }

        [PunRPC]
        public void UpdateItemNameRPC(string name, PhotonMessageInfo info)
        {
            Plugin.Logger.LogInfo("UpdateItemNameRPC Called");
            ItemAttributes inst = info.photonView.gameObject.GetComponent<ItemAttributes>();
            if (inst == null)
            {
                Plugin.Logger.LogError("UpdateItemNameRPC: Item lacks an ItemAttributes component and its name cannot be modified.");
                return;
            }
            IEnumerator coroutine = SetItemNameDelayed(inst, name);
            StartCoroutine(coroutine);
        }

        private IEnumerator SetItemNameDelayed(ItemAttributes inst, string newItemName)
        {
            yield return null;
            inst.itemName = newItemName.Replace("_", " ");
        }
    }
}
