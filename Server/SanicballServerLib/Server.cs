using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lidgren.Network;
using Newtonsoft.Json;
using Sanicball;
using Sanicball.Data;
using Sanicball.Logic;

namespace SanicballServerLib
{
    public class LogArgs : EventArgs
    {
        public LogEntry Entry { get; }

        public LogArgs(LogEntry entry)
        {
            Entry = entry;
        }
    }

    public enum LogType
    {
        Normal,
        Debug,
        Warning,
        Error
    }

    public struct LogEntry
    {
        public DateTime Timestamp { get; }
        public string Message { get; }
        public LogType Type { get; }

        public LogEntry(DateTime timestamp, string message, LogType type)
        {
            Timestamp = timestamp;
            Message = message;
            Type = type;
        }
    }

    public class Server : IDisposable
    {
        public const string CONFIG_FILENAME = "ServerConfig.json";
        private const string SETTINGS_FILENAME = "MatchSettings.json";
        private const int TICKRATE = 20;
        private const int STAGE_COUNT = 5; //Hardcoded stage count for now.. can't receive the actual count since it's part of a Unity prefab.
        private const float PLAYER_TIMEOUT_NOTIFY = 60; //Time in seconds until players get notified about their time left to race
        private const float PLAYER_TIMEOUT_DISQUALIFY = 120; //Time in seconds until players are disqualified

        public event EventHandler<LogArgs> OnLog;

        //Server utilities
        private List<LogEntry> log = new List<LogEntry>();
        private Dictionary<string, CommandHandler> commandHandlers = new Dictionary<string, CommandHandler>();
        private NetServer netServer;
        private CommandQueue commandQueue;
        private Random random = new Random();

        //Server state
        private bool running;
        private bool debugMode;
        private ServerConfig config;
        private List<ServClient> clients = new List<ServClient>();
        private List<ServPlayer> players = new List<ServPlayer>();
        private MatchSettings matchSettings;
        private bool inRace;

        #region Timers

        //Server browser ping timer
        private Stopwatch serverBrowserPingTimer = new Stopwatch();
        private const float serverBrowserPingGoal = 60;

        //Timer for starting a match by all players being ready
        private Stopwatch lobbyTimer = new Stopwatch();
        private const float lobbyTimerGoal = 3;

        //Timer for starting a match automatically
        private Stopwatch autoStartTimer = new Stopwatch();

        //Timeout for clients loading stage
        private Stopwatch stageLoadingTimeoutTimer = new Stopwatch();
        private const float stageLoadingTimeoutTimerGoal = 20;

        //Timer for going back to lobby at the end of a race
        private Stopwatch backToLobbyTimer = new Stopwatch();

        #endregion Timers

        public bool Running { get { return running; } }

