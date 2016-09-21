namespace SanicballCore.MatchMessages
{
    public class PlayerLeftMessage : MatchMessage
    {
        public System.Guid ClientGuid { get; private set; }
        public ControlType CtrlType { get; private set; }

        public PlayerLeftMessage(System.Guid clientGuid, ControlType ctrlType)
        {
            ClientGuid = clientGuid;
            CtrlType = ctrlType;
        }
    }
}