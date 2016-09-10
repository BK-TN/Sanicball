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