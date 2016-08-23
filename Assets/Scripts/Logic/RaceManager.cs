using System.Collections.Generic;
using System.Linq;
using Sanicball.Data;
using Sanicball.Gameplay;
using UnityEngine;

namespace Sanicball.Logic
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
        //Prefabs
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
        [SerializeField]
        private SpectatorView spectatorViewPrefab = null;

        //Race state
        private List<RacePlayer> players = new List<RacePlayer>();
        private RaceState currentState = RaceState.None;

        //Fields set in Init()
        private MatchSettings settings;
        private MatchManager matchManager;
        private MatchMessenger messenger;

        //Misc
        private WaitingCamera activeWaitingCam;
        private WaitingUI activeWaitingUI;
        private double raceTimer = 0f;
        private bool raceTimerOn = false;
        private UI.RaceUI raceUI;
        private float countdownOffset;

        //Properties
        public System.TimeSpan RaceTime { get { return System.TimeSpan.FromSeconds(raceTimer); } }
        public MatchSettings Settings { get { return settings; } }
        //PlayerCount gets number of players, indexer lets you retrieve them
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
                        activeWaitingUI.StageNameToShow = ActiveData.Stages[settings.StageId].name;
                        if (matchManager.OnlineMode)
                        {
                            activeWaitingUI.InfoToShow = "Waiting for other players...";
                        }
                        break;

                    case RaceState.Countdown:
                        //Create countdown
                        var countdown = Instantiate(raceCountdownPrefab);
                        countdown.ApplyOffset(countdownOffset);
                        countdown.OnCountdownFinished += Countdown_OnCountdownFinished;

                        //Create race UI
                        raceUI = Instantiate(raceUIPrefab);
                        raceUI.TargetManager = this;

                        //Create all balls
                        CreateBallObjects();

                        //If there are no local players, create a spectator camera
                        if (!matchManager.Players.Any(a => a.ClientGuid == matchManager.LocalClientGuid))
                        {
                            var specView = Instantiate(spectatorViewPrefab);
                            specView.TargetManager = this;
                            specView.Target = players[0];
                        }

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

        public void Init(MatchSettings settings, MatchManager matchManager, MatchMessenger messenger, bool raceIsInProgress)
        {
            this.settings = settings;
            this.matchManager = matchManager;
            this.messenger = messenger;

            messenger.CreateListener<StartRaceMessage>(StartRaceCallback);
            messenger.CreateListener<ClientLeftMessage>(ClientLeftCallback);

            if (raceIsInProgress)
            {
                Debug.Log("Starting race in progress");
                CreateBallObjects();
            }
        }

        private void StartRaceCallback(StartRaceMessage msg, float travelTime)
        {
            countdownOffset = travelTime;
            CurrentState = RaceState.Countdown;
        }

        private void ClientLeftCallback(ClientLeftMessage msg, float travelTime)
        {
            //Find and remove all RacePlayers associated with players from this client
            //TODO: Find some way to still have the player in the race, although disabled - so that players leaving while finished don't just disappear
            foreach (RacePlayer racePlayer in players.ToList())
            {
                if (racePlayer.AssociatedMatchPlayer != null && racePlayer.AssociatedMatchPlayer.ClientGuid == msg.ClientGuid)
                {
                    racePlayer.Destroy();
                    players.Remove(racePlayer);
                }
            }
        }

        private void CreateBallObjects()
        {
            int nextBallPosition = 0;
            RaceBallSpawner ballSpawner = StageReferences.Active.spawnPoint;

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
                matchPlayer.BallObject = ballSpawner.SpawnBall(
                    nextBallPosition,
                    BallType.Player,
                    local ? matchPlayer.CtrlType : ControlType.None,
                    matchPlayer.CharacterId,
                    name + " (" + GameInput.GetControlTypeName(matchPlayer.CtrlType) + ")"
                    );
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

                nextBallPosition++;
            }

            //Create all AI balls
            for (int i = 0; i < settings.AICount; i++)
            {
                //Create ball
                var aiBall = ballSpawner.SpawnBall(
                    nextBallPosition,
                    BallType.AI,
                    ControlType.None,
                    settings.GetAICharacter(i),
                    "AI #" + i
                    );
                aiBall.CanMove = false;

                //Create race player
                var racePlayer = new RacePlayer(aiBall, messenger, null);
                players.Add(racePlayer);
                racePlayer.FinishLinePassed += RacePlayer_FinishLinePassed;

                nextBallPosition++;
            }
        }

        private void RacePlayer_FinishLinePassed(object sender, System.EventArgs e)
        {
            //Every time a player passes the finish line, check if it's done
            var rp = (RacePlayer)sender;

            if (rp.FinishReport == null && rp.Lap > settings.Laps)
            {
                rp.FinishRace(new RaceFinishReport(players.IndexOf(rp) + 1, RaceTime));

                //Display scoreboard when all players have finished
                //TODO: Make proper scoreboard and have it trigger when only local players have finished
                if (!players.Any(a => a.IsPlayer && !a.RaceFinished))
                {
                    //TODO: Auto return to lobby after a timer
                    raceUI.ShowFinishedText();
                }
            }
        }

        #region Unity event functions

        private void Start()
        {
            CurrentState = RaceState.Waiting;

            //In online mode, send a RaceStartMessage as soon as the track is loaded (which is now)
            if (matchManager.OnlineMode)
            {
                messenger.SendMessage(new StartRaceMessage());
            }
        }

        private void Update()
        {
            //In offline mode, send a RaceStartMessage once Space (Or A on any joystick) is pressed
            if (!matchManager.OnlineMode && CurrentState == RaceState.Waiting && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton0)))
            {
                messenger.SendMessage(new StartRaceMessage());
            }

            //Increment the race timer if it's been started
            if (raceTimerOn)
            {
                raceTimer += Time.deltaTime;
                foreach (var p in players) p.UpdateTimer(Time.deltaTime);
            }

            //Order player list by position
            players = players.OrderByDescending(a => a.CalculateRaceProgress()).ToList();
            for (int i = 0; i < players.Count; i++)
                players[i].Position = i + 1;
        }

        private void OnDestroy()
        {
            //ALL listeners created in Init() should be removed from the messenger here
            //Otherwise the race manager won't get destroyed properly
            messenger.RemoveListener<StartRaceMessage>(StartRaceCallback);
            messenger.RemoveListener<ClientLeftMessage>(ClientLeftCallback);

            //Call the Destroy method on all players to properly dispose them
            foreach (RacePlayer p in players)
            {
                p.Destroy();
            }
        }

        #endregion Unity event functions
    }
}