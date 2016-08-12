using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Sanicball
{
    public enum RaceState
    {
        None,
        Waiting,
        Countdown,
        Racing,
        Finished
    }

    public class RaceManager : MonoBehaviour
    {
        private List<RacePlayer> players = new List<RacePlayer>();
        private RaceState currentState = RaceState.None;
        private Data.MatchSettings settings;

        private Match.MatchMessenger messenger;

        [SerializeField]
        private WaitingCamera waitingCamPrefab = null;
        [SerializeField]
        private WaitingUI waitingUIPrefab = null;
        [SerializeField]
        private UI.RaceCountdown raceCountdownPrefab = null;
        [SerializeField]
        private UI.PlayerUI playerUIPrefab = null;
        [SerializeField]
        private UI.RaceUI raceUIPrefab = null;

        private WaitingCamera activeWaitingCam;
        private WaitingUI activeWaitingUI;
        private double raceTimer = 0f;
        private bool raceTimerOn = false;
        private UI.RaceUI raceUI;

        public System.TimeSpan RaceTime { get { return System.TimeSpan.FromSeconds(raceTimer); } }
        public Data.MatchSettings Settings { get { return settings; } }
        public int PlayerCount { get { return players.Count; } }
        public RacePlayer this[int playerIndex] { get { return players[playerIndex]; } }

        private RaceState CurrentState
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
                        activeWaitingUI = Instantiate(waitingUIPrefab);
                        activeWaitingUI.StageNameToShow = Data.ActiveData.Stages[settings.StageId].name;
                        break;

                    case RaceState.Countdown:
                        var countdown = Instantiate(raceCountdownPrefab);
                        countdown.OnCountdownFinished += Countdown_OnCountdownFinished;
                        raceUI = Instantiate(raceUIPrefab);
                        raceUI.TargetManager = this;
                        CreateBallObjects();
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

        public void Init(Data.MatchSettings settings, Match.MatchMessenger messenger)
        {
            this.settings = settings;
            this.messenger = messenger;

            messenger.CreateListener<Match.StartRaceMessage>(StartRaceCallback);
        }

        public void OnDestroy()
        {
            //ALL created listeners should be removed from the messenger here
            //Otherwise the race manager won't get destroyed properly
            messenger.RemoveListener<Match.StartRaceMessage>(StartRaceCallback);
        }

        private void StartRaceCallback(Match.StartRaceMessage msg)
        {
            CurrentState = RaceState.Countdown;
        }

        private void Start()
        {
            CurrentState = RaceState.Waiting;
        }

        private void CreateBallObjects()
        {
            int nextPos = 0;

            var matchManager = FindObjectOfType<Match.MatchManager>();

            //Enable lap records if there is only one local player.
            bool enableLapRecords = matchManager.Players.Count(a => a.ClientGuid == matchManager.LocalClientGuid) == 1;

            //Store index for next local player, used to set up splitscreen cameras correctly
            int nextLocalPlayerIndex = 0;

            //Create all player balls
            for (int i = 0; i < matchManager.Players.Count; i++)
            {
                var player = matchManager.Players[i];

                bool local = player.ClientGuid == matchManager.LocalClientGuid;

                //Create ball
                player.BallObject = FindObjectOfType<RaceBallSpawner>().SpawnBall(nextPos, BallType.Player, local ? player.CtrlType : ControlType.None, player.CharacterId, "Ball - " + Utils.CtrlTypeStr(player.CtrlType));
                player.BallObject.CanMove = false;

                //Create race player
                var racePlayer = new RacePlayer(player.BallObject);
                players.Add(racePlayer);
                racePlayer.LapRecordsEnabled = enableLapRecords && local;
                racePlayer.FinishLinePassed += RacePlayer_FinishLinePassed;

                if (local)
                {
                    //Create player UI
                    var playerUI = Instantiate(playerUIPrefab);
                    playerUI.TargetPlayer = racePlayer;
                    playerUI.TargetManager = this;

                    //Connect UI to camera (when the camera has been instanced)
                    int persistentIndex = nextLocalPlayerIndex;
                    player.BallObject.CameraCreated += (sender, e) =>
                    {
                        playerUI.TargetCamera = e.CameraCreated.AttachedCamera;
                        var splitter = e.CameraCreated.AttachedCamera.GetComponent<CameraSplitter>();
                        if (splitter)
                            splitter.SplitscreenIndex = persistentIndex;
                    };

                    nextLocalPlayerIndex++;
                }

                nextPos++;
            }

            //Create all AI balls
            for (int i = 0; i < settings.AICount; i++)
            {
                //Spawn ball object
                var aiBall = FindObjectOfType<RaceBallSpawner>().SpawnBall(nextPos, BallType.AI, ControlType.None, settings.GetAICharacter(i), "AI #" + i);
                aiBall.CanMove = false;

                //Create race player
                var racePlayer = new RacePlayer(aiBall);
                players.Add(racePlayer);
                racePlayer.FinishLinePassed += RacePlayer_FinishLinePassed;

                nextPos++;
            }
        }

        private void RacePlayer_FinishLinePassed(object sender, System.EventArgs e)
        {
            var rp = (RacePlayer)sender;

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
                messenger.SendMessage(new Match.StartRaceMessage());
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