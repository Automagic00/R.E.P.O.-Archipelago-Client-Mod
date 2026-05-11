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
        static bool lureTrapActive = false;
        // we will process traps here

        // we need to keep track of how many instances of each trap have been used in datastorage. 
        // I guess we get how many need to be used from ItemData.AddItemToInventory and in the update() method (or soemthing else if appropriate), try to use one and sync the new number used if successful
        // APLC uses an EventHandler that emits an event every 5 seconds in StartOfRound.Update
        // maybe we call deathlink from here, too?

        internal static void UseTrapPatch(RunManager __instance)
        {
            // we may want to use an event-driven approach later. this is just a proof-of-concept
            float currentTime = Time.unscaledTime;

            if (!SemiFunc.IsMasterClientOrSingleplayer() || SemiFunc.MenuLevel() || RunManager.instance.levelCurrent == RunManager.instance.levelLobby) return;
            Level ___levelPrevious = (Level)AccessTools.Field(typeof(RunManager), "levelPrevious").GetValue(__instance);

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
                            if (!SemiFunc.RunIsLevel()) break;
                            EnemySetup randomEnemy = EnemyDirector.instance.enemiesDifficulty3[UnityEngine.Random.Range(0, EnemyDirector.instance.enemiesDifficulty3.Count)];
                            LevelPoint selectedSpawnPoint = LevelGenerator.Instance.LevelPathPoints[UnityEngine.Random.Range(0, LevelGenerator.Instance.LevelPathPoints.Count)];
                            LevelGenerator.Instance.EnemySpawn(randomEnemy, selectedSpawnPoint.transform.position);
                            RunManager.instance.enemiesSpawned.Add(randomEnemy);
                            trapUsed = true;
                            break;
                        case "Audit Trap":
                            if (!SemiFunc.RunIsShop()) break;
                            SemiFunc.StatSetRunCurrency(StatsManager.instance.GetRunStatCurrency() / 2);
                            trapUsed = true;
                            TutorialDirector.instance.ActivateTip("AuditTrap", 0.0f, false);
                            break;
                        case "Monster Lure Trap":
                            if (!SemiFunc.RunIsLevel()) break;
                            if (!lureTrapActive)
                            {
                                lureTrapActive = true;
                                RunManager.instance.StartCoroutine(LureTrapCycle());    // I really shouldn't be doing this
                                trapUsed = true;
                                TutorialDirector.instance.ActivateTip("LureTrap", 0.0f, false);
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

        [HarmonyPatch(typeof(RunManager), nameof(RunManager.CalculateMoonLevel))]       // THIS RUNS WHEN A CLIENT JOINS A LOBBY!?!?!?!?
        [HarmonyPrefix]
        static bool CalculateMoonLevelPatch(int _levelsCompleted, ref int __result)     // the moon phase DOES stay when restarting, but you don't see the popup
        {
            if (APSave.saveData == null || !APSave.saveData.trapsUsed.TryGetValue(ItemNames.moon_phase_trap, out int numUsed)) return false;
            __result = Math.Max((_levelsCompleted + 1) / 5, numUsed);
            return false;
        }

        private static IEnumerator LureTrapCycle()
        {
            for (int i = 0; i < 6; i++)
            {
                List<LevelPoint> inPlayerRooms = SemiFunc.LevelPointsGetInPlayerRooms();
                Vector3 investigatePoint;
                if (inPlayerRooms.Count > 0)
                    investigatePoint = inPlayerRooms[UnityEngine.Random.Range(0, inPlayerRooms.Count)].transform.position;
                else 
                    investigatePoint = GameDirector.instance.PlayerList[UnityEngine.Random.Range(0, GameDirector.instance.PlayerList.Count)].transform.position;
                if (investigatePoint == null) 
                {
                    Plugin.Logger.LogWarning("Unable to find a valid investigate point for lure trap");
                    yield return new WaitForSeconds(20f);
                    continue;
                }
                SemiFunc.EnemyInvestigate(investigatePoint, 100f, true);
                float investigateTime = (float)AccessTools.Field(typeof(EnemyDirector), "investigatePointTime").GetValue(EnemyDirector.instance);
                AccessTools.Field(typeof(EnemyDirector), "investigatePointTimer").SetValue(EnemyDirector.instance, investigateTime);    // EnemyDirector.instance.investigatePointTimer = investigateTime;
                AccessTools.Field(typeof(EnemyDirector), "investigatePointTime").SetValue(EnemyDirector.instance, Mathf.Min(investigateTime + 2f, 15f));   // EnemyDirector.instance.investigatePointTime = Mathf.Min(investigateTime + 2f, 30f);
                
                if (!SemiFunc.IsMultiplayer()) 
                    Plugin.customRPCManager.PingClientsWithNoise(nameof(PlayerAvatar.instance.truckReturn), investigatePoint);
                else 
                    Plugin.customRPCManager.CallPingClientsWithNoise(Plugin.customRPCManagerObject, nameof(PlayerAvatar.instance.truckReturn), investigatePoint);

                yield return new WaitForSeconds(20f);
            }
            lureTrapActive = false;
        }

    }
}
