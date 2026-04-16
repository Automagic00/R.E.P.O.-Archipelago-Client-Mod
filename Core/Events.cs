using System;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace RepoAP.Core
{
    public delegate void TickEventHandler(RunManager __instance);

    internal class Events
    {
        private static float lastProcessTime = 0f;
        const float TICK_FREQUENCY = 5f;

        [HarmonyPatch(typeof(RunManager), "Update")]
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