        public Server(CommandQueue commandQueue)
        {
            this.commandQueue = commandQueue;

            #region Command handlers

            AddCommandHandler("help", cmd =>
            {
                Log("Available commands:");
                foreach (string name in commandHandlers.Keys)
                {
                    Log(name);
                }
            });
            AddCommandHandler("toggleDebug", cmd =>
            {
                debugMode = !debugMode;
                Log("Debug mode set to " + debugMode);
            });
            AddCommandHandler("stop", cmd =>
            {
                running = false;
            });
            AddCommandHandler("say", cmd =>
            {
                if (cmd.Content.Trim() == string.Empty)
                {
                    Log("Usage: say [message]");
                }
                else
                {
                    SendToAll(new ChatMessage("Server", ChatMessageType.System, cmd.Content));
                    Log("Chat message sent");
                }
            });
            AddCommandHandler("clients", cmd =>
            {
                Log(clients.Count + " connected client(s)");
                foreach (ServClient client in clients)
                {
                    Log(client.Name);
                }
            });
            AddCommandHandler("players", cmd =>
                {
                    Log(clients.Count + " players(s) in match");
                });
            AddCommandHandler("kick", cmd =>
            {
                if (cmd.Content.Trim() == string.Empty)
                {
                    Log("Usage: kick [client name/part of name]");
                }
                else
                {
                    List<ServClient> matching = SearchClients(cmd.Content);
                    if (matching.Count == 0)
                    {
                        Log("No clients match your search.");
                    }
                    else if (matching.Count == 1)
                    {
                        Kick(matching[0], "Kicked by server");
                    }
                    else
                    {
                        Log("More than one client matches your search:");
                        foreach (ServClient client in matching)
                        {
                            Log(client.Name);
                        }
                    }
                }
            });
            AddCommandHandler("returnToLobby", cmd =>
            {
                ReturnToLobby();
            });
            AddCommandHandler("showSettings", cmd =>
            {
                Log(JsonConvert.SerializeObject(matchSettings, Formatting.Indented));
            });
            AddCommandHandler("loadSettings", cmd =>
            {
                bool success = false;
                if (cmd.Content.Trim() != string.Empty)
                {
                    success = LoadMatchSettings(cmd.Content.Trim());
                }
                else
                {
                    success = LoadMatchSettings();
                }
                if (success)
                {
                    SendToAll(new SettingsChangedMessage(matchSettings));
                }
            });
            AddCommandHandler("setStage", cmd =>
            {
                int inputInt;
                if (int.TryParse(cmd.Content, out inputInt) && inputInt >= 0 && inputInt < STAGE_COUNT)
                {
                    matchSettings.StageId = inputInt;
                    SendToAll(new SettingsChangedMessage(matchSettings));
                    Log("Stage set to " + inputInt);
                }
                else
                {
                    Log("Usage: setStage [0-" + (STAGE_COUNT - 1) + "]");
                }
            });
            AddCommandHandler("setLaps", cmd =>
            {
                int inputInt;
                if (int.TryParse(cmd.Content, out inputInt) && inputInt > 0)
                {
                    matchSettings.Laps = inputInt;
                    SendToAll(new SettingsChangedMessage(matchSettings));
                    Log("Lap count set to " + inputInt);
                }
                else
                {
                    Log("Usage: setLaps [>=0]");
                }
            });
            AddCommandHandler("setAutoStartTime", cmd =>
            {
                int inputInt;
                if (int.TryParse(cmd.Content, out inputInt) && inputInt > 0)
                {
                    matchSettings.AutoStartTime = inputInt;
                    SendToAll(new SettingsChangedMessage(matchSettings));
                    Log("Match auto start time set to " + inputInt);
                }
                else
                {
                    Log("Usage: setAutoStartTime [>=0]");
                }
            });
            AddCommandHandler("setAutoStartMinPlayers", cmd =>
            {
                int inputInt;
                if (int.TryParse(cmd.Content, out inputInt) && inputInt > 0)
                {
                    matchSettings.AutoStartMinPlayers = inputInt;
                    SendToAll(new SettingsChangedMessage(matchSettings));
                    Log("Match auto start minimum players set to " + inputInt);
                }
                else
                {
                    Log("Usage: setAutoStartMinPlayers [>=0]");
                }
            });
            AddCommandHandler("setStageRotationMode", cmd =>
            {
                StageRotationMode rotMode;
                if (Enum.TryParse(cmd.Content, out rotMode))
                {
                    matchSettings.StageRotationMode = rotMode;
                    SendToAll(new SettingsChangedMessage(matchSettings));
                    Log("Stage rotation mode set to " + rotMode);
                }
                else
                {
                    string[] modes = Enum.GetNames(typeof(StageRotationMode));
                    string modesStr = string.Join("|", modes);
                    Log("Usage: setStageRotationMode [" + modesStr + "]");
                }
            });

            #endregion Command handlers

            #region Server config wizard

            if (!File.Exists(CONFIG_FILENAME))
            {
                Console.WriteLine("No server configuration (" + CONFIG_FILENAME + ") found. ");

                ServerConfig newConfig = new ServerConfig();

                while (string.IsNullOrEmpty(newConfig.ServerName))
                {
                    Console.Write("Enter a server name: ");
                    newConfig.ServerName = Console.ReadLine().Trim();
                }

                while (newConfig.PrivatePort == 0)
                {
                    Console.Write("Enter a port to use (Leave blank to use 25000): ");

                    string input = Console.ReadLine();
                    if (input.Trim() == string.Empty)
                    {
                        newConfig.PrivatePort = 25000;
                    }
                    else
                    {
                        int inputInt;
                        if (int.TryParse(input, out inputInt) && inputInt >= System.Net.IPEndPoint.MinPort && inputInt <= System.Net.IPEndPoint.MaxPort)
                        {
                            newConfig.PrivatePort = inputInt;
                        }
                    }
                }

                while (newConfig.MaxPlayers <= 0 || newConfig.MaxPlayers > 64)
                {
                    Console.Write("Enter max players (1-64): ");
                    string input = Console.ReadLine();

                    int inputInt;
                    if (int.TryParse(input, out inputInt) && inputInt >= System.Net.IPEndPoint.MinPort && inputInt <= System.Net.IPEndPoint.MaxPort)
                    {
                        newConfig.MaxPlayers = inputInt;
                    }
                }

                bool inputCorrect = false;
                while (!inputCorrect)
                {
                    Console.Write("Show this server in the server browser? (yes|no): ");
                    string input = Console.ReadLine();
                    if (input == "yes")
                    {
                        newConfig.ShowInBrowser = true;
                        inputCorrect = true;
                    }
                    if (input == "no")
                    {
                        newConfig.ShowInBrowser = false;
                        inputCorrect = true;
                    }
                }

                if (newConfig.ShowInBrowser)
                {
                    while (string.IsNullOrEmpty(newConfig.PublicIP))
                    {
                        Console.Write("Enter the public IP adress to use when connecting from the server browser: ");
                        newConfig.PublicIP = Console.ReadLine().Trim();
                    }

                    while (newConfig.PublicPort == 0)
                    {
                        Console.Write("Enter the public port use when connecting from the server browser (Leave blank to use private port): ");

                        string input = Console.ReadLine();
                        if (input.Trim() == string.Empty)
                        {
                            newConfig.PublicPort = newConfig.PrivatePort;
                        }
                        else
                        {
                            int inputInt;
                            if (int.TryParse(input, out inputInt) && inputInt >= System.Net.IPEndPoint.MinPort && inputInt <= System.Net.IPEndPoint.MaxPort)
                            {
                                newConfig.PublicPort = inputInt;
                            }
                        }
                    }
                }

                using (StreamWriter sw = new StreamWriter(CONFIG_FILENAME))
                {
                    sw.Write(JsonConvert.SerializeObject(newConfig));
                    Console.WriteLine("Config saved!");
                }

                #endregion Server config wizard
            }
        }

