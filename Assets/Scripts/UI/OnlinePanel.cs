using System;
using System.Collections.Generic;
using Lidgren.Network;
using Sanicball.Logic;
using UnityEngine;
using UnityEngine.UI;

namespace Sanicball.UI
{
    public class OnlinePanel : MonoBehaviour
    {
        public Transform targetServerListContainer;
        public Text errorField;
        public Text serverCountField;
        public ServerListItem serverListItemPrefab;
        public Selectable aboveList;
        public Selectable belowList;

        private List<ServerListItem> servers = new List<ServerListItem>();

        private NetClient discoveryClient;

        public void RefreshServers()
        {
            discoveryClient.DiscoverLocalPeers(25000);

            serverCountField.text = "0 servers";
            errorField.enabled = false;

            //Clear old servers
            foreach (var serv in servers)
            {
                Destroy(serv.gameObject);
            }
            servers.Clear();
        }

        private void Awake()
        {
            errorField.enabled = false;

            NetPeerConfiguration config = new NetPeerConfiguration(OnlineMatchMessenger.APP_ID);
            config.EnableMessageType(NetIncomingMessageType.DiscoveryResponse);
            discoveryClient = new NetClient(config);
            discoveryClient.Start();
        }

        private void Update()
        {
            //Refresh on f5 (pretty nifty)
            if (Input.GetKeyDown(KeyCode.F5))
            {
                RefreshServers();
            }

            //Check for messages on the discovery client
            NetIncomingMessage msg;
            while ((msg = discoveryClient.ReadMessage()) != null)
            {
                switch (msg.MessageType)
                {
                    case NetIncomingMessageType.DiscoveryResponse:
                        ServerInfo info = Newtonsoft.Json.JsonConvert.DeserializeObject<ServerInfo>(msg.ReadString());

                        double timeDiff = (DateTime.UtcNow - info.Timestamp).TotalMilliseconds;

                        var server = Instantiate(serverListItemPrefab);
                        server.transform.SetParent(targetServerListContainer, false);
                        server.Init(info, msg.SenderEndPoint, (int)timeDiff);
                        servers.Add(server);
                        RefreshNavigation();

                        serverCountField.text = servers.Count + (servers.Count == 1 ? " server" : " servers");

                        break;

                    default:
                        Debug.Log("Server discovery client recieved an unhandled NetMessage (" + msg.MessageType + ")");
                        break;
                }
            }
        }

        private void RefreshNavigation()
        {
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