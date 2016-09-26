namespace SanicballCore.MatchMessages
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