        public void Start()
        {
            if (!LoadServerConfig())
                return;

            if (!LoadMatchSettings())
                matchSettings = MatchSettings.CreateDefault();

            //Welcome message
            Log("Welcome! Type 'help' for a list of commands. Type 'stop' to shut down the server.");

            running = true;

#if DEBUG
            debugMode = true;
#endif

            NetPeerConfiguration config = new NetPeerConfiguration(OnlineMatchMessenger.APP_ID);
            config.Port = this.config.PrivatePort;
            config.MaximumConnections = this.config.MaxPlayers;
            config.EnableMessageType(NetIncomingMessageType.ConnectionApproval);
            config.EnableMessageType(NetIncomingMessageType.DiscoveryRequest);

            netServer = new NetServer(config);
            netServer.Start();

            Log("Server started on port " + this.config.PrivatePort + "!");

            if (this.config.ShowInBrowser)
            {
                AddToServerBrowser();
                serverBrowserPingTimer.Start();
            }

            MessageLoop();
        }

        private bool LoadServerConfig()
        {
            if (File.Exists(CONFIG_FILENAME))
            {
                using (StreamReader sr = new StreamReader(CONFIG_FILENAME))
                {
                    try
                    {
                        config = JsonConvert.DeserializeObject<ServerConfig>(sr.ReadToEnd());
                        Log("Loaded server config from " + CONFIG_FILENAME);
                        return true;
                    }
                    catch (JsonException ex)
                    {
                        Log("Failed to load server config from " + CONFIG_FILENAME + ", server cannot start. Please fix or delete the file. (" + ex.Message + ")", LogType.Error);
                    }
                }
            }
            else
            {
                Log("Server config at " + CONFIG_FILENAME + " not found. Please create a server config.", LogType.Error);
            }
            return false;
        }

        private bool LoadMatchSettings(string path = SETTINGS_FILENAME)
        {
            if (File.Exists(path))
            {
                using (StreamReader sr = new StreamReader(path))
                {
                    try
                    {
                        matchSettings = JsonConvert.DeserializeObject<MatchSettings>(sr.ReadToEnd());
                        Log("Loaded match settings from " + path);
                        return true;
                    }
                    catch (JsonException ex)
                    {
                        Log("Failed to match settings from " + path + ", using default settings instead. (" + ex.Message + ")", LogType.Warning);
                    }
                }
            }
            else
            {
                Log("Match settings at " + path + " not found, using default settings", LogType.Warning);
            }
            return false;
        }

        private async void AddToServerBrowser()
        {
            using (var client = new HttpClient())
            {
                var values = new Dictionary<string, string>
                {
                    { "ip", config.PublicIP },
                    { "port", config.PublicPort.ToString() }
                };

                var content = new FormUrlEncodedContent(values);

                var response = await client.PostAsync("http://www.sanicball.com/servers/add/", content);
                if (response.IsSuccessStatusCode)
                {
                    Log("Server browser said: " + await response.Content.ReadAsStringAsync(), LogType.Debug);
                }
                else
                {
                    Log("Failed adding server to server browser - " + response.ReasonPhrase, LogType.Warning);
                }
            }
        }

