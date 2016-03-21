using UnityEngine;

namespace Sanicball
{
    public class MatchStarter : MonoBehaviour
    {
        public MatchManager prefabToUse;

        public void BeginLocalGame()
        {
            Instantiate(prefabToUse);
            //UnityEngine.SceneManagement.SceneManager.LoadScene("Lobby");
        }
    }
}