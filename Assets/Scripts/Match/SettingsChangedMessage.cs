using System.Collections;
using UnityEngine;

namespace Sanicball.Match
{
    public class SettingsChangedMessage : MatchMessage
    {
        public Data.MatchSettings NewMatchSettings { get; private set; }

        public SettingsChangedMessage(Data.MatchSettings newMatchSettings)
        {
            NewMatchSettings = newMatchSettings;
        }
    }
}