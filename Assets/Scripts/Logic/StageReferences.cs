using UnityEngine;

namespace Sanicball.Logic
{
    public class StageReferences : MonoBehaviour
    {
        public Gameplay.Checkpoint[] checkpoints;

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
