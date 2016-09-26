namespace SanicballCore.MatchMessages
{
    public class RaceFinishedMessage : MatchMessage
    {
        public System.Guid ClientGuid { get; private set; }
        public ControlType CtrlType { get; private set; }
        public float RaceTime { get; private set; }
        public int RacePosition { get; private set; }

        public RaceFinishedMessage(System.Guid clientGuid, ControlType ctrlType, float raceTime, int racePosition)
        {
            ClientGuid = clientGuid;
            CtrlType = ctrlType;
            RaceTime = raceTime;
            RacePosition = racePosition;
        }
    }
}