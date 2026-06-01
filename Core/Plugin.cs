using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Photon.Pun;
using RepoAP.Core;
using UnityEngine;

namespace RepoAP
{
    [BepInPlugin("Automagic.ArchipelagoREPO", "Archipelago Randomizer", "0.4.3")]
    [BepInDependency("nickklmao.menulib")]
    [BepInDependency("REPOLib")]

    public class Plugin : BaseUnityPlugin
    {

        internal new static ManualLogSource Logger = null!;

        public static ArchipelagoConnection connection;
        public static Task reconnectTask = null;
        public static PlayerController _player;
        public static CustomRPCs customRPCManager;
        public static GameObject customRPCManagerObject;


        //Conection Info
        public static string apAddress;
        public static string apPort;
        public static string apPassword;
        public static string apSlot;


        //Item tracking
        public static int LastShopItemChecked = 0;  // this is never read and I don't know why it exists
        public static List<int> ShopItemsBought = new List<int>();
        public static List<int> ShopItemsAvailable = new List<int>();

        internal static PluginConfig BoundConfig { get; private set; } = null!;

        private void Awake()
        {
            Logger = base.Logger;

            _player = PlayerController.instance;

            // Config
            BoundConfig = new PluginConfig(base.Config);
            apAddress = BoundConfig.APServerAddress.Value;
            apPort = BoundConfig.APServerPort.Value;
            apPassword = BoundConfig.APPassword.Value;
            apSlot = BoundConfig.APSlotName.Value;

            // Plugin startup logic
            Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
            var harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
            harmony.PatchAll();
            harmony.PatchAll(typeof(DeathLinkPatch));
            harmony.PatchAll(typeof(UpgradeSpawningPatch));
            harmony.PatchAll(typeof(TrapHandler));

            REPOLib.BundleLoader.OnAllBundlesLoaded += FixAPStoreItems;
            REPOLib.BundleLoader.OnAllBundlesLoaded += Initialize;

            DebugCommands.RegisterDebugCommands();
        }
        internal static void Initialize()
        {
            Logger.LogDebug("In Initialize");
            connection = new ArchipelagoConnection();
            ItemData.CreateItemDataTable();
            customRPCManagerObject = new GameObject("RepoAPCustomRPCManager")
            {
                hideFlags = HideFlags.HideAndDontSave,
            };
            customRPCManagerObject.SetActive(false);
            customRPCManagerObject.AddComponent<PhotonView>();
            customRPCManager = customRPCManagerObject.AddComponent<CustomRPCs>();
            DontDestroyOnLoad(customRPCManager);
            string myPrefabId = $"{MyPluginInfo.PLUGIN_GUID}/{customRPCManagerObject.name}";
            customRPCManagerObject.GetComponent<PhotonView>().ViewID = myPrefabId.GetHashCode();
            // this line is necessary to set the PhotonView ID to something unique (unless we find a way to do it dynamically and ensure all clients get the same ID)
            Plugin.Logger.LogDebug($"customRPCManagerObject has ViewID {customRPCManagerObject.GetComponent<PhotonView>().ViewID}");

            // I'm not sure if this is necessary, but it doesn't seem to hurt
            REPOLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(myPrefabId, Plugin.customRPCManagerObject);
            /*if (REPOLib.Modules.NetworkPrefabs.TryGetNetworkPrefabRef($"{MyPluginInfo.PLUGIN_GUID}/RepoAPCustomRPCManager", out PrefabRef registeredNetworkPrefab))
            {
                Plugin.Logger.LogInfo($"prefab is null? {registeredNetworkPrefab.Prefab == null}");
                customRPCManagerObject = REPOLib.Modules.NetworkPrefabs.SpawnNetworkPrefab(registeredNetworkPrefab, Vector3.zero, Quaternion.identity);
                customRPCManager = customRPCManagerObject?.GetComponent<CustomRPCs>();
            }*/
        }

        private static void FixAPStoreItems()
        {
            // it's one field referenced one time. not worth publicizing the whole mod
            List<Item> itemsToRegister = (List<Item>)AccessTools.Field(typeof(REPOLib.Modules.Items), "_itemsToRegister").GetValue(null);
            foreach (var item in itemsToRegister)
            {
                if (item?.itemName == "Archipelago Item")
                {
                    if (!item.prefab.Prefab.TryGetComponent<ItemAttributes>(out ItemAttributes attributes)) continue;
                    attributes.gameObject.AddComponent<ItemRPCs>();
                    Logger.LogDebug("Assigned ItemRPCs to the Archipelago Item prefab");
                }
            }
        }

        public static ArchipelagoConnection GetConnection()
        {
            return connection;
        }

        public void CheckLocation(long locID)
        {
            connection.ActivateCheck(locID);
        }

        public static void ProcessItems()
        {
            //Debug.Log("Update");
            if (!connection.connected) return;
            connection.checkItemsReceived?.MoveNext();
            connection.incomingItemHandler?.MoveNext();
            connection.outgoingItemHandler?.MoveNext();
            connection.messageHandler?.MoveNext();
        }

        public static void UpdateAPAddress(string input)
        {
            apAddress = input;
        }

    }

    [HarmonyPatch(typeof(RunManager))]
    internal static class RunManagerPatch
    {
        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        public static void UpdatePatch()
        {
            if (Plugin.connection == null) return;
            Plugin.ProcessItems();
        }
    }
}
