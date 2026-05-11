using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace RepoAP.Items
{

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

    [HarmonyPatch(typeof(StatsManager), nameof(StatsManager.SaveFileCreate))]
    class CreateRunWithAPItemsPatch
    {
        [HarmonyPostfix]
        static void RunStartStatsPatch()
        {
            Plugin.Logger.LogDebug("Granting ap items from StatsManager.SaveFileCreate");
            StartRunWithAPItems.GrantAPItems();
        }
    }

    [HarmonyPatch(typeof(StatsManager), nameof(StatsManager.LoadGame))]    // it turns out that RunStartStats runs before the save data loads, which is why we couldn't track which items we already had
    class LoadRunWithAPItemsPatch
    {
        [HarmonyPostfix]
        static void RunStartStatsPatch()
        {
            Plugin.Logger.LogDebug("Granting ap items from StatsManager.LoadGame");
            StartRunWithAPItems.GrantAPItems();
        }
    }

    [HarmonyPatch(typeof(RunManager), nameof(RunManager.ResetProgress))]    // ResetProgress is the best candidate because when levelCurrent == levelArena, it only runs once before gameOver is true
    class RestartRunWithAPItemsPatch
    {
        [HarmonyPostfix]
        static void RunStartStatsPatch(RunManager __instance, bool ___gameOver)
        {
            if (!___gameOver && SemiFunc.RunIsArena())
            {
                Plugin.Logger.LogDebug("Granting ap items from RunManager.ResetProgress");
                StartRunWithAPItems.GrantAPItems();
            }
        }
    }
}
