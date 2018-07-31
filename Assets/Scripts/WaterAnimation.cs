using UnityEngine;

namespace Sanicball
{
    public class WaterAnimation : MonoBehaviour
    {
        public Vector2 speed;
        private Vector2 offset;

        private void Update()
        {
            offset += new Vector2(speed.x * Time.deltaTime, speed.y * Time.deltaTime);
            if (offset.x >= 1)
            {
                offset += new Vector2(-1, 0);
            }
            if (offset.y >= 1)
            {
                offset += new Vector2(0, -1);
            }
            GetComponent<Renderer>().material.SetTextureOffset("_MainTex", offset);
        }
    }
}