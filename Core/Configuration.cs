using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.Configuration;
using UnityEngine;

namespace RepoAP
{
    public class PluginConfig
    {
        public ConfigEntry<bool> DisplayAPMessagesOnTruckScreen;
        public ConfigEntry<int> EnemyWeightIncrease;
        public ConfigEntry<int> ValuableSubstitutionChance;
        //public ConfigEntry<bool> DeathLink;   // for the future
        public PluginConfig(ConfigFile cfg)
        {
            DisplayAPMessagesOnTruckScreen = cfg.Bind("General", "Display Archipelago messages on truck screen", true, "If true, the truck screen will display messages from the multiworld chat. " +
                "Not recommended for large multiworlds due to the sheer volume of items being sent/received.");
            EnemyWeightIncrease = cfg.Bind("Bad Luck Protection", "Spawn weight increase for unextracted enemy souls", 50,
                "Once half of all souls have been extracted, the spawn weight of enemies whose souls haven't been extracted is raised by this amount. Every enemy has a default spawn weight of 100. " +
                "Remember that weights are NOT percentages, so a weight of 50 doesn't mean that a monster has a 50% chance to spawn. Minimum value is 0.");
            ValuableSubstitutionChance = cfg.Bind("Bad Luck Protection", "Chance to replace previously extracted valuables", 40,
                "The chance to replace previously extracted valuables with undiscovered ones in the same size group, if possible. 0 means no valuables will be replaced, " +
                "50 means roughly half of all valuables will be replaced, 100 means all valuables will be replaced when possible. Minimum value is 0, maximum is 100.");

            ClearUnusedEntries(cfg);
        }

        private void ClearUnusedEntries(ConfigFile cfg)
        {
            // Normally, old unused config entries don't get removed, so we do it with this piece of code. Credit to Kittenji.
            PropertyInfo orphanedEntriesProp = cfg.GetType().GetProperty("OrphanedEntries", BindingFlags.NonPublic | BindingFlags.Instance);
            var orphanedEntries = (Dictionary<ConfigDefinition, string>)orphanedEntriesProp.GetValue(cfg, null);
            orphanedEntries.Clear(); // Clear orphaned entries (Unbound/Abandoned entries)
            cfg.Save(); // Save the config file to save these changes
        }
    }
}
