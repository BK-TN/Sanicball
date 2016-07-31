using System.Collections.Generic;
using System.Linq;
using UnityEngine.Networking;
using UnityEngine;
using UnityEngine.UI;

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
        public List<RacePlayer> players = new List<RacePlayer>();
        private RaceState currentState = RaceState.None;

        private Data.MatchSettings settings = new Data.MatchSettings();

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

		private bool WaitingTimerOn =true;
		private float WaitingTimer = 7;
		private float WaitingTimerGlobal = 400;

		private bool areAllReady;


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

        public RacePlayer this[int playerIndex]
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
//					NetworkServer.Spawn(raceUIPrefab.gameObject);
                        raceUI.TargetManager = this;
                        CreateBallObjects();

					if (NetworkManager.singleton.GetComponent<SanicNetworkManager>().matchManager.isServer    ){
						for (int i = 0; i < NetworkManager.singleton.GetComponent<SanicNetworkManager>().matchManager.Players.Count; i++){
							if (NetworkManager.singleton.GetComponent<SanicNetworkManager>().matchManager.Players[i].BallObject.isLocalPlayer)
								{
								NetworkManager.singleton.GetComponent<SanicNetworkManager>().matchManager.Players[i].BallObject.RpcCountdown();
							}
						}
					}
					NetworkManager.singleton.GetComponent<SanicNetworkManager>().matchManager.firstTimeLoadingLobby=false;
					NetworkManager.singleton.GetComponent<SanicNetworkManager>().isSpawning=false;

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
			FindObjectOfType<Sanicball.UI.PlayerUI>().GenerateRaceAvatar();

        }

        private void Start()
        {

            CurrentState = RaceState.Waiting;

        }

        private void CreateBallObjects()
        {
			int nextPos = 0;
            var matchManager = FindObjectOfType<MatchManager>();
            for (int i = 0; i < matchManager.Players.Count; i++)
            {
                var player = matchManager.Players[i];
				if(NetworkManager.singleton.GetComponent<SanicNetworkManager>().matchManager.isServer){
					player.BallObject = FindObjectOfType<RaceBallSpawner>().SpawnBall(nextPos, BallType.Player, player.CtrlType, player.CharacterId, player.Name  ,player.ConnectionSelf);
	                player.BallObject.CanMove = false;

					if(!player.BallObject.isLocalPlayer){
						player.BallObject.RpcSetCanMove(false);
						player.BallObject.RpcSetSettings(settings.Laps, settings.StageId);
					}else{

					}
				}
					player.BallObject.transform.gameObject.GetComponent<NetworkTransform>().sendInterval=.14f;

	            	var racePlayer = new RacePlayer(player.BallObject);
	            	players.Add(racePlayer);
				
	                if (matchManager.Players.Count == 1)
	                    racePlayer.LapRecordsEnabled = true;

	                racePlayer.FinishLinePassed += RacePlayer_FinishLinePassed;

				if(player.BallObject.isLocalPlayer){
	                var playerUI = Instantiate(playerUIPrefab);
	                playerUI.TargetPlayer = racePlayer;
	                playerUI.TargetManager = this;


	                int persistentIndex = i;
					player.BallObject.CameraCreatedEvent += (sender, e) =>
	                {
						playerUI.TargetCamera = e.CameraCreated2.AttachedCamera;
	                    var splitter = e.CameraCreated2.AttachedCamera.GetComponent<CameraSplitter>();
	                    if (splitter)
	                        splitter.SplitscreenIndex = persistentIndex;
	                };
					//in thispart the init is forced to keyboard...
					player.BallObject.Init(Sanicball.BallType.Player , Sanicball.ControlType.Keyboard, player.BallObject.CharacterId, player.BallObject.NickName   );

					if(player.BallObject.isServer ){
						player.BallObject.RpcInit( player.BallObject.Type, player.BallObject.CtrlType,player.BallObject.CharacterId, player.BallObject.NickName   );
					

					}
					if(!player.BallObject.isServer  && player.BallObject.isClient){
						player.BallObject.CmdInit(Sanicball.BallType.Player , Sanicball.ControlType.Keyboard, player.BallObject.CharacterId,player.BallObject.NickName   );

					}

					player.BallObject.cameraBall.SetDirection( StageReferences.Active.checkpoints[0].transform.rotation );

	                nextPos++;

				}

            }

			if(NetworkManager.singleton.GetComponent<SanicNetworkManager>().matchManager.isServer){
	            for (int i = 0; i < settings.AICount; i++)
		            {
		                var aiBall = FindObjectOfType<RaceBallSpawner>().SpawnBall(nextPos, BallType.AI, ControlType.None, settings.GetAICharacter(i), "AI #" + i);
		                aiBall.CanMove = false;
						aiBall.GetComponent<NetworkTransform>().sendInterval= .25f;

		                var racePlayer = new RacePlayer(aiBall);
		                players.Add(racePlayer);
		                racePlayer.FinishLinePassed += RacePlayer_FinishLinePassed;

		                nextPos++;
		            }
			}

			// Filling the raceManager of the clients
			if( !NetworkManager.singleton.GetComponent<SanicNetworkManager>().matchManager.isServer  ){
				
				Ball[] listaBalls;
				listaBalls = FindObjectsOfType<Ball>();
				for(int j=0; j < listaBalls.Length ;j++){
					if(!listaBalls[j].isLocalPlayer){
						var racePlayer2 = new RacePlayer(listaBalls[j]);
						racePlayer2.FinishLinePassed += RacePlayer_FinishLinePassed;
						players.Add(racePlayer2);
					}
				}

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

			if(NetworkManager.singleton){
				
				if(NetworkManager.singleton.GetComponent<SanicNetworkManager>().matchManager.isServer){// debo agregar inputs de touch mobile...

					if (CurrentState == RaceState.Waiting){

						if (WaitingTimerOn && areAllReady == false )
						{
							float prevSecond;
							prevSecond =Mathf.Ceil(WaitingTimerGlobal);
							WaitingTimerGlobal -= Time.deltaTime;

							if(Mathf.Ceil(WaitingTimerGlobal)!=prevSecond){
								areAllReady = NetworkManager.singleton.GetComponent<SanicNetworkManager>().CheckIfAllReady();
							}

							if(areAllReady){
								Debug.Log("---------WE CAN START The RACE , press Jump");
							}

						}

						if (WaitingTimerOn && areAllReady == true )
						{

							float prevSecond2;
							prevSecond2 =Mathf.Ceil(WaitingTimer);
							WaitingTimer -= Time.deltaTime;

							if(Mathf.Ceil(WaitingTimer)!=prevSecond2){
								GameObject.Find("Waiting UI(Clone)").transform.FindChild("Instruction text (1)").GetComponent<Text>().text="Starting Race in.. "+ Mathf.Ceil(WaitingTimer) ;
							}

						}

						if (WaitingTimer <= 0)
						{					

							NetworkManager.singleton.GetComponent<SanicNetworkManager>().InitRaceAllPlayers();
							WaitingTimerOn=false;
							WaitingTimer= 7;

						}

					}

					if ( (CurrentState == RaceState.Waiting && areAllReady) && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton0)))
		            {
						NetworkManager.singleton.GetComponent<SanicNetworkManager>().InitRaceAllPlayers();

		            }

				}
			}

            if (raceTimerOn)
            {
                raceTimer += Time.deltaTime;
                foreach (var p in players) p.UpdateTimer(Time.deltaTime);
            }

			if(NetworkManager.singleton){
				
	            players = players.OrderByDescending(a => a.CalculateRaceProgress()).ToList();
	            for (int i = 0; i < players.Count; i++)
	                players[i].Position = i + 1;
			}
        }
    }
}