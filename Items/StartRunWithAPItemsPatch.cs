using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace RepoAP.Items
{
    /*[HarmonyPatch(typeof(StatsManager),"RunStartStats")]
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
            StatsManager.instance.SaveFileSave();
        }
    }*/

    class StartRunWithAPItems
    {
        internal static void GrantAPItems()
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
                    ItemData.AddItemToInventory(item.Key, true);
                }
            }
        }
    }

    [HarmonyPatch(typeof(StatsManager), "SaveFileCreate")]
    class CreateRunWithAPItemsPatch
    {
        [HarmonyPostfix]
        static void RunStartStatsPatch()
        {
            StartRunWithAPItems.GrantAPItems();
        }
    }

    [HarmonyPatch(typeof(StatsManager), "LoadGame")]    // it turns out that RunStartStats runs before the save data loads, which is why we couldn't track which items we already had
    class LoadRunWithAPItemsPatch
    {
        [HarmonyPostfix]
        static void RunStartStatsPatch()
        {
            StartRunWithAPItems.GrantAPItems();
        }
    }
}
