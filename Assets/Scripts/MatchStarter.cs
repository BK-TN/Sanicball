using UnityEngine;
using UnityEngine.Networking;
namespace Sanicball
{
    public class MatchStarter : MonoBehaviour
    {
        public MatchManagerLocal prefabToUse;

        public void BeginLocalGame()
        {
            Instantiate(prefabToUse);

//			NetworkServer.Spawn(prefabToUse.gameObject);
            UnityEngine.SceneManagement.SceneManager.LoadScene("LobbyLocal");
        }
    }
}