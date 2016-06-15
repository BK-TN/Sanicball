using System.Collections;
using UnityEngine;

namespace Sanicball.Net
{
    public class NetManager : MonoBehaviour
    {
        private MatchManager matchManager;

        public MatchManager MatchManager
        {
            get { return matchManager; }
            set
            {
                if (matchManager == null)
                    matchManager = value;
                else
                    throw new System.InvalidOperationException("Match manager already set");
            }
        }

        public void Start()
        {
            if (matchManager == null)
            {
                throw new System.NullReferenceException("Match manager has not been set");
            }

            DontDestroyOnLoad(gameObject);
        }

        public bool Connect()
        {
            return false;
        }

        public void OnApplicationQuit()
        {
        }

        private void Update()
        {
        }
    }
}