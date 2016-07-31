using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.UI;

namespace Sanicball.UI
{
    public class ServerListItem : MonoBehaviour
    {
        public Text serverNameText;
        public Text serverStatusText;
        public Text playerCountText;
        public Text pingText;
        public Image pingLoadingImage;

        [HideInInspector]
        public ServerInfo info;


        private bool pingDone = false;
        private float pingTimeout = 8f;

//        public void SetData(HostData data)
		public void SetData(MatchInfoSnapshot data)

        {

			long inLobby;

			serverNameText.text = data.name;

			playerCountText.text = data.currentSize.ToString()+ "/" + data.maxSize.ToString();

			/*
            info = new ServerInfo(data);
            serverNameText.text = info.GameName;
            serverStatusText.text = info.Status;
            if (info.UseNAT)
            {
                serverStatusText.text += " - uses NAT punching";
            }
            playerCountText.text = info.ConnectedPlayers + "/" + info.PlayerLimit;


*/

			GetComponent<Button>().onClick.AddListener(  delegate { test(data, name); });
        }

		public void test( MatchInfoSnapshot match, string ok){

			
			NetworkManager.singleton.matchMaker.JoinMatch(match.networkId,"","","",0,0, OnMatchJoined);


			Debug.Log (ok);
		}

		public void OnMatchJoined(bool success, string extendedInfo, MatchInfo matchInfo)
		{
			NetworkManager.singleton.GetComponent<SanicNetworkManager>().infoPanel.Display("Joining to Room..","Close",null);
			Debug.Log("Im joining a Match");
			if (LogFilter.logDebug)
			{
				Debug.Log("NetworkManager OnMatchJoined ");
			}
			if (success)
			{
				try
				{
//					Utility.SetAccessTokenForNetwork(matchInfo.networkId, new UnityEngine.Networking.Types.NetworkAccessToken(matchInfo.accessTokenString));
				}
				catch(System.Exception ex)
				{
					if (LogFilter.logError)
					{
						Debug.LogError(ex);
					}
				}
				NetworkManager.singleton.StartClient (matchInfo);

			}
			else if (LogFilter.logError)
			{
				NetworkManager.singleton.GetComponent<SanicNetworkManager>().infoPanel.Display("Cant join to this Match, please try later","Close",null);

				Debug.LogError(string.Concat("Join Failed:", matchInfo));
			}
		}



		


        private void Update()
        {
            if (info == null || pingDone) return;

            if (pingTimeout > 0f)
            {
                pingTimeout -= Time.deltaTime;
                if (pingTimeout <= 0f)
                {
                    pingLoadingImage.enabled = false;
                    pingText.text = "?";
                    pingDone = true;
                }
            }

            if (info.ping.isDone)
            {
                pingLoadingImage.enabled = false;
                pingText.text = info.ping.time + "ms";
                pingDone = true;
            }
        }
    }

    public class ServerInfo
    {
        public string Status;
        public float VersionFloat;
        public string VersionString;
        public bool UseNAT;
        public int RealPort;
        public Ping ping;
        private HostData rawHostData;

        //The port recieved via host data is sometimes incorrect
        public ServerInfo(HostData data)
        {
            string[] comment = data.comment.Split(';');
            if (comment.Length != 5) { Debug.LogWarning("Server '" + data.gameName + "' has the wrong number of comment arguments"); }
            else
            {
                Status = comment[0];
                if (!float.TryParse(comment[1], out VersionFloat))
                {
                    Debug.LogWarning("Failed to parse server version float for server '" + data.gameName + "'.");
                }
                VersionString = comment[2];
                UseNAT = comment[3] == "useNat";
                if (!int.TryParse(comment[4], out RealPort))
                {
                    Debug.LogWarning("Failed to parse real port for server '" + data.gameName + "'");
                }
            }

            ping = new Ping(data.ip[0]);

            rawHostData = data;
        }

        public string GameName { get { return rawHostData.gameName; } }
        public int ConnectedPlayers { get { return rawHostData.connectedPlayers; } }
        public int PlayerLimit { get { return rawHostData.playerLimit; } }
    }
}
