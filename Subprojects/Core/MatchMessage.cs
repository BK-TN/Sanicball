namespace SanicballCore
{
    public delegate void MatchMessageHandler<T>(T message, float travelTime) where T : MatchMessage;

    public abstract class MatchMessage
    {
        protected bool reliable = true;

        public bool Reliable { get { return reliable; } }
    }
}