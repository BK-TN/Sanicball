using UnityEngine;

namespace Sanicball
{
    [RequireComponent(typeof(ParticleSystem))]
    public class ParticleWarping : MonoBehaviour
    {
        public Transform target;
        public Vector3 limit;

        private ParticleSystem ps;

        private void Start()
        {
            ps = GetComponent<ParticleSystem>();
        }

        private void FixedUpdate()
        {
            if (Camera.main != null)
                target = Camera.main.transform;
            else
                return;
            Vector3 tpos = target.position;
            ParticleSystem.Particle[] particles = new ParticleSystem.Particle[10000];
            int plength = ps.GetParticles(particles);
            for (int a = 0; a < plength; a++)
            {
                Vector3 f = particles[a].position;
                //int limit = 750;
                //X
                if (f.x > tpos.x + limit.x)
                {
                    particles[a].position = new Vector3(f.x - limit.x * 2, f.y, f.z);
                }
                if (f.x < tpos.x - limit.x)
                {
                    particles[a].position = new Vector3(f.x + limit.x * 2, f.y, f.z);
                }
                //Y
                if (f.y > tpos.y + limit.y)
                {
                    particles[a].position = new Vector3(f.x, f.y - limit.y * 2, f.z);
                }
                if (f.y < tpos.y - limit.y)
                {
                    particles[a].position = new Vector3(f.x, f.y + limit.y * 2, f.z);
                }
                //Z
                if (f.z > tpos.z + limit.z)
                {
                    particles[a].position = new Vector3(f.x, f.y, f.z - limit.z * 2);
                }
                if (f.z < tpos.z - limit.z)
                {
                    particles[a].position = new Vector3(f.x, f.y, f.z + limit.z * 2);
                }
            }
            ps.SetParticles(particles, plength);
        }
    }
}
