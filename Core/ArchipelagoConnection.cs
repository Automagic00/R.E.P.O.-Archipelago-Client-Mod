using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using UnityEngine;

namespace RepoAP
{
    public class ArchipelagoConnection
    {
        
        public ArchipelagoSession session;
        public IEnumerator<bool> incomingItemHandler;
        public IEnumerator<bool> outgoingItemHandler;
        public IEnumerator<bool> checkItemsReceived;
        public IEnumerator<bool> messageHandler;

        private float messageDelay = 0;
        //private float messageTimeStamp = Time.;

        public bool sentCompletion = false;
        public bool sentRelease = false;
        public bool sentCollect = false;

        public Dictionary<string, object> slotData;
        public DeathLinkService deathLinkService;
        public int ItemIndex = 0;
        private ConcurrentQueue<(ItemInfo NetworkItem, int index)> incomingItems;
        private ConcurrentQueue<ItemInfo> outgoingItems;
        private ConcurrentQueue<messageData> messageItems;

        private struct messageData
        {
            public messageData(string m, UnityEngine.Color fc, UnityEngine.Color mc, float t)
            {
                message = m;
                flashCol = fc;
                mainCol = mc;
                time = t;
            }
            public string message {get;}
            public UnityEngine.Color flashCol { get; }
            public UnityEngine.Color mainCol { get; }
            public float time { get; }

        }


        public bool connected
        {
            get { return session != null ? session.Socket.Connected : false; }
        }

        public async void TryConnect(string adress, int port, string pass, string player)
        {
            Debug.Log("TryConnect");
            if (connected)
            {
                Debug.Log("Returning");
                return;
            }
            
            TryDisconnect();

            LoginResult result;

            if (session == null)
            {
                try
                {
                    session = ArchipelagoSessionFactory.CreateSession(adress, port);
                    Debug.Log("Session at " + session.ToString());
                }
                catch
                {
                    Debug.Log("Failed to create archipelago session!");
                }
            }

            incomingItemHandler = IncomingItemHandler();
            outgoingItemHandler = OutgoingItemHandler();
            checkItemsReceived = CheckItemsReceived();
            messageHandler = MessageHandler();
            incomingItems = new ConcurrentQueue<(ItemInfo NetworkItem, int index)>();
            outgoingItems = new ConcurrentQueue<ItemInfo>();
            messageItems = new ConcurrentQueue<messageData>();


            try
            {
                await session.ConnectAsync();
                result = await session.LoginAsync("R.E.P.O", player, ItemsHandlingFlags.AllItems, requestSlotData: true, password: pass);
                //result = session.TryConnectAndLogin("R.E.P.O", player, ItemsHandlingFlags.AllItems, requestSlotData: true, password: pass);
            }
            catch (Exception e)
            {
                result = new LoginFailure(e.GetBaseException().Message);
            }
            if (result is LoginSuccessful LoginSuccess)
            {

                slotData = LoginSuccess.SlotData;

                Debug.Log("Successfully connected to Archipelago Multiworld server!");
                APSave.Init();
                APSave.ScoutShopItems();

                //Send a message if in a gameplay level
                if (!SemiFunc.MenuLevel())
                {
                    messageData md = new messageData($"Successfully Connected!", UnityEngine.Color.white, UnityEngine.Color.green, 3f);


                    messageItems.Enqueue(md);
                }

                deathLinkService = session.CreateDeathLinkService();

               /* deathLinkService.OnDeathLinkReceived += (deathLinkObject) =>
                {
                    if (SceneManager.GetActiveScene().name != "TitleScreen" && _player != null && !_player.dead && !DeathLinkPatch.isDeathLink )
                    {
                        //Debug.Log("Death link received");
                        DeathLinkPatch.deathMsg = deathLinkObject.Cause == null ? $"{deathLinkObject.Source} died. Point and laugh." : $"{deathLinkObject.Cause}";
                        DeathLinkPatch.isDeathLink = true;

                    }
                };*/


                /*if ((bool)Plugin.connection.slotData["death_link"])
                {
                    deathLinkService.EnableDeathLink();
                }*/

                //SetupDataStorage();

            }
            else
            {
                LoginFailure loginFailure = (LoginFailure)result;
                Debug.Log("Error connecting to Archipelago:");
                //Notifications.Show($"\"Failed to connect to Archipelago!\"", $"\"Check your settings and/or log output.\"");
                foreach (string Error in loginFailure.Errors)
                {
                    Debug.Log(Error);
                }
                foreach (ConnectionRefusedError Error in loginFailure.ErrorCodes)
                {
                    Debug.Log(Error.ToString());
                }
                TryDisconnect();
            }
        }


