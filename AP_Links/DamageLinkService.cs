using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Converters;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Packets;
using HarmonyLib;
using Newtonsoft.Json.Linq;

namespace RepoAP.AP_Links
{
    public class DamageLinkService
    {
        // taking damage will send 1 point per hp lost
        // damage needed to hurt players is 1 per 1hp and capped at 75 per instance

        //
        // Summary:
        //     Creates DamageLinkService.OnDamageLinkReceived
        //     event for clients to hook into and decide what to do with the received DamageLink
        public delegate void DamageLinkReceivedHandler(DamageLink damageLink);

        private readonly IArchipelagoSocketHelper socket;
        private readonly IConnectionInfoProvider connectionInfoProvider;
        private bool _isDamaging = false;

        public bool IsDamaging() => _isDamaging;
        public void SetDamaging(bool value) => _isDamaging = value;

        //
        // Summary:
        //     Delegate event that supplies the created Archipelago.MultiClient.Net.BounceFeatures.DeathLink.DeathLink
        //     whenever one is received from the server as a bounce packet.
        public event DamageLinkReceivedHandler OnDamageLinkReceived;


        public DamageLinkService(IArchipelagoSocketHelper socket, IConnectionInfoProvider connectionInfoProvider)
        {
            this.socket = socket;
            this.connectionInfoProvider = connectionInfoProvider;
            socket.PacketReceived += OnPacketReceived;
        }


        private void OnPacketReceived(ArchipelagoPacketBase packet)
        {
            if (packet is BouncedPacket bouncedPacket && bouncedPacket.Tags.Contains("SharedDamage") && DamageLink.TryParse(bouncedPacket.Data, out var damageLink) && 
                damageLink.UUID != connectionInfoProvider.Uuid)
            {
                this.OnDamageLinkReceived(damageLink);
            }
        }

        //
        // Summary:
        //     Formats and sends a Bounce packet using the provided damageLink info.
        //
        // Parameters:
        //   damageLink:
        //     DamageLink object containing the information of the damage which was taken.
        //
        //
        // Exceptions:
        //   T:Archipelago.MultiClient.Net.Exceptions.ArchipelagoSocketClosedException:
        //     The websocket connection is not alive
        public void SendDamageLink(DamageLink damageLink)
        {

            /*
             * {
                    "cmd": "Bounce", "tags": ["SharedDamage"],
                    "data": {
                        "time": time.time(),
                        "uuid": Utils.get_unique_identifier(),
                        "source": ctx.player_names[ctx.slot],
                        "damage_points": damage_amount
                    }
               }
             * 
             */
            BouncePacket bouncePacket = new BouncePacket
            {
                Tags = new List<string> { "SharedDamage" },
                Data = new Dictionary<string, JToken>
                {
                    {
                        "time",
                        damageLink.Timestamp.ToUnixTimeStamp()
                    },
                    { "uuid", damageLink.UUID},
                    { "source",  damageLink.Source},
                    { "damage_points", damageLink.DamageAmount}
                }
            };

            socket.SendPacketAsync(bouncePacket);
        }

        //
        // Summary:
        //     Adds "SharedDamage" to your Archipelago.MultiClient.Net.ArchipelagoSession's tags
        //     and opts you in to receiving DamageLink packets
        public void EnableDamageLink()
        {
            if (Array.IndexOf(connectionInfoProvider.Tags, "SharedDamage") == -1)
            {
                connectionInfoProvider.UpdateConnectionOptions(connectionInfoProvider.Tags.Concat(new string[1] { "SharedDamage" }).ToArray());
            }
        }

        //
        // Summary:
        //     Removes the "SharedDamage" tag from your Archipelago.MultiClient.Net.ArchipelagoSession
        //     and opts out of further DamageLink packets
        public void DisableDamageLink()
        {
            if (Array.IndexOf(connectionInfoProvider.Tags, "SharedDamage") != -1)
            {
                connectionInfoProvider.UpdateConnectionOptions(connectionInfoProvider.Tags.Where((string t) => t != "SharedDamage").ToArray());
            }
        }


    }


    // Most of this was repurposed from Archipelago.MultiClient.Net's DeathLink class
    public class DamageLink
    {
        //
        // Summary:
        //     The Timestamp of the created DamageLink object
        public DateTime Timestamp { get; internal set; }

        //
        // Summary:
        //     The uuid of the client who sent the DamageLink
        public string UUID { get; }

        //
        // Summary:
        //     The name of the player who sent the DamageLink
        public string Source { get; }

        //
        // Summary:
        //     The amount of damage points taken by the player who sent the DamageLink
        public int DamageAmount { get; }

        //
        // Summary:
        //     A DamageLink object that gets sent and received via bounce packets.
        //
        // Parameters:
        //   sourcePlayer:
        //     Name of the player sending the DamageLink
        //   uuid: 
        //     UUID of the client sending the DamageLink
        //   amount: 
        //     Amount of damage points taken by the player sending the DamageLink
        public DamageLink(string sourcePlayer, string uuid, int amount)
        {
            Timestamp = DateTime.UtcNow;
            UUID = uuid;
            Source = sourcePlayer;
            DamageAmount = amount;
        }

        internal static bool TryParse(Dictionary<string, JToken> data, out DamageLink damageLink)
        {
            try
            {
                if (!data.TryGetValue("time", out var value) || !data.TryGetValue("source", out var value2) || !data.TryGetValue("uuid", out var value3))
                {
                    damageLink = null;
                    return false;
                }

                int amount = 0;
                if (data.TryGetValue("damage_points", out var value4))
                {
                    amount = value4.ToObject<int>();
                }

                damageLink = new DamageLink(value2.ToString(), value3.ToString(), amount)
                {
                    Timestamp = UnixTimeConverter.UnixTimeStampToDateTime(value.ToObject<double>())
                };
                return true;
            }
            catch
            {
                damageLink = null;
                return false;
            }
        }

        public bool Equals(DamageLink other)
        {
            if ((object)other == null)
            {
                return false;
            }

            if ((object)this == other)
            {
                return true;
            }

            if (Source == other.Source && Timestamp.Date.Equals(other.Timestamp.Date) && Timestamp.Hour == other.Timestamp.Hour && Timestamp.Minute == other.Timestamp.Minute)
            {
                return Timestamp.Second == other.Timestamp.Second;
            }

            return false;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (this == obj)
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((DamageLink)obj);
        }

        public override int GetHashCode()
        {
            return (Timestamp.GetHashCode() * 397) ^ ((Source != null) ? Source.GetHashCode() : 0);
        }

        public static bool operator ==(DamageLink lhs, DamageLink rhs)
        {
            return lhs?.Equals(rhs) ?? ((object)rhs == null);
        }

        public static bool operator !=(DamageLink lhs, DamageLink rhs)
        {
            return !(lhs == rhs);
        }
    }
}
