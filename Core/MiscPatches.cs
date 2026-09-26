using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace RepoAP.Core
{
    [HarmonyPatch(typeof(ValuableDirector), nameof(ValuableDirector.CosmeticWorldObjectLevelLoopsClampedGet))]
    internal class PreventCosmeticBoxSpawningPatch
    {
        [HarmonyPostfix]
        public static void PreventCosmeticBoxSpawning(ref int __result)
        {
            if (Plugin.BoundConfig.DisableCosmeticCrates.Value)
            {
                __result = -1;
            }
        }
    }
}
