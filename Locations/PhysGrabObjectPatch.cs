using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;

namespace RepoAP
{
    [HarmonyPatch(typeof(PhysGrabObject))]
    class PhysGrabObjectPatch
    {
        [HarmonyPatch(nameof(PhysGrabObject.GrabStarted)),HarmonyPostfix]
        static void OrbInfoTextEnabler(PhysGrabObject __instance)
        {
            if (!__instance.gameObject.name.Contains("soul")) { return; }
            SemiFunc.UIItemInfoText(null, __instance.gameObject.name);
        }
    }
}
