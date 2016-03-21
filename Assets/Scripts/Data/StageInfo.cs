using UnityEngine;

namespace Sanicball.Data
{
    [System.Serializable]
    public class StageInfo
    {
        public string name;
        public int id;
        public string sceneName;
        public Sprite picture;
        public GameObject overviewPrefab;
    }
}