        private void MessageLoop()
        {
            while (running)
            {
                Thread.Sleep(1000 / TICKRATE);

                //Check server browser ping timer
                if (serverBrowserPingTimer.IsRunning)
                {
                    if (serverBrowserPingTimer.Elapsed.TotalSeconds >= serverBrowserPingGoal)
                    {
                        AddToServerBrowser();
                        serverBrowserPingTimer.Restart();
                    }
                }

                //Check lobby timer
                if (lobbyTimer.IsRunning)
                {
                    if (lobbyTimer.Elapsed.TotalSeconds >= lobbyTimerGoal)
                    {
                        Log("The race has been started by all players being ready.");
                        LoadRace();
                    }
                }

                //Check stage loading timer
                if (stageLoadingTimeoutTimer.IsRunning)
                {
                    if (stageLoadingTimeoutTimer.Elapsed.TotalSeconds >= stageLoadingTimeoutTimerGoal)
                    {
                        SendToAll(new StartRaceMessage());
                        stageLoadingTimeoutTimer.Reset();

                        foreach (var c in clients.Where(a => a.CurrentlyLoadingStage))
                        {
                            Kick(c, "Took too long to load the race");
                        }
                    }
                }

                //Check auto start timer
                if (autoStartTimer.IsRunning)
                {
                    if (autoStartTimer.Elapsed.TotalSeconds >= matchSettings.AutoStartTime)
                    {
                        Log("The race has been automatically started.");
                        LoadRace();
                    }
                }

                //Check back to lobby timer
                if (backToLobbyTimer.IsRunning)
                {
                    if (backToLobbyTimer.Elapsed.TotalSeconds >= matchSettings.AutoReturnTime)
                    {
                        ReturnToLobby();
                        backToLobbyTimer.Reset();
                    }
                }

                //Check racing timeout timers
                foreach (ServPlayer p in players)
                {
                    if (!p.TimeoutMessageSent && p.RacingTimeout.Elapsed.TotalSeconds > PLAYER_TIMEOUT_NOTIFY)
                    {
                        SendToAll(new RaceTimeoutMessage(p.ClientGuid, p.CtrlType, PLAYER_TIMEOUT_DISQUALIFY - PLAYER_TIMEOUT_NOTIFY));
                        p.TimeoutMessageSent = true;
                    }
                    if (p.RacingTimeout.Elapsed.TotalSeconds > PLAYER_TIMEOUT_DISQUALIFY)
                    {
                        Log("A player was too slow to race and has been disqualified.");
                        FinishRace(p);

                        SendToAll(new DoneRacingMessage(p.ClientGuid, p.CtrlType, 0, true));
                    }
                }

                //Check command queue
                Command cmd;
                while ((cmd = commandQueue.ReadNext()) != null)
                {
                    CommandHandler handler;
                    if (commandHandlers.TryGetValue(cmd.Name, out handler))
                    {
                        handler(cmd);
                    }
                    else
                    {
                        Log("Command '" + cmd.Name + "' not found.");
                    }
                }

                //Check network message queue
                NetIncomingMessage msg;
                while ((msg = netServer.ReadMessage()) != null)
                {
                    switch (msg.MessageType)
                    {
                        case NetIncomingMessageType.VerboseDebugMessage:
                        case NetIncomingMessageType.DebugMessage:
                            Log("Lidgren debug: " + msg.ReadString(), LogType.Debug);
                            break;

                        case NetIncomingMessageType.WarningMessage:
                            Log("Lidgren warning: " + msg.ReadString(), LogType.Warning);
                            break;

                        case NetIncomingMessageType.ErrorMessage:
                            Log("Lidgren error: " + msg.ReadString(), LogType.Error);
                            break;

                        case NetIncomingMessageType.DiscoveryRequest:
                            ServerInfo info = new ServerInfo();
                            info.Config = config;
                            info.Players = netServer.ConnectionsCount;
                            info.InRace = inRace;
                            NetOutgoingMessage responseMessage = netServer.CreateMessage();
                            responseMessage.Write(JsonConvert.SerializeObject(info));
                            netServer.SendDiscoveryResponse(responseMessage, msg.SenderEndPoint);
                            Log("Sent discovery response to " + msg.SenderEndPoint, LogType.Debug);
                            break;

                        case NetIncomingMessageType.ConnectionApproval:
                            ClientInfo clientInfo = null;
                            try
                            {
                                clientInfo = JsonConvert.DeserializeObject<ClientInfo>(msg.ReadString());
                            }
                            catch (JsonException ex)
                            {
                                Log("Error reading client connection approval: \"" + ex.Message + "\". Client rejected.", LogType.Warning);
                                msg.SenderConnection.Deny("Invalid client info! You are likely using a different game version than the server.");
                                break;
                            }

                            if (clientInfo.Version != GameVersion.AS_FLOAT || clientInfo.IsTesting != GameVersion.IS_TESTING)
                            {
                                msg.SenderConnection.Deny("Wrong game version.");
                                break;
                            }

                            msg.SenderConnection.Approve();
                            break;

                        case NetIncomingMessageType.StatusChanged:
                            NetConnectionStatus status = (NetConnectionStatus)msg.ReadByte();

                            string statusMsg = msg.ReadString();
                            switch (status)
                            {
                                case NetConnectionStatus.Connected:
                                    //Send match state to newly connected client
                                    NetOutgoingMessage stateMsg = netServer.CreateMessage();
                                    stateMsg.Write(MessageType.InitMessage);

                                    float autoStartTimeLeft = 0;
                                    if (autoStartTimer.IsRunning)
                                    {
                                        autoStartTimeLeft = matchSettings.AutoStartTime - (float)autoStartTimer.Elapsed.TotalSeconds;
                                    }
                                    List<MatchClientState> clientStates = new List<MatchClientState>();
                                    foreach (ServClient c in clients)
                                    {
                                        clientStates.Add(new MatchClientState(c.Guid, c.Name));
                                    }
                                    List<MatchPlayerState> playerStates = new List<MatchPlayerState>();
                                    foreach (ServPlayer p in players)
                                    {
                                        playerStates.Add(new MatchPlayerState(p.ClientGuid, p.CtrlType, p.ReadyToRace, p.CharacterId));
                                    }

                                    MatchState state = new MatchState(clientStates, playerStates, matchSettings, inRace, autoStartTimeLeft);

                                    string str = JsonConvert.SerializeObject(state);
                                    Log("Sending match state: " + str, LogType.Debug);
                                    stateMsg.Write(str);

                                    netServer.SendMessage(stateMsg, msg.SenderConnection, NetDeliveryMethod.ReliableOrdered);

                                    Log("Sent match state to newly connected client", LogType.Debug);
                                    break;

                                case NetConnectionStatus.Disconnected:
                                    ServClient associatedClient = clients.FirstOrDefault(a => a.Connection == msg.SenderConnection);
                                    if (associatedClient != null)
                                    {
                                        //Remove all players created by this client
                                        players.RemoveAll(a => a.ClientGuid == associatedClient.Guid);

                                        //If no players are left and we're in a race, return to lobby
                                        if (players.Count == 0 && inRace)
                                        {
                                            Log("No players left in race!");
                                            ReturnToLobby();
                                        }

                                        //If there are now less players than AutoStartMinPlayers, stop the auto start timer
                                        if (players.Count < matchSettings.AutoStartMinPlayers && autoStartTimer.IsRunning)
                                        {
                                            Log("Too few players, match auto start timer stopped");
                                            StopAutoStartTimer();
                                        }

                                        //Remove the client
                                        clients.Remove(associatedClient);

                                        //Tell connected clients to remove the client+players
                                        SendToAll(new ClientLeftMessage(associatedClient.Guid));

                                        Broadcast(associatedClient.Name + " has left the match (" + statusMsg + ")");
                                    }
                                    else
                                    {
                                        Log("Unknown client disconnected (Client was most likely not done connecting)");
                                    }
                                    break;

                                default:
                                    Log("Status change received: " + status + " - Message: " + statusMsg, LogType.Debug);
                                    break;
                            }
                            break;

                        case NetIncomingMessageType.Data:
                            byte messageType = msg.ReadByte();
                            switch (messageType)
                            {
                                case MessageType.MatchMessage:

                                    double timestamp = msg.ReadTime(false);
                                    MatchMessage matchMessage = null;
                                    try
                                    {
                                        matchMessage = JsonConvert.DeserializeObject<MatchMessage>(msg.ReadString(), new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All });
                                    }
                                    catch (JsonException ex)
                                    {
                                        Log("Failed to deserialize received match message. Error description: " + ex.Message, LogType.Warning);
                                        continue; //Skip to next message in queue
                                    }

                                    if (matchMessage is ClientJoinedMessage)
                                    {
                                        var castedMsg = (ClientJoinedMessage)matchMessage;

                                        ServClient newClient = new ServClient(castedMsg.ClientGuid, castedMsg.ClientName, msg.SenderConnection);
                                        clients.Add(newClient);

                                        Broadcast(castedMsg.ClientName + " has joined the match");
                                        SendToAll(matchMessage);
                                    }

                                    if (matchMessage is PlayerJoinedMessage)
                                    {
                                        var castedMsg = (PlayerJoinedMessage)matchMessage;

                                        //Check if the message was sent from the same client it wants to act for
                                        ServClient client = clients.FirstOrDefault(a => a.Connection == msg.SenderConnection);

                                        if (client == null || castedMsg.ClientGuid != client.Guid)
                                        {
                                            Log("Received PlayerJoinedMessage with invalid ClientGuid property", LogType.Warning);
                                        }
                                        else
                                        {
                                            players.Add(new ServPlayer(castedMsg.ClientGuid, castedMsg.CtrlType, castedMsg.InitialCharacter));
                                            Log("Player " + client.Name + " (" + castedMsg.CtrlType + ") joined", LogType.Debug);
                                            SendToAll(matchMessage);

                                            if (players.Count >= matchSettings.AutoStartMinPlayers && !autoStartTimer.IsRunning && matchSettings.AutoStartTime > 0)
                                            {
                                                Log("Match will auto start in " + matchSettings.AutoStartTime + " seconds.");
                                                StartAutoStartTimer();
                                            }
                                        }
                                    }

                                    if (matchMessage is PlayerLeftMessage)
                                    {
                                        var castedMsg = (PlayerLeftMessage)matchMessage;

                                        //Check if the message was sent from the same client it wants to act for
                                        ServClient client = clients.FirstOrDefault(a => a.Connection == msg.SenderConnection);
                                        if (client == null || castedMsg.ClientGuid != client.Guid)
                                        {
                                            Log("Received PlayerLeftMessage with invalid ClientGuid property", LogType.Warning);
                                        }
                                        else
                                        {
                                            ServPlayer player = players.FirstOrDefault(a => a.ClientGuid == castedMsg.ClientGuid && a.CtrlType == castedMsg.CtrlType);
                                            players.Remove(player);
                                            Log("Player " + client.Name + " (" + castedMsg.CtrlType + ") left", LogType.Debug);
                                            SendToAll(matchMessage);

                                            if (players.Count < matchSettings.AutoStartMinPlayers && autoStartTimer.IsRunning)
                                            {
                                                Log("Too few players, match auto start timer stopped");
                                                StopAutoStartTimer();
                                            }
                                        }
                                    }

                                    if (matchMessage is CharacterChangedMessage)
                                    {
                                        var castedMsg = (CharacterChangedMessage)matchMessage;

                                        //Check if the message was sent from the same client it wants to act for
                                        ServClient client = clients.FirstOrDefault(a => a.Connection == msg.SenderConnection);
                                        if (client == null || client.Guid != castedMsg.ClientGuid)
                                        {
                                            Log("Received CharacterChangedMessage with invalid ClientGuid property", LogType.Warning);
                                        }
                                        else
                                        {
                                            ServPlayer player = players.FirstOrDefault(a => a.ClientGuid == castedMsg.ClientGuid && a.CtrlType == castedMsg.CtrlType);
                                            if (player != null)
                                            {
                                                int index = players.IndexOf(player);
                                                players[index].CharacterId = castedMsg.NewCharacter;
                                            }
                                            Log("Player " + client.Name + " (" + castedMsg.CtrlType + ") set character to " + castedMsg.NewCharacter, LogType.Debug);

                                            SendToAll(matchMessage);
                                        }
                                    }

                                    if (matchMessage is ChangedReadyMessage)
                                    {
                                        var castedMsg = (ChangedReadyMessage)matchMessage;

                                        //Check if the message was sent from the same client it wants to act for
                                        ServClient client = clients.FirstOrDefault(a => a.Connection == msg.SenderConnection);
                                        if (client == null || client.Guid != castedMsg.ClientGuid)
                                        {
                                            Log("Received ChangeReadyMessage with invalid ClientGuid property", LogType.Warning);
                                        }
                                        else
                                        {
                                            ServPlayer player = players.FirstOrDefault(a => a.ClientGuid == castedMsg.ClientGuid && a.CtrlType == castedMsg.CtrlType);
                                            if (player != null)
                                            {
                                                int index = players.IndexOf(player);
                                                players[index].ReadyToRace = castedMsg.Ready;
                                            }
                                            Log("Player " + client.Name + " (" + castedMsg.CtrlType + ") set ready to " + castedMsg.Ready, LogType.Debug);

                                            //Start lobby timer if all players are ready - otherwise reset it if it's running
                                            bool allPlayersReady = players.All(a => a.ReadyToRace);
                                            if (allPlayersReady)
                                            {
                                                lobbyTimer.Start();
                                                Log("All players ready, timer started", LogType.Debug);
                                            }
                                            else
                                            {
                                                if (lobbyTimer.IsRunning)
                                                {
                                                    lobbyTimer.Reset();
                                                    Log("Not all players are ready, timer stopped", LogType.Debug);
                                                }
                                            }

                                            SendToAll(matchMessage);
                                        }
                                    }

                                    if (matchMessage is SettingsChangedMessage)
                                    {
                                        //var castedMsg = (SettingsChangedMessage)matchMessage;
                                        //matchSettings = castedMsg.NewMatchSettings;
                                        //SendToAll(matchMessage);

                                        Log("A player tried to change match settings", LogType.Debug);
                                    }

                                    if (matchMessage is StartRaceMessage)
                                    {
                                        int clientsLoadingStage = clients.Count(a => a.CurrentlyLoadingStage);
                                        if (clientsLoadingStage > 0)
                                        {
                                            ServClient client = clients.FirstOrDefault(a => a.Connection == msg.SenderConnection);
                                            client.CurrentlyLoadingStage = false;
                                            clientsLoadingStage--;
                                            if (clientsLoadingStage > 0)
                                            {
                                                Log("Waiting for " + clientsLoadingStage + " client(s) to load");
                                            }
                                            else
                                            {
                                                Log("Starting race!");
                                                SendToAll(new StartRaceMessage());
                                                stageLoadingTimeoutTimer.Reset();
                                                //Indicate that all currently active players are racing
                                                players.ForEach(a =>
                                                {
                                                    a.CurrentlyRacing = true;
                                                    a.RacingTimeout.Start();
                                                });
                                            }
                                        }
                                    }

                                    if (matchMessage is ChatMessage)
                                    {
                                        var castedMsg = (ChatMessage)matchMessage;
                                        Log(string.Format("[{0}] {1}: {2}", castedMsg.Type, castedMsg.From, castedMsg.Text));

                                        if (castedMsg.Text.ToLower().Contains("shrek"))
                                        {
                                            ServClient client = clients.FirstOrDefault(a => a.Connection == msg.SenderConnection);
                                            ServPlayer[] playersFromClient = players.Where(a => a.ClientGuid == client.Guid).ToArray();
                                            foreach (ServPlayer p in playersFromClient)
                                            {
                                                SendToAll(new CharacterChangedMessage(p.ClientGuid, p.CtrlType, 15));
                                            }
                                        }

                                        SendToAll(matchMessage);
                                    }

                                    if (matchMessage is LoadLobbyMessage)
                                    {
                                        ServClient client = clients.FirstOrDefault(a => a.Connection == msg.SenderConnection);
                                        if (!client.WantsToReturnToLobby)
                                        {
                                            client.WantsToReturnToLobby = true;

                                            if (clients.All(a => a.WantsToReturnToLobby))
                                            {
                                                Broadcast("All clients have voted to return to the lobby.");
                                                ReturnToLobby();
                                            }
                                            else
                                            {
                                                int clientsNeeded = clients.Count - clients.Count(a => a.WantsToReturnToLobby);
                                                Broadcast(client.Name + " wants to return to the lobby. " + clientsNeeded + " more vote(s) needed.");
                                            }
                                        }
                                    }

                                    if (matchMessage is CheckpointPassedMessage)
                                    {
                                        var castedMsg = (CheckpointPassedMessage)matchMessage;

                                        ServPlayer player = players.FirstOrDefault(a => a.ClientGuid == castedMsg.ClientGuid && a.CtrlType == castedMsg.CtrlType);
                                        if (player != null)
                                        {
                                            //As long as all players are racing, timeouts should be reset.
                                            if (players.All(a => a.CurrentlyRacing))
                                            {
                                                player.RacingTimeout.Restart();
                                                if (player.TimeoutMessageSent)
                                                {
                                                    player.TimeoutMessageSent = false;
                                                    SendToAll(new RaceTimeoutMessage(player.ClientGuid, player.CtrlType, 0));
                                                }
                                            }
                                            SendToAll(matchMessage);
                                        }
                                        else
                                        {
                                            Log("Received CheckpointPassedMessage for invalid player", LogType.Debug);
                                        }
                                    }

                                    if (matchMessage is PlayerMovementMessage)
                                    {
                                        SendToAll(matchMessage);
                                    }

                                    if (matchMessage is DoneRacingMessage)
                                    {
                                        var castedMsg = (DoneRacingMessage)matchMessage;
                                        ServPlayer player = players.FirstOrDefault(a => a.ClientGuid == castedMsg.ClientGuid && a.CtrlType == castedMsg.CtrlType);
                                        if (player != null)
                                        {
                                            FinishRace(player);
                                        }
                                        SendToAll(matchMessage);
                                    }

                                    break;

                                default:
                                    Log("Received data message of unknown type", LogType.Debug);
                                    break;
                            }
                            break;

                        default:
                            Log("Received unhandled message of type " + msg.MessageType, LogType.Debug);
                            break;
                    }
                }
            }
        }

