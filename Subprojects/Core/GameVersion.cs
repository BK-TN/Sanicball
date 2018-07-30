namespace SanicballCore
{
    public class GameVersion
    {
        //The current version as a float, for checking which version is newest
        public const float AS_FLOAT = 0.82f;

        //To differentiate between testing builds and release builds
        public const bool IS_TESTING = false;

        //As a string, for displaying on the UI
        public const string AS_STRING = "alpha v0.8.2";

        //Something stupid, usually unique for every version
        public const string TAGLINE = "The best failed early access game";
    }
}