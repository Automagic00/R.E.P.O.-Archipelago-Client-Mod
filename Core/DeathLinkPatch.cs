using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Photon.Realtime;
using RepoAP.Items;
using UnityEngine;
using UnityEngine.Events;

namespace RepoAP.Core
{
    internal class DeathLinkPatch
    {
        //public static string deathMsg;
        public static string playerWhoDied;
        public static bool awaitingDeathLink = false;
        public static bool isDeadFromDeathLink = false;
        public static List<string> playersWithActiveDeathCountdown = [];   // this is to prevent multiple deathlinks stacking on one player

        /**
         * How deathlink works:
         *  Outgoing:
         *      all players dead and not from deathlink and not from killing each other in the arena? if yes, does player have deathlink enabled? if yes, send a death with a message
         *      
         *  Incoming:
         *      are player(s) in a level? if yes, are all players dead? if no, is it singleplayer? if yes, print the death cause and restart the current level. if no, choose one living 
         *      player who doesn't have a death countdown active and start a death countdown
         *      
         * 
         */

        /**
         * Sends a deathlink when all players die, unless a deathlink caused the last death
         */
        [HarmonyPatch(typeof(RunManager), nameof(RunManager.ChangeLevel))]    // players might be able to skip deathlink if they leave before the level changes, but that's not a big deal
        [HarmonyPrefix]
        static void SendDeadPatch(RunManager __instance, bool _levelFailed, bool ___gameOver, Level ___levelPrevious)
        {
            if (!SemiFunc.IsMasterClientOrSingleplayer() || !_levelFailed || ___gameOver || __instance.levelCurrent == __instance.levelArena || ___levelPrevious == __instance.levelArena || 
                __instance.levelCurrent == __instance.levelShop || SemiFunc.IsMainMenu()) return;   // that's a lot of conditions
            if (!isDeadFromDeathLink)
            {
                Plugin.Logger.LogInfo("All players dead. Sending death link");
                Plugin.connection.SendDeathLink();
            }
            isDeadFromDeathLink = false;
        }

        [HarmonyPatch(typeof(RunManager), nameof(RunManager.ChangeLevel))]
        [HarmonyPostfix]
        static void ResetActiveDeathlinkPlayerListInCaseOfIssues()
        {
            playersWithActiveDeathCountdown.Clear();
        }

        [HarmonyPatch(typeof(PlayerAvatar), nameof(PlayerAvatar.PlayerDeath))]
        [HarmonyPostfix]
        static void CallDeathLinkFinishedWhenLocalPlayerDead(PlayerAvatar __instance)
        {
            if (!SemiFunc.IsMultiplayer()) return;
            Plugin.customRPCManager.CallClientDeathLinkFinished(Plugin.customRPCManagerObject, (string)AccessTools.Field(typeof(PlayerAvatar), "steamID").GetValue(__instance));
        }