        #region Gameplay methods

        private void LoadRace()
        {
            lobbyTimer.Reset();
            StopAutoStartTimer();
            SendToAll(new LoadRaceMessage());
            inRace = true;
            //Set ready to false for all players
            players.ForEach(a => a.ReadyToRace = false);
            //Wait for clients to load the stage
            clients.ForEach(a => a.CurrentlyLoadingStage = true);
            //Start timeout timer
            stageLoadingTimeoutTimer.Start();
        }

        private void ReturnToLobby()
        {
            if (inRace)
            {
                Log("Returned to lobby");
                inRace = false;
                SendToAll(new LoadLobbyMessage());

                players.ForEach(a =>
                {
                    a.CurrentlyRacing = false;
                    a.RacingTimeout.Reset();
                    a.TimeoutMessageSent = false;
                });
                clients.ForEach(a => a.WantsToReturnToLobby = false);

                //Stage rotation
                switch (matchSettings.StageRotationMode)
                {
                    case StageRotationMode.Random:
                        Log("Picking random stage");
                        int newStage = random.Next(STAGE_COUNT);
                        matchSettings.StageId = newStage;
                        SendToAll(new SettingsChangedMessage(matchSettings));
                        break;

                    case StageRotationMode.Sequenced:
                        Log("Picking next stage");
                        int nextStage = matchSettings.StageId + 1;
                        if (nextStage >= STAGE_COUNT) nextStage = 0;
                        matchSettings.StageId = nextStage;
                        SendToAll(new SettingsChangedMessage(matchSettings));
                        break;
                }

                if (players.Count >= matchSettings.AutoStartMinPlayers && matchSettings.AutoStartTime > 0)
                {
                    Log("There are still players, autoStartTimer started", LogType.Debug);
                    StartAutoStartTimer();
                }
            }
            else
            {
                Log("Already in lobby");
            }
        }

