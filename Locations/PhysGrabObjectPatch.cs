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
            string name = __instance.gameObject.name;

            bool hasSoul = name.Contains("Soul");
            bool hasValuable = name.Contains("Valuable");

            if ((hasSoul || hasValuable))
            {
                string label = "";

                if (Plugin.connection.session != null)
                {
                    
                    long id = -1;
                    bool wasCollected = false;
                    bool huntObjective = false;

                    if (hasSoul)
                    {
                        id = LocationData.MonsterSoulNameToID(name);
                        wasCollected = APSave.WasMonsterSoulGathered(name);
                        huntObjective = APSave.saveData.monsterHunt;
                    }
                    else if (hasValuable)
                    {
                        id = LocationData.ValuableNameToID(name);
                        wasCollected = APSave.WasValuableGathered(name);
                        huntObjective = APSave.saveData.valuableHunt;
                    }

                    // Only display "EXTRACTED" when we are hunting 
                    if (huntObjective && wasCollected)
                    {
                        label = $"<br><color={poscol}>extracted";
                    }
                    else if(huntObjective && !APSave.saveData.valuablesScouted.ContainsKey(id))
                    {
                        label = $"<br><color={negcol}>not extracted";
                    }
                    else if(!wasCollected && APSave.saveData.valuablesScouted.ContainsKey(id))
                    {
                        ItemInfo iInfo = APSave.saveData.valuablesScouted[id];
                        label = $"<br><color={negcol}>{iInfo.Player}'s {iInfo.ItemName}";
                    }

                    Plugin.connection.ScoutLocation(id);
                }

                name = LocationData.GetBaseName(name);
                SemiFunc.UIItemInfoText(null, $"{name}{label}");
            }
        }
    }
}
