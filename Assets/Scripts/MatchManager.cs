using System;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine;
namespace Sanicball
{
    public class MatchPlayerEventArgs : EventArgs
    {
        public MatchPlayerEventArgs(MatchPlayer player)
        {
            Player = player;
        }

        public MatchPlayer Player { get; private set; }
    }

    /// <summary>
    /// Manages game state - scenes, players, all that jazz
    /// </summary>
	public class MatchManager : NetworkBehaviour
    {
        [SerializeField]
        private string lobbySceneName = "Lobby";

        //Prefabs
        [SerializeField]
        private UI.PauseMenu pauseMenuPrefab;
        [SerializeField]
        private RaceManager raceManagerPrefab;

        //Match state
        private Data.MatchSettings currentSettings = new Data.MatchSettings();
        public bool inLobby = false;
        private bool lobbyTimerOn = false;
        private const float lobbyTimerMax = 6;

        private float lobbyTimer = lobbyTimerMax;
	

		private float lobbyTimerServer = 59;
		private bool lobbyTimerServerOn = false;
		public int serverConnections=0;


        //Bools for scene initializing
        public bool loadingLobby = false;
        public bool loadingStage = false;
        public bool firstTimeLoadingLobby = false;

        //Events
        public event EventHandler<MatchPlayerEventArgs> MatchPlayerAdded;
        public event EventHandler<MatchPlayerEventArgs> MatchPlayerRemoved;

		public bool replace;

        /// <summary>
        /// Contains all players in the game, even ones from other clients in online races
        /// </summary>
		public List<MatchPlayer> Players { get; private set; }
        public Data.MatchSettings CurrentSettings { get { return currentSettings; } }


		private void Start()
		{
			currentSettings.CopyValues(Data.ActiveData.MatchSettings);
			Players = new List<MatchPlayer>();
			DontDestroyOnLoad(gameObject);
			InitMatch();

		}

		private void Update()
		{


			if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton7   )  )
			{
				if (!UI.PauseMenu.GamePaused)
				{
					Instantiate(pauseMenuPrefab);
				}
				else
				{
					var menu = FindObjectOfType<UI.PauseMenu>();
					if (menu)
						Destroy(menu.gameObject);
				}
			}

			if (inLobby && Input.GetKeyDown(KeyCode.O) )
			{
				LobbyReferences.Active.MatchSettingsPanel.Show();
			}



			//TIMERS ////

			//Timer 50 secs...
			// 20 for testing...
			if(isServer &&  UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Lobby"  && lobbyTimerServerOn && !loadingStage  ){

				float prevSecondServer;
				prevSecondServer =Mathf.Ceil(lobbyTimerServer);

				lobbyTimerServer -= Time.deltaTime;

				if(Mathf.Ceil(lobbyTimerServer)!=prevSecondServer){
					ChatRelayer.Instance.SetLogMessage("De3bug_Consol_Lodg Starting Match in... " + Mathf.Ceil(lobbyTimerServer));
				}

				if (lobbyTimerServer <= 0){
					lobbyTimerServerOn= false;

					lobbyTimerOn=true;

				}


			}

			// if there are more than two conections..
			if(!lobbyTimerOn &&  !loadingStage && isServer && UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Lobby"){

				if(!lobbyTimerServerOn ){

					if((serverConnections+1)>1){
						lobbyTimerServerOn= true;
					}
				}else{
					if((serverConnections+1)==1){

						lobbyTimerServerOn= false;
						lobbyTimerServer=55;

					}

				}

			}


			// NO MORE INPUT TO SERVER ALLOWED.
			// We are about to start
			//Timer 10 secs and START RACE...
			//NO MORE INPUT ALLOWED
			// CANT BE STOPED....

