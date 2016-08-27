using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Sanicball.Data
{
    public enum AISkillLevel
    {
        Retarded,
        Average,
        Dank
    }

    public enum StageRotationMode
    {
        None,
        Sequenced,
        Random
    }

    public struct MatchSettings
    {
        [Newtonsoft.Json.JsonProperty]
        private string aiCharacters;

        public int StageId { get; set; }
        public int Laps { get; set; }
        public int AICount { get; set; }
        public AISkillLevel AISkill { get; set; }

        public int AutoStartTime { get; set; }
        public int AutoStartMinPlayers { get; set; }
        public int AutoReturnTime { get; set; }
        public StageRotationMode StageRotationMode { get; set; }

        /// <summary>
        /// Creates a MatchSettings object with the game's default settings.
        /// </summary>
        /// <returns></returns>
        public static MatchSettings CreateDefault()
        {
            return new MatchSettings()
            {
                StageId = 0,
                Laps = 2,
                AICount = 7,
                AISkill = AISkillLevel.Average,
                aiCharacters = "1,2,3,4,5,6,7,8,9,10,11,12",

                AutoStartTime = 60,
                AutoStartMinPlayers = 2,
                AutoReturnTime = 15,
                StageRotationMode = StageRotationMode.None
            };
        }

        /// <summary>
        /// Gets the AI character ID on a position. Returns default character if out of bounds.
        /// </summary>
        /// <param name="pos">Target position</param>
        /// <returns></returns>
        public int GetAICharacter(int pos)
        {
            string[] charIDs = aiCharacters.Split(',');

            if (pos >= 0 && pos < charIDs.Length)
            {
                return int.Parse(charIDs[pos]);
            }
            else
            {
                //Default to Knackles if trying to get a position out of bounds
                return 1;
            }
        }

        /// <summary>
        /// Sets the AI character ID on a position. Positive numbers only. Increases the list size if setting above current bounds.
        /// </summaryiiiii>
        /// <param name="pos">Target position</param>
        /// <param name="characterId">Character ID to use there</param>
        public void SetAICharacter(int pos, int characterId)
        {
            string[] charIDs = aiCharacters.Split(',');

            if (pos >= 0)
            {
                if (pos >= charIDs.Length)
                {
                    System.Array.Resize(ref charIDs, pos + 1);
                }
                charIDs[pos] = characterId.ToString();
                aiCharacters = string.Join(",", charIDs);
            }
        }

        /// <summary>
        /// Removes the last AI character from the list. Use for reducing the list size to avoid bloat.
        /// </summary>
        public void RemoveLastAICharacter()
        {
            string[] charIDs = aiCharacters.Split(',');
            if (charIDs.Length > 1)
            {
                System.Array.Resize(ref charIDs, charIDs.Length - 1);
                aiCharacters = string.Join(",", charIDs);
            }
        }
    }
}