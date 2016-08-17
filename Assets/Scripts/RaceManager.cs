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

        private Match.MatchManager matchManager;
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
                        if (matchManager.OnlineMode)
                        {
                            activeWaitingUI.InfoToShow = "Waiting for other players...";
                        }
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

        public void Init(Data.MatchSettings settings, Match.MatchManager matchManager, Match.MatchMessenger messenger)
        {
            this.settings = settings;
            this.matchManager = matchManager;
            this.messenger = messenger;

            messenger.CreateListener<Match.StartRaceMessage>(StartRaceCallback);
            messenger.CreateListener<Match.ClientLeftMessage>(ClientLeftCallback);
        }

        public void OnDestroy()
        {
            //ALL created listeners should be removed from the messenger here
            //Otherwise the race manager won't get destroyed properly
            messenger.RemoveListener<Match.StartRaceMessage>(StartRaceCallback);
            messenger.RemoveListener<Match.ClientLeftMessage>(ClientLeftCallback);

            foreach (RacePlayer p in players)
            {
                p.Destroy();
            }
        }

        private void StartRaceCallback(Match.StartRaceMessage msg)
        {
            CurrentState = RaceState.Countdown;
        }

        private void ClientLeftCallback(Match.ClientLeftMessage msg)
        {
            foreach (RacePlayer racePlayer in players.ToList())
            {
                if (racePlayer.AssociatedMatchPlayer.ClientGuid == msg.ClientGuid)
                {
                    racePlayer.Destroy();
                    players.Remove(racePlayer);
                }
            }
        }

        private void Start()
        {
            CurrentState = RaceState.Waiting;

            //In online mode, send a RaceStartMessage as soon as the track is loaded (which is now)
            if (matchManager.OnlineMode)
            {
                messenger.SendMessage(new Match.StartRaceMessage());
            }
        }

        private void CreateBallObjects()
        {
            int nextPos = 0;

            //Enable lap records if there is only one local player.
            bool enableLapRecords = matchManager.Players.Count(a => a.ClientGuid == matchManager.LocalClientGuid) == 1;

            //Store index for next local player, used to set up splitscreen cameras correctly
            int nextLocalPlayerIndex = 0;

            //Create all player balls
            for (int i = 0; i < matchManager.Players.Count; i++)
            {
                var matchPlayer = matchManager.Players[i];

                bool local = matchPlayer.ClientGuid == matchManager.LocalClientGuid;

                //Create ball
                string name = matchManager.Clients.FirstOrDefault(a => a.Guid == matchPlayer.ClientGuid).Name;
                matchPlayer.BallObject = FindObjectOfType<RaceBallSpawner>().SpawnBall(nextPos, BallType.Player, local ? matchPlayer.CtrlType : ControlType.Remote, matchPlayer.CharacterId, name + " (" + Utils.CtrlTypeStr(matchPlayer.CtrlType) + ")");
                matchPlayer.BallObject.CanMove = false;

                //Create race player
                var racePlayer = new RacePlayer(matchPlayer.BallObject, messenger, matchPlayer);
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
                    matchPlayer.BallObject.CameraCreated += (sender, e) =>
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
                var aiBall = FindObjectOfType<RaceBallSpawner>().SpawnBall(nextPos, BallType.AI, ControlType.Remote, settings.GetAICharacter(i), "AI #" + i);
                aiBall.CanMove = false;

                //Create race player
                var racePlayer = new RacePlayer(aiBall, messenger, null);
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

                //Display scoreboard when all players have finished
                //TODO: Make proper scoreboard and have it trigger when only local players have finished
                if (!players.Any(a => a.IsPlayer && !a.RaceFinished))
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
            //Manual race starting in offline mode
            if (!matchManager.OnlineMode && CurrentState == RaceState.Waiting && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton0)))
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