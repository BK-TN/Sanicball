using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
        private const string SETTINGS_FILENAME = "MatchSettings.json";
        private const int TICKRATE = 20;

        public event EventHandler<LogArgs> OnLog;

        private List<LogEntry> log = new List<LogEntry>();
        private Dictionary<string, CommandHandler> commandHandlers = new Dictionary<string, CommandHandler>();
        private NetServer netServer;
        private bool running;
        private CommandQueue commandQueue;
        private Random random = new Random();

        //Match state
        private List<MatchClientState> matchClients = new List<MatchClientState>();
        private List<MatchPlayerState> matchPlayers = new List<MatchPlayerState>();
        private MatchSettings matchSettings;
        private bool inRace;

        //Lobby timer
        private Stopwatch lobbyTimer = new Stopwatch();
        private const float lobbyTimerGoal = 3;
        //Autostart timer
        private Stopwatch autoStartTimer = new Stopwatch();

        //List of clients that haven't loaded a stage yet
        private List<MatchClientState> clientsLoadingStage = new List<MatchClientState>();
        private Stopwatch stageLoadingTimeoutTimer = new Stopwatch();
        private const float stageLoadingTimeoutTimerGoal = 20;

        //List of players that are still racing
        private List<MatchPlayerState> playersStillRacing = new List<MatchPlayerState>();

        //Timer for going back to lobby at the end of a race
        private Stopwatch backToLobbyTimer = new Stopwatch();
        private const float backToLobbyTimerGoal = 15;

        //List of clients wanting to return to lobby
        private List<MatchClientState> clientsWantingToReturn = new List<MatchClientState>();

        //Associates connections with the match client they create (To identify which client is sending a message)
        private Dictionary<NetConnection, MatchClientState> matchClientConnections = new Dictionary<NetConnection, MatchClientState>();

        public bool Running { get { return running; } }

        public Server(CommandQueue commandQueue)
        {
            this.commandQueue = commandQueue;

            AddCommandHandler("help", cmd =>
            {
                Log("Available commands:");
                foreach (string name in commandHandlers.Keys)
                {
                    Log(name);
                }
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
                    SendToAll(new ChatMessage("Server", ChatMessageType.User, cmd.Content));
                    Log("Chat message sent");
                }
            });
            AddCommandHandler("clients", cmd =>
            {
                Log(matchClients.Count + " connected client(s)");
                foreach (MatchClientState client in matchClients)
                {
                    Log(client.Name);
                }
            });
            AddCommandHandler("kick", cmd =>
            {
                if (cmd.Content.Trim() == string.Empty)
                {
                    Log("Usage: kick [client name/part of name]");
                }
                else
                {
                    List<MatchClientState> matching = SearchClients(cmd.Content);
                    if (matching.Count == 0)
                    {
                        Log("No clients match your search.");
                    }
                    else if (matching.Count == 1)
                    {
                        NetConnection conn = matchClientConnections.FirstOrDefault(a => a.Value == matching[0]).Key;
                        Log("Kicked client " + matching[0].Name);
                        conn.Disconnect("Kicked by server");
                    }
                    else
                    {
                        Log("More than one client matches your search:");
                        foreach (MatchClientState client in matching)
                        {
                            Log(client.Name);
                        }
                    }
                }
            });
            AddCommandHandler("showsettings", cmd =>
            {
                Log(JsonConvert.SerializeObject(matchSettings, Formatting.Indented));
            });
            AddCommandHandler("loadsettings", cmd =>
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
            AddCommandHandler("returntolobby", cmd =>
            {
                ReturnToLobby();
            });
        }

        public void Start(int port)
        {
            //Welcome message
            Log("Welcome! Type 'help' for a list of commands. Type 'stop' to shut down the server.");

            if (!LoadMatchSettings())
                matchSettings = MatchSettings.CreateDefault();

            running = true;

            NetPeerConfiguration config = new NetPeerConfiguration(OnlineMatchMessenger.APP_ID);
            config.Port = 25000;
            config.EnableMessageType(NetIncomingMessageType.ConnectionApproval);
            config.EnableMessageType(NetIncomingMessageType.DiscoveryRequest);

            netServer = new NetServer(config);
            netServer.Start();

            Log("Server started on port " + port + "!");

            //Thread messageThread = new Thread(MessageLoop);
            MessageLoop();
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
                        Log("Failed to load " + path + ": " + ex.Message);
                    }
                }
            }
            else
            {
                Log("File " + path + " not found");
            }
            return false;
        }

        private void MessageLoop()
        {
            while (running)
            {
                Thread.Sleep(1000 / TICKRATE);

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
                        Log("Some players are still loading the race, starting anyway", LogType.Debug);
                        SendToAll(new StartRaceMessage());
                        stageLoadingTimeoutTimer.Reset();
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
                    if (backToLobbyTimer.Elapsed.TotalSeconds >= backToLobbyTimerGoal)
                    {
                        ReturnToLobby();
                        backToLobbyTimer.Reset();
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
                            Log(msg.ReadString(), LogType.Debug);
                            break;

                        case NetIncomingMessageType.WarningMessage:
                            Log(msg.ReadString(), LogType.Warning);
                            break;

                        case NetIncomingMessageType.ErrorMessage:
                            Log(msg.ReadString(), LogType.Error);
                            break;

                        case NetIncomingMessageType.DiscoveryRequest:
                            ServerInfo info = new ServerInfo(DateTime.UtcNow, "A server", netServer.ConnectionsCount, 99, inRace);
                            NetOutgoingMessage responseMessage = netServer.CreateMessage();
                            responseMessage.Write(JsonConvert.SerializeObject(info));
                            netServer.SendDiscoveryResponse(responseMessage, msg.SenderEndPoint);
                            Log("Sent discovery response to " + msg.SenderEndPoint, LogType.Debug);
                            break;

                        case NetIncomingMessageType.StatusChanged:
                            NetConnectionStatus status = (NetConnectionStatus)msg.ReadByte();

                            string statusMsg = msg.ReadString();
                            switch (status)
                            {
                                case NetConnectionStatus.Disconnected:
                                    MatchClientState associatedClient;
                                    if (matchClientConnections.TryGetValue(msg.SenderConnection, out associatedClient))
                                    {
                                        //Remove all players created by this client
                                        matchPlayers.RemoveAll(a => a.ClientGuid == associatedClient.Guid);
                                        playersStillRacing.RemoveAll(a => a.ClientGuid == associatedClient.Guid);

                                        //If no players are left and we're in a race, return to lobby
                                        if (matchPlayers.Count == 0 && inRace)
                                        {
                                            Log("No players left in race.");
                                            ReturnToLobby();
                                        }

                                        //If there are now less players than AutoStartMinPlayers, stop the auto start timer
                                        if (matchPlayers.Count < matchSettings.AutoStartMinPlayers && autoStartTimer.IsRunning)
                                        {
                                            Log("Player count now below AutoStartMinPlayers, autoStartTimer stopped", LogType.Debug);
                                            StopAutoStartTimer();
                                        }

                                        //Remove the client
                                        matchClients.Remove(associatedClient);
                                        matchClientConnections.Remove(msg.SenderConnection);
                                        clientsWantingToReturn.Remove(associatedClient);
                                        clientsLoadingStage.Remove(associatedClient);

                                        //Tell connected clients to remove the client+players
                                        SendToAll(new ClientLeftMessage(associatedClient.Guid));

                                        Broadcast(associatedClient.Name + " has left the match");
                                        Log("(Guid: " + associatedClient.Guid + ", reason: " + statusMsg + ")", LogType.Debug);
                                    }
                                    else
                                    {
                                        Log("Unknown client disconnected (Client was most likely not done connecting)");
                                    }
                                    break;

                                default:
                                    Log("Status change recieved: " + status + " - Message: " + statusMsg, LogType.Debug);
                                    break;
                            }
                            break;

                        case NetIncomingMessageType.ConnectionApproval:
                            ClientInfo clientInfo = null;
                            bool approved = false;
                            try
                            {
                                clientInfo = JsonConvert.DeserializeObject<ClientInfo>(msg.ReadString());
                                approved = true;
                            }
                            catch (JsonException ex)
                            {
                                Log("Error reading client connection approval: \"" + ex.Message + "\". Client rejected.");
                            }
                            if (!approved)
                            {
                                msg.SenderConnection.Deny("Invalid client info!");
                                break;
                            }

                            if (clientInfo.Version != GameVersion.AS_FLOAT || clientInfo.IsTesting != GameVersion.IS_TESTING)
                            {
                                msg.SenderConnection.Deny("Wrong game version.");
                                break;
                            }

                            //Create hail message with match state
                            NetOutgoingMessage hailMsg = netServer.CreateMessage();

                            float autoStartTimeLeft = 0;
                            if (autoStartTimer.IsRunning)
                            {
                                autoStartTimeLeft = matchSettings.AutoStartTime - (float)autoStartTimer.Elapsed.TotalSeconds;
                            }

                            MatchState state = new MatchState(new List<MatchClientState>(matchClients), new List<MatchPlayerState>(matchPlayers), matchSettings, inRace, autoStartTimeLeft);
                            string infoStr = JsonConvert.SerializeObject(state);

                            hailMsg.Write(infoStr);
                            msg.SenderConnection.Approve(hailMsg);
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
                                        Log("Failed to deserialize recieved match message. Error description: " + ex.Message);
                                        continue; //Skip to next message in queue
                                    }

                                    if (matchMessage is ClientJoinedMessage)
                                    {
                                        var castedMsg = (ClientJoinedMessage)matchMessage;

                                        MatchClientState newClient = new MatchClientState(castedMsg.ClientGuid, castedMsg.ClientName);
                                        matchClients.Add(newClient);
                                        matchClientConnections.Add(msg.SenderConnection, newClient);

                                        Broadcast(castedMsg.ClientName + " has joined the match");
                                        Log("(Guid: " + castedMsg.ClientGuid + ")", LogType.Debug);
                                        SendToAll(matchMessage);
                                    }

                                    if (matchMessage is PlayerJoinedMessage)
                                    {
                                        var castedMsg = (PlayerJoinedMessage)matchMessage;

                                        //Check if the message was sent from the same client it wants to act for

                                        if (castedMsg.ClientGuid != matchClientConnections[msg.SenderConnection].Guid)
                                        {
                                            Log("Recieved PlayerJoinedMessage with invalid ClientGuid property", LogType.Warning);
                                        }
                                        else
                                        {
                                            matchPlayers.Add(new MatchPlayerState(castedMsg.ClientGuid, castedMsg.CtrlType, false, castedMsg.InitialCharacter));
                                            Log("Player " + castedMsg.ClientGuid + "#" + castedMsg.CtrlType + " joined", LogType.Debug);
                                            SendToAll(matchMessage);

                                            if (matchPlayers.Count >= matchSettings.AutoStartMinPlayers && !autoStartTimer.IsRunning && matchSettings.AutoStartTime > 0)
                                            {
                                                Log("Player count is now above AutoStartMinPlayers, autoStartTimer started", LogType.Debug);
                                                StartAutoStartTimer();
                                            }
                                        }
                                    }

                                    if (matchMessage is PlayerLeftMessage)
                                    {
                                        var castedMsg = (PlayerLeftMessage)matchMessage;

                                        //Check if the message was sent from the same client it wants to act for
                                        if (castedMsg.ClientGuid != matchClientConnections[msg.SenderConnection].Guid)
                                        {
                                            Log("Recieved PlayerLeftMessage with invalid ClientGuid property", LogType.Warning);
                                        }
                                        else
                                        {
                                            MatchPlayerState player = matchPlayers.FirstOrDefault(a => a.ClientGuid == castedMsg.ClientGuid && a.CtrlType == castedMsg.CtrlType);
                                            matchPlayers.Remove(player);
                                            playersStillRacing.Remove(player);
                                            Log("Player " + castedMsg.ClientGuid + "#" + castedMsg.CtrlType + " left", LogType.Debug);
                                            SendToAll(matchMessage);

                                            if (matchPlayers.Count < matchSettings.AutoStartMinPlayers && autoStartTimer.IsRunning)
                                            {
                                                Log("Player count is now below AutoStartMinPlayers, autoStartTimer stopped", LogType.Debug);
                                                StopAutoStartTimer();
                                            }
                                        }
                                    }

                                    if (matchMessage is CharacterChangedMessage)
                                    {
                                        var castedMsg = (CharacterChangedMessage)matchMessage;

                                        //Check if the message was sent from the same client it wants to act for
                                        if (castedMsg.ClientGuid != matchClientConnections[msg.SenderConnection].Guid)
                                        {
                                            Log("Recieved CharacterChangedMessage with invalid ClientGuid property", LogType.Warning);
                                        }
                                        else
                                        {
                                            MatchPlayerState player = matchPlayers.FirstOrDefault(a => a.ClientGuid == castedMsg.ClientGuid && a.CtrlType == castedMsg.CtrlType);
                                            if (player != null)
                                            {
                                                int index = matchPlayers.IndexOf(player);
                                                matchPlayers[index] = new MatchPlayerState(player.ClientGuid, player.CtrlType, player.ReadyToRace, castedMsg.NewCharacter);
                                            }
                                            Log("Player " + castedMsg.ClientGuid + "#" + castedMsg.CtrlType + " set character to " + castedMsg.NewCharacter, LogType.Debug);

                                            SendToAll(matchMessage);
                                        }
                                    }

                                    if (matchMessage is ChangedReadyMessage)
                                    {
                                        var castedMsg = (ChangedReadyMessage)matchMessage;

                                        //Check if the message was sent from the same client it wants to act for
                                        if (castedMsg.ClientGuid != matchClientConnections[msg.SenderConnection].Guid)
                                        {
                                            Log("Recieved ChangeReadyMessage with invalid ClientGuid property", LogType.Warning);
                                        }
                                        else
                                        {
                                            MatchPlayerState player = matchPlayers.FirstOrDefault(a => a.ClientGuid == castedMsg.ClientGuid && a.CtrlType == castedMsg.CtrlType);
                                            if (player != null)
                                            {
                                                int index = matchPlayers.IndexOf(player);
                                                matchPlayers[index] = new MatchPlayerState(player.ClientGuid, player.CtrlType, castedMsg.Ready, player.CharacterId);
                                            }
                                            Log("Player " + castedMsg.ClientGuid + "#" + castedMsg.CtrlType + " set ready to " + castedMsg.Ready, LogType.Debug);

                                            //Start lobby timer if all players are ready - otherwise reset it if it's running
                                            bool allPlayersReady = matchPlayers.All(a => a.ReadyToRace);
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
                                                    Log("Timer stopped, not all players are ready", LogType.Debug);
                                                }
                                            }

                                            SendToAll(matchMessage);
                                        }
                                    }

                                    if (matchMessage is SettingsChangedMessage)
                                    {
                                        var castedMsg = (SettingsChangedMessage)matchMessage;
                                        matchSettings = castedMsg.NewMatchSettings;
                                        Log("New settings recieved", LogType.Debug);
                                        SendToAll(matchMessage);
                                    }

                                    if (matchMessage is StartRaceMessage)
                                    {
                                        if (clientsLoadingStage.Count > 0)
                                        {
                                            MatchClientState client = matchClientConnections[msg.SenderConnection];
                                            clientsLoadingStage.Remove(client);
                                            Log("Waiting for " + clientsLoadingStage.Count + " client(s) to load");

                                            if (clientsLoadingStage.Count == 0)
                                            {
                                                Log("Starting race!", LogType.Debug);
                                                SendToAll(new StartRaceMessage());
                                                stageLoadingTimeoutTimer.Reset();
                                                playersStillRacing.AddRange(matchPlayers);
                                            }
                                        }
                                    }

                                    if (matchMessage is ChatMessage)
                                    {
                                        var castedMsg = (ChatMessage)matchMessage;
                                        Log(string.Format("[{0}] {1}: {2}", castedMsg.Type, castedMsg.From, castedMsg.Text));

                                        SendToAll(matchMessage);
                                    }

                                    if (matchMessage is LoadLobbyMessage)
                                    {
                                        MatchClientState client = matchClientConnections[msg.SenderConnection];
                                        if (!clientsWantingToReturn.Contains(client))
                                        {
                                            clientsWantingToReturn.Add(client);

                                            if (clientsWantingToReturn.Count >= matchClients.Count)
                                            {
                                                Broadcast("All clients have voted to return to the lobby.");
                                                ReturnToLobby();
                                            }
                                            else
                                            {
                                                int clientsNeeded = matchClients.Count - clientsWantingToReturn.Count;
                                                Broadcast(client.Name + " wants to return to the lobby. " + clientsNeeded + " more vote(s) needed.");
                                            }
                                        }
                                    }

                                    if (matchMessage is CheckpointPassedMessage)
                                    {
                                        var castedMsg = (CheckpointPassedMessage)matchMessage;
                                        Log("Player entered checkpoint with lap time " + castedMsg.LapTime, LogType.Debug);
                                        SendToAll(matchMessage);
                                    }

                                    if (matchMessage is PlayerMovementMessage)
                                    {
                                        SendToAll(matchMessage);
                                    }

                                    if (matchMessage is DoneRacingMessage)
                                    {
                                        var castedMsg = (DoneRacingMessage)matchMessage;
                                        MatchPlayerState player = playersStillRacing.FirstOrDefault(a => a.ClientGuid == castedMsg.ClientGuid && a.CtrlType == castedMsg.CtrlType);
                                        if (player != null)
                                        {
                                            playersStillRacing.Remove(player);

                                            Log(playersStillRacing.Count + " players(s) still racing");

                                            if (playersStillRacing.Count == 0)
                                            {
                                                Log("All players are done racing, returning to lobby in " + backToLobbyTimerGoal + " seconds");
                                                backToLobbyTimer.Start();
                                            }
                                        }
                                    }

                                    break;

                                default:
                                    Log("Recieved data message of unknown type");
                                    break;
                            }
                            break;

                        default:
                            Log("Recieved unhandled message of type " + msg.MessageType, LogType.Debug);
                            break;
                    }
                }
            }
        }

        private void LoadRace()
        {
            lobbyTimer.Reset();
            StopAutoStartTimer();
            SendToAll(new LoadRaceMessage());
            inRace = true;
            for (int i = 0; i < matchPlayers.Count; i++)
            {
                MatchPlayerState player = matchPlayers[i];
                matchPlayers[i] = new MatchPlayerState(player.ClientGuid, player.CtrlType, false, player.CharacterId);
            }
            //Wait for clients to load the stage
            clientsLoadingStage.AddRange(matchClients);
            stageLoadingTimeoutTimer.Start();
        }

        private void ReturnToLobby()
        {
            if (inRace)
            {
                Log("Returned to lobby");
                inRace = false;
                SendToAll(new LoadLobbyMessage());

                playersStillRacing.Clear();
                clientsWantingToReturn.Clear();

                //Stage rotation
                const int stageCount = 5; //Hardcoded stage count for now.. can't recieve the actual count since it's part of a Unity prefab.
                switch (matchSettings.StageRotationMode)
                {
                    case StageRotationMode.Random:
                        Log("Picking random stage");
                        int newStage = random.Next(stageCount);
                        matchSettings.StageId = newStage;
                        SendToAll(new SettingsChangedMessage(matchSettings));
                        break;

                    case StageRotationMode.Sequenced:
                        Log("Picking next stage");
                        int nextStage = matchSettings.StageId + 1;
                        if (nextStage >= stageCount) nextStage = 0;
                        matchSettings.StageId = nextStage;
                        SendToAll(new SettingsChangedMessage(matchSettings));
                        break;
                }

                if (matchPlayers.Count >= matchSettings.AutoStartMinPlayers && matchSettings.AutoStartTime > 0)
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
            netServer.SendMessage(netMsg, netServer.Connections, NetDeliveryMethod.ReliableOrdered, 0);
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

        public void Dispose()
        {
            Log("Saving match settings...");
            using (StreamWriter sw = new StreamWriter(SETTINGS_FILENAME))
            {
                sw.Write(JsonConvert.SerializeObject(matchSettings));
            }
            netServer.Shutdown("Server was closed.");
            Log("The server has been closed.");

            //Write server log
            Directory.CreateDirectory("Logs\\");
            using (StreamWriter writer = new StreamWriter("Logs\\" + DateTime.Now.ToString("MM-dd-yyyy_HH-mm-ss") + ".txt"))
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

        private void Log(object message, LogType type = LogType.Normal)
        {
            LogEntry entry = new LogEntry(DateTime.Now, message.ToString(), type);
            OnLog?.Invoke(this, new LogArgs(entry));
            log.Add(entry);
        }

        private List<MatchClientState> SearchClients(string name)
        {
            return matchClients.Where(a => a.Name.Contains(name)).ToList();
        }

        public void AddCommandHandler(string commandName, CommandHandler handler)
        {
            commandHandlers.Add(commandName, handler);
        }
    }
}