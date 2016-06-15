using UnityEngine;

namespace Sanicball
{
    public class MatchStarter : MonoBehaviour
    {
        public MatchManager matchManagerPrefab;

        private void Update()
        {
            //H for HOST
            if (Input.GetKeyDown(KeyCode.H))
            {
                BeginOnlineGame();
            }

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

        public void BeginOnlineGame()
        {
            //TODO: Create network mananger and link it to the match manager

            MatchManager manager = Instantiate(matchManagerPrefab);
            manager.InitMatch();
        }

        public void JoinOnlineGame()
        {
            //TODO: Create network manager, join server, and wait for response
        }
    }
}