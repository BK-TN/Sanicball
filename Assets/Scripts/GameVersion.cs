using UnityEngine;

namespace Sanicball
{
    public class GameVersion : MonoBehaviour
    {
        //The current version as a float, for checking which version is newest
        public const float AS_FLOAT = 0.72f;

        //To differentiate between testing builds and release builds
        public const bool IS_TESTING = true;

        //As a string, for displaying on the UI
        public const string AS_STRING = "alpha v0.7.2";

        //Something stupid, usually unique for every version
        public const string TAGLINE = "& MULTIPLAYER";
    }
}