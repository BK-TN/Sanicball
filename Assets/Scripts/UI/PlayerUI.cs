﻿using System;
using System.Collections.Generic;
using System.Linq;
using Sanicball.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Sanicball.UI
{
    public class PlayerUI : MonoBehaviour
    {
        [SerializeField]
        private RectTransform fieldContainer;

        [SerializeField]
        private Text speedField = null;

        [SerializeField]
        private Text speedFieldLabel = null;

        [SerializeField]
        private Text lapField = null;

        [SerializeField]
        private Text timeField = null;

        [SerializeField]
        private Text checkpointTimeField = null;

        [SerializeField]
        private Text checkpointTimeDiffField = null;

        [SerializeField]
        private AudioClip checkpointSound;

        [SerializeField]
        private Marker markerPrefab;

        [SerializeField]
        private RectTransform markerContainer;

        private Marker checkpointMarker;

        private RacePlayer targetPlayer;
        private RaceManager targetManager;

        private readonly Color finishedColor = new Color(0f, 0.5f, 1f);

        public RacePlayer TargetPlayer
        {
            get { return targetPlayer; }
            set
            {
                if (targetPlayer != null)
                {
                    targetPlayer.NextCheckpointPassed -= TargetPlayer_NextCheckpointPassed;
                }

                targetPlayer = value;

                targetPlayer.NextCheckpointPassed += TargetPlayer_NextCheckpointPassed;
            }
        }

        public RaceManager TargetManager
        {
            get { return targetManager; }
            set
            {
                targetManager = value;
            }
        }

        public Camera TargetCamera { get; set; }

        private void TargetPlayer_NextCheckpointPassed(object sender, NextCheckpointPassArgs e)
        {
            UISound.Play(checkpointSound);
            checkpointTimeField.text = GetTimeString(e.CurrentLapTime);
            checkpointTimeField.GetComponent<ToggleCanvasGroup>().ShowTemporarily(2f);

            if (TargetPlayer.LapRecordsEnabled)
            {
                bool hyperspeed = ActiveData.Characters[targetPlayer.Character].hyperspeed;
                string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                int stage = ActiveData.Stages.Where(a => a.sceneName == sceneName).First().id;

                float time = (float)e.CurrentLapTime.TotalSeconds;
                RaceRecord bestRecord = ActiveData.RaceRecords.Where(a => a.Type == (hyperspeed ? RecordType.HyperspeedLap : RecordType.Lap) && a.Stage == stage).OrderBy(a => a.Time).FirstOrDefault();
                if (bestRecord != null)
                {
                    float diff = time - bestRecord.CheckpointTimes[e.IndexOfPreviousCheckpoint];

                    bool faster = diff < 0;
                    TimeSpan diffSpan = TimeSpan.FromSeconds(Mathf.Abs(diff));

                    checkpointTimeDiffField.text = (faster ? "-" : "+") + GetTimeString(diffSpan);
                    checkpointTimeDiffField.color = faster ? Color.blue : Color.red;
                    checkpointTimeDiffField.GetComponent<ToggleCanvasGroup>().ShowTemporarily(2f);

                    if (e.IndexOfPreviousCheckpoint == StageReferences.Active.checkpoints.Length - 1 && faster)
                    {
                        checkpointTimeDiffField.text = "New lap record!";
                    }
                }
                else
                {
                    if (e.IndexOfPreviousCheckpoint == StageReferences.Active.checkpoints.Length - 1)
                    {
                        checkpointTimeDiffField.text = "Lap record set!";
                        checkpointTimeDiffField.color = Color.blue;
                        checkpointTimeDiffField.GetComponent<ToggleCanvasGroup>().ShowTemporarily(2f);
                    }
                }
            }
        }

        private void Start()
        {
            checkpointMarker = Instantiate(markerPrefab);
            checkpointMarker.transform.SetParent(markerContainer, false);

            checkpointMarker.Text = "Checkpoint";



        }
		public void GenerateRaceAvatar(){

			Debug.Log("Making PlayerMArkers");
//			Ball[] listaBalls;
//			listaBalls = FindObjectsOfType<Ball>();
//			Debug.Log(listaBalls.Count());


			for(int j=0; j < SanicNetworkManager.singleton.GetComponent<SanicNetworkManager>().raceManager.players.Count() ;j++){

//				SanicNetworkManager.singleton.GetComponent<SanicNetworkManager>().matchManager.Players

				if(!SanicNetworkManager.singleton.GetComponent<SanicNetworkManager>().raceManager.players[j].ball.isLocalPlayer){

					Marker playerMarkerRace = Instantiate(markerPrefab);

					playerMarkerRace.transform.SetParent( markerContainer, false );
					playerMarkerRace.GetComponent<Image>().sprite= Data.ActiveData.Characters[SanicNetworkManager.singleton.GetComponent<SanicNetworkManager>().raceManager.players[j].ball.CharacterId].icon; 
					playerMarkerRace.GetComponent<RectTransform>().localScale= new Vector3(.7f,.7f,.7f);
//					playerMarkerRace.GetComponent<RectTransform>().rect.height=50;

					Color avatarColor= new Color(1,1,1,.6f);
					playerMarkerRace.GetComponent<Image>().color= avatarColor;


					playerMarkerRace.Target = SanicNetworkManager.singleton.GetComponent<SanicNetworkManager>().raceManager.players[j].ball.gameObject.transform;
					playerMarkerRace.Text= "";
					//						var racePlayer2 = new RacePlayer(listaBalls[j]);


				}
			}

		}

        private void Update()
        {
            if (TargetCamera)
            {
                fieldContainer.anchorMin = TargetCamera.rect.min;
                fieldContainer.anchorMax = TargetCamera.rect.max;
            }

            if (TargetPlayer == null || TargetManager == null) return;

			if( TargetPlayer.ball!= null){
	            	float speed = TargetPlayer.Speed;
	            string postfix = " ";

	            //Speed label
	            if (!ActiveData.GameSettings.useImperial)
	            {
	                postfix += (Mathf.Floor(speed) == 1f) ? "fast/h" : "fasts/h";
	            }
	            else
	            {
	                speed *= 0.62f;
	                postfix += (Mathf.Floor(speed) == 1f) ? "lightspeed" : "lightspeeds";
	                speedFieldLabel.fontSize = 62;
	            }

	            //Speed field size and color
	            var min = 96;
	            var max = 150;
	            var size = max - (max - min) * Mathf.Exp(-speed * 0.02f);
	            speedField.fontSize = (int)size;
	            speedField.text = Mathf.Floor(speed).ToString();
	            speedFieldLabel.text = postfix;

	            //Lap counter
	            if (TargetPlayer.Lap < TargetManager.Laps + 1)
	            {
	                lapField.text = "Lap " + TargetPlayer.Lap + "/" + TargetManager.Laps;
	            }
	            else
	            {
	                lapField.text = "Race finished";
	                lapField.color = finishedColor;
	            }

	            //Race time
	            System.TimeSpan timeToUse = TargetManager.RaceTime;
	            if (TargetPlayer.FinishReport != null)
	            {
	                timeToUse = TargetPlayer.FinishReport.Time;
	                timeField.color = finishedColor;
	            }
	            timeField.text = GetTimeString(timeToUse);

	            //Checkpoint marker
	            checkpointMarker.Target = TargetPlayer.NextCheckpoint.transform;
	            checkpointMarker.CameraToUse = TargetCamera;
			}
        }

        private string GetTimeString(System.TimeSpan timeToUse)
        {
            return string.Format("{0:00}:{1:00}.{2:000}", timeToUse.Minutes, timeToUse.Seconds, timeToUse.Milliseconds);
        }
    }
}