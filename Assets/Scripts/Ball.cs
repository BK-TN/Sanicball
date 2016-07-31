using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;


namespace Sanicball
{
    public enum BallType
    {
        Player,
        LobbyPlayer,
        AI
    }

	public class CheckpointPassArgs : System.EventArgs
    {
        public CheckpointPassArgs(Checkpoint c)
        {
            CheckpointPassed = c;
        }

        public Checkpoint CheckpointPassed { get; private set; }
    }

    public class CameraCreationArgs : System.EventArgs
    {
		public CameraCreationArgs(IBallCamera cameraCreated, GameObject oldCamera, GameObject newCamera)
        {
            CameraCreated2 = cameraCreated;// aqui usamos el private set...
			OldCamera2= oldCamera;
			NewCamera2  = newCamera;
        }

        public IBallCamera CameraCreated2 { get; private set; }
		public GameObject OldCamera2 { get; private set; }
		public GameObject NewCamera2  { get; private set; }

    }

    [System.Serializable]
    public class BallMotionSounds
    {
        [SerializeField]
        private AudioSource jump;
        [SerializeField]
        private AudioSource roll;
        [SerializeField]
        private AudioSource speedNoise;
        [SerializeField]
        private AudioSource brake;

        public AudioSource Jump { get { return jump; } }
        public AudioSource Roll { get { return roll; } }
        public AudioSource SpeedNoise { get { return speedNoise; } }
        public AudioSource Brake { get { return brake; } }
    }

    [System.Serializable]
    public class BallPrefabs
    {
        [SerializeField]
        private DriftySmoke smoke;
        [SerializeField]
        private OmniCamera camera;
        [SerializeField]
        private PivotCamera oldCamera;
        [SerializeField]
        private AITarget aiTarget;

        public DriftySmoke Smoke { get { return smoke; } }
        public OmniCamera Camera { get { return camera; } }
        public PivotCamera OldCamera { get { return oldCamera; } }
        public AITarget AiTarget { get { return aiTarget; } }
    }

    [RequireComponent(typeof(Rigidbody))]
	public class Ball : NetworkBehaviour
    {

//		public Material characterMat;

        //These are set using Init() when balls are instantiated
        //But you can set them from the editor to quickly test out a track
        [Header("Initial stats")]
        [SerializeField]
		[SyncVar]
        private BallType type;
        [SerializeField]
		[SyncVar]
        private ControlType ctrlType;
        [SerializeField]
		[SyncVar(hook="OnSyncMyChar")]
        private int characterId;
		[SerializeField]
		[SyncVar]
        private string nickname;

		public IBallCamera cameraBall;
		private bool  respawnMe;
		public NetworkConnection ballConnection;
		public int currentCheckPoin;


        public BallType Type { get { return type; } }
        public ControlType CtrlType { get { return ctrlType; } }
        public int CharacterId { get { return characterId; } }
		public string NickName { get { return nickname; } }


        [Header("Subcategories")]
        [SerializeField]
        private BallPrefabs prefabs;
        [SerializeField]
        private BallMotionSounds sounds;

        //State
        private BallStats characterStats;
        private bool canMove = true;
        private BallControlInput input;
        private bool grounded = false;
        private float groundedTimer = 0;
        private DriftySmoke smoke;

        public bool CanMove { get { return canMove; } set { canMove = value; } }
        public Vector3 DirectionVector { get; set; }
        public Vector3 Up { get; set; }
        public bool Brake { get; set; }

        //Component caches
        private Rigidbody rb;
        public BallControlInput Input { get { return input; } }

        //Events
        public event System.EventHandler<CheckpointPassArgs> CheckpointPassed;
        public event System.EventHandler RespawnRequested;
        public event System.EventHandler<CameraCreationArgs> CameraCreatedEvent;
		public event System.EventHandler SwitchCamerasEvent;


		//

		public void OnSyncMyChar(int value){
//			Debug.Log("srincronizando mi ID HOOK o WATERVERTRERE");

		}
        public void Jump()
        {
			if(isLocalPlayer){
	            if (grounded && CanMove)
	            {
	                rb.AddForce(Up * characterStats.jumpHeight, ForceMode.Impulse);
	                if (sounds.Jump != null)
	                {
	                    sounds.Jump.Play();
	                }
	                grounded = false;
	            }
			}
		
        }
		public override void OnStartLocalPlayer(){
			
			if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Lobby"){

				if(!isServer){
//					NetworkManager.singleton.GetComponent<SanicNetworkManager>().raceManager=
//						Instantiate( NetworkManager.singleton.GetComponent<SanicNetworkManager>().raceManagerPrefab);
					
//					NetworkManager.singleton.GetComponent<SanicNetworkManager>().raceManager.currentState=Sanicball.RaceState.Countdown;

				}


			}
				