        public void TryDisconnect()
        {
            try
            {
                if (session != null)
                {
                    session.Socket.DisconnectAsync();
                    session = null;
                }

                //incomingItemHandler = null;
                //outgoingItemHandler = null;
                //checkItemsReceived = null;
                incomingItems = new ConcurrentQueue<(ItemInfo NetworkItem, int ItemIndex)>();
                outgoingItems = new ConcurrentQueue<ItemInfo>();
                deathLinkService = null;
                slotData = null;
                ItemIndex = 0;
                //Locations.CheckedLocations.Clear();
                //ItemLookup.ItemList.Clear();

                Debug.Log("Disconnected from Archipelago");
            }
            catch
            {
                Debug.Log("Encountered an error disconnecting from Archipelago!");
            }
        }

        public void ClientDisconnected()
        {
            try
            {
                messageData md = new messageData($"Client Disconnected! Trying to Reconnect...", UnityEngine.Color.white, UnityEngine.Color.red, 4f);
                TryConnect(Plugin.apAdress, int.Parse(Plugin.apPort), Plugin.apPassword, Plugin.apSlot);
            }
            catch(Exception e)
            {
                Debug.Log("Failure in reconnecting: " + e.Message);
            }
        }

        public void ActivateCheck(long locationID)
        {
            Debug.Log("Checked Location " + locationID);
            session.Locations.CompleteLocationChecksAsync(locationID);


            Debug.Log("TrySave");
            APSave.AddLocationChecked(locationID);
            
            Debug.Log("TrySync");
            session.Locations.ScoutLocationsAsync(locationID)
                .ContinueWith(locationInfoPacket =>
                {
                    foreach (ItemInfo itemInfo in locationInfoPacket.Result.Values)
                    {
                        outgoingItems.Enqueue(itemInfo);
                    }
                });
        }
        
        public void SyncLocations()
        {
            int serverLocCount = session.Locations.AllLocationsChecked.Count;
            Dictionary<string, int> clientLocCount = StatsManager.instance.dictionaryOfDictionaries["archipelago items sent to other players"];

            if (serverLocCount != clientLocCount.Count)
            {
                Debug.Log("Locations Unsynced, resyncing...");
                Dictionary<string,int> clientLocs = StatsManager.instance.dictionaryOfDictionaries["Locations Obtained"];
                Debug.Log("Server: " + serverLocCount + "\nClient Count: " + clientLocCount + "\nClient Raw: " + clientLocs.Count);

                /*foreach (string location in clientLocs)
                {
                    ActivateCheck(long.Parse(location));
                }*/
            }
        }

        public void ScoutLocation(long id)
        {
            if (session != null)
            {
                session.Locations.ScoutLocationsAsync(id);
            }
        }

        public string GetLocationName(long id)
        {
            string locationName = session.Locations.GetLocationNameFromId(id);
            return locationName;
        }

        public long GetLocationID(string name)
        {
            long id = session.Locations.GetLocationIdFromName("Another Crabs Treasure", name);
            return id;
        }

        public string GetItemName(long id)
        {
            string name = session.Items.GetItemName(id) ?? $"Item: {id}";
            return name;
        }

