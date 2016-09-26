namespace SanicballCore
{
    public class ClientInfo
    {
        public float Version { get; private set; }
        public bool IsTesting { get; private set; }

        public ClientInfo(float version, bool isTesting)
        {
            Version = version;
            IsTesting = isTesting;
        }
    }
}