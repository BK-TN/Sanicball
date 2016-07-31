using UnityEngine;

namespace Sanicball
{
    public class LobbyReferences : MonoBehaviour
    {
        [SerializeField]
        private UI.LobbyStatusBar statusBar = null;

        [SerializeField]
        private UI.LocalPlayerManager localPlayerManager = null;

        [SerializeField]
        private UI.MatchSettingsPanel matchSettingsPanel = null;

        [SerializeField]
        private LobbyBallSpawner ballSpawner = null;

        [SerializeField]
        private UnityEngine.UI.Text countdownField = null;


		//tihis was Changed to make it multiplayer....
		[SerializeField]
		private SanicNetworkManager sanicNetwork = null;

        public static LobbyReferences Active
        {
            get; private set;
        }

        public UI.LobbyStatusBar StatusBar { get { return statusBar; } }
        public UI.LocalPlayerManager LocalPlayerManager { get { return localPlayerManager; } }
        public UI.MatchSettingsPanel MatchSettingsPanel { get { return matchSettingsPanel; } }
        public LobbyBallSpawner BallSpawner { get { return ballSpawner; } }
		public SanicNetworkManager SanicNetwork  {  get { return sanicNetwork; } }

        public UnityEngine.UI.Text CountdownField { get { return countdownField; } }

        private void Awake()
        {
            Active = this;
            CameraFade.StartAlphaFade(Color.black, true, 1f);
        }
    }
}