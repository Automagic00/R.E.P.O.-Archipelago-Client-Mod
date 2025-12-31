using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace RepoAP
{
    [HarmonyPatch(typeof(MenuPageSaves), nameof(MenuPageSaves.OnLoadGame))]
    class PreventLoadingRunWhenNotConnectedPatch
    {
        [HarmonyPrefix]
        static bool OnLoadGamePatch()
        {
            if (Plugin.connection.session == null || !Plugin.connection.connected)
            {
                MenuManager.instance.PageCloseAllAddedOnTop();
                MenuManager.instance.PagePopUp("Not Connected", UnityEngine.Color.red, "Not connected to AP server. Please connect before loading a save.", "OK", true);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(MenuPageSaves), nameof(MenuPageSaves.OnNewGame))]
    class PreventNewRunWhenNotConnectedPatch
    {
        [HarmonyPrefix]
        static bool OnNewGamePatch()
        {
            if (Plugin.connection.session == null || !Plugin.connection.connected)
            {
                MenuManager.instance.PageCloseAllAddedOnTop();
                MenuManager.instance.PagePopUp("Not Connected", UnityEngine.Color.red, "Not connected to AP server. Please connect before creating a save.", "OK", true);
                return false;
            }
            return true;
        }
    }
}
