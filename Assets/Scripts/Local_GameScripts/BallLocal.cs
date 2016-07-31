using UnityEngine;

namespace Sanicball
{


    [RequireComponent(typeof(Rigidbody))]
    public class BallLocal : MonoBehaviour
    {
        //These are set using Init() when balls are instantiated
        //But you can set them from the editor to quickly test out a track
        [Header("Initial stats")]
        [SerializeField]
        private BallType type;
        [SerializeField]
        private ControlType ctrlType;
        [SerializeField]
        private int characterId;
        [SerializeField]
        private string nickname;

        public BallType Type { get { return type; } }
        public ControlType CtrlType { get { return ctrlType; } }
        public int CharacterId { get { return characterId; } }

        [Header("Subcategories")]
        [SerializeField]
        private BallPrefabs prefabs;
        [SerializeField]
        private BallMotionSounds sounds;

        //State
        private BallStats characterStats;
        private bool canMove = true;
        private BallControlInputLocal input;
        private bool grounded = false;
        private float groundedTimer = 0;
        private DriftySmoke smoke;

        public bool CanMove { get { return canMove; } set { canMove = value; } }
        public Vector3 DirectionVector { get; set; }
        public Vector3 Up { get; set; }
        public bool Brake { get; set; }

        //Component caches
        private Rigidbody rb;
        public BallControlInputLocal Input { get { return input; } }

        //Events
        public event System.EventHandler<CheckpointPassArgs> CheckpointPassed;
        public event System.EventHandler RespawnRequested;
        public event System.EventHandler<CameraCreationArgs> CameraCreated;
		public event System.EventHandler SwitchCamerasEvent;


        public void Jump()
        {
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
        }

        private void Start()
        {
            Up = Vector3.up;

            //Set up drifty smoke
//            smoke = Instantiate(prefabs.Smoke);
//            smoke.target = this;
//            smoke.DriftAudio = sounds.Brake;

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
            if (type == BallType.Player)
            {
                IBallCamera camera;
				OmniCamera newCam =Instantiate(prefabs.Camera) ;
				newCam.Target = rb;
				newCam.CtrlType= ctrlType;
				PivotCamera oldCam= Instantiate(prefabs.OldCamera); 
				oldCam.Target = rb;
				oldCam.CtrlType= ctrlType;

                //Create camera
                if (Data.ActiveData.GameSettings.useOldControls)//camara vieja
                {
					camera= oldCam;
					newCam.gameObject.SetActive(false);
					((PivotCamera)camera).UseMouse = (ctrlType == ControlType.Keyboard );
                }
                else
                {
					camera= newCam;
					oldCam.gameObject.SetActive(false);
                }
                camera.Target = rb;
                camera.CtrlType = ctrlType;
                if (CameraCreated != null)
					CameraCreated(this, new CameraCreationArgs(camera,oldCam.gameObject,newCam.gameObject));
            }
            if (type == BallType.LobbyPlayer)
            {
                //Make the lobby camera follow this ball
                var cam = FindObjectOfType<LobbyCameraLocal>();
                if (cam)
                {
                    cam.AddBall(this);
                }
            }
            if ((type == BallType.Player || type == BallType.LobbyPlayer))
            {
                //Create input component
                input = gameObject.AddComponent<BallControlInputLocal>();
            }
            if (type == BallType.AI)
            {
                //Create AI component
				var ai = gameObject.AddComponent<BallControlAILocal>();
				ai.pathToFollow = FindObjectOfType<Path>();

				//Create target for the AI
				AITarget pFollower = Instantiate(prefabs.AiTarget);
				pFollower.GetComponent<PathFollower>().path = ai.pathToFollow;
				pFollower.stupidness = ai.stupidness;
				pFollower.GetComponent<Patroller>().target = gameObject;
				ai.target = pFollower;
            }
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
            BallLocal motion = GetComponent<BallLocal>();
            if (motion != null)
                motion.characterStats = c.stats;
        }

        private void FixedUpdate()
        {
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
            var c = other.GetComponent<Checkpoint>();

            if (c)
            {
                if (CheckpointPassed != null)
                    CheckpointPassed(this, new CheckpointPassArgs(c));
            }

            if (other.GetComponent<TriggerRespawn>())
                RequestRespawn();
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