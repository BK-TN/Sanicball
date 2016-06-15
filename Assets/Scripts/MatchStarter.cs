using Sanicball.Net;
using UnityEngine;

namespace Sanicball
{
    public class MatchStarter : MonoBehaviour
    {
        [SerializeField]
        private MatchManager matchManagerPrefab = null;
        [SerializeField]
        private NetManager netManagerPrefab = null;

        private void Update()
        {
            //J for JOIN
            if (Input.GetKeyDown(KeyCode.J))
            {
                JoinOnlineGame();
            }
        }

        public void BeginLocalGame()
        {
            MatchManager manager = Instantiate(matchManagerPrefab);
            manager.InitMatch();
        }

        public void JoinOnlineGame()
        {
            //TODO: Create network manager, join server, and wait for response
            NetManager netManager = Instantiate(netManagerPrefab);
            MatchManager matchManager = Instantiate(matchManagerPrefab);

            netManager.MatchManager = matchManager;
            if (netManager.Connect())
                matchManager.InitMatch();
        }
    }
}