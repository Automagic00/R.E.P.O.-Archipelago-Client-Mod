using System;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace RepoAP.Core
{
    public delegate void TickEventHandler(RunManager __instance);

    [HarmonyPatch(typeof(RunManager), "Update")]
    internal class Events
    {
        private static float lastProcessTime = 0f;
        const float TICK_FREQUENCY = 5f;

        [HarmonyPostfix]
        static void TickUpdate(RunManager __instance)
        {
            float currentTime = Time.unscaledTime;

            if (!SemiFunc.IsMasterClientOrSingleplayer() || SemiFunc.MenuLevel() || RunManager.instance.levelCurrent == RunManager.instance.levelLobby || currentTime - lastProcessTime < TICK_FREQUENCY) return;
            lastProcessTime = currentTime;
            Plugin.connection.TickUpdate(__instance);
        }
    }
}
