using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;
using UnityEngine;

namespace LidgrenTestLobby
{
    class Player
    {
        public string Name;

        public Vector2 Position;
        public Vector2 Velocity;
        public bool Grounded;


        public Player(int clientId)
        {
            Name = "Player from client: " + clientId;
            Position = new Vector2(40, 150);
        }

        public void SetMovement(Vector2 position, bool grounded)
        {
            Position = position;
            //Omit velocity for now
            //Velocity = velocity;
            Grounded = grounded;
        }



    }
}
