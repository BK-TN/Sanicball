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
            manager.InitLocalMatch();
        }

        public void JoinOnlineGame()
        {
            NetManager netManager = Instantiate(netManagerPrefab);
            netManager.Connect("127.0.0.1", 25000);
        }
    }
}