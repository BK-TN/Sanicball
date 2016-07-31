using UnityEngine;

namespace Sanicball
{
    public class StageReferences : MonoBehaviour
    {
        public Checkpoint[] checkpoints;

        public RaceBallSpawner spawnPoint;

        public CameraOrientation[] waitingCameraOrientations;

        public static StageReferences Active
        {
            get; private set;
        }

        private void Awake()
        {
            Active = this;
        }
    }
}
