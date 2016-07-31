using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;

using System.Linq;


using Sanicball;
public class SanicNetworkManager : NetworkManager {

	public Sanicball.LobbyBallSpawner ballSpawner;
	public Sanicball.RaceBallSpawner raceSpawner;
	public Sanicball.RaceBallSpawner raceSpawnerPrefab;
	

	public Prototype.NetworkLobby.LobbyInfoPanel infoPanel;

	public MatchManager matchManagerPrefab;
	public MatchManager matchManager;
	public RaceManager raceManagerPrefab;
	public RaceManager raceManager;

	int PlayersInRace=0;

//	private Sanicball.Ball ballPrefab ;


	public Sanicball.BallType tipo;
	public Sanicball.ControlType controlTipo;
	public int personaje;
	public string nickName;
	public  bool isSpawning = false;


	// parece que esto solo corre en el servidor
	// aqui no hay que hacer networkserver.spawn
	public override void OnServerAddPlayer(NetworkConnection conn, short controllerID){
		Debug.Log("This section is triggered by ClientScene.AddPlayer");
		Sanicball.Ball ball;
		if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Lobby"){

			ball = (Sanicball.Ball)Instantiate( ballSpawner.ballPrefab, ballSpawner.transform.position, ballSpawner.transform.rotation);

		}else{

			raceSpawner = FindObjectOfType<RaceBallSpawner>();
			ball = (Sanicball.Ball)Instantiate( raceSpawner.ballPrefab, raceSpawner.transform.position, raceSpawner.transform.rotation);

		}

		NetworkServer.AddPlayerForConnection(conn, ball.gameObject, controllerID);

		if(ball.gameObject.GetComponent<NetworkIdentity>().isLocalPlayer ){

		}else{
			ball.ballConnection= conn;

		}



		if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Lobby"){

