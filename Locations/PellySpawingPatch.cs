using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;
using System.Reflection;

namespace RepoAP
{
    [HarmonyPatch(typeof(ValuableDirector),"Start")]
    class PellySpawingPatch
    {
        [HarmonyPrefix]
        static void RemovePellysFromList(ValuableDirector __instance)
        {
            //Make sure pelly spawn is set to false
            if (Plugin.connection.session != null && APSave.saveData.pellySpawning == false)
            {
                Plugin.Logger.LogDebug("InValuableDirector");

                FieldInfo field = AccessTools.Field(typeof(ValuableDirector), "mediumValuables");
                List<GameObject> value = (List<GameObject>)field.GetValue(__instance);


                foreach (LevelValuables levelValuables in LevelGenerator.Instance.Level.ValuablePresets)
                {
                    foreach (var pelly in levelValuables.medium.Where(x => x.PrefabName.Contains("Pelly")).ToList())
                    {
                        Plugin.Logger.LogInfo($"Pelly Found: {pelly.PrefabName}");
                        if (APSave.saveData.pellysRequired.All(x => !pelly.PrefabName.Contains(x.ToString())))
                        {
                            Plugin.Logger.LogInfo($"Removing: {pelly.PrefabName}");
                            levelValuables.medium.Remove(pelly);
                        }
                        
                    }

                    //levelValuables.medium.RemoveAll(x => x.name.Contains("Pelly") && APSave.saveData.pellysRequired.Contains(x.name.Replace("Pelly", "").Replace("Statue", "").Replace(' ', '\0')));
                }
            }
        }
    }
}
