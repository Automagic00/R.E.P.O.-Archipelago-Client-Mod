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
        public List<int> shopItemsPurchased = new List<int>();
        public Dictionary<long, int> itemsRecieved = new Dictionary<long, int>();
        public Dictionary<string, bool> levelsUnlocked = new Dictionary<string, bool>();
        public int itemRecievedIndex = 0;
        public Dictionary<long, ItemInfo> shopItemsScouted = new Dictionary<long, ItemInfo>();
        public JArray pellysRequired = new JArray();
        public long levelQuota;
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
            var test1 = Plugin.connection.slotData["pellys_required"];
            var test2 = Plugin.connection.slotData["level_quota"];
            Debug.Log(test1.GetType());
            Debug.Log(test2.GetType());

            saveData.levelQuota = (long)Plugin.connection.slotData["level_quota"];
            Debug.Log("Between");
            saveData.pellysRequired = (JArray)Plugin.connection.slotData["pellys_required"];

            
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

        public static void AddItemRecieved(long itemId)
        {
            if (Plugin.connection.session == null)
            {
                return;
            }
            //If Key Exists, add 1
            if (saveData.itemsRecieved.ContainsKey(itemId))
            {
                saveData.itemsRecieved[itemId]++;
                
            }
            //If not, set to 1
            else
            {
                saveData.itemsRecieved.Add(itemId, 1);
            }

            //Increase Item Index
            saveData.itemRecievedIndex++;

            //Save
            ES3.Save<APSaveData>(saveKey, saveData, es3Settings);
            //Debug.Log("Saved " + itemId);
        }
        public static int GetItemRecievedIndex()
        {
            if (Plugin.connection.session == null)
            {
                return 0;
            }
            return ES3.Load<APSaveData>(saveKey, es3Settings).itemRecievedIndex;
        }
        public static Dictionary<long, int> GetItemsRecieved()
        {
            if (Plugin.connection.session == null)
            {
                return null;
            }
            return ES3.Load<APSaveData>(saveKey, es3Settings).itemsRecieved;
        }

        public static void AddLevelRecieved(string levelName)
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
                Debug.LogError(levelName + " has already been recieved!");
            }

            ES3.Save<APSaveData>(saveKey, saveData, es3Settings);
        }

        public static Dictionary<string,bool> GetLevelsRecieved()
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

        public static bool CheckCompletion()
        {
            if (Plugin.connection.session == null)
            {
                return false;
            }
            Debug.Log("CheckComplete");
            //Check if Level Quota is Met
            Debug.Log($"Current Level: {StatsManager.instance.GetRunStatLevel()}\nQuota: {saveData.levelQuota}");
            if (StatsManager.instance.GetRunStatLevel() < saveData.levelQuota)
            {
                Debug.Log($"Level Quota not met");
                return false;
            }

            var pellys = saveData.pellysRequired;
            List<string> levels = new List<string>() { LocationNames.headman_manor, LocationNames.mcjannek, LocationNames.swiftbroom };
            //Check if Pelly Hunt is Complete
            Debug.Log("\nPellys Required:");
            foreach(var pelly in saveData.pellysRequired)
            {
                Debug.Log($"-{pelly.ToString()} {saveData.pellysRequired.Count}");
            }
            Debug.Log("\nPellys Gathered:");
            foreach (string pelly in saveData.pellysGathered)
            {
                Debug.Log($"-{pelly}");
            }

            foreach (string pelly in pellys)
            {
                foreach(string level in levels)
                {
                    if (!saveData.pellysGathered.Exists(x => x.Contains(level) && x.Contains(pelly)))
                    {
                        Debug.Log($"Pelly hunt not complete.");
                        return false;
                    }
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
