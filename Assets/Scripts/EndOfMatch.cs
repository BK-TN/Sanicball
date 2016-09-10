using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sanicball.Logic;
using Sanicball.UI;
using UnityEngine;

namespace Sanicball
{
    public class EndOfMatch : MonoBehaviour
    {
        [SerializeField]
        private Transform[] topPositionSpawnpoints = null;
        [SerializeField]
        private Transform lowerPositionsSpawnpoint = null;
        [SerializeField]
        private Scoreboard scoreboardPrefab = null;
        [SerializeField]
        private Camera cam = null;
        [SerializeField]
        private Rotate camRotate = null;

        private Scoreboard activeScoreboard;
        private bool hasActivatedOnce = false;

        private List<RacePlayer> movedPlayers = new List<RacePlayer>();

        public void Activate(RaceManager manager)
        {
            if (!hasActivatedOnce)
            {
                hasActivatedOnce = true;

                activeScoreboard = Instantiate(scoreboardPrefab);

                for (int i = 0; i < manager.PlayerCount; i++)
                {
                    RacePlayer p = manager[i];
                    if (p.Camera != null)
                        p.Camera.Remove();
                }

                foreach (RaceUI ui in FindObjectsOfType<RaceUI>())
                {
                    Destroy(ui.gameObject);
                }

                foreach (PlayerUI ui in FindObjectsOfType<PlayerUI>())
                {
                    Destroy(ui.gameObject);
                }

                cam.gameObject.SetActive(true);
                camRotate.angle = new Vector3(0, 1, 0);
                //cam.enabled = true;
            }
            activeScoreboard.DisplayResults(manager);

            RacePlayer[] movablePlayers = manager.Players.Where(a => a.RaceFinished).OrderBy(a => a.FinishReport.Position).ToArray();
            for (int i = 0; i < movablePlayers.Length; i++)
            {
                Vector3 spawnpoint = lowerPositionsSpawnpoint.position;
                if (i < topPositionSpawnpoints.Length)
                {
                    spawnpoint = topPositionSpawnpoints[i].position;
                }
                RacePlayer playerToMove = movablePlayers[i];
                if (!movedPlayers.Contains(playerToMove))
                {
                    playerToMove.Ball.transform.position = spawnpoint;
                    playerToMove.Ball.GetComponent<Rigidbody>().velocity = Random.insideUnitSphere * 1f;
                    playerToMove.Ball.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
                    playerToMove.Ball.CanMove = false;
                    movedPlayers.Add(playerToMove);
                }
            }
        }
    }
}