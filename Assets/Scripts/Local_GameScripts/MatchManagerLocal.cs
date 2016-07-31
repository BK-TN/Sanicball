using System;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine;
namespace Sanicball
{



	public class MatchPlayerLocalEventArgs : EventArgs
	{
		public MatchPlayerLocalEventArgs(MatchPlayerLocal player)
		{
			Player = player;
		}

		public MatchPlayerLocal Player { get; private set; }
	}
 
    /// <summary>
    /// Manages game state - scenes, players, all that jazz
    /// </summary>
	public class MatchManagerLocal : MonoBehaviour
    {
        [SerializeField]
        private string lobbySceneName = "LobbyLocal";

        //Prefabs
        [SerializeField]
        private UI.PauseMenu pauseMenuPrefab;
        [SerializeField]
        private RaceManagerLocal raceManagerPrefab;

        //Match state
        private Data.MatchSettings currentSettings = new Data.MatchSettings();
        public bool inLobby = false;
        private bool lobbyTimerOn = false;
        private const float lobbyTimerMax = 3;
        private float lobbyTimer = lobbyTimerMax;

        //Bools for scene initializing
        public bool loadingLobby = false;
        private bool loadingStage = false;
        public bool firstTimeLoadingLobby = false;

        //Events
        public event EventHandler<MatchPlayerLocalEventArgs> MatchPlayerAdded;
        public event EventHandler<MatchPlayerLocalEventArgs> MatchPlayerRemoved;

		public bool replace;

        /// <summary>
        /// Contains all players in the game, even ones from other clients in online races
        /// </summary>
		public List<MatchPlayerLocal> Players { get; private set; }
        public Data.MatchSettings CurrentSettings { get { return currentSettings; } }


		private void Start()
		{
//			NetworkManager.singleton.StartClient();
			currentSettings.CopyValues(Data.ActiveData.MatchSettings);
			Players = new List<MatchPlayerLocal>();
			DontDestroyOnLoad(gameObject);
			InitMatch();

		}

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton7   ) )
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
				//hace que el menu aparezca en el lobby,..
			}



			if (lobbyTimerOn && inLobby)
			{
				lobbyTimer -= Time.deltaTime;
				LobbyReferences.Active.CountdownField.text = "Match starts in " + Mathf.Ceil(lobbyTimer);

				if (lobbyTimer <= 0)
				{
					GoToStage();
					StopLobbyTimer();
				}
			}
		}
        public MatchPlayerLocal CreatePlayer(string name, ControlType ctrlType, int characterId)
        {
			//  Esto aparece la primera vez que estamos iniciando un personajew
			Debug.Log("Creating Player");

            var p = new MatchPlayerLocal(name, ctrlType, characterId);
			// en servidor, esto ocurre una vez....
            Players.Add(p);
            if (inLobby)
			{
                SpawnLobbyBall(p);
            }

            p.ChangedReady += AnyPlayerChangedReadyHandler;

            StopLobbyTimer();

            if (MatchPlayerAdded != null)
                MatchPlayerAdded(this, new MatchPlayerLocalEventArgs(p));

            return p;
        }

        public void RemovePlayer(MatchPlayerLocal player)
        {
            if (Players.Contains(player))
            {
                Players.Remove(player);

                if (player.BallObject)
                {
                    Destroy(player.BallObject.gameObject);
                }

                if (MatchPlayerRemoved != null)
                    MatchPlayerRemoved(this, new MatchPlayerLocalEventArgs(player));
            }
        }

        public void SetCharacter(MatchPlayerLocal player, int character)
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
            if (!allReady && lobbyTimerOn)
            {
                StopLobbyTimer();
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
            LobbyReferences.Active.CountdownField.enabled = false;
        }

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
			var targetStage = Data.ActiveData.Stages[CurrentSettings.StageId];

			loadingStage = true;
			loadingLobby = false;

			CameraFade.StartAlphaFade(Color.black, false, 0.3f, 0.05f, () =>
				{
					UnityEngine.SceneManagement.SceneManager.LoadScene(targetStage.sceneName);
				});
		}



		public  void LevelStarted()
		{

			Debug.Log("Level Started Method...");
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

		public MatchPlayerLocal AddReadyEvent(MatchPlayerLocal p ){

			p.ChangedReady += AnyPlayerChangedReadyHandler;
			if (MatchPlayerAdded != null)
				MatchPlayerAdded(this, new MatchPlayerLocalEventArgs(p));
			
			return p;
		}

	

        //Check if we were loading the lobby or the race
        private void OnLevelWasLoaded(int level)
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

        private void InitMatch()
        {
            firstTimeLoadingLobby = true;
            GoToLobby();
	
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
			raceManager.Settings.CopyValues(CurrentSettings);

        }

        public void QuitMatch()
        {
			UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
			Destroy(gameObject);
        }

        #endregion Scene changing / race loading
		/*
		public static void OnDestroyMatch(BasicResponse response)
		{
			Debug.Log("Match Destroyed" + response.ToString());
			if(NetworkManager.singleton){
				Destroy(GameObject.Find("Match Manager(Clone)"));

				NetworkManager.singleton.ServerChangeScene("Menu");

				NetworkManager.singleton.StopHost();
				NetworkManager.singleton.StopMatchMaker();
				NetworkManager.Shutdown();
//				Destroy(NetworkManager.singleton.GetComponent<SanicNetworkManager>().matchManager.gameObject);Match Manager(Clone)

				Destroy(GameObject.Find("SanicNetworkManager"));

				NetworkTransport.Shutdown();


			}

		} 
		*/

		
		// aparece el lobbyBall
		private void SpawnLobbyBall(MatchPlayerLocal player)
		{
//			var spawner = LobbyReferences.Active.BallSpawner;

			var spawner = FindObjectOfType<LobbyBallSpawnerLocal>();

			if (player.BallObject != null)
			{
				Destroy(player.BallObject.gameObject);
			}
			player.BallObject = spawner.SpawnBall(Data.PlayerType.Normal, player.CtrlType, player.CharacterId, player.Name);
		}







    }


	/// <summary>
	/// Represents a player and its personal settings.
	/// </summary>
	[Serializable]
	public class MatchPlayerLocal
	{
		private string name;
		private ControlType ctrlType;

		private bool readyToRace;

		public MatchPlayerLocal(string name, ControlType ctrlType, int initialCharacterId)
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
		public BallLocal BallObject { get; set; }

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