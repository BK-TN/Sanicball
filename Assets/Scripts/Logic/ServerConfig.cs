using System;
using System.Collections;
using UnityEngine;

namespace Sanicball.Logic
{
    //Used as response when a client sends a server a discovery request.
    public struct ServerConfig
    {
        public string ServerName { get; set; }
        public bool ShowInBrowser { get; set; }
        public int PrivatePort { get; set; }
        public string PublicIP { get; set; }
        public int PublicPort { get; set; }
        public int MaxPlayers { get; set; }
    }
}