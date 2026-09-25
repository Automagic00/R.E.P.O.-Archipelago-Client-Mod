using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace RepoAP.AP_Links
{
    class DamageLinkHandler
    {
        public static bool clientTakingLinkedDamage = false;

        public static void DealDamageLinkDamage(DamageLink damageLink)
        {
            if (!SemiFunc.IsMasterClientOrSingleplayer() || (!SemiFunc.RunIsLevel() && !SemiFunc.RunIsShop()) || SemiFunc.IsLevelArena(RunManager.instance.levelPrevious)) return;

            if (!GameManager.Multiplayer())
            {
                PlayerAvatar playerToHurt = PlayerAvatar.instance;
                if (playerToHurt.isDisabled)
                {
                    Plugin.connection.damageLinkService.SetDamaging(false);
                    return;
                }

                int damageToDeal = damageLink.DamageAmount;
                if (damageToDeal > 75) damageToDeal = 75;
                if (damageToDeal >= playerToHurt.playerHealth.health) damageToDeal = playerToHurt.playerHealth.health - 1;

                Plugin.connection.damageLinkService.SetDamaging(true);
                PlayerAvatar.instance.playerHealth.Hurt(damageToDeal, false);
            }
            else
            {
                List<PlayerAvatar> candidatePlayers = GameDirector.instance.PlayerList.Where(player => !player.isDisabled).ToList();
                if (candidatePlayers.Count < 1)
                {
                    Plugin.connection.damageLinkService.SetDamaging(false);
                    return;
                }
                int totalDamageToDeal = damageLink.DamageAmount > 100 ? 100 : damageLink.DamageAmount;
                int damageToDealPerPlayer = totalDamageToDeal / candidatePlayers.Count;
                if (totalDamageToDeal <= 0) totalDamageToDeal = 1;
                Plugin.connection.damageLinkService.SetDamaging(true);
                Plugin.customRPCManager.CallClientTakeDamageFromDamageLink(Plugin.customRPCManagerObject, /*damageLink*/damageToDealPerPlayer, PlayerAvatar.instance.transform.position);
            }
            Plugin.connection.damageLinkService.SetDamaging(false); // just for good measure

            Plugin.Logger.LogDebug("should have taken damage");
        }

        public static void SendDamageLinkDamage(int damage)
        {
            if (!SemiFunc.IsMasterClientOrSingleplayer() || Plugin.connection?.session == null || 
                (!SemiFunc.RunIsLevel() && !SemiFunc.RunIsShop()) || SemiFunc.IsLevelArena(RunManager.instance.levelPrevious)) return;
            Plugin.Logger.LogInfo("sending a damage link");
            if (!Plugin.connection.DamageLinkEnabled() || damage <= 0) return;
            if (Plugin.connection.damageLinkService.IsDamaging())
            {
                Plugin.Logger.LogDebug("DamageLink damage came from self. Not sending.");
                return;
            }
            Plugin.Logger.LogInfo($"sending {damage} damage");
            Plugin.connection.damageLinkService.SendDamageLink(new DamageLink(
                Plugin.connection.session.Players.ActivePlayer.Name,
                Plugin.connection.session.ConnectionInfo.Uuid,
                damage));
            Plugin.connection.damageLinkService.SetDamaging(false);
        }
    }

    [HarmonyPatch(typeof(PlayerHealth))]
    class DamageLinkPatches
    {

        [HarmonyPostfix, HarmonyPatch(nameof(PlayerHealth.Hurt))]
        static void HurtPlayerPatch(ref int damage)
        {
            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                if (!DamageLinkHandler.clientTakingLinkedDamage)
                    DamageLinkHandler.SendDamageLinkDamage(damage);
            }
            else
            {
                if (damage <= 0) return;
                if (DamageLinkHandler.clientTakingLinkedDamage)
                {
                    Plugin.Logger.LogDebug("Client is taking linked damage. Not sending damage info to host.");
                    DamageLinkHandler.clientTakingLinkedDamage = false;
                    return;
                }

                Plugin.customRPCManager.CallClientSendDamageForDamageLink(Plugin.customRPCManagerObject, damage);
            }
        }
    }
}
