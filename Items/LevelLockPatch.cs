using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using REPOLib.Modules;
using UnityEngine;

namespace RepoAP
{
    [HarmonyPatch(typeof(RunManager), "SetRunLevel")]
    class LevelLockPatch
    {
        static internal int levelIndex = -1;
        [HarmonyPostfix]
        static void SetRunLevelPre(RunManager __instance)
        {
            if (APSave.GetLevelsReceived()?.Count == 0 || Plugin.connection.session == null)
            {
                Plugin.Logger.LogError("No Levels found in Save!");
                return;
            }

            //Get what levels the player has unlocked
            var levels = APSave.GetLevelsReceived();

            //Add levels to a list
            List<string> levelList = new();
            foreach (var level in levels)
            {
                Plugin.Logger.LogDebug("Player has " + level.Key);
                levelList.Add(level.Key);
            }

            // Go through every level that has a NarrativeName in levelList and get the total valuables it has and how many have been gathered. Get the percentage (floored) and 
            // use that as a weight for selecting the level.
            int[] weights = new int[levelList.Count];
            int levelWeightIndex = 0;
            Plugin.Logger.LogDebug("Calculating level weights based on valuables gathered...");
            foreach (string levelName in levelList) 
            {
                Level levelCandidate = __instance.levels.FirstOrDefault(lev => levelName.Contains(lev.NarrativeName));
                if (levelCandidate == null)
                {
                    Plugin.Logger.LogWarning($"Level {levelName} not found in RunManager levels when selecting a weight!");
                    weights[levelWeightIndex] = 0;
                    levelWeightIndex++;
                    continue;
                }
                List<PrefabRef> allValuables = [];
                foreach (var levelValuables in levelCandidate.ValuablePresets)
                {
                    allValuables.AddRange(levelValuables.tiny);
                    allValuables.AddRange(levelValuables.small);
                    allValuables.AddRange(levelValuables.medium);
                    allValuables.AddRange(levelValuables.big);
                    allValuables.AddRange(levelValuables.wide);
                    allValuables.AddRange(levelValuables.tall);
                    allValuables.AddRange(levelValuables.veryTall);
                }

                int totalValuables = allValuables.Count;
                int ungatheredNonPellyValuables = allValuables.Count(val => !val.PrefabName.Contains("Pelly") && !APSave.WasValuableGathered(val.PrefabName));
                Plugin.Logger.LogDebug($"Level {levelCandidate.NarrativeName} has {ungatheredNonPellyValuables}/{totalValuables} valuables left to collect (besides pellys).");
                int weight = (int)(100f * ungatheredNonPellyValuables / totalValuables);    // changing the multiplier won't alter the chances
                foreach (var valuable in allValuables.Where(val => val.PrefabName.Contains("Pelly")))
                {
                    weight += APSave.WasPellyGathered(valuable.PrefabName, levelCandidate.name) ? 0 : 5;     // increase the weight by 5 for each uncollected pelly
                }
                Plugin.Logger.LogDebug($"Assigned level {levelCandidate.NarrativeName} a weight of {weight}");
                weights[levelWeightIndex] = weight;
                levelWeightIndex++;
            }

            if (weights.Sum() == 0)
            {
                Plugin.Logger.LogInfo("All valuables collected! Defaulting to random level selection.");
                levelIndex = Random.RandomRangeInt(0, levelList.Count);
            }
            else
            {
                int selectedVal = Random.RandomRangeInt(0, weights.Sum());
                int currVal = 0;
                for (int i = 0; i < weights.Length; i++)
                {
                    currVal += weights[i];
                    if (selectedVal < currVal)
                    {
                        levelIndex = i;
                        Plugin.Logger.LogInfo($"Selected {levelList[levelIndex]} ({weights[levelIndex] * 100.0f / weights.Sum()}% chance)");
                        break;
                    }
                }
            }

            var levelChoiceName = levelList[levelIndex];
            Level levelChoice = null;
            Plugin.Logger.LogInfo("Setting level to " + levelChoiceName);
            //Set level to choice
            foreach (var level in __instance.levels)
            {
                if (levelChoiceName.Contains(level.NarrativeName))
                {
                    levelChoice = level;
                }
                else
                {
                    Plugin.Logger.LogDebug(level.NarrativeName + " != " + levelChoiceName);
                }

                //Headman Manor : Level - Manor
                //Swiftbroom Academy : Level - Wizard
                //McJannek Station : Level - Arctic
                //Museum of Human Art : Level - Museum
                //Debug.Log($"{level.NarrativeName} : {level.name}");
            }// we really need a fallback in case the level isn't found, otherwise we might end up with a null reference exception and a softlock
            __instance.levelCurrent = levelChoice;
            Plugin.Logger.LogDebug("Returning " + __instance.levelCurrent.name);
            Plugin.customRPCManager.CallSyncSlotDataWithClientsRpc(Plugin.customRPCManagerObject);  // Might be involved in a race condition with SyncCompletionProgress, but it should only affect the info clients see

        }
    }
}
