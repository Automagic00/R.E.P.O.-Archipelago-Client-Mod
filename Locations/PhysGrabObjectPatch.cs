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
            string name = __instance.gameObject.name;

            if (name.Contains("Soul") || name.Contains("Valuable"))
            {
                name = LocationData.GetBaseName(name);
                SemiFunc.UIItemInfoText(null, name);
            }
      }
    }
}
