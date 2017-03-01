using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Lidgren.Network;
using LudimoLibrary.Core.World;
using LudimoLibrary.Util.IO;

namespace LidgrenTestLobby
{
    class Room
    {
        public int Id;
        public List<Client> Clients;
        public string Name = "TestRoom";
        public List<string> RoomConsole;
        public TextBox LogOutputBox;

        public WorldData WorldData;

        private bool _roomAlive;
        private Task _roomLoopTask;

        public Room()
        {
            Clients = new List<Client>();
            RoomConsole = new List<string>();

            _roomLoopTask = new Task(RunRoomLoop);
            _roomLoopTask.Start();

            WorldData = FileManager.Load<WorldData>("saves/test5.world");
        }

        public void DestroyRoom()
        {
            _roomAlive = false;
            _roomLoopTask?.Wait();
        }

        private async void RunRoomLoop()
        {
            int sendRate = 1000 / 66; // 1 sec = 1000ms as Sleep uses ms.

            for (_roomAlive = true; _roomAlive; await Task.Delay(sendRate))
            {
                
            }
        }

        public void ClientEntersRoom(Client client)
        {
            //Add client to this room and let the client know he has joined this specific room
            Clients.Add(client);
            client.JoinRoom(this);

            //Send back room Id for verification and the WorldData for room settings and static world information
            NetOutgoingMessage outgoingMessage = LobbyManager.Server.CreateMessage();
            outgoingMessage.Write((byte)PacketTypes.EnterRoom);
            outgoingMessage.Write(Id);
            byte[] worldDataArray = FileManager.ObjectToByteArray(WorldData);
            outgoingMessage.Write(worldDataArray.Length);
            outgoingMessage.Write(worldDataArray);
            LobbyManager.Server.SendMessage(outgoingMessage, client.Connection, NetDeliveryMethod.ReliableOrdered, 1);

            foreach (Client roomClient in Clients.Where(c => c != client))
            {
                outgoingMessage = LobbyManager.Server.CreateMessage();
                outgoingMessage.Write((byte)PacketTypes.AddPlayer);
                outgoingMessage.Write(roomClient.Id);
                outgoingMessage.Write(roomClient.Player.Name);
                outgoingMessage.Write(roomClient.Player.Position);
                LobbyManager.Server.SendMessage(outgoingMessage, client.Connection, NetDeliveryMethod.ReliableOrdered, 7);
            }
            
            //Send back nice room welcome message
            outgoingMessage = LobbyManager.Server.CreateMessage();
            outgoingMessage.Write((byte)PacketTypes.Message);
            outgoingMessage.Write("You (Client: " + client.Id + ") now got into a room, welcome to room number " + Id + ".");
            LobbyManager.Server.SendMessage(outgoingMessage, client.Connection, NetDeliveryMethod.ReliableOrdered, 0);

            ConsoleWrite("New player has joined the room: Client " + client.Id + ".");
        }

        public void ClientLeavesRoom(Client client)
        {
            //Remove client from this room and let the client know he has left his room
            Clients.Remove(client);
            client.LeaveRoom();

            //Send back nice room goodbye message
            NetOutgoingMessage outgoingMessage = LobbyManager.Server.CreateMessage();
            outgoingMessage.Write((byte)PacketTypes.Message);
            outgoingMessage.Write("You (Client: " + client.Id + ") just left the room nr " + Id + ". Byebye!");
            LobbyManager.Server.SendMessage(outgoingMessage, client.Connection, NetDeliveryMethod.ReliableOrdered, 0);

            ConsoleWrite("A player has left the room: Client " + client.Id + ".");
        }

        public void ConsoleWrite(string message)
        {
            //Add message to pool
            RoomConsole.Add(message);

            //Update message in given LogOutputBox
            LogOutputBox?.Dispatcher.BeginInvoke(new Action(() =>
            {
                LogOutputBox.AppendText(message + "\r\n");
            }));
        }

        public override string ToString()
        {
            return Name + " ID: " + Id + " PlayerCount: " + Clients.Count;
            
        }
    }
}
