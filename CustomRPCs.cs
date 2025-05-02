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
        [PunRPC]
        public static void UpdateItemNameRPC(string name, GameObject inst)
        {
            if (inst.GetComponent<ItemUpgrade>() && inst.GetComponent<ItemAttributes>())
            {
                ItemAttributes att = inst.GetComponent<ItemAttributes>();

                FieldInfo field = AccessTools.Field(typeof(ItemAttributes), "itemName");
                field.SetValue(att, name);
            }
        }
    }
}
