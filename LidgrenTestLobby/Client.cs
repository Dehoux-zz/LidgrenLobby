using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lidgren.Network;
using UnityEngine;

namespace LidgrenTestLobby
{
    class Client
    {
        public int Id { get; }
        public NetConnection Connection { get; }
        public int LastBeat;
        public double KeepAlive = 0, LastKeepAlive = 0;
        public Room JoinedRoom;

        public Player Player;

        public Client(int id, NetConnection connection, int curBeat)
        {
            Id = id;
            Connection = connection;
            LastBeat = curBeat;

            AssignIdToClient();
        }

        //public Player GetPlayer()
        //{
        //    return Player ?? (Player = new Player(Id));
        //}

        public void JoinRoom(Room joinedRoom)
        {
            JoinedRoom = joinedRoom;
            Player = new Player(Id);
            SentPlayerToOthers();
        }

        public void LeaveRoom()
        {
            JoinedRoom = null;
        }

        /// <summary>
        /// Let the Client know he has been accepted by the server and has been given an ID.
        /// </summary>
        private async void AssignIdToClient()
        {
            //Wait a moment before sending to client, client may not be ready yet <--- tricky code
            await Task.Delay(2000);

            NetOutgoingMessage outgoingMessage = LobbyManager.Server.CreateMessage();
            outgoingMessage.Write((byte)PacketTypes.AssignId);
            outgoingMessage.Write(Id);
            LobbyManager.Server.SendMessage(outgoingMessage, Connection, NetDeliveryMethod.ReliableOrdered, 1);

            outgoingMessage = LobbyManager.Server.CreateMessage();
            outgoingMessage.Write((byte)PacketTypes.Message);
            outgoingMessage.Write("You (Client: " + Id + ") now got an ID, welcome on the Server.");
            LobbyManager.Server.SendMessage(outgoingMessage, Connection, NetDeliveryMethod.ReliableOrdered, 0);
        }

        public void StatusChange(NetIncomingMessage incomingMessage)
        {
            NetConnectionStatus status = (NetConnectionStatus)incomingMessage.ReadByte();
            string reason = incomingMessage.ReadString();

            if (Connection.Status == NetConnectionStatus.Disconnected || Connection.Status == NetConnectionStatus.Disconnecting)
            {
                LobbyManager.Instance.ManageDisonnectionClient(this);
            }

            Console.WriteLine("Client id " + Id + "; status changed to " + status + " (" + reason + ") " + ".");
        }


        /// <summary>
        /// Send startplayer information to other players
        /// </summary>
        public void SentPlayerToOthers()
        {
            Console.WriteLine("Send Player to all Players");

            foreach (Client client in JoinedRoom.Clients.Where(c => c != this))
            {
                NetOutgoingMessage outgoingMessage = LobbyManager.Server.CreateMessage();
                outgoingMessage.Write((byte)PacketTypes.AddPlayer);
                outgoingMessage.Write(Id);
                outgoingMessage.Write(Player.Name);
                outgoingMessage.Write(Player.Position);
                LobbyManager.Server.SendMessage(outgoingMessage, client.Connection, NetDeliveryMethod.ReliableOrdered, 7);
            }
        }

        public void HandlePlayerMovement(NetIncomingMessage incomingMessage)
        {
            Vector2 playerPosition = incomingMessage.ReadVector2();
            bool playerGrounded = incomingMessage.ReadBoolean();
            Player.SetMovement(playerPosition, playerGrounded);

            //Console.WriteLine("Name: " + Player.Name + " Position: " + playerPosition + " Velocity: " + playerVelocity + " Grounded: " + playerGrounded);
            foreach (Client client in JoinedRoom.Clients.Where(c => c != this))
            {
                //Console.WriteLine("Sending player movement info");

                NetOutgoingMessage outgoingMessage = LobbyManager.Server.CreateMessage();
                outgoingMessage.Write((byte)PacketTypes.PlayerMovement);
                outgoingMessage.Write((Int16)Id);
                outgoingMessage.Write(Player.Position);
                outgoingMessage.Write(Player.Grounded);
                //outgoingMessage.Write(player.Connection.AverageRoundtripTime / 2f);
                LobbyManager.Server.SendMessage(outgoingMessage, client.Connection, NetDeliveryMethod.UnreliableSequenced, 10);
            }
        }

        public void HandlePlayerJump()
        {
            foreach (Client client in JoinedRoom.Clients)
            {
                NetOutgoingMessage outgoingMessage = LobbyManager.Server.CreateMessage();
                outgoingMessage = LobbyManager.Server.CreateMessage();
                outgoingMessage.Write((byte)PacketTypes.PlayerJump);
                outgoingMessage.Write((Int16)Id);
                LobbyManager.Server.SendMessage(outgoingMessage, client.Connection, NetDeliveryMethod.ReliableUnordered, 11);

            }
        }
    }
}
