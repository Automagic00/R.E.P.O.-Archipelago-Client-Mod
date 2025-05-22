using HarmonyLib;
using UnityEngine;
using System.Reflection;
namespace RepoAP
{

    [HarmonyPatch(typeof(EnemyParent))]
    class EnemyDespawnPatch
    {
        [HarmonyPatch(nameof(EnemyParent.Despawn)), HarmonyPostfix]
        static void OrbNaming(ref string ___enemyName, ref Enemy ___Enemy)
        {
            FieldInfo field1 = AccessTools.Field(typeof(Enemy), "HasHealth");
            bool hasHealth = (bool)field1.GetValue(___Enemy);

            FieldInfo field2 = AccessTools.Field(typeof(Enemy), "Health");
            EnemyHealth health = (EnemyHealth)field2.GetValue(___Enemy);

            FieldInfo field3 = AccessTools.Field(typeof(EnemyHealth), "healthCurrent");
            int healthCurrent = (int)field3.GetValue(health);

            //if enemy died, not despawned thus if he spawned an orb
            if (!hasHealth || !health.spawnValuable || healthCurrent > 0)
            {
                return;
            }
            EnemyValuable[] orbs = (EnemyValuable[])GameObject.FindObjectsByType(typeof(EnemyValuable), FindObjectsSortMode.None);
            foreach (EnemyValuable orb in orbs)
            {

                //if orb is already named, move on
                if (!orb.name.Contains("Enemy Valuable")) { continue; }
                orb.name = ___enemyName + " Soul";
            }
        }
    }
}