        private IEnumerator<bool> CheckItemsReceived()
        {
            while (connected)
            {
                if (session.Items.AllItemsReceived.Count > ItemIndex)
                {
                    //NetworkItem Item = session.Items.AllItemsReceived[ItemIndex];
                    ItemInfo Item = session.Items.AllItemsReceived[ItemIndex];
                    string ItemReceivedName = Item.ItemName;
                    Debug.Log("Placing item " + ItemReceivedName + " with index " + ItemIndex + " in queue.");
                    incomingItems.Enqueue((Item, ItemIndex));
                    ItemIndex++;
                    yield return true;
                }
                else
                {
                    yield return true;
                    continue;
                }
            }
        }
        private IEnumerator<bool> MessageHandler()
        {
            while (connected)
            {
                messageDelay -= Time.deltaTime;
                if (!messageItems.TryDequeue(out var messageData) || messageDelay > 0)
                {
                    yield return true;
                    continue;
                }

                messageDelay = 3.5f;
                Plugin.customRPCManager.CallFocusTextRPC(messageData.message, messageData.mainCol, messageData.flashCol, messageData.time, Plugin.customRPCManagerObject);
                yield return true;
            }
        }
        private IEnumerator<bool> OutgoingItemHandler()
        {
            while (connected)
            {
                if (!outgoingItems.TryDequeue(out var networkItem))
                {
                    yield return true;
                    continue;
                }

                var itemName = networkItem.ItemName;
                var location = networkItem.LocationName;
                var locID = networkItem.LocationId;
                var receiver = session.Players.GetPlayerName(networkItem.Player);

                Debug.Log("Sent " + itemName + " at " + location + " for " + receiver);

                if (networkItem.Player != session.ConnectionInfo.Slot)
                {
                    
                    //CrabFile.current.SetInt("archipelago items sent to other players", CrabFile.current.GetInt("archipelago items sent to other players") + 1);
                    //CrabFile.current.SetString("Locations Obtained", CrabFile.current.GetString("Locations Obtained") + locID + ",");
                    
                }


                yield return true;
            }
        }

        private IEnumerator<bool> IncomingItemHandler()
        {
            //Debug.Log("InItemHandler");
            while (connected)
            {

                if (!incomingItems.TryPeek(out var pendingItem))
                {
                    yield return true;
                    continue;
                }

                var networkItem = pendingItem.NetworkItem;
                var itemName = networkItem.ItemName;

                var itemDisplayName = itemName + " (" + networkItem.ItemName + ") at index " + pendingItem.index;

                if (APSave.GetItemReceivedIndex() > pendingItem.index)
                {
                    incomingItems.TryDequeue(out _);
                    //TunicRandomizer.Tracker.SetCollectedItem(itemName, false);
                    Debug.Log("Skipping item " + itemName + " at index " + pendingItem.index + " as it has already been processed.");
                    yield return true;
                    continue;
                }

                //CrabFile.current.SetInt($"randomizer processed item index {pendingItem.index}", 1);
                Debug.Log("ItemHandler " + networkItem.ItemId);
                APSave.AddItemReceived(networkItem.ItemId);

                List<Level> nonGameLevels = new List<Level> { RunManager.instance.levelMainMenu, RunManager.instance.levelLobby, RunManager.instance.levelLobbyMenu };

                //Make sure player isn't in a non-game Level
                if (!nonGameLevels.Contains( RunManager.instance.levelCurrent))
                {
                    ItemData.AddItemToInventory(networkItem.ItemId,false);

                    messageData md = new messageData($"Recieved {itemName}", UnityEngine.Color.green, UnityEngine.Color.white, 3f);


                    messageItems.Enqueue(md);
                    //Plugin.customRPCManager.CallFocusTextRPC($"Received {itemName}", Plugin.customRPCManagerObject);
                }

                //ItemSwapData.GetItem(networkItem.ItemId);
                incomingItems.TryDequeue(out _);

                yield return true;
            }
        }

        public void SendCompletion()
        {
            StatusUpdatePacket statusUpdatePacket = new StatusUpdatePacket();
            statusUpdatePacket.Status = ArchipelagoClientState.ClientGoal;
            session.Socket.SendPacket(statusUpdatePacket);
            //UpdateDataStorage("Reached an Ending", true);
        }

        public void Release()
        {
            if (connected && sentCompletion && !sentRelease)
            {
                session.Socket.SendPacket(new SayPacket() { Text = "!release" });
                sentRelease = true;
                Debug.Log("Released remaining checks.");
            }
        }

        public void Collect()
        {
            if (connected && sentCompletion && !sentCollect)
            {
                session.Socket.SendPacket(new SayPacket() { Text = "!collect" });
                sentCollect = true;
                Debug.Log("Collected remaining items.");
            }
        }

        public void SendDeathLink()
        {
            if (connected)
            {
                deathLinkService.SendDeathLink(new DeathLink(session.Players.ActivePlayer.Name));
            }
        }

    }
}
