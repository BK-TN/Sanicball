using System.Collections;
using Sanicball.Gameplay;
using Sanicball.Logic;
using UnityEngine;

namespace Sanicball
{
    public class SpectatorView : MonoBehaviour
    {
        [SerializeField]
        private OmniCamera omniCameraPrefab = null;

        private RacePlayer target;
        private OmniCamera activeOmniCamera;

        private bool leftPressed;
        private bool rightPressed;

        public RacePlayer Target
        {
            get
            {
                return target;
            }
            set
            {
                if (activeOmniCamera == null)
                {
                    activeOmniCamera = Instantiate(omniCameraPrefab);
                }
                activeOmniCamera.Target = value.Transform.GetComponent<Rigidbody>();

                target = value;
            }
        }
        public RaceManager TargetManager { get; set; }

        private void Start()
        {
        }

        private void Update()
        {
            if (GameInput.MovementVector(ControlType.Keyboard).x < 0)
            {
                if (!leftPressed)
                {
                    int prevIndex = FindIndex() - 1;
                    if (prevIndex < 0) prevIndex = TargetManager.PlayerCount - 1;

                    Target = TargetManager[prevIndex];

                    leftPressed = true;
                }
            }
            else if (leftPressed) leftPressed = false;

            if (GameInput.MovementVector(ControlType.Keyboard).x > 0)
            {
                if (!rightPressed)
                {
                    int nextIndex = FindIndex() + 1;
                    if (nextIndex >= TargetManager.PlayerCount) nextIndex = 0;

                    Target = TargetManager[nextIndex];

                    rightPressed = true;
                }
            }
            else if (rightPressed) rightPressed = false;
        }

        private int FindIndex()
        {
            for (int i = 0; i < TargetManager.PlayerCount; i++)
            {
                if (TargetManager[i] == Target)
                {
                    return i;
                }
            }
            return -1;
        }
    }
}