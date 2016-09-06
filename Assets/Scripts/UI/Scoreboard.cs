using System.Collections;
using System.Collections.Generic;
using Sanicball.Logic;
using UnityEngine;

namespace Sanicball.UI
{
    public class Scoreboard : MonoBehaviour
    {
        [SerializeField]
        private ScoreboardEntry entryPrefab = null;
        [SerializeField]
        private RectTransform entryContainer = null;

        private List<ScoreboardEntry> activeEntries = new List<ScoreboardEntry>();

        public void DisplayResults(RaceManager manager)
        {
            foreach (ScoreboardEntry e in activeEntries)
            {
                Destroy(e.gameObject);
            }
            activeEntries.Clear();

            for (int i = 0; i < manager.PlayerCount; i++)
            {
                if (manager[i].RaceFinished)
                {
                    ScoreboardEntry e = Instantiate(entryPrefab);
                    e.transform.SetParent(entryContainer, false);
                    e.Init(manager[i]);
                    activeEntries.Add(e);
                }
            }
        }
    }
}