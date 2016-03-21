using System.Collections.Generic;
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

    public class MatchSettings
    {
        [Newtonsoft.Json.JsonProperty]
        private int[] aiCharacters = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };

        public MatchSettings()
        {
            Laps = 2;
            StageId = 0;
            AICount = 7;
            AISkill = AISkillLevel.Average;
        }

        public void CopyValues(MatchSettings original)
        {
            Laps = original.Laps;
            StageId = original.StageId;
            AICount = original.AICount;
            AISkill = original.AISkill;
            aiCharacters = (int[])original.aiCharacters.Clone();
        }

        public int StageId { get; set; }
        public int Laps { get; set; }
        public int AICount { get; set; }
        public AISkillLevel AISkill { get; set; }

        public int GetAICharacter(int pos)
        {
            if (pos >= 0 && pos < aiCharacters.Length)
            {
                return aiCharacters[pos];
            }
            else
            {
                return 1;
            }
        }

        public void SetAICharacter(int pos, int characterId)
        {
            if (pos >= 0 && pos < aiCharacters.Length)
            {
                aiCharacters[pos] = characterId;
            }
        }
    }
}