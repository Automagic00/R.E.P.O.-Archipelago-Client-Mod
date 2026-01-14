using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Archipelago.MultiClient.Net.Models;
using Newtonsoft.Json.Linq;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UIElements;


namespace RepoAP
{
    class APSaveData
    {
        //public APConnectionData connectionData = new APConnectionData();
        public List<long> locationsChecked = new List<long>(); //By Location ID
        public List<string> pellysGathered = new List<string>();
        public List<string> valuablesGathered = new List<string>();
        public List<string> monsterSoulsGathered = new List<string>();
        //public List<int> shopItemsPurchased = new List<int>();    // unused
        public long shopStockSlotData;
        public int shopStockReceived;
        public Dictionary<long, int> itemsReceived = new Dictionary<long, int>();
        public Dictionary<string, bool> levelsUnlocked = new Dictionary<string, bool>();
        public int itemReceivedIndex = 0;
        //public Dictionary<long, ItemInfo> locationsScouted = new Dictionary<long, ItemInfo>();
        public Dictionary<long, SerializableItemInfo> locationsScouted = new Dictionary<long, SerializableItemInfo>();
        public JArray pellysRequired = new JArray();
        public bool pellySpawning;
        public long levelQuota;
        public long levelsCompleted;
        public long upgradeLocations;
        public bool valuableHunt;
        public bool monsterHunt;
    }

    static class APSave
    {
        public static ES3Settings es3Settings;
        public static APSaveData saveData;
        public static string fileName;
        static string saveKey = "archipelago";

        public static void Init()
        {
            if (!SemiFunc.IsMasterClientOrSingleplayer())
                return;
            string path = string.Concat(Application.persistentDataPath, "/archipelago");
            fileName = BuildFileName();

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            path += "/saves";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            path += fileName;
            if (!File.Exists(path))
            {
                es3Settings = new ES3Settings(path, ES3.EncryptionType.None);
                saveData = new APSaveData();
                SaveSlotDataToFile();
                ES3.Save<APSaveData>(saveKey, saveData, es3Settings);
            }
            else
            {
                es3Settings = new ES3Settings(path, ES3.EncryptionType.None);
                Plugin.Logger.LogInfo("Loading save data from " + path);
                saveData = ES3.Load<APSaveData>(saveKey, saveData, es3Settings);
                SaveSlotDataToFile();
                ES3.Save<APSaveData>(saveKey, saveData, es3Settings);
            }
        }

        static string BuildFileName()
        {
            if (Plugin.connection.session == null)
            {
                return null;
            }

            string fileName = $"/{Plugin.connection.session.Players.ActivePlayer.Name}___{Plugin.connection.session.RoomState.Seed}.es3";
            return fileName;
        }

        static void SaveSlotDataToFile()
        {
            if (Plugin.connection.session == null)
            {
                Plugin.Logger.LogWarning("Not connected. Cannot save slot data.");
                return;
            }
            /*var test1 = Plugin.connection.slotData["pellys_required"];
            var test2 = Plugin.connection.slotData["level_quota"];
            Debug.Log(test1.GetType());
            Debug.Log(test2.GetType());*/

            saveData.levelQuota = (long)Plugin.connection.slotData["level_quota"];
            saveData.pellysRequired = (JArray)Plugin.connection.slotData["pellys_required"];
            saveData.pellySpawning = (bool)Plugin.connection.slotData["pelly_spawning"];
            saveData.upgradeLocations = (long)Plugin.connection.slotData["upgrade_locations"];
            saveData.shopStockSlotData = (long)Plugin.connection.slotData["shop_stock"];
            saveData.valuableHunt = (bool)Plugin.connection.slotData["valuable_hunt"];
            saveData.monsterHunt = (bool)Plugin.connection.slotData["monster_hunt"];
            
        }

        public static void SyncServerLocationsToSave()  // unused?
        {
            if (!Plugin.connection.connected)
            {
                return;
            }

            var sesh = Plugin.connection.session;

            var locsChecked = sesh.Locations.AllLocationsChecked;

            foreach (var serverLoc in locsChecked)
            {
                if (!GetLocationsChecked().Contains(serverLoc))
                {
                    AddLocationChecked(serverLoc);
                }

                string locName = sesh.Locations.GetLocationNameFromId(serverLoc);

                if (locName.Contains("Pelly"))
                {
                    AddPellyGathered(locName);
                }

                if (locName.Contains("Valuable"))
                {
                    AddValuableGathered(LocationData.ValuableIDToName(serverLoc));
                }

                if (locName.Contains("Soul"))
                {
                    AddMonsterSoulGathered(LocationData.MonsterSoulIDToName(serverLoc));
                }
            }
        }

