using UnityEngine;
using UnityEngine.UI;

namespace Sanicball.UI
{
    [RequireComponent(typeof(Image))]
    public class SpriteSwap : MonoBehaviour
    {
        public Sprite replacementSprite;
        public Color replacementColor;

        private Sprite originalSprite;
        private Color originalColor;

        private Image img;

        private bool usingReplacement = false;

        public void Swap()
        {
            if (usingReplacement)
            {
                img.sprite = originalSprite;
                img.color = originalColor;
            }
            else
            {
                img.sprite = replacementSprite;
                img.color = replacementColor;
            }
            usingReplacement = !usingReplacement;
        }

        private void Start()
        {
            img = GetComponent<Image>();

            originalSprite = img.sprite;
            originalColor = img.color;
        }
    }
}
