using System.Collections;
using Sanicball.UI;
using UnityEngine;

namespace Sanicball
{
    public class EndOfMatch : MonoBehaviour
    {
        [SerializeField]
        private Transform[] topPositionSpawnpoints = null;
        [SerializeField]
        private Scoreboard scoreboardPrefab = null;
        [SerializeField]
        private Camera cam = null;

        private Scoreboard activeScoreboard;
        private bool hasActivatedOnce = false;

        public void Activate(Logic.RaceManager manager)
        {
            if (!hasActivatedOnce)
            {
                hasActivatedOnce = true;

                activeScoreboard = Instantiate(scoreboardPrefab);
                //cam.enabled = true;
            }
            activeScoreboard.DisplayResults(manager);
        }
    }
}