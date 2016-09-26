namespace SanicballCore.MatchMessages
{
    public class PlayerJoinedMessage : MatchMessage
    {
        public System.Guid ClientGuid { get; private set; }
        public ControlType CtrlType { get; private set; }
        public int InitialCharacter { get; private set; }

        public PlayerJoinedMessage(System.Guid clientGuid, ControlType ctrlType, int initialCharacter)
        {
            ClientGuid = clientGuid;
            CtrlType = ctrlType;
            InitialCharacter = initialCharacter;
        }
    }
}