			// esto corre cada vez que instancio a mi proppia pelota, en mi propia identidad.
			//no se instancia con el vecino...
			Debug.Log("Client as been started Locally");


				this.type = GameObject.Find("SanicNetworkManager").GetComponent<SanicNetworkManager>().tipo;
				this.ctrlType = 	GameObject.Find("SanicNetworkManager").GetComponent<SanicNetworkManager>().controlTipo;
				this.characterId = 	GameObject.Find("SanicNetworkManager").GetComponent<SanicNetworkManager>().personaje;
				this.nickname = GameObject.Find("SanicNetworkManager").GetComponent<SanicNetworkManager>().nickName;

			
			if(!isServer  ){
					
					if(  NetworkManager.singleton.GetComponent<SanicNetworkManager>().matchManager.Players.Count>0 ){
						if( NetworkManager.singleton.GetComponent<SanicNetworkManager>().matchManager.Players[0].BallObject == null ){

							if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Lobby" ){

								CmdInit( this.type,this.ctrlType, this.characterId, this.nickname );
							}

						}
						else{

						}


					}else{
					if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Lobby" ){
						Debug.Log("There are not initialized local players, the player didnt choose a character.");

						if(isLocalPlayer){
							var p = new MatchPlayer("Player._.", ControlType.Keyboard, 0);

							NetworkManager.singleton.GetComponent<SanicNetworkManager>().matchManager.Players.Add(p);
					
						}


					}
				}
					

				CmdSetMatchManager();

