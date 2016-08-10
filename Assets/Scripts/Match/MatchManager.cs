using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace Sanicball.Match
{
    /// <summary>
    /// Manages game state - scenes, players, all that jazz
    /// </summary>
    public class MatchManager : MonoBehaviour
    {
        #region Events

        public event EventHandler<MatchPlayerEventArgs> MatchPlayerAdded;
        public event EventHandler<MatchPlayerEventArgs> MatchPlayerRemoved;
        public event EventHandler MatchSettingsChanged;

        #endregion Events

        #region Exposed fields

        [SerializeField]
        private string lobbySceneName = "Lobby";

        //Prefabs
        [SerializeField]
        private UI.PauseMenu pauseMenuPrefab;
        [SerializeField]
        private RaceManager raceManagerPrefab;

        #endregion Exposed fields

        #region Match state

        //List of all clients in the match. Only serves a purpose in online play.
        //In local play, this list will always only contain the local client.
        private List<MatchClient> clients = new List<MatchClient>();

        //Holds the guid of the local client, to check if messages are directed at it.
        private Guid myGuid;

        //List of all players - players are seperate from clients because each client can have
        //up to 4 players playing in splitscreen.
        private List<MatchPlayer> players = new List<MatchPlayer>();

        //These settings will be used when starting a race
        private Data.MatchSettings currentSettings;

        //Lobby timer stuff
        private bool lobbyTimerOn = false;
        private const float lobbyTimerMax = 3;
        private float lobbyTimer = lobbyTimerMax;

        #endregion Match state

        #region Scenes and scene initializing

        //Bools for scene initializing and related stuff
        private bool inLobby = false; //Are we in the lobby or in a race?
        private bool loadingLobby = false;
        private bool loadingStage = false;
        private bool showSettingsOnLobbyLoad = false; //If true, the match settings window will pop up when the lobby scene is entered.

        #endregion Scenes and scene initializing

        //Match messenger used to send and recieve state changes.
        //This will be either a LocalMatchMessenger or OnlineMatchMessenger, but each are used the same way.
        private Match.MatchMessenger messenger;

        //Timer used for syncing realtime stuff in online
        private float netUpdateTimer = 0;
        private const int netUpdatesPerSecond = 20;

        #region Properties

        /// <summary>
        /// True if playing online. Used for enabling online-only behaviour, like the client list and the chat
        /// </summary>
        public bool OnlineMode { get { return messenger is OnlineMatchMessenger; } }

        /// <summary>
        /// Contains all clients connected to the game. In offline matches this will always only contain one client.
        /// </summary>
        public ReadOnlyCollection<MatchClient> Clients { get { return clients.AsReadOnly(); } }

        /// <summary>
        /// Contains all players in the game, even ones from other clients in online races
        /// </summary>
        public ReadOnlyCollection<MatchPlayer> Players { get { return players.AsReadOnly(); } }

        /// <summary>
        /// Current settings for this match. On remote clients, this is only used for showing settings on the UI.
        /// </summary>
        public Data.MatchSettings CurrentSettings { get { return currentSettings; } }

        public Guid LocalClientGuid { get { return myGuid; } }

        #endregion Properties

        #region State changing methods

        public void RequestSettingsChange(Data.MatchSettings newSettings)
        {
            messenger.SendMessage(new Match.SettingsChangedMessage(newSettings));
        }

        public void RequestPlayerJoin(ControlType ctrlType, int initialCharacter)
        {
            messenger.SendMessage(new Match.PlayerJoinedMessage(myGuid, ctrlType, initialCharacter));
        }

        public void RequestPlayerLeave(ControlType ctrlType)
        {
            messenger.SendMessage(new Match.PlayerLeftMessage(myGuid, ctrlType));
        }

        public void RequestCharacterChange(ControlType ctrlType, int newCharacter)
        {
            messenger.SendMessage(new Match.CharacterChangedMessage(myGuid, ctrlType, newCharacter));
        }

        public void RequestReadyChange(ControlType ctrlType, bool ready)
        {
            messenger.SendMessage(new Match.ChangedReadyMessage(myGuid, ctrlType, ready));
        }

        #endregion State changing methods

        #region Match message callbacks

        private void SettingsChangedCallback(SettingsChangedMessage msg)
        {
            currentSettings = msg.NewMatchSettings;
            if (MatchSettingsChanged != null)
                MatchSettingsChanged(this, EventArgs.Empty);
        }

        private void ClientJoinedCallback(ClientJoinedMessage msg)
        {
            clients.Add(new MatchClient(msg.ClientGuid, msg.ClientName));
            Debug.Log("New client " + msg.ClientName);
        }

        private void PlayerJoinedCallback(PlayerJoinedMessage msg)
        {
            var p = new MatchPlayer(msg.ClientGuid, msg.CtrlType, msg.InitialCharacter);
            players.Add(p);

            if (inLobby)
            {
                SpawnLobbyBall(p);
            }

            StopLobbyTimer(); //TODO: look into moving this (make the server trigger it while somehow still having it work in local play)

            if (MatchPlayerAdded != null)
                MatchPlayerAdded(this, new MatchPlayerEventArgs(p, msg.ClientGuid == myGuid));
        }

        private void PlayerLeftCallback(PlayerLeftMessage msg)
        {
            var player = players.FirstOrDefault(a => a.ClientGuid == msg.ClientGuid && a.CtrlType == msg.CtrlType);
            if (player != null)
            {
                players.Remove(player);

                if (player.BallObject)
                {
                    Destroy(player.BallObject.gameObject);
                }

                if (MatchPlayerRemoved != null)
                    MatchPlayerRemoved(this, new MatchPlayerEventArgs(player, msg.ClientGuid == myGuid)); //TODO: determine if removed player was local
            }
        }

        private void CharacterChangedCallback(CharacterChangedMessage msg)
        {
            if (!inLobby)
            {
                Debug.LogError("Cannot set character outside of lobby!");
            }

            var player = players.FirstOrDefault(a => a.ClientGuid == msg.ClientGuid && a.CtrlType == msg.CtrlType);
            if (player != null)
            {
                player.CharacterId = msg.NewCharacter;
                SpawnLobbyBall(player);
            }
        }

        private void ChangedReadyCallback(ChangedReadyMessage msg)
        {
            var player = players.FirstOrDefault(a => a.ClientGuid == msg.ClientGuid && a.CtrlType == msg.CtrlType);
            if (player != null)
            {
                player.ReadyToRace = !player.ReadyToRace;

                //Check if all players are ready and start/stop lobby timer accordingly
                var allReady = players.TrueForAll(a => a.ReadyToRace);
                if (allReady && !lobbyTimerOn)
                {
                    StartLobbyTimer();
                }
                if (!allReady && lobbyTimerOn)
                {
                    StopLobbyTimer();
                }
            }
        }

        private void ChatMessageCallback(ChatMessage msg)
        {
            Debug.Log("Chat message from " + msg.From + ": " + msg.Text);
        }

        private void LoadRaceCallback(LoadRaceMessage msg)
        {
            GoToStage();
            StopLobbyTimer();
        }

        private void PlayerMovementCallback(PlayerMovementMessage msg)
        {
            if (msg.ClientGuid == myGuid) return;

            MatchPlayer player = players.FirstOrDefault(a => a.ClientGuid == msg.ClientGuid && a.CtrlType == msg.CtrlType);
            if (player != null && player.BallObject != null)
            {
                Rigidbody ballRb = player.BallObject.GetComponent<Rigidbody>();

                player.BallObject.transform.position = msg.Position.ToVector3();
                player.BallObject.transform.rotation = Quaternion.Euler(msg.Rotation.ToVector3());
                ballRb.velocity = msg.Velocity.ToVector3();
                ballRb.angularVelocity = msg.AngularVelocity.ToVector3();
                player.BallObject.DirectionVector = msg.DirectionVector.ToVector3();
            }
        }

        #endregion Match message callbacks

        #region Match initializing

        public void InitLocalMatch()
        {
            currentSettings = Data.ActiveData.MatchSettings;

            messenger = new LocalMatchMessenger();

            showSettingsOnLobbyLoad = true;
            GoToLobby();
        }

        public void InitOnlineMatch(Lidgren.Network.NetClient client, Lidgren.Network.NetConnection serverConnection, MatchState matchState)
        {
            //Recieve match status and sync up
            foreach (var clientInfo in matchState.Clients)
            {
                clients.Add(new MatchClient(clientInfo.Guid, clientInfo.Name));
            }

            foreach (var playerInfo in matchState.Players)
            {
                MatchPlayer p = new MatchPlayer(playerInfo.ClientGuid, playerInfo.CtrlType, playerInfo.CharacterId);
                p.ReadyToRace = playerInfo.ReadyToRace;
                players.Add(p);

                if (inLobby)
                {
                    SpawnLobbyBall(p);
                }
            }

            currentSettings = matchState.Settings;

            messenger = new OnlineMatchMessenger(client, serverConnection);

            showSettingsOnLobbyLoad = true;
            GoToLobby();
        }

        #endregion Match initializing

        private void Start()
        {
            DontDestroyOnLoad(gameObject);

            //A messenger should be created by now! Time to create some message listeners
            messenger.CreateListener<SettingsChangedMessage>(SettingsChangedCallback);
            messenger.CreateListener<ClientJoinedMessage>(ClientJoinedCallback);
            messenger.CreateListener<PlayerJoinedMessage>(PlayerJoinedCallback);
            messenger.CreateListener<PlayerLeftMessage>(PlayerLeftCallback);
            messenger.CreateListener<CharacterChangedMessage>(CharacterChangedCallback);
            messenger.CreateListener<ChangedReadyMessage>(ChangedReadyCallback);
            messenger.CreateListener<ChatMessage>(ChatMessageCallback);
            messenger.CreateListener<PlayerMovementMessage>(PlayerMovementCallback);
            messenger.CreateListener<LoadRaceMessage>(LoadRaceCallback);

            //Create this client
            myGuid = Guid.NewGuid();
            messenger.SendMessage(new ClientJoinedMessage(myGuid, "client#" + myGuid));
        }

        private void Update()
        {
            messenger.UpdateListeners();

            //Pausing/unpausing
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton7))
            {
                if (!UI.PauseMenu.GamePaused)
                {
                    UI.PauseMenu menu = Instantiate(pauseMenuPrefab);
                    menu.OnlineMode = OnlineMode;
                }
                else
                {
                    var menu = FindObjectOfType<UI.PauseMenu>();
                    if (menu)
                        Destroy(menu.gameObject);
                }
            }

            //Shortcut to show match settings
            if (inLobby && Input.GetKeyDown(KeyCode.O))
            {
                LobbyReferences.Active.MatchSettingsPanel.Show();
            }

            if (lobbyTimerOn && inLobby)
            {
                lobbyTimer -= Time.deltaTime;
                LobbyReferences.Active.CountdownField.text = "Match starts in " + Mathf.Max(1f, Mathf.Ceil(lobbyTimer));

                //LoadRaceMessages don't need to be sent in online mode - the server will ignore it anyway.
                if (lobbyTimer <= 0 && !OnlineMode)
                {
                    messenger.SendMessage(new LoadRaceMessage());
                }
            }

            if (OnlineMode)
            {
                netUpdateTimer -= Time.deltaTime;

                if (netUpdateTimer <= 0)
                {
                    netUpdateTimer = 1f / netUpdatesPerSecond;

                    //Send local player positions to other clients
                    foreach (MatchPlayer player in players)
                    {
                        if (player.ClientGuid == myGuid && player.BallObject)
                        {
                            Rigidbody ballRb = player.BallObject.GetComponent<Rigidbody>();
                            messenger.SendMessage(new PlayerMovementMessage(myGuid, player.CtrlType,
                                player.BallObject.transform.position.ToSimpleVector3(),
                                player.BallObject.transform.rotation.eulerAngles.ToSimpleVector3(),
                                ballRb.velocity.ToSimpleVector3(),
                                ballRb.angularVelocity.ToSimpleVector3(),
                                player.BallObject.DirectionVector.ToSimpleVector3()
                                ));
                        }
                    }
                }
            }
        }

        #region Players ready and lobby timer

        private void StartLobbyTimer()
        {
            lobbyTimerOn = true;
            LobbyReferences.Active.CountdownField.enabled = true;
        }

        private void StopLobbyTimer()
        {
            lobbyTimerOn = false;
            lobbyTimer = lobbyTimerMax;
            LobbyReferences.Active.CountdownField.enabled = false;
        }

        #endregion Players ready and lobby timer

        #region Scene changing / race loading

        public void GoToLobby()
        {
            if (inLobby) return;

            loadingStage = false;
            loadingLobby = true;
            UnityEngine.SceneManagement.SceneManager.LoadScene(lobbySceneName);
        }

        public void GoToStage()
        {
            var targetStage = Data.ActiveData.Stages[currentSettings.StageId];

            loadingStage = true;
            loadingLobby = false;

            CameraFade.StartAlphaFade(Color.black, false, 0.3f, 0.05f, () =>
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(targetStage.sceneName);
            });
        }

        //Check if we were loading the lobby or the race
        private void OnLevelWasLoaded(int level)
        {
            if (loadingLobby)
            {
                InitLobby();
                loadingLobby = false;
                if (showSettingsOnLobbyLoad)
                {
                    //Let the player pick settings first time entering the lobby
                    LobbyReferences.Active.MatchSettingsPanel.Show();
                    showSettingsOnLobbyLoad = false;
                }
            }
            if (loadingStage)
            {
                InitRace();
                loadingStage = false;
                foreach (var p in Players)
                {
                    p.ReadyToRace = false;
                }
            }
        }

        //Initiate the lobby after loading lobby scene
        private void InitLobby()
        {
            inLobby = true;
            foreach (var p in Players)
            {
                SpawnLobbyBall(p);
            }
        }

        //Initiate a race after loading the stage scene
        private void InitRace()
        {
            inLobby = false;
            var raceManager = Instantiate(raceManagerPrefab);
            raceManager.Settings = currentSettings;
        }

        public void QuitMatch()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
            Destroy(gameObject);
        }

        #endregion Scene changing / race loading

        private void SpawnLobbyBall(MatchPlayer player)
        {
            var spawner = LobbyReferences.Active.BallSpawner;
            if (player.BallObject != null)
            {
                Destroy(player.BallObject.gameObject);
            }
            player.BallObject = spawner.SpawnBall(Data.PlayerType.Normal, (player.ClientGuid == myGuid) ? player.CtrlType : ControlType.None, player.CharacterId, "Player");
        }
    }
}