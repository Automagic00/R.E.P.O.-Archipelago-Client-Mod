using Archipelago.MultiClient.Net.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;


namespace RepoAP
{
    class APSaveData
    {
        public APConnectionData connectionData = new APConnectionData();
        public List<long> locationsChecked = new List<long>(); //By Location ID
        public List<string> pellysGathered = new List<string>();
        public List<string> valuablesGathered = new List<string>();
        public List<string> monsterSoulsGathered = new List<string>();
        public List<int> shopItemsPurchased = new List<int>();
        public long shopStockSlotData;
        public int shopStockReceived;
        public Dictionary<long, int> itemsReceived = new Dictionary<long, int>();
        public Dictionary<string, bool> levelsUnlocked = new Dictionary<string, bool>();
        public int itemReceivedIndex = 0;
        public Dictionary<long, ItemInfo> shopItemsScouted = new Dictionary<long, ItemInfo>();
        public JArray pellysRequired = new JArray();
        public bool pellySpawning;
        public long levelQuota;
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

        public static void AddLocationChecked(long locToAdd)
        {
            if (Plugin.connection.session == null)
            {
                return;
            }
            saveData.locationsChecked.Add(locToAdd);
            ES3.Save<APSaveData>(saveKey, saveData, es3Settings);
        }
        public static List<long> GetLocationsChecked()
        {
            if (Plugin.connection.session == null)
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
            if (Plugin.connection.session == null)
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
            if (Plugin.connection.session == null)
            {
                return 0;
            }
            return ES3.Load<APSaveData>(saveKey, es3Settings).itemReceivedIndex;
        }

        public static void AddStockReceived()
        {
            if (Plugin.connection.session == null)
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
            if (Plugin.connection.session == null)
            {
                return null;
            }
            return ES3.Load<APSaveData>(saveKey, es3Settings).itemsReceived;
        }

        public static void AddLevelReceived(string levelName)
        {
            if (Plugin.connection.session == null)
            {
                return;
            }
            if (!saveData.levelsUnlocked.ContainsKey(levelName) || saveData.levelsUnlocked[levelName] == false)
            {
                saveData.levelsUnlocked.Add(levelName, true);
            }
            else
            {
                Debug.LogError(levelName + " has already been received!");
            }

            ES3.Save<APSaveData>(saveKey, saveData, es3Settings);
        }

        public static Dictionary<string,bool> GetLevelsReceived()
        {
            if (Plugin.connection.session == null)
            {
                return null;
            }
            return ES3.Load<APSaveData>(saveKey, es3Settings).levelsUnlocked;
        }

        public static async void ScoutShopItems()
        {
            if (Plugin.connection.session == null)
            {
                return;
            }
            long[] idsToScout = new long[100] ;
            for (int i = 1; i <= 100; i++)
            {
                //Debug.Log("Adding "+ LocationData.AddBaseId(i));
                idsToScout[i-1] = LocationData.AddBaseId(i);
            }
            //Debug.Log(idsToScout);
            try
            {
                var scout = await Plugin.connection.session.Locations.ScoutLocationsAsync(idsToScout);
                saveData.shopItemsScouted = new Dictionary<long, ItemInfo>();
                //Debug.Log(scout.Values.Count);
                foreach (ItemInfo item in scout.Values)
                {
                    //Debug.Log("Saving " + item.LocationId + " " + item.ItemDisplayName);
                    saveData.shopItemsScouted.Add(item.LocationId, item);
                }
            }
            catch(Exception e)
            {
                Debug.LogError(e);
            }

            /*foreach(var item in saveData.shopItemsScouted.Values)
            {
                Debug.Log("InSaveData " + item.LocationId + " " + item.ItemDisplayName);
            }*/
            ES3.Save<APSaveData>(saveKey, saveData, es3Settings);
        }

