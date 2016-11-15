using System.Collections;
using UnityEngine;

namespace Sanicball.UI
{
    public class CharacterFilter : MonoBehaviour
    {
        [SerializeField]
        private CharacterButton buttonPrefab;
        [SerializeField]
        private RectTransform buttonContainer;

        private bool[] filter;

        private void Start()
        {
            filter = new bool[Data.ActiveData.Characters.Length];

            for (int i = 0; i < Data.ActiveData.Characters.Length; i++)
            {
                Data.CharacterInfo c = Data.ActiveData.Characters[i];
                int i2 = i;

                if (c.hidden) continue;

                CharacterButton button = Instantiate(buttonPrefab);
                button.Mark(true);
                button.transform.SetParent(buttonContainer, false);
                button.OnClick.AddListener(() =>
                {
                    Debug.Log("derp");
                    filter[i2] = !filter[i2];
                    button.Mark(!filter[i2]);
                });
                button.Init(c);
            }
        }

        private void Update()
        {
        }
    }
}