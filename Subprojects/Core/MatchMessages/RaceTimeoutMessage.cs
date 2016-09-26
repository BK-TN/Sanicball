namespace SanicballCore.MatchMessages
{
    public class RaceTimeoutMessage : MatchMessage
    {
        public System.Guid ClientGuid { get; private set; }
        public ControlType CtrlType { get; private set; }
        public float Time { get; private set; }

        public RaceTimeoutMessage(System.Guid clientGuid, ControlType ctrlType, float time)
        {
            ClientGuid = clientGuid;
            CtrlType = ctrlType;
            Time = time;
        }
    }
}