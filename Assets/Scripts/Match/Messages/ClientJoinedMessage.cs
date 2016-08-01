using System.Collections;
using UnityEngine;

namespace Sanicball.Match
{
    public class ClientJoinedMessage : MatchMessage
    {
        public System.Guid ClientGuid { get; private set; }
        public string ClientName { get; private set; }

        public ClientJoinedMessage(System.Guid clientGuid, string clientName)
        {
            ClientGuid = clientGuid;
            ClientName = clientName;
        }
    }
}