using BepInEx;
using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;
using System.Threading.Tasks;
using BepInEx.Logging;

namespace RepoAP
{
    [BepInPlugin("Automagic.ArchipelagoREPO", "Archipelago Randomizer", "0.3.0")]
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

        //Connection GUI
        public static bool showMenu = true;


        //Conection Info
        public static string apAdress = "archipelago.gg";
        public static string apPort = "";
        public static string apPassword = "";
        public static string apSlot = "";

        /*//Config Entries
        private ConfigEntry<string> apAdressConfig;
        private ConfigEntry<string> apPortConfig;
        private ConfigEntry<string> apPassConfig;
        private ConfigEntry<string> apSlotConfig;*/

        //Item tracking
        public static int LastShopItemChecked = 0;
        public static List<int> ShopItemsBought = new List<int>();
        public static List<int> ShopItemsAvailable = new List<int>();

        internal static PluginConfig BoundConfig { get; private set; } = null!;

        private void Awake()
        {
            Logger = base.Logger;

            /*apAdressConfig = Config.Bind("Archipelago", "Server Adress", "archipelago.gg");
            apPortConfig = Config.Bind("Archipelago", "Server Port", "");
            apPassConfig = Config.Bind("Archipelago", "Server Password", "");
            apSlotConfig = Config.Bind("Archipelago", "Player Slot", "");

            apAdress = apAdressConfig.Value;
            apPort = apPortConfig.Value;
            apPassword = apPassConfig.Value;
            apSlot = apSlotConfig.Value;*/

            _player = PlayerController.instance;
            BoundConfig = new PluginConfig(base.Config);
            // Plugin startup logic
            Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
            var harmony = new Harmony("com.example.patch");
            harmony.PatchAll();
        }
        private void Start()
        {
            Logger.LogDebug("In Start");
            connection = new ArchipelagoConnection();
            customRPCManagerObject = new GameObject("RepoAPCustomRPCManager")
            {
                hideFlags = HideFlags.HideAndDontSave,
            };
            customRPCManagerObject.SetActive(false);
            customRPCManager = customRPCManagerObject.AddComponent<CustomRPCs>();
            customRPCManagerObject.AddComponent<PhotonView>();
            DontDestroyOnLoad(customRPCManager);
            // I'm not sure if these next few lines are necessary, but they don't seem to hurt
            string myPrefabId = $"{MyPluginInfo.PLUGIN_GUID}/{customRPCManagerObject.name}";
            PrefabRef registeredNetworkPrefab = REPOLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(myPrefabId, customRPCManagerObject);
            if (registeredNetworkPrefab != null)
            {
                REPOLib.Modules.NetworkPrefabs.SpawnNetworkPrefab(registeredNetworkPrefab, Vector3.zero, Quaternion.identity);
                Logger.LogInfo("Registered customRPCManagerObject for multiplayer RPCs.");
            }
            else
                Logger.LogError("Failed to register customRPCManagerObject. Multiplayer may be borked.");
            // this line is necessary to set the PhotonView ID to something unique (unless we find a way to do it dunamically and encure all clients get the same ID)
            customRPCManagerObject.GetComponent<PhotonView>().ViewID = myPrefabId.GetHashCode();
            ItemData.CreateItemDataTable();

        }
        public static ArchipelagoConnection GetConnection()
        {
            return connection;
        }

        public void CheckLocation(long locID)
        {
            connection.ActivateCheck(locID);
        }

        public void Update()
        {
            //Debug.Log("Update");
            if (!connection.connected)
            {
                return;
            }
            if (connection.checkItemsReceived != null)
            {
                connection.checkItemsReceived.MoveNext();
            }


            //if (_player != null)
            //{
                //Debug.Log("Try Item");
            if (connection.incomingItemHandler != null)
            {
                connection.incomingItemHandler.MoveNext();
            }

            if (connection.outgoingItemHandler != null)
            {
                connection.outgoingItemHandler.MoveNext();
            }
            if (connection.messageHandler != null)
            {
                connection.messageHandler.MoveNext();
            }
        }

        public static void UpdateAPAddress(string input)
        {
            apAdress = input;
        }

        /*
        public void OnGUI()
        {
            if (showFadingLabel && alphaAmount < 1f)
            {
                alphaAmount += 0.3f * Time.deltaTime;
                GUI.color = new UnityEngine.Color(originalColor.r, originalColor.g, originalColor.b, alphaAmount);
                GUI.Label(new Rect(Screen.width / 2, 40, 200f, 50f), fadingLabelContent);
            }
            else if (alphaAmount >= 1f)
            {
                alphaAmount = 0f;
                GUI.color = originalColor;
                showFadingLabel = false;
            }

            if (showMenu && (SceneManager.GetActiveScene().name == "Title" || SceneManager.GetActiveScene().name == "Pretitle"))
            {
                GUI.backgroundColor = backgroundColor;

                if (windowWidth < 200)
                {
                    windowWidth = 200;
                }

                windowRect = new Rect(0, 0, windowWidth, 150);
                windowRect = GUI.Window(0, windowRect, APConnectMenu, "Archipelago");
            }
        }

        */

        //AP Connection info on Main Menu
        /*void APConnectMenu(int windowID)
        {
            if (showMenu)
            {
                GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                GUILayout.BeginVertical(GUILayout.Width(80), GUILayout.ExpandWidth(true));

                GUILayout.Label("Address");
                GUILayout.Label("Port");
                GUILayout.Label("Password");
                GUILayout.Label("Slot");


                GUILayout.EndVertical();
                GUILayout.BeginVertical(GUILayout.Width(80), GUILayout.ExpandWidth(true));
                apAdress = GUILayout.TextField(apAdress, GUILayout.ExpandWidth(true));
                apPort = GUILayout.TextField(apPort, GUILayout.ExpandWidth(true));
                apPassword = GUILayout.TextField(apPassword, GUILayout.ExpandWidth(true));
                apSlot = GUILayout.TextField(apSlot, GUILayout.ExpandWidth(true));

                if (!connection.connected)
                {
                    if (GUILayout.Button("Connect"))
                    {
                        Debug.Log("Button");
                        connection.TryConnect(apAdress, Int32.Parse(apPort), apPassword, apSlot);
                    }
                }

                GUILayout.Label("Press [Insert] to toggle menu.");
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();

            }
        }*/
    }
}
