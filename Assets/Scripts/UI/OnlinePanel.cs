using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Sanicball.UI
{
    public class OnlinePanel : MonoBehaviour
    {
        public Transform targetServerListContainer;
        public Image spinnerImage;
        public Text errorField;
        public Text serverCountField;
        public ServerListItem serverListItemPrefab;
        public Selectable aboveList;
        public Selectable belowList;
        private string masterServerGameName = "sanicball";
        private bool refreshing = false;

        private List<ServerListItem> servers = new List<ServerListItem>();

        public void RefreshServers()
        {
            if (refreshing) return;

            Debug.Log("Connecting to master server...");

            serverCountField.text = "Refreshing...";
            spinnerImage.enabled = true;
            refreshing = true;
            errorField.enabled = false;
            //Clear old servers
            foreach (var serv in servers)
            {
                Destroy(serv.gameObject);
            }
            servers.Clear();
            MasterServer.RequestHostList(masterServerGameName);
        }

        public void OnFailedToConnectToMasterServer(NetworkConnectionError error)
        {
            spinnerImage.enabled = false;
            refreshing = false;
            errorField.enabled = true;
            errorField.text = "Could not get server list! Try again later.";
            serverCountField.text = servers.Count + (servers.Count == 1 ? " server" : " servers");
        }

        private void Awake()
        {
            masterServerGameName = "sanicball" + GameVersion.AS_FLOAT;
            errorField.enabled = false;
        }

        private void Update()
        {
            //Refresh on f5 (pretty nifty)
            if (Input.GetKeyDown(KeyCode.F5))
            {
                RefreshServers();
            }
        }

        private void OnMasterServerEvent(MasterServerEvent e)
        {
            if (e == MasterServerEvent.HostListReceived)
            {
                spinnerImage.enabled = false;
                refreshing = false;
                //Create new servers
                var hostList = MasterServer.PollHostList();
                Debug.Log(hostList.Length + " servers recieved.");
                serverCountField.text = hostList.Length + (hostList.Length == 1 ? " server" : " servers");
                if (hostList.Length == 0)
                {
                    errorField.enabled = true;
                    errorField.text = @"No servers ¯\_(ツ)_/¯";
                }
                errorField.enabled = hostList.Length == 0;
                foreach (var host in hostList)
                {
                    var server = Instantiate(serverListItemPrefab);
                    server.transform.SetParent(targetServerListContainer, false);
                    server.SetData(host);
                    servers.Add(server);
                }
                //Add navigation links
                for (var i = 0; i < servers.Count; i++)
                {
                    var button = servers[i].GetComponent<Button>();
                    if (button)
                    {
                        var nav = new Navigation() { mode = Navigation.Mode.Explicit };
                        //Up navigation
                        if (i == 0)
                        {
                            nav.selectOnUp = aboveList;
                            var nav2 = aboveList.navigation;
                            nav2.selectOnDown = button;
                            aboveList.navigation = nav2;
                        }
                        else
                        {
                            nav.selectOnUp = servers[i - 1].GetComponent<Button>();
                        }
                        //Down navigation
                        if (i == servers.Count - 1)
                        {
                            nav.selectOnDown = belowList;
                            var nav2 = belowList.navigation;
                            nav2.selectOnUp = button;
                            belowList.navigation = nav2;
                        }
                        else
                        {
                            nav.selectOnDown = servers[i + 1].GetComponent<Button>();
                        }

                        button.navigation = nav;
                    }
                }
            }
        }
    }
}
