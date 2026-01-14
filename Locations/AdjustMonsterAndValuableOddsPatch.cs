using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.UIElements.UIR.Implementation.UIRStylePainter;

namespace RepoAP.Locations
{
    [HarmonyPatch(typeof(EnemyDirector), "PickEnemies")]
    class AdjustMonsterOddsPatch
    {
        static readonly RarityPreset apBoosted = ScriptableObject.CreateInstance<RarityPreset>();
        static bool initialized = false;
        static void InitApBoostedPreset()
        {
            apBoosted.name = "AP Boosted";
            apBoosted.chance = 100.0f + Math.Max(0, Plugin.BoundConfig.EnemyWeightIncrease.Value);
        }
        static Dictionary<string, RarityPreset> originalPresets = [];
        [HarmonyPrefix]
        static void RaiseSpawnChanceOfUndefeatedEnemies(ref List<EnemySetup> _enemiesList)
        {
            if (!initialized)
            {
                InitApBoostedPreset();
                initialized = true;
            }
            int soulsCollected = 0;
            foreach (var soul in LocationNames.all_monster_souls)
            {
                if (APSave.saveData.monsterSoulsGathered.Contains(soul))
                {
                    soulsCollected++;
                }
            }

            if (Plugin.connection.session != null && Plugin.connection.connected && (float)soulsCollected / LocationNames.all_monster_souls.Count > 0.5f)
            {
                foreach (EnemySetup enemy in _enemiesList.Where(enemy => enemy.spawnObjects.Count == 1 &&
                enemy.spawnObjects[0].Prefab.GetComponent<EnemyParent>() != null))
                {
                    bool wasCollected = APSave.WasMonsterSoulGathered(enemy.spawnObjects[0].Prefab.GetComponent<EnemyParent>()?.enemyName + " Soul");
                    if (!wasCollected && enemy.rarityPreset != apBoosted)
                    {
                        originalPresets[enemy.name] = enemy.rarityPreset;
                        enemy.rarityPreset = apBoosted;
                        Plugin.Logger.LogInfo($"Raised spawn weight of enemy '{enemy.name}' to {enemy.rarityPreset.chance}");
                    }
                    else if (wasCollected && enemy.rarityPreset == apBoosted)
                    {
                        enemy.rarityPreset = originalPresets[enemy.name];
                        Plugin.Logger.LogInfo($"Reduced spawn weight of enemy '{enemy.name}' back to the default value");
                    }
                }

            }
        }
    }
    [HarmonyPatch(typeof(ValuableDirector), "Spawn")]  // LevelGenerator.Generate performs all steps of generation. In the valuables phase, it calls ValuableDirector.SetupHost
    class AdjustValuableOddsPatch
    {
        [HarmonyPrefix]
        static void ReplaceFoundValuableWithChance(ref ValuableDirector __instance, ref PrefabRef _valuable, ref ValuableVolume _volume, ref string _path)
        {
            bool willReplaceValuable = UnityEngine.Random.Range(0, 100) < Mathf.Clamp(Plugin.BoundConfig.ValuableSubstitutionChance.Value, 0, 100);
            var volumeType = _volume.VolumeType;
            PrefabRef realValuable = _valuable;

            string valuableGroup = volumeType switch
            {
                ValuableVolume.Type.Tiny => "tinyValuables",
                ValuableVolume.Type.Small => "smallValuables",
                ValuableVolume.Type.Medium => "mediumValuables",
                ValuableVolume.Type.Big => "bigValuables",
                ValuableVolume.Type.Wide => "wideValuables",
                ValuableVolume.Type.Tall => "tallValuables",
                ValuableVolume.Type.VeryTall => "veryTallValuables",
                _ => ""
            };
            if (willReplaceValuable && valuableGroup != "" && _valuable.PrefabName.Contains("Pelly") ? 
                APSave.WasPellyGathered(_valuable.PrefabName, RunManager.instance.levelCurrent.name) : 
                APSave.WasValuableGathered(_valuable.PrefabName))
            {
                string nameBefore = realValuable.PrefabName;
                List<PrefabRef> unfoundValuables = [.. ((List<PrefabRef>)AccessTools.Field(typeof(ValuableDirector), valuableGroup).GetValue(__instance))
                    .Where(prefab => prefab != realValuable && !(prefab.PrefabName.Contains("Pelly") ? 
                        APSave.WasPellyGathered(prefab.PrefabName, RunManager.instance.levelCurrent.name) : 
                        APSave.WasValuableGathered(prefab.PrefabName)))];
                if (unfoundValuables.Count == 0)
                {
                    Plugin.Logger.LogInfo($"No undiscovered valuables available to replace {nameBefore} in group {valuableGroup}");
                    return;
                }
                _valuable = unfoundValuables[UnityEngine.Random.Range(0, unfoundValuables.Count)];
                Plugin.Logger.LogInfo($"Replaced valuable {nameBefore} with {_valuable.PrefabName}");
            }
        }
    }
}
