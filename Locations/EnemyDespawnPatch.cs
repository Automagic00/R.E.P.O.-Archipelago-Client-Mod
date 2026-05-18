using HarmonyLib;
using UnityEngine;
using System.Reflection;
namespace RepoAP
{

    class EnemyDespawnPatch
    {
        [HarmonyPatch(typeof(EnemyParent), nameof(EnemyParent.Despawn)), HarmonyPostfix]
        static void OrbNaming(ref string ___enemyName, ref Enemy ___Enemy)
        {
            //if enemy died, not despawned thus if he spawned an orb
            if (!___Enemy.HasHealth || !___Enemy.Health.spawnValuable || ___Enemy.Health.health > 0)       // EnemyParent.Despawn is only called on the host, so clients don't see the correct name of the orb
            {
                return;
            }
            if (!SemiFunc.IsMultiplayer())
                ChangeEnemyOrbNames(___enemyName);
            else 
                Plugin.customRPCManager.CallClientChangeMonsterOrbName(Plugin.customRPCManagerObject, ___enemyName);
        }
        internal static void ChangeEnemyOrbNames(string enemyName)
        {
            EnemyValuable[] orbs = (EnemyValuable[])GameObject.FindObjectsByType(typeof(EnemyValuable), FindObjectsSortMode.None);
            foreach (EnemyValuable orb in orbs)
            {
                //if orb is already named, move on
                if (!orb.name.Contains("Enemy Valuable")) { continue; }
                orb.name = enemyName + " Soul";
            }
        }
    }
}