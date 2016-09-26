namespace SanicballCore
{
    public class GameVersion
    {
        //The current version as a float, for checking which version is newest
        public const float AS_FLOAT = 0.73f;

        //To differentiate between testing builds and release builds
        public const bool IS_TESTING = false;

        //As a string, for displaying on the UI
        public const string AS_STRING = "alpha v0.7.3";

        //Something stupid, usually unique for every version
        public const string TAGLINE = "YOU'RE WINNER";
    }
}