        public static ItemInfo GetScoutedShopItem(long id)
        {
            if (Plugin.connection.session == null)
            {
                return null;
            }
            var shopItems = saveData.shopItemsScouted;

            /*foreach (var item in shopItems.Values)
            {
                Debug.Log($"{item.LocationId} : {item.ItemDisplayName}");
            }*/
            
            return shopItems[id];
        }

        //For when the player extracts a Pelly
        public static void AddPellyGathered(string name)
        {
            if (Plugin.connection.session == null)
            {
                return;
            }
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
            if (Plugin.connection.session == null)
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
            if (Plugin.connection.session == null)
            {
                return;
            }
            if (!saveData.monsterSoulsGathered.Contains(name))
            {
                saveData.monsterSoulsGathered.Add(name);
            }
            ES3.Save<APSaveData>(saveKey, saveData, es3Settings);
        }


        public static bool CheckCompletion()
        {
            if (Plugin.connection.session == null)
            {
                return false;
            }
            var locationsChecked = Plugin.connection.session.Locations.AllLocationsChecked;
            var locationsMissing = Plugin.connection.session.Locations.AllMissingLocations;
            
            Debug.Log("CheckComplete");


            //Check if Level Quota is Met
            Debug.Log($"Current Level: {StatsManager.instance.GetRunStatLevel()}\nQuota: {saveData.levelQuota}");
            if (StatsManager.instance.GetRunStatLevel() < saveData.levelQuota)
            {
                Debug.Log($"Level Quota not met");
                return false;
            }

            var pellys = saveData.pellysRequired;
            
            //Check if Pelly Hunt is Complete
            Debug.Log("Pellys Required:");
            foreach(var pelly in saveData.pellysRequired)
            {
                Debug.Log($"-{pelly.ToString()} {saveData.pellysRequired.Count}");
            }
            Debug.Log("Pellys Gathered:");
            foreach (string pelly in saveData.pellysGathered)
            {
                Debug.Log($"-{pelly}");
            }

            foreach (long locID in locationsMissing)
            {
                string locName = Plugin.connection.session.Locations.GetLocationNameFromId(locID);
                if (locName.Contains("Pelly"))
                {
                    Debug.Log("Missing " + locName);
                }
            }

            foreach (string pelly in pellys)
            {
                foreach(string level in LocationNames.all_levels_short)
                {
                    if (!saveData.pellysGathered.Exists(x => x.Contains(level) && x.Contains(pelly)))
                    {
                        Debug.Log($"Pelly hunt not complete.");
                        return false;
                    }
                }
            }

            //Check if Monster Hunt is complete
            if(saveData.monsterHunt)
            {
                bool monsterHuntComplete = true;
                Debug.Log("Monster Hunt");
                foreach(var soul in LocationNames.all_monster_souls)
                {
                    if (!saveData.monsterSoulsGathered.Contains(soul))
                    {
                        Debug.Log($"{soul} has not been extracted");
                        monsterHuntComplete = false;
                    }
                    else
                    {
                        Debug.Log($"{soul} hunted");
                    }
                }

                if(monsterHuntComplete == false)
                {
                    return false;
                }
            }

            //Check if Valuable Hunt is complete
            if(saveData.valuableHunt)
            {
                bool valuableHuntComplete = true;
                Debug.Log("Valuable Hunt");
                foreach (var valuable in LocationNames.all_valuables)
                {
                    if (!saveData.valuablesGathered.Contains(valuable))
                    {
                        Debug.Log($"{valuable} has not been extracted");
                        valuableHuntComplete = false;
                    }
                    else
                    {
                        Debug.Log($"{valuable} extracted");
                    }
                }

                if (valuableHuntComplete == false)
                {
                    return false;
                }
            }

            //If nothing else fails
            Debug.Log("All Goals Complete.");
            return true;
        }
    }

    


    class APConnectionData
    {
        string address = "archipelago.gg";
        string port = "";
        string password = "";
        string slot = "";
    }
}
