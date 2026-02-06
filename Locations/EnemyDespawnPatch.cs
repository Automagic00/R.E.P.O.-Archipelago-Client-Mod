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
            bool hasHealth = (bool)AccessTools.Field(typeof(Enemy), "HasHealth").GetValue(___Enemy);

            EnemyHealth health = (EnemyHealth)AccessTools.Field(typeof(Enemy), "Health").GetValue(___Enemy);

            int healthCurrent = (int)AccessTools.Field(typeof(EnemyHealth), "healthCurrent").GetValue(health);

            //if enemy died, not despawned thus if he spawned an orb
            if (!hasHealth || !health.spawnValuable || healthCurrent > 0)       // EnemyParent.Despawn is only called on the host, so clients don't see the correct name of the orb
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