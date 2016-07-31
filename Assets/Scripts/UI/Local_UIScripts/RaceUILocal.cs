using System.Collections;
using UnityEngine;

namespace Sanicball.UI
{
    public class RaceUILocal : MonoBehaviour
    {
        [SerializeField]
        private PlayerPortraitLocal portraitPrefab = null;

        [SerializeField]
        private Transform portraitContainer = null;

        [SerializeField]
        private RectTransform finishedText;

        public RaceManagerLocal TargetManager { get; set; }

        private void Start()
        {
            for (int i = 0; i < TargetManager.PlayerCount; i++)
            {
                var p = Instantiate(portraitPrefab);
                p.transform.SetParent(portraitContainer, false);
                p.TargetPlayer = TargetManager[i];
            }
        }

        public void ShowFinishedText()
        {
            finishedText.gameObject.SetActive(true);
        }
    }
}