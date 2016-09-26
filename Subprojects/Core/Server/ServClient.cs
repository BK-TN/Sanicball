using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;

namespace SanicballCore.Server
{
    public class ServClient
    {
        public Guid Guid { get; }
        public string Name { get; }

        public NetConnection Connection { get; }

        public bool CurrentlyLoadingStage { get; set; }
        public bool WantsToReturnToLobby { get; set; }

        public ServClient(Guid guid, string name, NetConnection connection)
        {
            Guid = guid;
            Name = name;
            Connection = connection;
        }
    }
}