        public static void AddLocationChecked(long locToAdd)
        {
            if (Plugin.connection.session == null || !SemiFunc.IsMasterClientOrSingleplayer())
            {
                return;
            }
            saveData.locationsChecked.Add(locToAdd);
            ES3.Save<APSaveData>(saveKey, saveData, es3Settings);
        }

        public static List<long> GetLocationsChecked()
        {
            if (Plugin.connection.session == null || !SemiFunc.IsMasterClientOrSingleplayer())
            {
                return null;
            }
            return ES3.Load<APSaveData>(saveKey, es3Settings).locationsChecked;
        }

        public static List<int> GetShopLocationsChecked()
        {
            if (Plugin.connection.session == null)
            {
                return null;
            }
            List<long> allLocs = GetLocationsChecked();
            List<int> shopLocs = new List<int>();

            foreach (var loc in allLocs)
            {
                if (LocationData.RemoveBaseId(loc) <= 100)
                {
                    shopLocs.Add((int)LocationData.RemoveBaseId(loc));
                }
            }

            return shopLocs;
        }

        public static void AddItemReceived(long itemId)
        {
            if (Plugin.connection.session == null || !SemiFunc.IsMasterClientOrSingleplayer())
            {
                return;
            }
            //If Key Exists, add 1
            if (saveData.itemsReceived.ContainsKey(itemId))
            {
                saveData.itemsReceived[itemId]++;
                
            }
            //If not, set to 1
            else
            {
                saveData.itemsReceived.Add(itemId, 1);
            }

            //Increase Item Index
            saveData.itemReceivedIndex++;

            //Save
            ES3.Save<APSaveData>(saveKey, saveData, es3Settings);
            //Debug.Log("Saved " + itemId);
        }
        public static int GetItemReceivedIndex()
        {
            if (Plugin.connection.session == null || !SemiFunc.IsMasterClientOrSingleplayer())
            {
                return 0;
            }
            return ES3.Load<APSaveData>(saveKey, es3Settings).itemReceivedIndex;
        }

        public static void AddStockReceived()
        {
            if (Plugin.connection.session == null || !SemiFunc.IsMasterClientOrSingleplayer())
            {
                return;
            }
            if (saveData.itemsReceived.ContainsKey(ItemData.AddBaseId(ItemData.shopStockID)))
            {
                saveData.shopStockReceived = saveData.itemsReceived[ItemData.AddBaseId(ItemData.shopStockID)];
            }
            else
            {
                saveData.shopStockReceived = 0;
            }
            //Save
            ES3.Save<APSaveData>(saveKey, saveData, es3Settings);
            //Debug.Log("Saved " + itemId);
        }

        public static void UpdateAvailableItems()
        {
            if (Plugin.connection.session == null)
            {
                return;
            }
            Plugin.ShopItemsAvailable = new List<int>();
            Plugin.ShopItemsBought = GetShopLocationsChecked();

            for (int i = 1; i <= (APSave.saveData.shopStockSlotData * (APSave.saveData.shopStockReceived + 1)); i++)
            {
                //Debug.Log($"Stocking item {i}");
                if (!Plugin.ShopItemsBought.Contains(i) && Plugin.connection.session.Locations.AllMissingLocations.Contains(ItemData.AddBaseId(i)))
                {
                    Plugin.ShopItemsAvailable.Add(i);
                }
            }
        }

        public static Dictionary<long, int> GetItemsReceived()
        {
            if (Plugin.connection.session == null || !SemiFunc.IsMasterClientOrSingleplayer())
            {
                return null;
            }
            return ES3.Load<APSaveData>(saveKey, es3Settings).itemsReceived;
        }

        public static bool IsItemReceived(long id, int count=1)
        {
            if (!Plugin.connection.connected || !SemiFunc.IsMasterClientOrSingleplayer())
            {
                return false;
            }
            var itemsReceived = ES3.Load<APSaveData>(saveKey, es3Settings).itemsReceived;
            bool output = false;
            if (itemsReceived.ContainsKey(id) && itemsReceived[id] >= count)
            {
                output = true;
            }
            return output;
        }

        public static void AddLevelReceived(string levelName)
        {
            if (Plugin.connection.session == null || !SemiFunc.IsMasterClientOrSingleplayer())
            {
                return;
            }
            if (!saveData.levelsUnlocked.ContainsKey(levelName) || saveData.levelsUnlocked[levelName] == false)
            {
                saveData.levelsUnlocked.Add(levelName, true);
            }
            else
            {
                Plugin.Logger.LogDebug(levelName + " has already been received!");
            }

            ES3.Save<APSaveData>(saveKey, saveData, es3Settings);
        }

        public static Dictionary<string,bool> GetLevelsReceived()
        {
            if (Plugin.connection.session == null || !SemiFunc.IsMasterClientOrSingleplayer())
            {
                return null;
            }
            return ES3.Load<APSaveData>(saveKey, es3Settings).levelsUnlocked;
        }

