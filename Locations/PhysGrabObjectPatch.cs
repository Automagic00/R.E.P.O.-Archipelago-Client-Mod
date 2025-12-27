using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Experimental.AI;
using static ModulePropSwitch;
using static System.Collections.Specialized.BitVector32;


namespace RepoAP
{
    [HarmonyPatch(typeof(PhysGrabObject))]
    class PhysGrabObjectPatch
    {
        [HarmonyPatch(nameof(PhysGrabObject.GrabStarted)), HarmonyPostfix]
        static void OrbInfoTextEnabler(PhysGrabObject __instance)
        {
            const string poscol = "#67a9cf";
            const string negcol = "#ef8a62"; 
            const string unknowncol = "#edf03c";
            string name = __instance.gameObject.name;

            bool hasSoul = name.Contains("Soul");
            bool hasValuable = name.Contains("Valuable");
            bool hasPelly = name.Contains("Pelly");
            bool isSurplus = name.Contains("Surplus");

            if (!isSurplus && (hasSoul || hasValuable || hasPelly))
            {
                string label = "";

                if (Plugin.connection.session != null)
                {
                    
                    long id = -1;
                    bool wasCollected = false;
                    bool huntObjective = false;

                    if(hasSoul)
                    {
                        id = LocationData.MonsterSoulNameToID(name);
                        wasCollected = APSave.WasMonsterSoulGathered(name);
                        huntObjective = APSave.saveData.monsterHunt;
                    }
                    else if(hasPelly)
                    {
                        id = LocationData.PellyNameToID(name + RunManager.instance.levelCurrent.name);
                        wasCollected = APSave.WasPellyGathered(name, RunManager.instance.levelCurrent.name);
                        huntObjective = APSave.IsPellyRequired(name);
                    }
                    else if (hasValuable)
                    {
                        id = LocationData.ValuableNameToID(name);
                        wasCollected = APSave.WasValuableGathered(name);
                        huntObjective = APSave.saveData.valuableHunt;
                    }

                    ItemInfo iInfo = APSave.GetScoutedLocation(id);

                    // Only display "EXTRACTED" when we are hunting the item class
                    if (huntObjective && wasCollected)
                    {
                        label = $"<br><color={poscol}>extracted";
                    }
                    else if (huntObjective && iInfo == null)
                    {
                        label = $"<br><color={negcol}>not extracted";
                    }
                    else if (wasCollected && iInfo != null)
                    {
                        label = $"<br><color={poscol}>found";
                    }
                    else if (!wasCollected && iInfo != null)
                    {
                        try
                        {
                            label = $"<br><color={negcol}>{iInfo.Player}'s {iInfo.ItemName}";
                        }
                        catch (Exception e)
                        {
                            Plugin.Logger.LogWarning($"OrbInfoTextEnabler: {e.Message}");
                            label = $"<br><color={unknowncol}>unknown";
                        }
                    }
                }

                name = LocationData.GetBaseName(name);
                SemiFunc.UIItemInfoText(null, $"{name}{label}");
            }
        }
    }
}