        private void StartAutoStartTimer()
        {
            autoStartTimer.Reset();
            autoStartTimer.Start();
            SendToAll(new AutoStartTimerMessage(true));
        }

        private void StopAutoStartTimer()
        {
            autoStartTimer.Reset();
            SendToAll(new AutoStartTimerMessage(false));
        }

        private void FinishRace(ServPlayer p)
        {
            p.CurrentlyRacing = false;
            p.RacingTimeout.Reset();

            int playersStillRacing = players.Count(a => a.CurrentlyRacing);
            if (playersStillRacing == 0)
            {
                Log("All players are done racing.");
                if (matchSettings.AutoReturnTime > 0)
                {
                    Broadcast("Returning to lobby in " + matchSettings.AutoReturnTime + " seconds");
                    backToLobbyTimer.Start();
                }
            }
            else
            {
                Log(playersStillRacing + " players(s) still racing");
            }
        }

        #endregion Gameplay methods

        #region Utility methods

        private void SendToAll(MatchMessage matchMsg)
        {
            if (matchMsg.Reliable)
            {
                Log("Sending message of type " + matchMsg.GetType() + " to " + netServer.Connections.Count + " connection(s)", LogType.Debug);
            }
            if (netServer.ConnectionsCount == 0) return;
            string matchMsgSerialized = JsonConvert.SerializeObject(matchMsg, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });

