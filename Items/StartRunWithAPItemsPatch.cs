using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace RepoAP.Items
{
    [HarmonyPatch(typeof(StatsManager),"RunStartStats")]
    class StartRunWithAPItemsPatch
    {
        [HarmonyPostfix]
        static void RunStartStatsPatch()
        {
            if (Plugin.connection.session == null)
            {
                return;
            }

            Plugin.Logger.LogInfo("Start Run With AP Items");
            var itemsReceived = APSave.GetItemsReceived();

            foreach (var item in itemsReceived)
            {
                for (int i = 0; i < item.Value; i++)
                {
                    ItemData.AddItemToInventory(item.Key,true);
                }
            }
        }
    }
}
