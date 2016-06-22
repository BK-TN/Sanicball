using System.Collections;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using UnityEngine;

namespace Sanicball.Data
{
    public class ReadOnlyMatchSettings
    {
        [JsonProperty]
        private readonly MatchSettings target;

        [JsonIgnore]
        public int StageId { get { return target.StageId; } }
        [JsonIgnore]
        public int Laps { get { return target.Laps; } }
        [JsonIgnore]
        public int AICount { get { return target.AICount; } }
        [JsonIgnore]
        public AISkillLevel AISkill { get { return target.AISkill; } }
        [JsonIgnore]
        public ReadOnlyCollection<int> AICharacters { get { return new ReadOnlyCollection<int>(target.AICharacters); } }

        public ReadOnlyMatchSettings(MatchSettings target)
        {
            this.target = target;
        }

        public int GetAICharacter(int pos)
        {
            return target.GetAICharacter(pos);
        }
    }
}