using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using static System.Collections.Specialized.BitVector32;
using static UnityEngine.UIElements.UIR.Implementation.UIRStylePainter;

namespace RepoAP.Core
{
    internal class TrapHandler
    {
        private static float lastProcessTime = 0f;
        const float TRAP_TRIGGER_FREQUENCY = 5f;
        static bool pingTrapActive = false;
        // we will process traps here

        // we need to keep track of how many instances of each trap have been used in datastorage. 
        // I guess we get how many need to be used from ItemData.AddItemToInventory and in the update() method (or soemthing else if appropriate), try to use one and sync the new number used if successful
        // APLC uses an EventHandler that emits an event every 5 seconds in StartOfRound.Update
        // maybe we call deathlink from here, too?

        [HarmonyPatch(typeof(RunManager), "Update")]
        [HarmonyPostfix]
        static void UseTrapPatch(RunManager __instance, Level ___levelPrevious)
        {
            // we may want to use an event-driven approach later. this is just a proof-of-concept
            float currentTime = Time.unscaledTime;

            if (!SemiFunc.IsMasterClientOrSingleplayer() || SemiFunc.MenuLevel() || RunManager.instance.levelCurrent == RunManager.instance.levelLobby || currentTime - lastProcessTime < TRAP_TRIGGER_FREQUENCY) return;
            lastProcessTime = currentTime;
            List<string> trapNames = [.. APSave.saveData.trapsUsed.Keys];
            foreach (string trap in trapNames)
            {
                long trapID = ItemData.AddBaseId(ItemData.itemNameToID[trap]);
                if (APSave.saveData.itemsReceived.ContainsKey(trapID) && APSave.saveData.itemsReceived[trapID] > APSave.saveData.trapsUsed[trap])
                {
                    Plugin.Logger.LogInfo($"Trying to use trap '{trap}'");
                    bool trapUsed = false;
                    switch (trap)
                    {
                        case "Extra Monster Trap":
                            if (__instance.levelCurrent == __instance.levelArena || ___levelPrevious == __instance.levelArena || 
                                __instance.levelCurrent == __instance.levelShop || SemiFunc.IsMainMenu()) break;
                            EnemySetup randomEnemy = EnemyDirector.instance.enemiesDifficulty3[UnityEngine.Random.Range(0, EnemyDirector.instance.enemiesDifficulty3.Count)];
                            LevelPoint selectedSpawnPoint = LevelGenerator.Instance.LevelPathPoints[UnityEngine.Random.Range(0, LevelGenerator.Instance.LevelPathPoints.Count)];
                            LevelGenerator.Instance.EnemySpawn(randomEnemy, selectedSpawnPoint.transform.position);
                            RunManager.instance.enemiesSpawned.Add(randomEnemy);
                            trapUsed = true;
                            break;
                        case "Audit Trap":
                            SemiFunc.StatSetRunCurrency(StatsManager.instance.GetRunStatCurrency() / 2);
                            trapUsed = true;
                            break;
                        case "Ping Trap":
                            if (__instance.levelCurrent == __instance.levelArena || ___levelPrevious == __instance.levelArena ||
                                __instance.levelCurrent == __instance.levelShop || SemiFunc.IsMainMenu()) break;
                            if (!pingTrapActive)
                            {
                                pingTrapActive = true;
                                RunManager.instance.StartCoroutine(PingTrapCycle());    // I really shouldn't be doing this
                                trapUsed = true;
                            }
                            break;
                        case "Progressive Moon Phase Trap":
                            trapUsed = true;    // for consistency. we don't really need to keep track of "usage" for this one because we handle it in CalculateMoonLevelPatch
                            break;
                        default:
                            Plugin.Logger.LogWarning($"Attempted to use trap '{trap}' which is not known");
                            break;

                    }
                    if (trapUsed)
                    {
                        APSave.saveData.trapsUsed[trap]++;
                        Plugin.Logger.LogInfo($"Successfully used trap '{trap}'");
                    }
                }
            }
            Plugin.connection.session.DataStorage[$"REPO-{Plugin.connection.session.Players.GetPlayerName(Plugin.connection.session.ConnectionInfo.Slot)}-trapsUsed"] = JObject.FromObject(APSave.saveData.trapsUsed);// needs better syncing later
        }

        [HarmonyPatch(typeof(RunManager), nameof(RunManager.CalculateMoonLevel))]
        [HarmonyPrefix]
        static bool CalculateMoonLevelPatch(int _levelsCompleted, ref int __result)
        {
            __result = Math.Max((_levelsCompleted + 1) / 5, APSave.saveData.trapsUsed[ItemNames.moon_phase_trap]);
            return false;
        }

        private static IEnumerator PingTrapCycle()
        {
            for (int i = 0; i < 6; i++)
            {
                List<LevelPoint> inPlayerRooms = SemiFunc.LevelPointsGetInPlayerRooms();
                if (inPlayerRooms.Count > 0)
                    SemiFunc.EnemyInvestigate(inPlayerRooms[UnityEngine.Random.Range(0, inPlayerRooms.Count)].transform.position, 100f, true);
                float investigateTime = (float)AccessTools.Field(typeof(EnemyDirector), "investigatePointTime").GetValue(EnemyDirector.instance);
                AccessTools.Field(typeof(EnemyDirector), "investigatePointTimer").SetValue(EnemyDirector.instance, investigateTime);    // EnemyDirector.instance.investigatePointTimer = investigateTime;
                AccessTools.Field(typeof(EnemyDirector), "investigatePointTime").SetValue(EnemyDirector.instance, Mathf.Min(investigateTime + 2f, 15f));   // EnemyDirector.instance.investigatePointTime = Mathf.Min(investigateTime + 2f, 30f);
                PlayerAvatar.instance.truckReturn.Play(PlayerAvatar.instance.PlayerVisionTarget.VisionTransform.position);
                yield return new WaitForSeconds(20f);
            }
            pingTrapActive = false;
        }

    }
}
