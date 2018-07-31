using System;
using System.Collections.Generic;
using System.Net;
using Lidgren.Network;
using Sanicball.Data;
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

        //Stores server browser IPs, so they can be differentiated from LAN servers
        private List<string> serverBrowserIPs = new List<string>();

        private NetClient discoveryClient;
        private WWW serverBrowserRequester;
        private DateTime latestLocalRefreshTime;
        private DateTime latestBrowserRefreshTime;

        public void RefreshServers()
        {
            serverBrowserIPs.Clear();

            discoveryClient.DiscoverLocalPeers(25000);
            latestLocalRefreshTime = DateTime.Now;

			serverBrowserRequester = new WWW(ActiveData.GameSettings.serverListURL);

            serverCountField.text = "Refreshing servers, hang on...";
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

            //Check for response from the server browser requester
            if (serverBrowserRequester != null && serverBrowserRequester.isDone)
            {
                if (string.IsNullOrEmpty(serverBrowserRequester.error))
                {
                    latestBrowserRefreshTime = DateTime.Now;

                    string result = serverBrowserRequester.text;
                    string[] entries = result.Split(new string[] { "<br>" }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string entry in entries)
                    {
                        int seperationPoint = entry.LastIndexOf(':');
                        string ip = entry.Substring(0, seperationPoint);
                        string port = entry.Substring(seperationPoint + 1, entry.Length - (seperationPoint + 1));

                        int portInt;
                        if (int.TryParse(port, out portInt))
                        {
                            System.Threading.Thread discoverThread = new System.Threading.Thread(() => { discoveryClient.DiscoverKnownPeer(ip, portInt); });
                            discoverThread.Start();
                            serverBrowserIPs.Add(ip);
                        }
                    }
					serverCountField.text = "0 servers";
                }
                else
                {
                    Debug.LogError("Failed to receive servers - " + serverBrowserRequester.error);
					serverCountField.text = "Cannot access server list URL!";
                }

                serverBrowserRequester = null;
            }

            //Check for messages on the discovery client
            NetIncomingMessage msg;
            while ((msg = discoveryClient.ReadMessage()) != null)
            {
                switch (msg.MessageType)
                {
                    case NetIncomingMessageType.DiscoveryResponse:
                        ServerInfo info;
                        try
                        {
                            info = Newtonsoft.Json.JsonConvert.DeserializeObject<ServerInfo>(msg.ReadString());
                        }
                        catch (Newtonsoft.Json.JsonException ex)
                        {
                            Debug.LogError("Failed to deserialize info for a server: " + ex.Message);
                            continue;
                        }

                        //double timeDiff = (DateTime.UtcNow - info.Timestamp).TotalMilliseconds;
                        bool isLocal = !serverBrowserIPs.Contains(msg.SenderEndPoint.Address.ToString());

                        DateTime timeToCompareTo = isLocal ? latestLocalRefreshTime : latestBrowserRefreshTime;
                        double timeDiff = (DateTime.Now - timeToCompareTo).TotalMilliseconds;

                        var server = Instantiate(serverListItemPrefab);
                        server.transform.SetParent(targetServerListContainer, false);
                        server.Init(info, msg.SenderEndPoint, (int)timeDiff, isLocal);
                        servers.Add(server);
                        RefreshNavigation();

                        serverCountField.text = servers.Count + (servers.Count == 1 ? " server" : " servers");

                        break;

                    default:
                        Debug.Log("Server discovery client received an unhandled NetMessage (" + msg.MessageType + ")");
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
