using System.Collections.Generic;
using System.Linq;
using UnityEngine.Networking;
using UnityEngine;

namespace Sanicball
{

	public class RaceManagerLocal : MonoBehaviour
    {
        public List<RacePlayerLocal> players = new List<RacePlayerLocal>();
        private RaceState currentState = RaceState.None;

        private Data.MatchSettings settings = new Data.MatchSettings();

        [SerializeField]
        private WaitingCamera waitingCamPrefab = null;
        [SerializeField]
        private WaitingUI waitingUIPrefab = null;
        [SerializeField]
        private UI.RaceCountdown raceCountdownPrefab = null;
        [SerializeField]
        private UI.PlayerUILocal playerUIPrefab = null;
        [SerializeField]
        private UI.RaceUILocal raceUIPrefab = null;

        private WaitingCamera activeWaitingCam;
        private WaitingUI activeWaitingUI;
        private double raceTimer = 0f;
        private bool raceTimerOn = false;
        private UI.RaceUILocal raceUI;

        public System.TimeSpan RaceTime
        {
            get
            {
                return System.TimeSpan.FromSeconds(raceTimer);
            }
        }

        public int Laps { get { return settings.Laps; } }

        public Data.MatchSettings Settings { get { return settings; } }

        public int PlayerCount
        {
            get
            {
                return players.Count;
            }
        }

        public RacePlayerLocal this[int playerIndex]
        {
            get
            {
                return players[playerIndex];
            }
        }

		public RaceState CurrentState
        {
            get
            {
                return currentState;
            }

            set
            {
                //Shut down old state
                switch (currentState)
                {
                    case RaceState.Waiting:
                        if (activeWaitingCam)
                            Destroy(activeWaitingCam.gameObject);
                        if (activeWaitingUI)
                            Destroy(activeWaitingUI.gameObject);
                        break;
                }
                //Start new state
                switch (value)
                {
                    case RaceState.Waiting:
                        activeWaitingCam = Instantiate(waitingCamPrefab);
//					NetworkServer.Spawn(waitingCamPrefab.gameObject);
                        activeWaitingUI = Instantiate(waitingUIPrefab);
//					NetworkServer.Spawn(waitingUIPrefab.gameObject);

                        activeWaitingUI.StageNameToShow = Data.ActiveData.Stages[settings.StageId].name;
                        break;

                    case RaceState.Countdown:



                        var countdown = Instantiate(raceCountdownPrefab);
//					NetworkServer.Spawn(raceCountdownPrefab.gameObject);
                        countdown.OnCountdownFinished += Countdown_OnCountdownFinished;
                        raceUI = Instantiate(raceUIPrefab);
                        raceUI.TargetManager = this;
                        CreateBallObjects();// aqui es cuando se generan todos los ballObjects en la carrera
                        break;
                    case RaceState.Racing:
                        raceTimerOn = true;
                        var music = FindObjectOfType<MusicPlayer>();
                        if (music)
                        {
                            music.Play();
                        }
                        foreach (var p in players)
                        {
                            p.StartRace();

                        }
                        break;
                }
                currentState = value;
            }
        }

        private void Countdown_OnCountdownFinished(object sender, System.EventArgs e)
        {
            CurrentState = RaceState.Racing;
        }

        private void Start()
        {

            	CurrentState = RaceState.Waiting;
		
        }

		private void CreateBallObjects()
		{
			int nextPos = 0;

			var matchManager = FindObjectOfType<MatchManagerLocal>();
			for (int i = 0; i < matchManager.Players.Count; i++)
			{
				var player = matchManager.Players[i];

				player.BallObject = FindObjectOfType<RaceBallSpawnerLocal>().SpawnBall(nextPos, BallType.Player, player.CtrlType, player.CharacterId, player.Name);
				player.BallObject.CanMove = false;

				var racePlayer = new RacePlayerLocal(player.BallObject);
				players.Add(racePlayer);
				if (matchManager.Players.Count == 1)
					racePlayer.LapRecordsEnabled = true;

				racePlayer.FinishLinePassed += RacePlayer_FinishLinePassed;

				var playerUI = Instantiate(playerUIPrefab);
				playerUI.TargetPlayer = racePlayer;
				playerUI.TargetManager = this;

				int persistentIndex = i;

				player.BallObject.CameraCreated += (sender, e) =>
				{
					playerUI.TargetCamera = e.CameraCreated2.AttachedCamera; // aqui usamos el get de la clase -- CameraCreationArgs : System.EventArgs
					var splitter = e.CameraCreated2.AttachedCamera.GetComponent<CameraSplitter>();
					if (splitter)
						splitter.SplitscreenIndex = persistentIndex;
				};

				nextPos++;
			}

			for (int i = 0; i < settings.AICount; i++)
			{
				var aiBall = FindObjectOfType<RaceBallSpawnerLocal>().SpawnBall(nextPos, BallType.AI, ControlType.None, settings.GetAICharacter(i), "AI #" + i);
				aiBall.CanMove = false;

				var racePlayer = new RacePlayerLocal(aiBall);
				players.Add(racePlayer);
				racePlayer.FinishLinePassed += RacePlayer_FinishLinePassed;

				nextPos++;
			}
		}        

        private void RacePlayer_FinishLinePassed(object sender, System.EventArgs e)
        {
            var rp = (RacePlayerLocal)sender;

            if (rp.FinishReport == null && rp.Lap > settings.Laps)
            {
                rp.FinishRace(new RaceFinishReport(players.IndexOf(rp) + 1, RaceTime));

                Debug.LogWarning("oooo shit, a player has finished! Elapsed time: " + RaceTime);

                //Display scoreboard when local players have finished
                if (!players.Any(a => a.IsLocalPlayer && !a.RaceFinished))
                {

                    raceUI.ShowFinishedText();
                }
                //Auto return to lobby once all players have finished
                if (!players.Any(a => !a.RaceFinished))
                {
                    Debug.LogWarning("RACE IS DONE! LAST TIME: " + RaceTime);
                }
            }
        }

		private void Update()
		{
			if (CurrentState == RaceState.Waiting && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton0)))
			{
				CurrentState = RaceState.Countdown;
			}

			if (raceTimerOn)
			{
				raceTimer += Time.deltaTime;
				foreach (var p in players) p.UpdateTimer(Time.deltaTime);
			}

			players = players.OrderByDescending(a => a.CalculateRaceProgress()).ToList();
			for (int i = 0; i < players.Count; i++)
				players[i].Position = i + 1;
		}
    }
}