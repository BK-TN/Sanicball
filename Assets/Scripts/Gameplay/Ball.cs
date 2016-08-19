using Sanicball.Data;
using UnityEngine;

namespace Sanicball.Gameplay
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
        public CameraCreationArgs(IBallCamera cameraCreated)
        {
            CameraCreated = cameraCreated;
        }

        public IBallCamera CameraCreated { get; private set; }
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
        [SerializeField]
        private ParticleSystem removalParticles;

        public DriftySmoke Smoke { get { return smoke; } }
        public OmniCamera Camera { get { return camera; } }
        public PivotCamera OldCamera { get { return oldCamera; } }
        public AITarget AiTarget { get { return aiTarget; } }
        public ParticleSystem RemovalParticles { get { return removalParticles; } }
    }

    [RequireComponent(typeof(Rigidbody))]
    public class Ball : MonoBehaviour
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
        private BallControlInput input;
        private bool grounded = false;
        private float groundedTimer = 0;
        private DriftySmoke smoke;

        public bool CanMove { get { return canMove; } set { canMove = value; } }
        public Vector3 DirectionVector { get; set; }
        public Vector3 Up { get; set; }
        public bool Brake { get; set; }
        public string Nickname { get { return nickname; } }

        //Component caches
        private Rigidbody rb;
        public BallControlInput Input { get { return input; } }

        //Events
        public event System.EventHandler<CheckpointPassArgs> CheckpointPassed;
        public event System.EventHandler RespawnRequested;
        public event System.EventHandler<CameraCreationArgs> CameraCreated;

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
            smoke = Instantiate(prefabs.Smoke);
            smoke.target = this;
            smoke.DriftAudio = sounds.Brake;

            //Grab reference to Rigidbody
            rb = GetComponent<Rigidbody>();
            //Set angular velocity (This is necessary for fast)
            rb.maxAngularVelocity = 1000f;

            //Set object name
            gameObject.name = type.ToString() + " - " + nickname;

            //Set character
            if (CharacterId >= 0 && CharacterId < ActiveData.Characters.Length)
            {
                SetCharacter(ActiveData.Characters[CharacterId]);
            }

            //Create objects and components based on ball type
            if (type == BallType.Player)
            {
                if (ctrlType != ControlType.None)
                {
                    IBallCamera camera;
                    //Create camera
                    if (ActiveData.GameSettings.useOldControls)
                    {
                        camera = Instantiate(prefabs.OldCamera);
                        ((PivotCamera)camera).UseMouse = ctrlType == ControlType.Keyboard;
                    }
                    else
                    {
                        camera = Instantiate(prefabs.Camera);
                    }
                    camera.Target = rb;
                    camera.CtrlType = ctrlType;

                    if (CameraCreated != null)
                        CameraCreated(this, new CameraCreationArgs(camera));
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
            if ((type == BallType.Player || type == BallType.LobbyPlayer) && ctrlType != ControlType.None)
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

        public void CreateRemovalParticles()
        {
            //TODO: Create a special version of the particle system for Super Sanic that has a cloud of pot leaves instead. No, really.
            Instantiate(prefabs.RemovalParticles, transform.position, transform.rotation);
        }
    }
}
