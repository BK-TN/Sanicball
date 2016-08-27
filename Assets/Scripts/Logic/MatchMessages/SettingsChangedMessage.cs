using System.Collections;
using Sanicball.Data;
using UnityEngine;

namespace Sanicball.Logic
{
    public class SettingsChangedMessage : MatchMessage
    {
        public MatchSettings NewMatchSettings { get; private set; }

        public SettingsChangedMessage(MatchSettings newMatchSettings)
        {
            NewMatchSettings = newMatchSettings;
        }
    }
}
