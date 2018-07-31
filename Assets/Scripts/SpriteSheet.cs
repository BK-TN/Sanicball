using UnityEngine;

namespace Sanicball
{
    public class SpriteSheet : MonoBehaviour
    {
        //vars for the whole sheet
        public int colCount = 4;

        public int rowCount = 4;

        //vars for animation
        public int rowNumber = 0; //Zero Indexed

        public int colNumber = 0; //Zero Indexed
        public int totalCells = 4;
        public int fps = 10;

        //Maybe this should be a private var
        private Vector2 offset;

        //Update
        private void Update()
        { SetSpriteAnimation(colCount, rowCount, rowNumber, colNumber, totalCells, fps); }

        //SetSpriteAnimation
        private void SetSpriteAnimation(int colCount, int rowCount, int rowNumber, int colNumber, int totalCells, int fps)
        {
            // Calculate index
            int index = (int)(Time.time * fps);
            // Repeat when exhausting all cells
            index = index % totalCells;

            // Size of every cell
            float sizeX = 1.0f / colCount;
            float sizeY = 1.0f / rowCount;
            Vector2 size = new Vector2(sizeX, sizeY);

            // split into horizontal and vertical index
            var uIndex = index % colCount;
            var vIndex = index / colCount;

            // build offset
            // v coordinate is the bottom of the image in opengl so we need to invert.
            float offsetX = (uIndex + colNumber) * size.x;
            float offsetY = (1.0f - size.y) - (vIndex + rowNumber) * size.y;
            Vector2 offset = new Vector2(offsetX, offsetY);

            GetComponent<Renderer>().material.SetTextureOffset("_MainTex", offset);
            GetComponent<Renderer>().material.SetTextureScale("_MainTex", size);
        }
    }
}