				NetworkManager.singleton.GetComponent<SanicNetworkManager>().matchManager.Players[0].BallObject= this.GetComponent<Ball>();

		

			}else{
				Debug.Log("As Im server, I need to send RpcInit to star the balls in clients" );

				RpcInit(this.type,this.ctrlType, this.characterId, this.nickname );
			}


		}


		public void MyListener(bool value)
		{
	
			if(	NetworkManager.singleton.GetComponent<SanicNetworkManager>().isSpawning !=true	){
				if(!isServer){

					CmdToggleReady(Sanicball.Data.ActiveData.GameSettings.nickname);

					LobbyReferences.Active.CountdownField.enabled = true;
					LobbyReferences.Active.CountdownField.text = "Wait Until the Race Begin...";

				}else{
					Debug.Log("Ready ?? + "  + isServer);

					if (NetworkManager.singleton.GetComponent<SanicNetworkManager>().matchManager != null  && ( NetworkManager.singleton.GetComponent<SanicNetworkManager>().matchManager.serverConnections+1 )>1)
					{
						
						if(value){
							ChatRelayer.Instance.SetLogMessage(Sanicball.Data.ActiveData.GameSettings.nickname + " is READY to RACE");
						}else{
							ChatRelayer.Instance.SetLogMessage(Sanicball.Data.ActiveData.GameSettings.nickname + " is NOT READY to RACE");
						}

						//				for( int i =0 ; i< NetworkServer.connections.Count; i ++  ){
						for( int i =0 ; i< NetworkManager.singleton.GetComponent<SanicNetworkManager>().matchManager.Players.Count ; i ++  ){


							if(this.connectionToClient.connectionId == 	NetworkManager.singleton.GetComponent<SanicNetworkManager>().matchManager.Players[i].ConnectionSelf.connectionId ){

								NetworkManager.singleton.GetComponent<SanicNetworkManager>().matchManager.Players[i].ReadyToRace =value;

							}

						}

					}

				}

		}



		}


        public void RequestRespawn()
        {
            if (RespawnRequested != null)
                RespawnRequested(this, System.EventArgs.Empty);
        }



		public void SwitchCameraBall()
		{
			if (SwitchCamerasEvent != null)
				SwitchCamerasEvent(this, System.EventArgs.Empty);
		}




        public void Init(BallType type, ControlType ctrlType, int characterId, string nickname)
        {

            this.type = type;
            this.ctrlType = ctrlType;
            this.characterId = characterId;
            this.nickname = nickname;

			Up = Vector3.up;

			//Set up drifty smoke
			smoke = Instantiate(prefabs.Smoke);
			smoke.target = this;
			smoke.DriftAudio = sounds.Brake;

			//Grab reference to Rigidbody
			rb = GetComponent<Rigidbody>();
			//Set angular velocity (This is necessary for fast)
			rb.maxAngularVelocity = 1000f;

			//Set object name

			gameObject.name =  nickname;




			//Set character
			if (CharacterId >= 0 && CharacterId < Data.ActiveData.Characters.Length)
			{
				SetCharacter(Data.ActiveData.Characters[CharacterId]);
			}
				
			//Create objects and components based on ball type
			if (type == BallType.Player && cameraBall ==null && isLocalPlayer)
			{

				OmniCamera newCam =Instantiate(prefabs.Camera) ;
				newCam.Target = rb;
				newCam.CtrlType= ctrlType;
				PivotCamera oldCam= Instantiate(prefabs.OldCamera); 
				oldCam.Target = rb;
				oldCam.CtrlType= ctrlType;

				//Create camera
					
				if (Data.ActiveData.GameSettings.useOldControls)
				{

					cameraBall = oldCam;
					newCam.gameObject.SetActive(false);

					((PivotCamera)cameraBall).UseMouse = ctrlType == ControlType.Keyboard;


					}
					else
					{
					cameraBall= newCam;
					oldCam.gameObject.SetActive(false);
						

					}
				cameraBall.Target = rb;
				cameraBall.CtrlType = ctrlType;
				//_______________----------------
				if (CameraCreatedEvent != null){
					CameraCreatedEvent(this, new CameraCreationArgs(cameraBall, oldCam.gameObject, newCam.gameObject ));
				}else{

				}
			}
			if (type == BallType.LobbyPlayer)
			{
				//Make the lobby camera follow this ball
				var cam = FindObjectOfType<LobbyCamera>();
				if (cam)
				{
					cam.AddBall(this);
				}
			}
			if ((type == BallType.Player || type == BallType.LobbyPlayer) && input==null)
			{
				//Create input component
				input = gameObject.AddComponent<BallControlInput>();
			}
			if (type == BallType.AI)
			{

				var ai = gameObject.AddComponent<BallControlAI>();
				ai.pathToFollow = FindObjectOfType<Path>();

				//Create target for the AI
				AITarget pFollower = Instantiate(prefabs.AiTarget);
				pFollower.GetComponent<PathFollower>().path = ai.pathToFollow;
				pFollower.stupidness = ai.stupidness;
				pFollower.GetComponent<Patroller>().target = gameObject;
				ai.target = pFollower;
			}



        }
		[Command]
		public void CmdInit(BallType type, ControlType ctrlType, int characterId, string nickname)
		{
			Debug.Log("Init Character with Command" + characterId);

			this.type = type;
			this.ctrlType = ctrlType;
			this.characterId = characterId;
			this.nickname = nickname;
			Up = Vector3.up;

			//Set up drifty smoke
			smoke = Instantiate(prefabs.Smoke);
			smoke.target = this;
			smoke.DriftAudio = sounds.Brake;

			//Grab reference to Rigidbody
			rb = GetComponent<Rigidbody>();
			//Set angular velocity (This is necessary for fast)
			rb.maxAngularVelocity = 1000f;

			//Set object name
			// le damos el nombre al Ball spawneado
			gameObject.name =  nickname;


			//Set character
			if (CharacterId >= 0 && CharacterId < Data.ActiveData.Characters.Length)
			{
				SetCharacter(Data.ActiveData.Characters[CharacterId]);
			}

			//Create objects and components based on ball type
			if (type == BallType.Player && isLocalPlayer)
			{

				OmniCamera newCam =Instantiate(prefabs.Camera) ;
				newCam.Target = rb;
				newCam.CtrlType= ctrlType;
				PivotCamera oldCam= Instantiate(prefabs.OldCamera); 
				oldCam.Target = rb;
				oldCam.CtrlType= ctrlType;


				//Create camera
				if (Data.ActiveData.GameSettings.useOldControls)
				{
					cameraBall= oldCam;
					newCam.gameObject.SetActive(false);

					((PivotCamera)cameraBall).UseMouse = ctrlType == ControlType.Keyboard;

				}
				else
				{
					cameraBall= newCam;
					oldCam.gameObject.SetActive(false);


				}
				cameraBall.Target = rb;
				cameraBall.CtrlType = ctrlType;

				if (CameraCreatedEvent != null)
					CameraCreatedEvent(this, new CameraCreationArgs(cameraBall, oldCam.gameObject, newCam.gameObject));
			}
			if (type == BallType.LobbyPlayer)
			{
				//Make the lobby camera follow this ball
				var cam = FindObjectOfType<LobbyCamera>();
				if (cam)
				{
					cam.AddBall(this);
				}
			}
			if ((type == BallType.Player || type == BallType.LobbyPlayer) && input == null)
			{
				//Create input component
				input = gameObject.AddComponent<BallControlInput>();
			}
			if (type == BallType.AI)
			{
				//Create AI component
				var ai = gameObject.AddComponent<BallControlAI>();
				ai.pathToFollow = FindObjectOfType<Path>();

				//Create target for the AI
				AITarget pFollower = Instantiate(prefabs.AiTarget);
				pFollower.GetComponent<PathFollower>().path = ai.pathToFollow;
				pFollower.stupidness = ai.stupidness;
				pFollower.GetComponent<Patroller>().target = gameObject;
				ai.target = pFollower;
			}


			RpcInit( type, ctrlType ,characterId,nickname);

		}
		[Command]
		public void CmdSetMatchManager( ){

			if( isClient && !isLocalPlayer ){
				bool repeated=false;
				for( int i =0 ; i< NetworkServer.connections.Count; i ++  ){

					if(NetworkServer.connections[i]!=null){
						if(this.connectionToClient.connectionId == NetworkServer.connections[i].connectionId )
							Debug.Log("THE CONNECTION WAS FOUND  "+ this.connectionToClient.connectionId);
							
					}
				}

				for( int i =0;i< NetworkServer.connections.Count; i ++  )
				{
					if( NetworkServer.connections[i]!=null ){
					
						if(this.connectionToClient.connectionId ==NetworkServer.connections[i].connectionId  )
						{//HE CONNECTION WAS FOUND  in the SERVER..
							for( int j =0 ; j< NetworkManager.singleton.GetComponent<SanicNetworkManager>().matchManager.Players.Count; j ++  )
							{

								if( this.connectionToClient.connectionId == NetworkManager.singleton.GetComponent<SanicNetworkManager>().matchManager.Players[j].ConnectionSelf.connectionId )
								{
									repeated =true;

								}

							}

							if(!repeated)
							{
								MatchPlayer p = new MatchPlayer(this.name, this.ctrlType, this.characterId);
								p.CharacterId=this.characterId;
								p.ConnectionSelf = NetworkServer.connections[i];
								p.BallObject= GetComponent<Ball>();
								p= NetworkManager.singleton.GetComponent<SanicNetworkManager>().matchManager.AddReadyEvent(p);

								NetworkManager.singleton.GetComponent<SanicNetworkManager>().matchManager.Players.Add(p);

							}
						}
					}

				}
			}


			/*
			if( NetworkManager.singleton.GetComponent<SanicNetworkManager>().matchManager.Players.Count==1)
			{
				
				MatchPlayer p = new MatchPlayer(this.name, this.ctrlType, this.characterId);

				NetworkManager.singleton.GetComponent<SanicNetworkManager>().matchManager.Players.Add(p);
					Debug.Log("knknknkjnkjnknkjnkjnkjnk2");
			}
			if( NetworkManager.singleton.GetComponent<SanicNetworkManager>().matchManager.Players.Count==2){
				NetworkManager.singleton.GetComponent<SanicNetworkManager>().matchManager.Players[1].CharacterId= this.characterId;
				NetworkManager.singleton.GetComponent<SanicNetworkManager>().matchManager.Players[1].ConnectionSelf= NetworkServer.connections[1];

				Debug.Log("THE CONNECTION  "+ NetworkManager.singleton.GetComponent<SanicNetworkManager>().matchManager.Players[1].ConnectionSelf);




			}
*/



		}
		[Command]
		public void CmdSetcharacterFromBall( int cha ){


			for( int i =0;i< NetworkServer.connections.Count; i ++  )
			{

				if(this.connectionToClient.connectionId ==NetworkServer.connections[i].connectionId  )
				{//HE CONNECTION WAS FOUND  in the SERVER..
					for( int j =0 ; j< NetworkManager.singleton.GetComponent<SanicNetworkManager>().matchManager.Players.Count; j ++  )
					{

						if( this.connectionToClient.connectionId == NetworkManager.singleton.GetComponent<SanicNetworkManager>().matchManager.Players[j].ConnectionSelf.connectionId )
						{
							NetworkManager.singleton.GetComponent<SanicNetworkManager>().matchManager.SetCharacter(
								NetworkManager.singleton.GetComponent<SanicNetworkManager>().matchManager.Players[j], cha);
							


						}

					}

				}

			}


		}

		[ClientRpc]
		public void RpcSetSettings( int laps, int stageID){
			if(!isServer)
				NetworkManager.singleton.GetComponent<SanicNetworkManager>().raceManager.Settings.SetClientValues( laps, stageID );

		}

		[Command]
		public void	CmdSpawnMe(   ){

			NetworkServer.ReplacePlayerForConnection(  NetworkClient.allClients[0].connection  , this.gameObject, 0   ); // ESTO FU

		}

		void OnApplicationQuit() {
			Debug.Log("Application ending after " + Time.time + " seconds");

			if(isServer){

				NetworkManager.singleton.StopHost();
			}else{
				NetworkManager.singleton.StopClient();
			}
		}

		[Command]
		public void CmdToggleReady(string namePlayerReady){

			if (NetworkManager.singleton.GetComponent<SanicNetworkManager>().matchManager != null)
			{

				for( int i =0 ; i< NetworkManager.singleton.GetComponent<SanicNetworkManager>().matchManager.Players.Count ; i ++  ){
						
					if(this.connectionToClient.connectionId == 	NetworkManager.singleton.GetComponent<SanicNetworkManager>().matchManager.Players[i].ConnectionSelf.connectionId ){

						NetworkManager.singleton.GetComponent<SanicNetworkManager>().matchManager.Players[i].ReadyToRace = !NetworkManager.singleton.GetComponent<SanicNetworkManager>().matchManager.Players[i].ReadyToRace;

						if(NetworkManager.singleton.GetComponent<SanicNetworkManager>().matchManager.Players[i].ReadyToRace){
							ChatRelayer.Instance.SetLogMessage(Sanicball.Data.ActiveData.GameSettings.nickname + " is READY to RACE");
						}else{
							ChatRelayer.Instance.SetLogMessage(Sanicball.Data.ActiveData.GameSettings.nickname + " is NOT READY to RACE");
						}

					}

				}

			}

		}

		[ClientRpc]
		public void RpcInit(BallType type, ControlType ctrlType, int characterId, string nickname)
		{

			this.type = type;
			this.ctrlType = ctrlType;
			this.characterId = characterId;
			this.nickname = nickname;


			Debug.Log("We re running RPCInit character is _ " + this.characterId );

			Up = Vector3.up;

			//Set up drifty smoke
			smoke = Instantiate(prefabs.Smoke);
			smoke.target = this;
			smoke.DriftAudio = sounds.Brake;

			//Grab reference to Rigidbody
			rb = GetComponent<Rigidbody>();
			//Set angular velocity (This is necessary for fast)
			rb.maxAngularVelocity = 1000f;

			//Set object name
			gameObject.name =  nickname;

			//Set character
			if (CharacterId >= 0 && CharacterId < Data.ActiveData.Characters.Length)
			{
				SetCharacter(Data.ActiveData.Characters[CharacterId]);
			}

			//Create objects and components based on ball type
			if (type == BallType.Player && cameraBall ==null && isLocalPlayer)
			{

				OmniCamera newCam =Instantiate(prefabs.Camera) ;
				newCam.Target = rb;
				newCam.CtrlType= ctrlType;
				PivotCamera oldCam= Instantiate(prefabs.OldCamera); 
				oldCam.Target = rb;
				oldCam.CtrlType= ctrlType;

				//Create camera
				if (Data.ActiveData.GameSettings.useOldControls)
				{
					cameraBall= oldCam;
					newCam.gameObject.SetActive(false);

					((PivotCamera)cameraBall).UseMouse = ctrlType == ControlType.Keyboard;

				}
				else
				{
					cameraBall= newCam;
					oldCam.gameObject.SetActive(false);
				}
				cameraBall.Target = rb;
				cameraBall.CtrlType = ctrlType;

				if (CameraCreatedEvent != null)
					CameraCreatedEvent(this, new CameraCreationArgs(cameraBall, oldCam.gameObject, newCam.gameObject));
			}
			if (type == BallType.LobbyPlayer)
			{
				//Make the lobby camera follow this ball
				var cam = FindObjectOfType<LobbyCamera>();
				if (cam)
				{
					cam.AddBall(this);
				}
			}
			if ((type == BallType.Player || type == BallType.LobbyPlayer && isLocalPlayer) && input == null)
			{
				//Create input component
				input = gameObject.AddComponent<BallControlInput>();
			}
			if (type == BallType.AI)
			{

				//Create AI component
				var ai = gameObject.AddComponent<BallControlAI>();
				ai.pathToFollow = FindObjectOfType<Path>();

				//Create target for the AI
				AITarget pFollower = Instantiate(prefabs.AiTarget);
				pFollower.GetComponent<PathFollower>().path = ai.pathToFollow;
				pFollower.stupidness = ai.stupidness;
				pFollower.GetComponent<Patroller>().target = gameObject;
				ai.target = pFollower;
			}

			if(!isServer){

			}

		}

        private void Start()
		{
			if(!isLocalPlayer && !isServer){
	            Up = Vector3.up;
	            //Set up drifty smoke
//	            smoke = Instantiate(prefabs.Smoke);
//	            smoke.target = this;
//	            smoke.DriftAudio = sounds.Brake;

	            //Grab reference to Rigidbody
	            rb = GetComponent<Rigidbody>();
	            //Set angular velocity (This is necessary for fast)
	            rb.maxAngularVelocity = 1000f;

	            //Set object name

				gameObject.name =  nickname;

	            //Set character
	            if (CharacterId >= 0 && CharacterId < Data.ActiveData.Characters.Length)
	            {
	                SetCharacter(Data.ActiveData.Characters[CharacterId]);
	            }

	            //Create objects and components based on ball type
				if (type == BallType.Player && cameraBall ==null && isLocalPlayer)
					
	            {


					OmniCamera newCam =Instantiate(prefabs.Camera) ;
					newCam.Target = rb;
					newCam.CtrlType= ctrlType;
					PivotCamera oldCam= Instantiate(prefabs.OldCamera); 
					oldCam.Target = rb;
					oldCam.CtrlType= ctrlType;

	                //Create camera
	                if (Data.ActiveData.GameSettings.useOldControls)
	                {
						cameraBall = oldCam;
						newCam.gameObject.SetActive(false);

						((PivotCamera)cameraBall).UseMouse = ctrlType == ControlType.Keyboard;
	                }
	                else
	                {
						cameraBall = newCam;
						oldCam.gameObject.SetActive(false);

	                }
					cameraBall.Target = rb;
					cameraBall.CtrlType = ctrlType;

	                if (CameraCreatedEvent != null)
						CameraCreatedEvent(this, new CameraCreationArgs(cameraBall , oldCam.gameObject, newCam.gameObject));
	            }
	            if (type == BallType.LobbyPlayer)
	            {
	                //Make the lobby camera follow this ball
	                var cam = FindObjectOfType<LobbyCamera>();
	                if (cam)
	                {
	                    cam.AddBall(this);
	                }
	            }
				if ((type == BallType.Player || type == BallType.LobbyPlayer)  && isLocalPlayer && input ==null)
	            {
	                //Create input component
	                input = gameObject.AddComponent<BallControlInput>();
	            }
	            if (type == BallType.AI)
	            {

	            }

			}

        }


		[Command]
		public void CmdNetworkDestroy(){
			ClientScene.RemovePlayer(playerControllerId);
			NetworkServer.Destroy(this.gameObject);
		}

        private void SetCharacter(Data.CharacterInfo c)
        {
            GetComponent<Renderer>().material = c.material;

            GetComponent<TrailRenderer>().material = c.trail;
            transform.localScale = new Vector3(c.ballSize, c.ballSize, c.ballSize);
            if (c.alternativeMesh != null)
            {
                GetComponent<MeshFilter>().mesh = c.alternativeMesh;
            }
            //set collision mesh too
            if (c.collisionMesh != null)
            {
                if (c.collisionMesh.vertexCount <= 255)
                {
                    Destroy(GetComponent<Collider>());
                    MeshCollider mc = gameObject.AddComponent<MeshCollider>();
                    mc.sharedMesh = c.collisionMesh;
                    mc.convex = true;
                }
                else
                {
                    Debug.LogWarning("Vertex count for " + c.name + "'s collision mesh is bigger than 255!");
                }
            }
            Ball motion = GetComponent<Ball>();
            if (motion != null)
                motion.characterStats = c.stats;
        }

        private void FixedUpdate()
        {
			if(type==BallType.AI && isServer ){
				if (CanMove)
				{
					//If grounded use torque
					if (DirectionVector != Vector3.zero)
					{
						rb.AddTorque(DirectionVector * characterStats.rollSpeed);
					}
					//If not use both
					if (!grounded)
					{
						rb.AddForce((Quaternion.Euler(0, -90, 0) * DirectionVector) * characterStats.airSpeed);
					}
				}
				else
				{
					//Always brake when canControl is off
					Brake = true;
				}

				//Braking
				if (Brake)
				{
					//Force ball to brake by resetting angular velocity every update
					rb.angularVelocity = Vector3.zero;
				}

				// Downwards torque for extra grip - currently not used
				if (grounded)
				{
					//rigidbody.AddForce(-up*stats.grip * (rigidbody.velocity.magnitude/400)); //Downwards gravity to increase grip
					//Debug.Log(stats.grip * Mathf.Pow(rigidbody.velocity.magnitude/100,2));
				}
			}


			if( (!isLocalPlayer  ) )
				return;
			
	            if (CanMove)
	            {
	                //If grounded use torque
	                if (DirectionVector != Vector3.zero)
	                {
	                    rb.AddTorque(DirectionVector * characterStats.rollSpeed);
	                }
	                //If not use both
	                if (!grounded)
	                {
	                    rb.AddForce((Quaternion.Euler(0, -90, 0) * DirectionVector) * characterStats.airSpeed);
	                }
	            }
	            else
	            {
	                //Always brake when canControl is off
	                Brake = true;
	            }

	            //Braking
	            if (Brake)
	            {
	                //Force ball to brake by resetting angular velocity every update
	                rb.angularVelocity = Vector3.zero;
	            }

	            // Downwards torque for extra grip - currently not used
	            if (grounded)
	            {
	                //rigidbody.AddForce(-up*stats.grip * (rigidbody.velocity.magnitude/400)); //Downwards gravity to increase grip
	                //Debug.Log(stats.grip * Mathf.Pow(rigidbody.velocity.magnitude/100,2));
	            }
			
        }

        private void Update()
	    {
			if(type==BallType.AI && isServer && !isClient){
				//Rolling sounds
				if (grounded)
				{
					float rollSpd = Mathf.Clamp(rb.angularVelocity.magnitude / 230, 0, 16);
					float vel = (-128f + rb.velocity.magnitude) / 256; //Start at 128 fph, end at 256

					vel = Mathf.Clamp(vel, 0, 1);
					if (sounds.Roll != null)
					{
						sounds.Roll.pitch = Mathf.Max(rollSpd, 0.8f);
						sounds.Roll.volume = Mathf.Min(rollSpd, 1);
					}
					if (sounds.SpeedNoise != null)
					{
						sounds.SpeedNoise.pitch = 0.8f + vel;
						sounds.SpeedNoise.volume = vel;
					}
				}
				else
				{
					//Fade sounds out when in the air
					if (sounds.Roll != null && sounds.Roll.volume > 0)
					{
						sounds.Roll.volume = Mathf.Max(0, sounds.Roll.volume - 0.2f);
					}
					if (sounds.SpeedNoise != null && sounds.SpeedNoise.volume > 0)
					{
						sounds.SpeedNoise.volume = Mathf.Max(0, sounds.SpeedNoise.volume - 0.01f);
					}
				}

				//Grounded timer
				if (groundedTimer > 0)
				{
					groundedTimer = Mathf.Max(0, groundedTimer - Time.deltaTime);
					if (groundedTimer <= 0)
					{
						grounded = false;
						Up = Vector3.up;
					}
				}

				//Smoke
				if (smoke != null)
				{
					smoke.grounded = grounded;
				}

			}

			if(!isLocalPlayer )
				return;

			if (GameInput.IsRespawning(ctrlType)){


				if(	NetworkManager.singleton.GetComponent<SanicNetworkManager>().isSpawning !=true	){
					if(!isServer){

			
						CmdToggleReady(Sanicball.Data.ActiveData.GameSettings.nickname);



						LobbyReferences.Active.CountdownField.enabled = true;
						LobbyReferences.Active.CountdownField.text = "Wait Until the Race Begin...";




					}else{


						Debug.Log("Ready ?? + "  + isServer);

						if (NetworkManager.singleton.GetComponent<SanicNetworkManager>().matchManager != null  && ( NetworkManager.singleton.GetComponent<SanicNetworkManager>().matchManager.serverConnections+1 )>1)
						{

	

							//				for( int i =0 ; i< NetworkServer.connections.Count; i ++  ){
							for( int i =0 ; i< NetworkManager.singleton.GetComponent<SanicNetworkManager>().matchManager.Players.Count ; i ++  ){


								if(this.connectionToClient.connectionId == 	NetworkManager.singleton.GetComponent<SanicNetworkManager>().matchManager.Players[i].ConnectionSelf.connectionId ){

									NetworkManager.singleton.GetComponent<SanicNetworkManager>().matchManager.Players[i].ReadyToRace = !NetworkManager.singleton.GetComponent<SanicNetworkManager>().matchManager.Players[i].ReadyToRace;

									if(NetworkManager.singleton.GetComponent<SanicNetworkManager>().matchManager.Players[i].ReadyToRace){
										ChatRelayer.Instance.SetLogMessage(Sanicball.Data.ActiveData.GameSettings.nickname + " is READY to RACE");
									}else{
										ChatRelayer.Instance.SetLogMessage(Sanicball.Data.ActiveData.GameSettings.nickname + " is NOT READY to RACE");
									}

								}

							}

						}



					}


				}else{
					if ( this.CanMove)
					{
						if(isServer){
							this.RequestRespawn();
						}else{
							CmdRequestSpawn(currentCheckPoin,netId);
						}
					}
				}
			}

	            //Rolling sounds
	            if (grounded)
	            {
	                float rollSpd = Mathf.Clamp(rb.angularVelocity.magnitude / 230, 0, 16);
	                float vel = (-128f + rb.velocity.magnitude) / 256; //Start at 128 fph, end at 256

	                vel = Mathf.Clamp(vel, 0, 1);
	                if (sounds.Roll != null)
	                {
	                    sounds.Roll.pitch = Mathf.Max(rollSpd, 0.8f);
	                    sounds.Roll.volume = Mathf.Min(rollSpd, 1);
	                }
	                if (sounds.SpeedNoise != null)
	                {
	                    sounds.SpeedNoise.pitch = 0.8f + vel;
	                    sounds.SpeedNoise.volume = vel;
	                }
	            }
	            else
	            {
	                //Fade sounds out when in the air
	                if (sounds.Roll != null && sounds.Roll.volume > 0)
	                {
	                    sounds.Roll.volume = Mathf.Max(0, sounds.Roll.volume - 0.2f);
	                }
	                if (sounds.SpeedNoise != null && sounds.SpeedNoise.volume > 0)
	                {
	                    sounds.SpeedNoise.volume = Mathf.Max(0, sounds.SpeedNoise.volume - 0.01f);
	                }
	            }

	            //Grounded timer
	            if (groundedTimer > 0)
	            {
	                groundedTimer = Mathf.Max(0, groundedTimer - Time.deltaTime);
	                if (groundedTimer <= 0)
	                {
	                    grounded = false;
	                    Up = Vector3.up;
	                }
	            }

	            //Smoke
	            if (smoke != null)
	            {
	                smoke.grounded = grounded;
	            }

        }

        private void OnTriggerEnter(Collider other)
        {
//			Debug.Log("ALGUIEN CAYO AL AGUA...");
		            var c = other.GetComponent<Checkpoint>();

		            if (c)
		            {
		                if (CheckpointPassed != null)
		                    CheckpointPassed(this, new CheckpointPassArgs(c));
		            }

				if (other.GetComponent<TriggerRespawn>()){
					if( isServer ){
						this.RequestRespawn();

						if(isLocalPlayer || Type== BallType.AI){

						}else{

						}
					}
					else{
						if(isClient && isLocalPlayer){

						}
					}

				}

        }
		[ClientRpc]
		public void RpcRequestSpawn(int currentCheck){
			if(!isServer){
				transform.position = StageReferences.Active.checkpoints[currentCheck].GetRespawnPoint() + Vector3.up * transform.localScale.x * 0.5f;
				GetComponent<Rigidbody>().velocity = Vector3.zero;
				GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
				if (cameraBall != null)
					
				{
					this.cameraBall.SetDirection(StageReferences.Active.checkpoints[currentCheck].transform.rotation);
				}
			}


		}

		[Command]
		public void CmdRequestSpawn(int currentCheck, NetworkInstanceId myID){

			RpcRequestSpawn( currentCheck);

		}

		[ClientRpc]
		public void RpcSetCanMove(bool move){
			canMove= move;
		}


		[ClientRpc]
		public void RpcRemoveThePlayerFromList(NetworkInstanceId id){
			if(!isServer){

				Sanicball.UI.PlayerPortrait[] iconos=  FindObjectsOfType<Sanicball.UI.PlayerPortrait>();
				Debug.Log(iconos.Length );

				for (int m=0; m< iconos.Length ; m ++){

					if(iconos[m].TargetPlayer.ball.netId ==id){
						Destroy(iconos[m].gameObject);
					}

				}

				Marker[] avatars=  FindObjectsOfType<Marker>();

				for (int k=0; k< avatars.Length ; k ++){
					if(avatars[k].Target.GetComponent<Ball>() != null){
						
						if(avatars[k].Target.GetComponent<Ball>().netId ==id){
							Destroy(avatars[k].gameObject);
						}
					}
				}

				for( int i =0; i < NetworkManager.singleton.GetComponent<SanicNetworkManager>().raceManager.players.Count; i++ ){
					if (NetworkManager.singleton.GetComponent<SanicNetworkManager>().raceManager.players[i].ball.netId == id){
						NetworkManager.singleton.GetComponent<SanicNetworkManager>().raceManager.players.Remove( NetworkManager.singleton.GetComponent<SanicNetworkManager>().raceManager.players[i] );
					}
				}


			}
		}


		[ClientRpc]
		public void RpcCountdown(){
			if(!isServer){
				NetworkManager.singleton.GetComponent<SanicNetworkManager>().raceManager.CurrentState = Sanicball.RaceState.Countdown;
			}

		}


        private void OnCollisionStay(Collision c)
        {
	            //Enable grounded and reset timer
	            grounded = true;
	            groundedTimer = 0;
	            Up = c.contacts[0].normal;

        }

        private void OnCollisionExit(Collision c)
        {
            //Disable grounded when timer is done
            	groundedTimer = 0.08f;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, Up);
        }
    }
}