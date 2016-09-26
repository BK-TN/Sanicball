using System;
using System.Collections;

namespace SanicballCore.Server
{
    //Used as response when a client sends a server a discovery request.
    public struct ServerInfo
    {
        public ServerConfig Config { get; set; }
        public int Players { get; set; }
        public bool InRace { get; set; }
    }
}