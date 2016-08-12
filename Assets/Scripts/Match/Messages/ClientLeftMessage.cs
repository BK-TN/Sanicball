using System.Collections;
using UnityEngine;

namespace Sanicball.Match
{
    public class ClientLeftMessage : MatchMessage
    {
        public System.Guid ClientGuid { get; private set; }

        public ClientLeftMessage(System.Guid clientGuid)
        {
            ClientGuid = clientGuid;
        }
    }
}