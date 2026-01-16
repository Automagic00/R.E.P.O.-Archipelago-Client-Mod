using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
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
                Plugin.Logger.LogInfo("Player has " + level.Key);
                levelList.Add(level.Key);
            }

            //Choose a random level from list at first
            //And than cycle through the available levels
            if (levelIndex < 0) levelIndex = Random.RandomRangeInt(0, levelList.Count);
            else levelIndex = (levelIndex + 1) % levelList.Count;

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
            }
            __instance.levelCurrent = levelChoice;
            Plugin.Logger.LogInfo("Returning " + __instance.levelCurrent.name);
            Plugin.customRPCManager.CallSyncSlotDataWithClientsRpc(Plugin.customRPCManagerObject);

        }
    }
}
