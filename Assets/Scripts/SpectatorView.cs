using System.Collections;
using Sanicball.Gameplay;
using Sanicball.Logic;
using Sanicball.UI;
using SanicballCore;
using UnityEngine;
using UnityEngine.UI;

namespace Sanicball
{
    public class SpectatorView : MonoBehaviour
    {
        [SerializeField]
        private OmniCamera omniCameraPrefab = null;
        [SerializeField]
        private PlayerUI playerUIPrefab = null;

        [SerializeField]
        private Text spectatingField = null;

        private RacePlayer target;
        private OmniCamera activeOmniCamera;
        private PlayerUI activePlayerUI;

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
                target = value;
                spectatingField.text = "Spectating <b>" + target.Name + "</b>";

                if (activeOmniCamera == null)
                {
                    activeOmniCamera = Instantiate(omniCameraPrefab);
                }
                activeOmniCamera.Target = target.Transform.GetComponent<Rigidbody>();

                if (activePlayerUI == null)
                {
                    activePlayerUI = Instantiate(playerUIPrefab);
                    activePlayerUI.TargetManager = TargetManager;
                }
                activePlayerUI.TargetCamera = activeOmniCamera.AttachedCamera;
                activePlayerUI.TargetPlayer = target;
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