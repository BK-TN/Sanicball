using System;
using System.Collections;
using UnityEngine;

namespace Sanicball.Logic
{
    //Used as response when a client sends a server a discovery request.
    public struct ServerInfo
    {
        public ServerConfig Config { get; set; }
        public int Players { get; set; }
        public bool InRace { get; set; }
    }
}