        /**
         * When a deathlink is received, either restart the level (singleplayer) or posess a random player and start a death countdown (multiplayer)
         */
        [HarmonyPatch(typeof(RunManager), "Update")]
        [HarmonyPostfix]
        static void ReceiveDeathLinkPatch(RunManager __instance, bool ___allPlayersDead)
        {
            if (!SemiFunc.IsMasterClientOrSingleplayer() || !awaitingDeathLink) return;
            if (___allPlayersDead || SemiFunc.MenuLevel() || RunManager.instance.levelCurrent == RunManager.instance.levelLobby)
            {
                awaitingDeathLink = false;
                return; 
            }
            if (SemiFunc.IsMultiplayer() && GameDirector.instance.PlayerList.Count > 1)
            {
                Plugin.Logger.LogDebug("Running multiplayer deathlink");
                List<PlayerAvatar> candidatePlayers = GameDirector.instance.PlayerList.Where(player => !(bool)AccessTools.Field(typeof(PlayerAvatar), "isDisabled").GetValue(player) && 
                !playersWithActiveDeathCountdown.Contains((string)AccessTools.Field(typeof(PlayerAvatar), "steamID").GetValue(player))).ToList();   // Find all players who are alive and don't have an active death countdown
                if (candidatePlayers.Count <= 0)
                {
                    Plugin.Logger.LogDebug("No deathlink candidates");
                    awaitingDeathLink = false;
                    return;
                }
                Plugin.Logger.LogDebug($"Found {candidatePlayers.Count} deathlink candidate(s)");

                int idOfChosenPlayer = UnityEngine.Random.Range(0, candidatePlayers.Count);
                string steamIdOfChosenPlayer = (string)AccessTools.Field(typeof(PlayerAvatar), "steamID").GetValue(candidatePlayers[idOfChosenPlayer]);

                if (steamIdOfChosenPlayer == (string)AccessTools.Field(typeof(PlayerAvatar), "steamID").GetValue(PlayerAvatar.instance))
                {
                    Plugin.Logger.LogDebug("Candidate was me. Starting deathlink countdown");
                    PosessDeathlink(playerWhoDied, steamIdOfChosenPlayer);  // will eventually need to send deathlink info here if we want to get creative with death messages
                }
                else
                {
                    Plugin.Logger.LogDebug($"Candidate was not me. Starting countdown for player with steam id {idOfChosenPlayer}");
                    Plugin.customRPCManager.CallSendClientDeathLink(Plugin.customRPCManagerObject, playerWhoDied, steamIdOfChosenPlayer);
                }
                
                playersWithActiveDeathCountdown.Add(steamIdOfChosenPlayer);
                awaitingDeathLink = false;
                if (candidatePlayers.Count <= 1)
                    isDeadFromDeathLink = true;
            }
            else
            {
                Plugin.Logger.LogDebug("Running singleplayer deathlink");
                if (__instance.levelCurrent == __instance.levelShop) RunManager.instance.ChangeLevel(false, false, _changeLevelType: RunManager.ChangeLevelType.Shop);
                else RunManager.instance.ChangeLevel(false, false, _changeLevelType: RunManager.ChangeLevelType.RunLevel);
                awaitingDeathLink = false;
                DeathLinkFinished("it doesn't matter what goes here because playersWithActiveDeathCountdown isn't checked in singleplayer");
            }

        }
        // test code for use with UnityExplorer: RepoAP.Core.DeathLinkPatch.PosessDeathlink("JazzMatt", 0);
        /**
         * Handles the deathlink countdown and self destruct sequence
         */
        public static void PosessDeathlink(string playerWhoDied, string playerSteamIDToPosess)  // eventually need to make this internal
        {
            if (playerSteamIDToPosess != (string)AccessTools.Field(typeof(PlayerAvatar), "steamID").GetValue(PlayerAvatar.instance)) {
                Plugin.Logger.LogInfo("Deathlink came through but it wasn't for me");
                return; 
            }
            Plugin.Logger.LogInfo($"Starting deathlink sequence for player with steam id {playerSteamIDToPosess}");
            if ((bool)AccessTools.Field(typeof(PlayerAvatar), "isDisabled").GetValue(PlayerAvatar.instance))
                return;
            Plugin.Logger.LogDebug("This player is marked for deathlink");
            try
            {
                ChatManager.instance.PossessChatScheduleStart(2);
                List<string> deathCountdownStartStrings = new List<string>()
            {
              $"How could {playerWhoDied} throw away their life like that?",
              $"I can't believe {playerWhoDied} is dead.",
              $"This is your fault, {playerWhoDied}.",
              $"Why did {playerWhoDied} have to die?",
              $"No, {playerWhoDied}, you were supposed to live!",
              $"ALERT: {playerWhoDied} SUCKS AT STAYING ALIVE"
            };
                ChatManager.instance.PossessChat(ChatManager.PossessChatID.Betrayal, deathCountdownStartStrings[UnityEngine.Random.Range(0, deathCountdownStartStrings.Count)], 2f, Color.red, sendInTaxmanChat: true, sendInTaxmanChatEmojiInt: 2);
                ChatManager.instance.PossessChat(ChatManager.PossessChatID.Betrayal, "Now I am going to die in...", 1.5f, Color.red, _messageDelay:0.5f, sendInTaxmanChat: true, sendInTaxmanChatEmojiInt: 2);
                ChatManager.instance.PossessChat(ChatManager.PossessChatID.Betrayal, "5...", 0.25f, Color.red, 0.3f, true, 2);
                ChatManager.instance.PossessChat(ChatManager.PossessChatID.Betrayal, "4...", 0.25f, Color.red, 0.3f, true, 2);
                ChatManager.instance.PossessChat(ChatManager.PossessChatID.Betrayal, "3...", 0.25f, Color.red, 0.3f, true, 2);
                ChatManager.instance.PossessChat(ChatManager.PossessChatID.Betrayal, "2...", 0.25f, Color.red, 0.3f, true, 2);
                ChatManager.instance.PossessChat(ChatManager.PossessChatID.Betrayal, "1...", 0.5f, Color.red, 0.3f, true, 2);
                UnityEvent eventExecutionAfterMessageIsDone = new UnityEvent();
                eventExecutionAfterMessageIsDone.AddListener(() => DeathlinkSelfDestruct(playerSteamIDToPosess));
                List<string> deathCountdownEndStrings = new List<string>()
            {
              "Goodbye, cruel world",
              $"Curse you {playerWhoDied}",
              "Remember me",
              "aaaaaaaaaaaaaa",
              "Kaboo-",
              "Tell the Taxman I-",
              "Wait, it's okay, I'm not going to-",
              "[CENSORED]",
              "I hate this job",
              "I can see the light",
              "They are coming for you next"
            };
                ChatManager.instance.PossessChat(ChatManager.PossessChatID.SelfDestruct, deathCountdownEndStrings[UnityEngine.Random.Range(0, deathCountdownEndStrings.Count)], 2f, Color.red, sendInTaxmanChat: true, sendInTaxmanChatEmojiInt: 2);
                ChatManager.instance.PossessChat(ChatManager.PossessChatID.SelfDestruct, "", 2f, Color.red, eventExecutionAfterMessageIsDone: eventExecutionAfterMessageIsDone);
                ChatManager.instance.PossessChatScheduleEnd();  // this produces an error in singleplayer
            }
            catch (NullReferenceException e)    // parts of the ChatManager can be null in singleplayer and in a few edge cases
            {
                Plugin.Logger.LogError($"Deathlink scheduling failed: {e}\n");
                Plugin.Logger.LogWarning("Some additional information related to the problem:\n" +
                    $"\tCurrent level: {RunManager.instance.levelCurrent}");
                Plugin.customRPCManager.CallClientDeathLinkFinished(Plugin.customRPCManagerObject, playerSteamIDToPosess);
                ChatManager.instance.ClearAllChatBatches();
            }
            Plugin.Logger.LogDebug("Finished deathlink scheduling. Let's see what happens");
        }

        public static void DeathlinkSelfDestruct(string playerIdWhoWasPosessed)
        {
            AccessTools.Field(typeof(PlayerHealth), "health").SetValue(PlayerAvatar.instance.playerHealth, 0);
            PlayerAvatar.instance.playerHealth.Hurt(1, false);
            Plugin.Logger.LogDebug("I should be dead from deathlink now");
            Plugin.customRPCManager.CallClientDeathLinkFinished(Plugin.customRPCManagerObject, playerIdWhoWasPosessed);
        }

        /**
         * Called when a deathlink countdown has finished via RPC to the master client
         */
        public static void DeathLinkFinished(string playerIdWhoWasPosessed)
        {
            playersWithActiveDeathCountdown.Remove(playerIdWhoWasPosessed);
            if ((bool)AccessTools.Field(typeof(RunManager), "allPlayersDead").GetValue(RunManager.instance))
                isDeadFromDeathLink = true;
        }
    }
}
