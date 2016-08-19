using UnityEngine;

namespace Sanicball.Gameplay
{
    public class DriftySmoke : MonoBehaviour
    {
        public bool grounded = false;
        public AudioSource DriftAudio { get; set; }
        public Ball target;

        private ParticleSystem pSystem;

        private void Start()
        {
            pSystem = GetComponent<ParticleSystem>();
        }

        private void FixedUpdate()
        {
            if (target == null)
            {
                Destroy(gameObject);
                return;
            }

            Rigidbody rBody = target.GetComponent<Rigidbody>();
            AudioSource aSource = DriftAudio;

            float speed = rBody.velocity.magnitude;
            float rot = rBody.angularVelocity.magnitude / 2;
            float angle = Vector3.Angle(rBody.velocity, Quaternion.Euler(0, -90, 0) * rBody.angularVelocity);

            if (((angle > 50 && (rot > 10 || speed > 10)) || (rot > 30 && speed < 30)) && grounded)
            {
                var emitParams = new ParticleSystem.EmitParams
                {
                    position = target.transform.position - new Vector3(0, +0.5f, 0) + Random.insideUnitSphere * 0.25f,
                    velocity = Vector3.zero,
                    startSize = Random.Range(3f, 5f),
                    startLifetime = 5,
                    startColor = Color.white
                };
                pSystem.Emit(emitParams, 1);

                if (aSource && aSource.volume < 1) { aSource.volume = Mathf.Min(aSource.volume + 0.5f, 1); }
                //aSource.pitch = 0.8f+Mathf.Min(rot/400f,1.2f);
            }
            else
            {
                if (aSource && aSource.volume > 0) { aSource.volume = Mathf.Max(aSource.volume - 0.2f, 0); }
            }
        }

        private float Rand()
        {
            return Random.Range(-1f, 1f);
        }
    }
}