			if (lobbyTimerOn && inLobby && isServer)
			{
				float prevSecond;
				prevSecond =Mathf.Ceil(lobbyTimer);

				lobbyTimer -= Time.deltaTime;

				if(Mathf.Ceil(lobbyTimer)!=prevSecond){
					ChatRelayer.Instance.SetLogMessage("De3bug_Consol_Lodg  Spawning in... " + Mathf.Ceil(lobbyTimer));
				}

				LobbyReferences.Active.CountdownField.text = "Match starts in " + Mathf.Ceil(lobbyTimer);


				if (lobbyTimer <= 0)
				{					
					ChatRelayer.Instance.SetLogMessage("De3bug_Consol_Lodg  Server is Loading Match, please wait.");
					
					LobbyReferences.Active.CountdownField.text = "Loading Match, please wait.";
					StopLobbyTimer();

					GoToStage();
				}
			}

		}

        public MatchPlayer CreatePlayer(string name, ControlType ctrlType, int characterId)
        {
            var p = new MatchPlayer(name, ctrlType, characterId);
            Players.Add(p);
            if (inLobby)
			{
                SpawnLobbyBall(p);
            }

            p.ChangedReady += AnyPlayerChangedReadyHandler;

            StopLobbyTimer();

            if (MatchPlayerAdded != null)
                MatchPlayerAdded(this, new MatchPlayerEventArgs(p));

            return p;
        }

        public void RemovePlayer(MatchPlayer player)
        {
            if (Players.Contains(player))
            {
                Players.Remove(player);

                if (player.BallObject)
                {
                    Destroy(player.BallObject.gameObject);
                }

                if (MatchPlayerRemoved != null)
                    MatchPlayerRemoved(this, new MatchPlayerEventArgs(player));
            }
        }

        public void SetCharacter(MatchPlayer player, int character)
        {
            if (!inLobby)
            {
                Debug.LogError("Cannot set character outside of lobby!");
            }

            player.CharacterId = character;
            SpawnLobbyBall(player);
        }

        private void AnyPlayerChangedReadyHandler(object sender, EventArgs e)
        {
            var allReady = Players.TrueForAll(a => a.ReadyToRace);
            if (allReady && !lobbyTimerOn)
            {

                StartLobbyTimer();
            }

        }

        private void StartLobbyTimer()
        {
            lobbyTimerOn = true;
            LobbyReferences.Active.CountdownField.enabled = true;
        }


        private void StopLobbyTimer()
        {
            lobbyTimerOn = false;
            lobbyTimer = lobbyTimerMax;
			lobbyTimerServer= 55;
            LobbyReferences.Active.CountdownField.enabled = false;
        }

        #region Scene changing / race loading

        public void GoToLobby()
        {
			if (inLobby || !isServer) return;
	
            loadingStage = false;
            loadingLobby = true;

			if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Lobby" && UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Menu" ){

				NetworkManager.singleton.ServerChangeScene(lobbySceneName);

			}
        }

        public void GoToStage()
		{
            var targetStage = Data.ActiveData.Stages[CurrentSettings.StageId];

            loadingStage = true;
            loadingLobby = false;

            CameraFade.StartAlphaFade(Color.black, false, 0.3f, 0.05f, () =>
            {
					NetworkManager.singleton.ServerChangeScene(targetStage.sceneName);
            });
        }


		public  void LevelStarted()
		{

			if (loadingLobby)
			{
				InitLobby();
				loadingLobby = false;
				if (firstTimeLoadingLobby)
				{
					//Let the player pick settings first time entering the lobby
					LobbyReferences.Active.MatchSettingsPanel.Show();

					firstTimeLoadingLobby = false;
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

		public MatchPlayer AddReadyEvent(MatchPlayer p ){

			p.ChangedReady += AnyPlayerChangedReadyHandler;
			if (MatchPlayerAdded != null)
				MatchPlayerAdded(this, new MatchPlayerEventArgs(p));
			
			return p;
		}

		public  void LevelStartedFromRace()
		{

			Debug.Log("Level Started From Race...");
			if (loadingLobby)
			{
				inLobby = true;
				foreach (var p in Players)
				{
					//addind the event for Ready...
					p.ChangedReady += AnyPlayerChangedReadyHandler;
					if (MatchPlayerAdded != null)
						MatchPlayerAdded(this, new MatchPlayerEventArgs(p));

					SpawnLobbyBallFromRace(p);

				}

				loadingLobby = false;
				if (firstTimeLoadingLobby)
				{
					//Let the player pick settings first time entering the lobby
					LobbyReferences.Active.MatchSettingsPanel.Show();
					firstTimeLoadingLobby = false;
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

        //Check if we were loading the lobby or the race
        private void OnLevelWasLoaded(int level)
        {

			Debug.Log(Players.Count);
			if(Players.Count==0 &&  UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Lobby" ){

				if (loadingLobby )
	            {
	                InitLobby();
	                loadingLobby = false;
	                if (firstTimeLoadingLobby)
	                {
	                    //Let the player pick settings first time entering th3e lobby
	                    LobbyReferences.Active.MatchSettingsPanel.Show();
	                    firstTimeLoadingLobby = false;
	                }
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

        private void InitMatch()
        {

			if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name== "Lobby")
            	firstTimeLoadingLobby = true;
			
            GoToLobby();
			if(!isServer && UnityEngine.SceneManagement.SceneManager.GetActiveScene().name== "Lobby")
			{
				NetworkManager.singleton.GetComponent<SanicNetworkManager>().ballSpawner = GameObject.Find("BallSpawner").GetComponent<LobbyBallSpawner>();
				NetworkManager.singleton.GetComponent<SanicNetworkManager>().matchManager = this;
				LevelStarted();
				GameObject.FindObjectOfType<Sanicball.UI.LocalPlayerManager>().InitManager();

			}
        }

        //Initiate the lobby after loading lobby scene
		public void InitLobby()
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
			if(isServer){
				NetworkManager.singleton.GetComponent<SanicNetworkManager>().raceManager= raceManager;
			}
            raceManager.Settings.CopyValues(CurrentSettings);

        }

        public void QuitMatch()
        {

			if(isServer){

				NetworkManager.singleton.matchMaker.DestroyMatch ( NetworkManager.singleton.matchInfo.networkId,0 , OnDestroyMatch) ;
			
			}else{
				Destroy(GameObject.Find("Match Manager(Clone)"));

				NetworkManager.singleton.ServerChangeScene("Menu");

				NetworkManager.singleton.StopHost();
				NetworkManager.singleton.StopMatchMaker();
				NetworkManager.Shutdown();
				Destroy(GameObject.Find("SanicNetworkManager"));
				NetworkTransport.Shutdown();
			
			}


        }

        #endregion Scene changing / race loading


		public void OnDestroyMatch(bool success, string extendedInfo)
		{
			
			if(NetworkManager.singleton){
				Destroy(GameObject.Find("Match Manager(Clone)"));
				NetworkManager.singleton.ServerChangeScene("Menu");
				NetworkManager.singleton.StopHost();
				NetworkManager.singleton.StopMatchMaker();
				NetworkManager.Shutdown();
				Destroy(GameObject.Find("SanicNetworkManager"));
				NetworkTransport.Shutdown();


			}

		} 


		public void AddNotInitializatedPlayer(){
			// this method is used when a player didnt choose a character, we proceed to assign him a default one
			for(int i =0; i< NetworkServer.connections.Count; i++ ){
				bool iniciado = false;
				for( int j=0; j< Players.Count; j++  ) {

					if (NetworkServer.connections[i] == Players[j].ConnectionSelf){
						iniciado= true;

					}else{

					}
				}

				if(!iniciado && NetworkServer.connections[i] !=null){
					var p = new MatchPlayer("Player_", ControlType.Keyboard, 0);

					p.ConnectionSelf =NetworkServer.connections[i];
					Players.Add(p);
					if (inLobby)
					{
					}

					p.ChangedReady += AnyPlayerChangedReadyHandler;


					if (MatchPlayerAdded != null)
						MatchPlayerAdded(this, new MatchPlayerEventArgs(p));

				}

			}

		}


		public void RemoveNotInitializatedPlayer(){
			// this is not used yet
			for(int i =0; i< NetworkServer.connections.Count; i++ ){
				bool iniciado = false;
				for( int j=0; j< Players.Count; j++  ) {

					if (NetworkServer.connections[i] == Players[j].ConnectionSelf){
						iniciado= true;

					}else{
		
					}
				}
				Debug.Log( NetworkServer.connections[i] +  "  HA SIDO INICIADO.??..." + iniciado );
				if(!iniciado){
					NetworkServer.connections[i].Disconnect();
					NetworkServer.connections[i].Dispose();

				}

			}

		}
				
        private void SpawnLobbyBall(MatchPlayer player)
        {
			// for multiplayer...

			LobbyBallSpawner spawner =  LobbyReferences.Active.BallSpawner;

			if( player.ConnectionSelf != null  )
            {

				replace= true;

				Debug.Log("We delete the gameobject and then proceed to make a replacement.");
				NetworkServer.Destroy(player.ConnectionSelf.playerControllers[0].gameObject);

			}

			if(isServer && !replace){
				
				player.ConnectionSelf = NetworkServer.connections[0];

			}

			if(!replace){
				Debug.Log("I create a new connection using singleton.client.connection");
				player.BallObject = spawner.SpawnBall(Data.PlayerType.Normal, player.CtrlType, player.CharacterId, player.Name,NetworkManager.singleton.client.connection);// en cliente esto da nulo,, creo.

			}
			else{
				Debug.Log("The ball will be replaced using existing connection stored in ball.connectionSelf");

				player.BallObject = spawner.SpawnBall(Data.PlayerType.Normal, player.CtrlType, player.CharacterId, player.Name,player.ConnectionSelf);// en cliente esto da nulo,, creo.

			}

        }


		// aparece el lobbyBall
		private void SpawnLobbyBallFromRace(MatchPlayer player)
		{

			LobbyBallSpawner spawner =  LobbyReferences.Active.BallSpawner;

			if( player.ConnectionSelf != null  )
			{

				replace= true;

				Debug.Log("We delete the gameobject and then proceed to make a replacement.");

			}
			if(isServer && !replace)
				player.ConnectionSelf = NetworkServer.connections[0];

			if(!replace){
				player.BallObject = spawner.SpawnBall(Data.PlayerType.Normal, player.CtrlType, player.CharacterId, "Player",NetworkManager.singleton.client.connection);// en cliente esto da nulo,, creo.

			}
			else{
				player.BallObject = spawner.SpawnBall(Data.PlayerType.Normal, player.CtrlType, player.CharacterId, "Player",player.ConnectionSelf);// en cliente esto da nulo,, creo.

			}

		}





    }

    /// <summary>
    /// Represents a player and its personal settings.
    /// </summary>
    [Serializable]
    public class MatchPlayer
    {
        private string name;
        private ControlType ctrlType;
		private NetworkConnection connectionSelf;

        private bool readyToRace;

        public MatchPlayer(string name, ControlType ctrlType, int initialCharacterId)
        {
            this.name = name;
            this.ctrlType = ctrlType;
            CharacterId = initialCharacterId;
        }

        public event EventHandler LeftMatch;
        public event EventHandler ChangedReady;

        public string Name { get { return name; } }
        public ControlType CtrlType { get { return ctrlType; } }
        public int CharacterId { get; set; }
        public Ball BallObject { get; set; }

		public NetworkConnection ConnectionSelf 
		{
			get 
			{ 
				return connectionSelf;
			}
			set
			{  
				connectionSelf = value;
			}
		}

        public bool ReadyToRace
        {
            get { return readyToRace; }
            set
            {
                readyToRace = value;
                if (ChangedReady != null)
                    ChangedReady(this, EventArgs.Empty);
            }
        }
    }
}