        public static async void ScoutLocations()
        {
            if (Plugin.connection.session == null || !SemiFunc.IsMasterClientOrSingleplayer())
            {
                return;
            }

            Plugin.Logger.LogInfo("Scouting Locations...");

            int shop_item_count = 100;
            int pelly_count = LocationNames.all_pellys.Count * LocationNames.all_levels.Count;
            int valuable_count = LocationNames.all_valuables.Count;
            int monster_count = LocationNames.all_monster_souls.Count;

            Plugin.Logger.LogInfo($"Checking {shop_item_count} shop items...");
            Plugin.Logger.LogInfo($"Checking {pelly_count} pelly statues...");
            Plugin.Logger.LogInfo($"Checking {valuable_count} valuables...");
            Plugin.Logger.LogInfo($"Checking {monster_count} monster souls...");

            long[] idsToScout = new long[shop_item_count + pelly_count + valuable_count + monster_count];

            int p = 0;

            // Shop Items
            for (int i = 1; i <= shop_item_count; i++) idsToScout[p++] = LocationData.AddBaseId(i);

            // Pellys
            for (int i = 1; i <= pelly_count; i++) idsToScout[p++] = LocationData.AddBaseId(LocationData.pellyOffset + i);

            // Valuables
            for (int i = 0; i < valuable_count; i++) idsToScout[p++] = LocationData.AddBaseId(LocationData.valuableOffset + i);

            // Souls
            for (int i = 0; i < monster_count; i++) idsToScout[p++] = LocationData.AddBaseId(LocationData.monsterOffset + i);

            try
            {
                var scout = await Plugin.connection.session.Locations.ScoutLocationsAsync(idsToScout);
                saveData.locationsScouted = new Dictionary<long, SerializableItemInfo>();
                foreach (ItemInfo item in scout.Values)
                {
                    saveData.locationsScouted.Add(item.LocationId, item.ToSerializable());
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            ES3.Save<APSaveData>(saveKey, saveData, es3Settings);
        }

        public static SerializableItemInfo GetScoutedLocation(long id)
        {
            if ((Plugin.connection.session != null || !SemiFunc.IsMasterClientOrSingleplayer()) && APSave.saveData.locationsScouted.ContainsKey(id))
            {
                return saveData.locationsScouted[id];
            }
            return null;
        }

        public static SerializableItemInfo GetScoutedShopItem(long id)
        {
            return GetScoutedLocation(id);
        }

        //For when the player extracts a Pelly
        public static void AddPellyGathered(string name)
        {
            if (Plugin.connection.session == null || !SemiFunc.IsMasterClientOrSingleplayer())
            {
                return;
            }

            
            if (LocationNames.all_levels.Any(x => name.Contains(x)))
            {
                string replace = LocationNames.all_levels.FirstOrDefault(x => name.Contains(x));
                int index = LocationNames.all_levels.IndexOf(replace);
                name.Replace(replace, LocationNames.all_levels_short[index]);
            }
            name = name.Replace("Valuable", "").Replace("(Clone)", "");

            if (!saveData.pellysGathered.Contains(name))
            {
                saveData.pellysGathered.Add(name);
            }
            ES3.Save<APSaveData>(saveKey, saveData, es3Settings);
        }

        //For when the player extracts a Valuable
        public static void AddValuableGathered(string name)
        {
            name = LocationData.GetBaseName(name);
            if (Plugin.connection.session == null || !SemiFunc.IsMasterClientOrSingleplayer())
            {
                return;
            }
            if (!saveData.valuablesGathered.Contains(name))
            {
                saveData.valuablesGathered.Add(name);
            }
            ES3.Save<APSaveData>(saveKey, saveData, es3Settings);
        }

        //For when the player extracts a Monster Soul
        public static void AddMonsterSoulGathered(string name)
        {
            name = LocationData.GetBaseName(name);
            if (Plugin.connection.session == null || !SemiFunc.IsMasterClientOrSingleplayer())
            {
                return;
            }
            if (!saveData.monsterSoulsGathered.Contains(name))
            {
                saveData.monsterSoulsGathered.Add(name);
            }
            ES3.Save<APSaveData>(saveKey, saveData, es3Settings);
        }

        public static bool WasValuableGathered(string name)
        {
            name = LocationData.GetBaseName(name);
            return saveData.valuablesGathered.Contains(name);
        }

        public static bool WasMonsterSoulGathered(string name)
        {
            name = LocationData.GetBaseName(name);
            return saveData.monsterSoulsGathered.Contains(name);
        }

        public static bool WasPellyGathered(string pelly, string level)
        {
            pelly = LocationData.GetBaseName(pelly);
            return saveData.pellysGathered.Exists(x => x.Contains(level) && x.Contains(pelly));
        }

        public static bool IsPellyRequired(string pelly)
        {
            pelly = LocationData.GetBaseName(pelly);
            return saveData.pellysRequired.Any(x => pelly.Contains(x.ToString()));
        }

        public static bool CheckCompletion(out string status)
        {
            if (Plugin.connection.session == null)
            {
                status = string.Empty;
                return false;
            }
            var locationsChecked = Plugin.connection.session.Locations.AllLocationsChecked;
            var locationsMissing = Plugin.connection.session.Locations.AllMissingLocations;

            bool goalMet = true;
            var completedLevels = Math.Max(StatsManager.instance.GetRunStatLevel(), saveData.levelsCompleted);
            saveData.levelsCompleted = completedLevels;

            status = "";
            
            Plugin.Logger.LogInfo("CheckComplete");

            //Check if Level Quota is Met
            Plugin.Logger.LogInfo($"Current Level: {completedLevels}\nQuota: {saveData.levelQuota}");
            if (completedLevels < saveData.levelQuota)
            {
                Plugin.Logger.LogInfo($"Level Quota not met");
                goalMet = false;
            }

            status = $"{{truck}} Levels - {completedLevels}/{saveData.levelQuota}{(completedLevels >= saveData.levelQuota ? " {check}" : " {X}")}";

            var pellys = saveData.pellysRequired;
            var totalCount = saveData.pellysRequired.Count * LocationNames.all_levels_short.Count;
            var collectedCount = 0;
            
            //Check if Pelly Hunt is Complete
            Plugin.Logger.LogInfo("Pellys Required:");
            foreach(var pelly in saveData.pellysRequired)
            {
                Plugin.Logger.LogInfo($"-{pelly} {saveData.pellysRequired.Count}");
            }
            Plugin.Logger.LogInfo("Pellys Gathered:");

            foreach (string pellyName in saveData.pellysGathered)
            {
                Plugin.Logger.LogInfo($"-{pellyName}");
                if (saveData.pellysRequired.Any(x => pellyName.Contains(x.ToString())))
                {
                    collectedCount++;
                }
            }
            Plugin.Logger.LogInfo($"Pellys Collected: {collectedCount}/{totalCount}");
            if (collectedCount < totalCount)
            {
                Plugin.Logger.LogInfo($"Pelly hunt not complete.");
                goalMet = false;
            }

            foreach (long locID in locationsMissing)
            {
                string locName = Plugin.connection.session.Locations.GetLocationNameFromId(locID);
                if (locName.Contains("Pelly"))
                {
                    Plugin.Logger.LogInfo("Missing " + locName);
                }
            }




            status += $"<br>{{?}} Pelly - {collectedCount}/{totalCount}{(collectedCount == totalCount ? " {check}" : " {X}")}";

            //Check if Monster Hunt is complete
            if (saveData.monsterHunt)
            {
                totalCount = LocationNames.all_monster_souls.Count;
                collectedCount = 0;
                Plugin.Logger.LogInfo("Monster Hunt");
                foreach(var soul in LocationNames.all_monster_souls)
                {
                    if (!saveData.monsterSoulsGathered.Contains(soul))
                    {
                        Plugin.Logger.LogInfo($"{soul} has not been extracted");
                        goalMet = false;
                    }
                    else
                    {
                        Plugin.Logger.LogInfo($"{soul} hunted");
                        collectedCount++;
                    }
                }

                status += $"<br>{{ghost}} Souls - {collectedCount}/{totalCount}{(collectedCount == totalCount ? " {check}" : " {X}")}";
            }

            //Check if Valuable Hunt is complete
            if(saveData.valuableHunt)
            {
                totalCount = LocationNames.all_valuables.Count;
                collectedCount = 0;
                Plugin.Logger.LogInfo("Valuable Hunt");
                foreach (var valuable in LocationNames.all_valuables)
                {
                    if (!saveData.valuablesGathered.Contains(valuable))
                    {
                        Plugin.Logger.LogInfo($"{valuable} has not been extracted");
                        goalMet = false;
                    }
                    else
                    {
                        Debug.Log($"{valuable} extracted");
                        collectedCount++;
                    }
                }

                status += $"<br>{{$$$}} Valuables - {collectedCount}/{totalCount}{(collectedCount == totalCount ? " {check}" : " {X}")}";
            }

            if(goalMet) Plugin.Logger.LogInfo("All Goals Complete.");

            _ = Plugin.connection.SyncCompletionProgress(completedLevels, APSave.saveData.pellysGathered, APSave.saveData.valuablesGathered, APSave.saveData.monsterSoulsGathered, APSave.saveData.shopStockReceived);

            return goalMet;
        }
    }

    


    /*class APConnectionData
    {
        string address = "archipelago.gg";
        string port = "";
        string password = "";
        string slot = "";
    }*/
}
