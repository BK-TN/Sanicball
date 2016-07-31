using System.Collections.Generic;
using UnityEngine;

namespace Sanicball.Data
{
    public enum PlayerType
    {
        Anon = -2,
        Normal = -1,
        Donator = 0,
        Special = 1,
        Developer = 2,
    }

    [System.Serializable]
    public class GameJoltInfo
    {
        public int gameID;
        public string privateKey;
        public bool verbose;
        public bool disabled;

        private Dictionary<string, PlayerType> specialUsers = new Dictionary<string, PlayerType>();

        public void Init()
        {
            if (disabled) return;
            GJAPI.Init(gameID, privateKey, verbose, 1);

            //The following two methods are seperate from this Init() method
            //so that they may later be easily called multiple times, for something like timed login checks

            //Load in special users
            GJAPI.Data.Get("players");
            GJAPI.Data.GetCallback += LoadSpecialUsersCallback;

            //Check if current game jolt info is legit
            string username = ActiveData.GameSettings.gameJoltUsername;
            string token = ActiveData.GameSettings.gameJoltToken;
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(token)) return;

            GJAPI.Users.Verify(username, token);
            GJAPI.Users.VerifyCallback += CheckIfSignedInCallback;
        }

        /// <summary>
        /// Gets the player type of this username. Returns PlayerType.Normal on null/empty username.
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public PlayerType GetPlayerType(string username)
        {
            if (disabled) return PlayerType.Normal;
            PlayerType result;
            if (!string.IsNullOrEmpty(username) && specialUsers.TryGetValue(username, out result))
            {
                return result;
            }
            return PlayerType.Normal;
        }

        public Color GetPlayerColor(PlayerType type)
        {
            switch (type)
            {
                case PlayerType.Anon:
                    return new Color(0.88f, 0.88f, 0.88f);

                case PlayerType.Normal:
                    return Color.white;

                case PlayerType.Developer:
                    return new Color(0.6f, 0.7f, 1);

                case PlayerType.Special:
                    return new Color(0.2f, 0.8f, 0.2f);

                case PlayerType.Donator:
                    return new Color(1, 0.8f, 0.4f);

                default:
                    return Color.white;
            }
        }

        private void LoadSpecialUsersCallback(string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                //Oh shit, the connection failed
                Debug.LogError("Failed to load special player types using GJAPI.");
                GJAPI.Data.GetCallback -= LoadSpecialUsersCallback;
                return;
            }
            string[] pairs = data.Split(';');
            foreach (string s in pairs)
            {
                string[] nameTypePair = s.Split('=');
                if (nameTypePair.Length != 2) continue;
                string nameStr = nameTypePair[0];
                string typeStr = nameTypePair[1];

                int typeInt;
                if (int.TryParse(typeStr, out typeInt))
                {
                    specialUsers.Add(nameStr, (PlayerType)typeInt);
                }
            }
            Debug.Log("Special user list loaded.");
            GJAPI.Data.GetCallback -= LoadSpecialUsersCallback;
        }

        private void CheckIfSignedInCallback(bool isLegit)
        {
            if (!isLegit)
            {
                //Not legit! Remove info
                ActiveData.GameSettings.gameJoltUsername = string.Empty;
                ActiveData.GameSettings.gameJoltToken = string.Empty;
            }
            GJAPI.Users.VerifyCallback -= CheckIfSignedInCallback;
        }
    }
}