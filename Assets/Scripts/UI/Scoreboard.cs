using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        [SerializeField]
        private SlideCanvasGroup slide = null;

        private bool slideShouldOpen = false;

        private List<ScoreboardEntry> activeEntries = new List<ScoreboardEntry>();

        private void Update()
        {
            if (slideShouldOpen && !slide.isOpen)
            {
                slide.Open();
            }
        }

        public void DisplayResults(RaceManager manager)
        {
            slideShouldOpen = true;

            for (int i = 0; i < manager.PlayerCount; i++)
            {
                if (activeEntries.Any(a => a.Player == manager[i])) continue;

                if (manager[i].RaceFinished && !manager[i].FinishReport.Disqualified)
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