			if(ballSpawner.isClient && ballSpawner.isServer){

			}else{

			}
			ballSpawner.pelota = ball;
		}else{
			raceSpawner.pelota= ball;
		}

	}

	public bool CheckIfAllReady(){
		if(matchManager.Players.Count != (matchManager.serverConnections +1)){
			matchManager.AddNotInitializatedPlayer();
		}

		Debug.Log("Players in matchmanager.... --  "+ matchManager.Players.Count );
		Debug.Log("serverconnections..--  "+  (matchManager.serverConnections+1) );

		bool AllReady = matchManager.Players.All(pet =>pet.ConnectionSelf.isReady ==true && ( matchManager.Players.Count == (matchManager.serverConnections+1) ) );
		return AllReady;

	}

	public void InitRaceAllPlayers(){
		if(matchManager.isServer){
			Debug.Log("Players en matchmanager.... --  "+ matchManager.Players.Count );
			Debug.Log("CONECCIONES serverconnections..--  "+  (matchManager.serverConnections+1) );

			bool AllReady= CheckIfAllReady();

			if(AllReady){
				Debug.Log("Everyone are ready");

				raceManager.CurrentState = RaceState.Countdown;

			}else{
				Debug.Log("Someone is not ready");
			}
		}


	}

	public override  void OnStartHost()
	{	

	}



	void Start(){

		if(PlayerPrefs.GetInt("kicked")==1 ){
			PlayerPrefs.SetInt("kicked",0);

			infoPanel.Display("Race already started","close",null);

		}


		if(PlayerPrefs.GetInt("serverDisconnect")==1 ){
			PlayerPrefs.SetInt("serverDisconnect",0);

			infoPanel.Display("The Server was Disconnected","close",null);

		}



	}

	public override  void OnStartServer()
	{
		

	}



	public override void OnStartClient(   NetworkClient client ){

	}




	public override void OnClientConnect(NetworkConnection conn) {//		Instantiate(matchManager);

		base.OnClientConnect(conn);

		Debug.Log("Joining/Creating a Match"  +UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);

		//Kick player
		/*
		if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Lobby"){
			
			NetworkManager.singleton.ServerChangeScene("Menu");
			PlayerPrefs.SetInt("kicked",1);


			//		NetworkManager.singleton.ServerChangeScene("Menu");

			NetworkManager.singleton.StopHost();
			NetworkManager.singleton.StopMatchMaker();
			NetworkManager.Shutdown();
			NetworkTransport.Shutdown();
			Destroy(this.gameObject);



		}
		

		*/

	}

	private void OnLevelWasLoaded(int level)
	{
		if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "LobbyLocal"){
			Debug.Log("In Offline, NetworkManager needs to be shutdown");
			NetworkManager.Shutdown();
			NetworkTransport.Shutdown();
			Destroy(this.gameObject);
		}

	}

	public override void OnServerReady( NetworkConnection conn ){


		base.OnServerReady(conn);

		Debug.Log ("OnServerReady event : is Matchmanager server ?"  + matchManager.GetComponent<NetworkIdentity>().isServer);

		if(matchManager.isServer){
			if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Lobby"){


			}else{


			}

			if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name!="Lobby" && UnityEngine.SceneManagement.SceneManager.GetActiveScene().name!="Menu"){

			}

		}

	}

	public override void OnServerConnect (NetworkConnection conn){
		
	
		base.OnServerConnect(conn);

		Debug.Log("Someone is connecting to server");

		if(NetworkServer.connections.Count() >1)
			matchManager.serverConnections+=1;
			
		if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Lobby" && UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Menu"){

			if( raceManager.CurrentState != RaceState.Waiting){
				Debug.Log("Disconnecting player from match");

				for (int j=0; j< matchManager.Players.Count ; j ++){
					if( matchManager.Players[j].ConnectionSelf== conn  ){
						matchManager.Players.Remove( matchManager.Players[j] );
					}

				}

				// disconnecting a client
				conn.Disconnect();
				conn.Dispose();

			}else if( raceManager.CurrentState == RaceState.Waiting){

			}
		}else{
			if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Lobby"){
				if(matchManager.loadingStage){
					conn.Disconnect();
					conn.Dispose();
				}else{

					
				}

			}

		}

	}



	public void StartRaceOnline(){
		
		//this was needed on Unity 5.3, in Unity 5.4 create match receive all parameters directly in the constructor
//		CreateMatchRequest create = new CreateMatchRequest();
//		create.name = PlayerPrefs.GetString("nickname") + " Room" ;
//		create.size = 6 ;
//		create.advertise = true ;
//		create.password = "" ;
//		create.eloScore=0;
//		matchMaker.CreateMatch(create, MatchCreated);


		matchMaker.CreateMatch(PlayerPrefs.GetString("nickname") + " Room",6,true,"" ,"","",0,0,OnMatchCreated);

	}

	public void OnMatchCreated (bool success, string extendedInfo, MatchInfo matchInfo)
	{
		if (success)
		{

			StartHost(matchInfo) ;
			matchManager = Instantiate ((MatchManager)matchManagerPrefab);
			NetworkServer.Spawn(matchManager.gameObject);

		}else{
			infoPanel.Display("Mutiplayer Service not available for the moment","close",null);

			Debug.Log("Multiplayer Service not available for the moment....");
		}
	}

	public  void OnDestroyMatch(bool success, string extendedInfo)
	{
		Debug.Log("Match Destroyed" );
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



	public void StartClientOnline(){

		StartClient();

	}




	public override void OnServerSceneChanged(string sceneName){

		base.OnServerSceneChanged(sceneName);
		Debug.Log("Multiplayer Scene Was changed");
		if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Lobby" ){
			raceSpawner= Instantiate(raceSpawnerPrefab);
			raceSpawner.transform.position= GameObject.Find("SpawnpointPlaceHolder").transform.position;
			raceSpawner.transform.rotation= GameObject.Find("SpawnpointPlaceHolder").transform.rotation;
			NetworkServer.Spawn(raceSpawner.gameObject);

			matchMaker.SetMatchAttributes(matchInfo.networkId,false,0,OnSetMatchAttributes);
		}

	}

	public void OnMatchJoined (bool success, string extendedInfo, MatchInfo matchInfo){


		if (LogFilter.logDebug)
		{
			Debug.Log("NetworkManager OnMatchJoined ");
		}
		if (success)
		{
			try
			{
				//Utility.SetAccessTokenForNetwork(matchInfo.networkId, new UnityEngine.Networking.Types.NetworkAccessToken(matchInfo.accessTokenString));
			}
			catch(System.Exception ex)
			{
				if (LogFilter.logError)
				{
					Debug.LogError(ex);
				}
			}
			this.StartClient(matchInfo);
		}
		else if (LogFilter.logError)
		{
			infoPanel.Display("Right now, you cant join to a Match, please try later","close",null);

			Debug.LogError(string.Concat("Join Failed:", matchInfo));
		}



	}

	public void OnMatchList(bool success, string extendedInfo, List<MatchInfoSnapshot> matchList){

		GameObject.Find("Online panel").GetComponent<Sanicball.UI.OnlinePanel>().OnGUIMatchList(success, extendedInfo,matchList  ) ;

	}



	public override void OnStopClient(){

		Debug.Log("Client stoped");

	}

	public override void OnClientDisconnect(NetworkConnection conn){
		base.OnClientDisconnect(conn);
		// Esto si ocurre...
		Debug.Log("Server has been disconnected");
		PlayerPrefs.SetInt("serverDisconnect",1);

		NetworkManager.singleton.ServerChangeScene("Menu");

		NetworkManager.singleton.StopHost();
		NetworkManager.singleton.StopMatchMaker();
		NetworkManager.Shutdown();
		NetworkTransport.Shutdown();
		Destroy(this.gameObject);

	}

	public override void OnServerError(NetworkConnection conn, int errorCode){
		base.OnServerError(conn, errorCode);


	}

	public override void OnServerRemovePlayer(NetworkConnection conn, PlayerController player){
		base.OnServerRemovePlayer(conn, player);

	}

	public override void OnServerDisconnect (NetworkConnection conn){
		// esto ocurre...

		for (int j=0; j< matchManager.Players.Count ; j ++){
			if( matchManager.Players[j].ConnectionSelf== conn  ){
				matchManager.Players.Remove( matchManager.Players[j] );
			}

		}
		matchManager.serverConnections-=1;
		if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Lobby"){
			Sanicball.UI.PlayerPortrait[] iconos=  FindObjectsOfType<Sanicball.UI.PlayerPortrait>();

			for (int m=0; m< iconos.Length ; m ++){
				if(iconos[m].TargetPlayer.ball.connectionToClient ==conn){
					Destroy(iconos[m].gameObject);
				}
			}

			Marker[] avatars=  FindObjectsOfType<Marker>();

			for (int k=0; k< avatars.Length ; k ++){
				if(avatars[k].Target.GetComponent<Ball>() != null){
					if(avatars[k].Target.GetComponent<Ball>().connectionToClient ==conn){
						Debug.Log("Deleting Avatar UI");
						Destroy(avatars[k].gameObject);
					}
				}

			}

			Debug.Log("Client left Match, removing their UI elements");

			for (int j=0; j< raceManager.players.Count ; j ++){
				if(raceManager.players[j].ball.connectionToClient ==conn){

					NetworkInstanceId removedID= raceManager.players[j].ball.netId;
					raceManager.players.Remove(raceManager.players[j] );

					//eliminamos...
					for (int i=0;i < raceManager.players.Count; i++){
						raceManager.players[i].ball.RpcRemoveThePlayerFromList(removedID);
					}

				}
			}

			if(raceManager.CurrentState== RaceState.Waiting ){

			}

		}else if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Lobby"  ){

		}
			



		base.OnServerDisconnect (conn);

		Debug.Log("Client has gone");

	}

	/*

	public virtual void MatchJoined(JoinMatchResponse matchInfo)
	{
		Debug.Log("DE BU GEDEBUUU DEBUGEANANDP");
		if (LogFilter.logDebug)
		{
			Debug.Log("NetworkManager OnMatchJoined ");
		}
		if (matchInfo.success)
		{
			try
			{
//				Utility.SetAccessTokenForNetwork(matchInfo.networkId, new UnityEngine.Networking.Types.NetworkAccessToken(matchInfo.accessTokenString));
			}
			catch(System.Exception ex)
			{
				if (LogFilter.logError)
				{
					Debug.LogError(ex);
				}
			}
			this.StartClient(new MatchInfo(matchInfo));
		}
		else if (LogFilter.logError)
		{
			infoPanel.Display("Right now, you cant join to a Match, please try later","close",null);

			Debug.LogError(string.Concat("Join Failed:", matchInfo));
		}
	}

	*/




	public override void OnClientSceneChanged(NetworkConnection conn){

		base.OnClientSceneChanged(conn);

		if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Lobby"){

			if(matchManager){
				if(matchManager.Players.Count!=0){
					matchManager.replace=true;
					matchManager.firstTimeLoadingLobby = true;
					matchManager.LevelStartedFromRace();	
				}

				if( matchManager.isServer){
					NetworkServer.SpawnObjects();

					ballSpawner = GameObject.Find("BallSpawner").GetComponent<LobbyBallSpawner>();

				}else{

				}
				matchManager.inLobby= true;

			}

		}else{
			Debug.Log("Client Changed Scene");

			if(matchManager){
				matchManager.inLobby= false;
				matchManager.firstTimeLoadingLobby= false;

				if( !matchManager.isServer){

					NetworkServer.SpawnObjects();

					NetworkManager.singleton.GetComponent<SanicNetworkManager>().raceManager=
						Instantiate( NetworkManager.singleton.GetComponent<SanicNetworkManager>().raceManagerPrefab);

				}
			}
	
			if(!matchManager){
				//instantiating the racemanager when we are in a race and we are not the Server
				if( !GameObject.FindObjectOfType<MatchManager>().isServer ){

					matchManager = GameObject.FindObjectOfType<MatchManager>();
					matchManager.inLobby= false;
					matchManager.firstTimeLoadingLobby= false;


					NetworkManager.singleton.GetComponent<SanicNetworkManager>().raceManager=
						Instantiate( NetworkManager.singleton.GetComponent<SanicNetworkManager>().raceManagerPrefab);


				}

			}

		}


	}



}
