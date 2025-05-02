using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace RepoAP
{
    [HarmonyPatch(typeof(TruckScreenText), "GotoNextLevel")]
    class NextLevelCheckCompletePatch
    {
        [HarmonyPostfix]
        static void CheckComplete()
        {
            Debug.Log("Truck Go To Next");
            bool complete = APSave.CheckCompletion();
            if (complete)
            {
                Plugin.connection.SendCompletion();
            }
        }
    }
}
