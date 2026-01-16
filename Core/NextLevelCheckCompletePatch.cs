using HarmonyLib;
using RepoAP.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using static ChatManager;

namespace RepoAP
{
    [HarmonyPatch(typeof(TruckScreenText), "GotoNextLevel")]
    class NextLevelCheckCompletePatch
    {
        [HarmonyPostfix]
        static void CheckComplete()
        {
            Plugin.Logger.LogInfo("Truck Go To Next");
            string status;
            bool complete = APSave.CheckCompletion(out status);
            if (complete)
            {
                Plugin.connection.SendCompletion();
            }
        }
    }
}
