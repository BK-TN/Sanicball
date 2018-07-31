using System.Collections.Generic;
using System.Linq;
using Sanicball.Data;
using Sanicball.Gameplay;
using Sanicball.UI;
using SanicballCore;
using SanicballCore.MatchMessages;
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
        private RaceCountdown raceCountdownPrefab = null;
        [SerializeField]
        private PlayerUI playerUIPrefab = null;
        [SerializeField]
        private RaceUI raceUIPrefab = null;
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
        private double raceTimer = 0;
        private bool raceTimerOn = false;
        private RaceUI raceUI;
        private float countdownOffset;
        private bool joinedWhileRaceInProgress;

        //Properties
        public System.TimeSpan RaceTime { get { return System.TimeSpan.FromSeconds(raceTimer); } }
        public MatchSettings Settings { get { return settings; } }

        //PlayerCount gets number of players, indexer lets you retrieve them
        public int PlayerCount { get { return players.Count; } }
        public RacePlayer this[int playerIndex] { get { return players[playerIndex]; } }

        //Return players as read only collection (easier to query)
        public System.Collections.ObjectModel.ReadOnlyCollection<RacePlayer> Players { get { return new System.Collections.ObjectModel.ReadOnlyCollection<RacePlayer>(players); } }

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
                            if (joinedWhileRaceInProgress)
                            {
                                activeWaitingUI.InfoToShow = "Waiting for race to end.";
                            }
                            else
                            {
                                activeWaitingUI.InfoToShow = "Waiting for other players...";
                            }
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
            messenger.CreateListener<DoneRacingMessage>(DoneRacingCallback);

            if (raceIsInProgress)
            {
                Debug.Log("Starting race in progress");
                joinedWhileRaceInProgress = true;
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

                //Create race player
                var racePlayer = new RacePlayer(matchPlayer.BallObject, messenger, matchPlayer);
                players.Add(racePlayer);
                racePlayer.LapRecordsEnabled = enableLapRecords && local;
                racePlayer.FinishLinePassed += RacePlayer_FinishLinePassed;

                nextBallPosition++;
            }

            if (!matchManager.OnlineMode)
            {
                //Create all AI balls (In local play only)
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

            int nextLocalPlayerIndex = 0;
            foreach (RacePlayer p in players.Where(a => a.CtrlType != ControlType.None))
            {
                //Create player UI
                var playerUI = Instantiate(playerUIPrefab);
                playerUI.TargetManager = this;
                playerUI.TargetPlayer = p;

                //Connect UI to camera (when the camera has been instanced)
                int persistentIndex = nextLocalPlayerIndex;
                p.AssociatedMatchPlayer.BallObject.CameraCreated += (sender, e) =>
                {
                    playerUI.TargetCamera = e.CameraCreated.AttachedCamera;
                    var splitter = e.CameraCreated.AttachedCamera.GetComponent<CameraSplitter>();
                    if (splitter)
                        splitter.SplitscreenIndex = persistentIndex;
                };

                nextLocalPlayerIndex++;
            }
        }

        private void RacePlayer_FinishLinePassed(object sender, System.EventArgs e)
        {
            //Every time a player passes the finish line, check if it's done
            var rp = (RacePlayer)sender;

            if (rp.FinishReport == null && rp.Lap > settings.Laps)
            {
                //Race finishing is handled differently depending on what type of racer this is

                if (rp.AssociatedMatchPlayer != null)
                {
                    if (rp.AssociatedMatchPlayer.ClientGuid == matchManager.LocalClientGuid)
                    {
                        //For local player balls, send a DoneRacingMessage.
                        messenger.SendMessage(new DoneRacingMessage(rp.AssociatedMatchPlayer.ClientGuid, rp.AssociatedMatchPlayer.CtrlType, raceTimer, false));
                    }
                    //For remote player balls, do nothing.
                }
                else
                {
                    //For AI balls, call DoneRacingInner directly to bypass the messaging system.
                    DoneRacingInner(rp, raceTimer, false);
                }
            }
        }

        private void DoneRacingCallback(DoneRacingMessage msg, float travelTime)
        {
            RacePlayer rp = players.FirstOrDefault(a => a.AssociatedMatchPlayer != null
            && a.AssociatedMatchPlayer.ClientGuid == msg.ClientGuid
            && a.AssociatedMatchPlayer.CtrlType == msg.CtrlType);

            DoneRacingInner(rp, msg.RaceTime, msg.Disqualified);
        }

        private void DoneRacingInner(RacePlayer rp, double raceTime, bool disqualified)
        {
            int pos = players.IndexOf(rp) + 1;
            if (disqualified) pos = RaceFinishReport.DISQUALIFIED_POS;
            rp.FinishRace(new RaceFinishReport(pos, System.TimeSpan.FromSeconds(raceTime)));

            //Display scoreboard when all players have finished
            //TODO: Make proper scoreboard and have it trigger when only local players have finished
            if (!players.Any(a => a.IsPlayer && !a.RaceFinished))
            {
                StageReferences.Active.endOfMatchHandler.Activate(this);
            }
        }

        #region Unity event functions

        private void Start()
        {
            if (joinedWhileRaceInProgress)
            {
                CurrentState = RaceState.Racing;

                var specView = Instantiate(spectatorViewPrefab);
                specView.TargetManager = this;
                specView.Target = players[0];
            }
            else
            {
                CurrentState = RaceState.Waiting;
            }

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
            messenger.RemoveListener<DoneRacingMessage>(DoneRacingCallback);

            //Call the Destroy method on all players to properly dispose them
            foreach (RacePlayer p in players)
            {
                p.Destroy();
            }
        }

        #endregion Unity event functions
    }
}