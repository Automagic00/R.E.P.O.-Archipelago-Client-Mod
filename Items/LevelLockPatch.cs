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
        [HarmonyPostfix]
        static void SetRunLevelPre(RunManager __instance)
        {
            if (APSave.GetLevelsRecieved().Count == 0 || Plugin.connection.session == null)
            {
                Debug.LogError("No Levels found in Save!");
                return;
            }

            //Get what levels the player has unlocked
            var levels = APSave.GetLevelsRecieved();

            //Add levels to a list
            List<string> levelList = new();
            foreach (var level in levels)
            {
                Debug.Log("Player has " + level.Key);
                levelList.Add(level.Key);
            }

            //Choose a random level from list
            var levelChoiceName = levelList[Random.RandomRangeInt(0, levelList.Count)];
            Level levelChoice = null;
            Debug.Log("Setting level to " + levelChoiceName);
            //Set level to choice
            foreach (var level in __instance.levels)
            {
                if (levelChoiceName.Contains(level.NarrativeName))
                {
                    levelChoice = level;
                }
                else
                {
                    Debug.Log(level.NarrativeName + " != " + levelChoiceName);
                }
                //Headman Manor : Level - Manor
                //Swiftbroom Academy : Level - Wizard
                //McJannek Station : Level - Arctic
                //Debug.Log($"{level.NarrativeName} : {level.name}");
            }
            __instance.levelCurrent = levelChoice;
            Debug.Log("Returning " + __instance.levelCurrent.name);
            
        }
    }
}
