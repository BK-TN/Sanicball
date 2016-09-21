using System.Collections;
using UnityEngine;

namespace Sanicball.Logic
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