using UnityEngine;

public class GameVersion : MonoBehaviour
{
    //The current version as a float, for checking which version is newest
    public const float AsFloat = 0.7f;

    //Use GUID format to make it truly unique
    public const string UniqueID = "bbe19daf-e58a-4f55-b0df-861ed6685479";

    //As a string, for displaying on the UI
    public const string AsString = "alpha v0.7";

    //Something stupid, usually unique for every version
    public const string Slogan = "& KNACKLES";
}