            NetOutgoingMessage netMsg = netServer.CreateMessage();
            netMsg.Write(MessageType.MatchMessage);
            netMsg.WriteTime(false);
            netMsg.Write(matchMsgSerialized);
            netServer.SendMessage(netMsg, netServer.Connections, matchMsg.Reliable ? NetDeliveryMethod.ReliableOrdered : NetDeliveryMethod.Unreliable, 0);
        }

        /// <summary>
        /// Logs a string and sends it as a chat message to all clients.
        /// </summary>
        /// <param name="text"></param>
        private void Broadcast(string text)
        {
            Log(text);
            SendToAll(new ChatMessage("Server", ChatMessageType.System, text));
        }

        /// <summary>
        /// Writes a message to the server log.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="type"></param>
        public void Log(object message, LogType type = LogType.Normal)
        {
            if (!debugMode && type == LogType.Debug)
                return;
            LogEntry entry = new LogEntry(DateTime.Now, message.ToString(), type);
            OnLog?.Invoke(this, new LogArgs(entry));
            log.Add(entry);
        }

        private List<ServClient> SearchClients(string name)
        {
            return clients.Where(a => a.Name.Contains(name)).ToList();
        }

        public void AddCommandHandler(string commandName, CommandHandler handler)
        {
            commandHandlers.Add(commandName, handler);
        }

        public void Kick(ServClient client, string reason)
        {
            client.Connection.Disconnect(reason);
        }

        #endregion Utility methods

        public void Dispose()
        {
            Log("Saving server config...");
            using (StreamWriter sw = new StreamWriter(CONFIG_FILENAME))
            {
                sw.Write(JsonConvert.SerializeObject(config));
            }

            Log("Saving match settings...");
            using (StreamWriter sw = new StreamWriter(SETTINGS_FILENAME))
            {
                sw.Write(JsonConvert.SerializeObject(matchSettings));
            }

            if (netServer != null)
                netServer.Shutdown("Server was closed.");
            Log("The server has been closed.");

            //Write server log
            Directory.CreateDirectory("Logs" + System.IO.Path.DirectorySeparatorChar);
            using (StreamWriter writer = new StreamWriter("Logs" + System.IO.Path.DirectorySeparatorChar + DateTime.Now.ToString("MM-dd-yyyy_HH-mm-ss") + ".txt"))
            {
                foreach (LogEntry entry in log)
                {
                    string logTypeText = "";
                    switch (entry.Type)
                    {
                        case LogType.Debug:
                            logTypeText = " [DEBUG]";
                            break;

                        case LogType.Warning:
                            logTypeText = " [WARNING]";
                            break;

                        case LogType.Error:
                            logTypeText = " [ERROR]";
                            break;
                    }
                    writer.WriteLine(entry.Timestamp + logTypeText + " - " + entry.Message);
                }
            }
        }
    }
}