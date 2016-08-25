using System;
using System.Collections;
using UnityEngine;

namespace Sanicball.Logic
{
    //Used as response when a client sends a server a discovery request.
    public class ServerInfo
    {
        public DateTime Timestamp { get; private set; }
        public string ServerName { get; private set; }
        public int Players { get; private set; }
        public int MaxPlayers { get; private set; }
        public bool InRace { get; private set; }

        public ServerInfo(DateTime timestamp, string serverName, int players, int maxPlayers, bool inRace)
        {
            Timestamp = timestamp;
            ServerName = serverName;
            Players = players;
            MaxPlayers = maxPlayers;
            InRace = inRace;
        }
    }
}