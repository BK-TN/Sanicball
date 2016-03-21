using UnityEngine;

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
        public CameraCreationArgs(Camera cameraCreated)
        {
            CameraCreated = cameraCreated;
        }

        public Camera CameraCreated { get; private set; }
    }

    [System.Serializable]
    public class BallMotionSounds
    {
        public AudioSource jump;
        public AudioSource roll;
        public AudioSource speedNoise;
        public AudioSource brake;
    }

    [System.Serializable]
    public class BallPrefabs
    {
        public DriftySmoke smoke;
        public OmniCamera camera;
        public AITarget aiTarget;
        public Material minimapIconMaterial;
    }

    [RequireComponent(typeof(Rigidbody))]
    public class Ball : MonoBehaviour
    {
        //Set before Start()
        public BallType type;

        public Data.PlayerType playerType;
        public int character;
        public string nickname;
        public ControlType controlType;
        public bool canMove;

        public BallPrefabs prefabs;
        public BallStats stats;
        public BallMotionSounds sounds;

        public bool brake;
        public Vector3 directionVector;

        public Vector3 up = Vector3.up;

        //Cache of input component
        [HideInInspector]
        public BallControlInput input;

        private bool grounded = false;
        private float groundedTimer = 0;
        private DriftySmoke smoke;
        //Vector3 forward = Vector3.forward;

        private Rigidbody rb;

        public event System.EventHandler<CheckpointPassArgs> CheckpointPassed;
        public event System.EventHandler RespawnRequested;
        public event System.EventHandler<CameraCreationArgs> CameraCreated;

        public void Init()
        {
        }

        public void Jump()
        {
            if (grounded && canMove)
            {
                rb.AddForce(up * stats.jumpHeight, ForceMode.Impulse);
                if (sounds.jump != null)
                {
                    sounds.jump.Play();
                }
                grounded = false;
            }
        }

        public void RequestRespawn()
        {
            if (canMove && RespawnRequested != null)
                RespawnRequested(this, System.EventArgs.Empty);
        }

        private void Start()
        {
            //Set up drifty smoke
            smoke = Instantiate(prefabs.smoke);
            smoke.target = this;

            //Grab reference to Rigidbody
            rb = GetComponent<Rigidbody>();
            //Set angular velocity (This is necessary for fast)
            rb.maxAngularVelocity = 1000f;

            //Set object name
            gameObject.name = type.ToString() + " - " + nickname;

            //Set character
            if (character >= 0 && character < Data.ActiveData.Characters.Length)
            {
                SetCharacter(Data.ActiveData.Characters[character]);
                prefabs.minimapIconMaterial = Data.ActiveData.Characters[character].minimapIcon;
            }

            //Create objects and components based on ball type
            if (type == BallType.Player)
            {
                //Create camera
                OmniCamera camera = Instantiate(prefabs.camera);
                camera.target = rb;

                if (CameraCreated != null)
                    CameraCreated(this, new CameraCreationArgs(camera.GetComponent<Camera>()));
            }
            if (type == BallType.LobbyPlayer)
            {
                var cam = FindObjectOfType<LobbyCamera>();
                if (cam)
                {
                    cam.AddBall(this);
                }
            }
            if ((type == BallType.Player || type == BallType.LobbyPlayer))
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
                AITarget pFollower = Instantiate(prefabs.aiTarget);
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
                motion.stats = c.stats;
        }

        private void FixedUpdate()
        {
            if (canMove)
            {
                //If grounded use torque
                if (directionVector != Vector3.zero)
                {
                    rb.AddTorque(directionVector * stats.rollSpeed);
                }
                //If not use both
                if (!grounded)
                {
                    rb.AddForce((Quaternion.Euler(0, -90, 0) * directionVector) * stats.airSpeed);
                }
            }
            else
            {
                //Always brake when canControl is off
                brake = true;
            }

            //Braking
            if (brake)
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

                //if (GetComponent<BallControlInput>() != null) Debug.Log(rb.velocity.magnitude + " = " + vel);

                vel = Mathf.Clamp(vel, 0, 1);
                if (sounds.roll != null)
                {
                    sounds.roll.pitch = Mathf.Max(rollSpd, 0.8f);
                    sounds.roll.volume = Mathf.Min(rollSpd, 1);
                }
                if (sounds.speedNoise != null)
                {
                    sounds.speedNoise.pitch = 0.8f + vel;
                    sounds.speedNoise.volume = vel;//Mathf.Clamp(-0.5f + vel, 0f, 1f);
                }
            }
            else
            {
                //Fade sounds out when in the air
                if (sounds.roll != null && sounds.roll.volume > 0)
                {
                    sounds.roll.volume = Mathf.Max(0, sounds.roll.volume - 0.2f);
                }
                if (sounds.speedNoise != null && sounds.speedNoise.volume > 0)
                {
                    sounds.speedNoise.volume = Mathf.Max(0, sounds.speedNoise.volume - 0.01f);
                }
            }

            //Grounded timer
            if (groundedTimer > 0)
            {
                groundedTimer = Mathf.Max(0, groundedTimer - Time.deltaTime);
                if (groundedTimer <= 0)
                {
                    grounded = false;
                    up = Vector3.up;
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
            up = c.contacts[0].normal;
        }

        private void OnCollisionExit(Collision c)
        {
            //Disable grounded when timer is done
            groundedTimer = 0.08f;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, up);
        }
    }
}
