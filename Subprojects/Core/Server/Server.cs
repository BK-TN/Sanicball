using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

//using System.Threading.Tasks;
using Lidgren.Network;
using Newtonsoft.Json;
using SanicballCore.MatchMessages;

namespace SanicballCore.Server
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
        private const string MOTD_FILENAME = "MOTD.txt";
		private const string DEFAULT_SERVER_LIST_URL = "https://sanicball.bdgr.zone/servers";
        private const int TICKRATE = 20;
        private const int STAGE_COUNT = 5; //Hardcoded stage count for now.. can't receive the actual count since it's part of a Unity prefab.
        private readonly CharacterTier[] characterTiers = new[] { //Hardcoded character tiers, same reason
            CharacterTier.Normal,       //Sanic
            CharacterTier.Normal,       //Knackles
            CharacterTier.Normal,       //Taels
            CharacterTier.Normal,       //Ame
            CharacterTier.Normal,       //Shedew
            CharacterTier.Normal,       //Roge
            CharacterTier.Normal,       //Asspio
            CharacterTier.Odd,          //Big
            CharacterTier.Odd,          //Aggmen
            CharacterTier.Odd,          //Chermy
            CharacterTier.Normal,       //Sulver
            CharacterTier.Normal,       //Bloze
            CharacterTier.Normal,       //Vactor
            CharacterTier.Hyperspeed,   //Super Sanic
            CharacterTier.Odd,       //Metal Sanic
            CharacterTier.Odd,          //Ogre
        };

        public event EventHandler<LogArgs> OnLog;

        //Server utilities
        private List<LogEntry> log = new List<LogEntry>();
        private Dictionary<string, CommandHandler> commandHandlers = new Dictionary<string, CommandHandler>();
        private Dictionary<string, string> commandHelp = new Dictionary<string, string>();
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
        private string motd;
        private bool inRace;

        #region Timers

        //Server browser ping timer
        private Stopwatch serverListPingTimer = new Stopwatch();
        private const float SERVER_BROWSER_PING_INTERVAL = 600;

        //Timer for starting a match by all players being ready
        private Stopwatch lobbyTimer = new Stopwatch();
        private const float LOBBY_MATCH_START_TIME = 3;

        //Timer for starting a match automatically
        private Stopwatch autoStartTimer = new Stopwatch();

        //Timeout for clients loading stage
        private Stopwatch stageLoadingTimeoutTimer = new Stopwatch();
        private const float STAGE_LOADING_TIMEOUT = 20;

        //Timer for going back to lobby at the end of a race
        private Stopwatch backToLobbyTimer = new Stopwatch();

        #endregion Timers

        public bool Running { get { return running; } }

        public Server(CommandQueue commandQueue)
        {
            this.commandQueue = commandQueue;

            #region Command handlers

            AddCommandHandler("help",
            "help help help",
            cmd =>
            {
                if (cmd.Content.Trim() == string.Empty)
                {
                    Log("Available commands:");
                    foreach (string name in commandHandlers.Keys)
                    {
                        Log(name);
                    }
                    Log("Use 'help [command name]' for a command decription");
                }
                else
                {
                    string help;
                    if (commandHelp.TryGetValue(cmd.Content.Trim(), out help))
                    {
                        Log(help);
                    }
                }
            });
            AddCommandHandler("toggleDebug",
            "Debug mode displays a ton of extra technical output. Useful if you suspect something is wrong with the server.",
            cmd =>
            {
                debugMode = !debugMode;
                Log("Debug mode set to " + debugMode);
            });
            AddCommandHandler("stop",
            "Stops the server. I recommend stopping it this way - any other probably won't save the server log.",
            cmd =>
            {
                running = false;
            });
            AddCommandHandler("say",
            "Chat to clients on the server.",
            cmd =>
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
            AddCommandHandler("clients",
            "Displays a list of connected clients. (A client is a technical term another Sanicball instance)",
            cmd =>
            {
                Log(clients.Count + " connected client(s)");
                foreach (ServClient client in clients)
                {
                    Log(client.Name);
                }
            });
            AddCommandHandler("players",
            "Displays a list of active players. Clients can have multiple players for splitscreen, or none at all to spectate.",
            cmd =>
                {
                    Log(clients.Count + " players(s) in match");
                });
            AddCommandHandler("kick", 
            "Kicks a player from the server. Of course he could just re-join, but hopefully he'll get the message.",
            cmd =>
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
            AddCommandHandler("returnToLobby",
            "Force exits any ongoing race.",
            cmd =>
            {
                ReturnToLobby();
            });
            AddCommandHandler("forceStart",
            "Force starts a race when in the lobby. Use this carefully, players may not be ready for racing",
            cmd =>
            {
                if (inRace == false)
                {
                    Log("The race has been forcefully started.");
                    LoadRace();
                }
                else
                {
                    Log("Race can only be force started in the lobby.");
                }
            });
            AddCommandHandler("showSettings",
            "Shows match settings. Settings like stage rotation are just shown as a number (Example: if StageRotationMode shows '1', it means 'Sequenced')",
            cmd =>
            {
                Log(JsonConvert.SerializeObject(matchSettings, Formatting.Indented));
            });
            AddCommandHandler("reloadSettings",
            "Reloads match settings from the default file, or optionally a custom file path.",
            cmd =>
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
                    CorrectPlayerTiers();
                    SendToAll(new SettingsChangedMessage(matchSettings));
                }
            });
            AddCommandHandler("reloadMOTD",
            "Reloads message of the day from the default file, or optionally a custom file path.",
            cmd =>
            {
                bool success = false;
                if (cmd.Content.Trim() != string.Empty)
                {
                    LoadMOTD(cmd.Content.Trim());
                }
                else
                {
                    LoadMOTD();
                }
            });
            AddCommandHandler("setStage",
            "Sets the stage by index. 0 = Green Hill, 1 = Flame Core, 2 = Ice Mountain, 3 = Rainbow Road, 4 = Dusty Desert",
            cmd =>
            {
                int inputInt;
                if (int.TryParse(cmd.Content, out inputInt) && inputInt >= 0 && inputInt < STAGE_COUNT)
                {
                    matchSettings.StageId = inputInt;
                    SaveMatchSettings();
                    SendToAll(new SettingsChangedMessage(matchSettings));
                    Log("Stage set to " + inputInt);
                }
                else
                {
                    Log("Usage: setStage [0-" + (STAGE_COUNT - 1) + "]");
                }
            });
            AddCommandHandler("setLaps",
            "Sets the number of laps per race. Laps are pretty long so 2 or 3 is recommended.",
            cmd =>
            {
                int inputInt;
                if (int.TryParse(cmd.Content, out inputInt) && inputInt > 0)
                {
                    matchSettings.Laps = inputInt;
                    SaveMatchSettings();
                    SendToAll(new SettingsChangedMessage(matchSettings));
                    Log("Lap count set to " + inputInt);
                }
                else
                {
                    Log("Usage: setLaps [>0]");
                }
            });
            AddCommandHandler("setAutoStartTime",
            "Sets the time required (in seconds) with enough players in the lobby before a race is automatically started.", 
            cmd =>
            {
                int inputInt;
                if (int.TryParse(cmd.Content, out inputInt) && inputInt > 0)
                {
                    matchSettings.AutoStartTime = inputInt;
                    SaveMatchSettings();
                    SendToAll(new SettingsChangedMessage(matchSettings));
                    Log("Match auto start time set to " + inputInt);
                }
                else
                {
                    Log("Usage: setAutoStartTime [>0]");
                }
            });
            AddCommandHandler("setAutoStartMinPlayers",
            "Sets the minimum amount of players needed in the lobby before the auto start countdown begins.",
            cmd =>
            {
                int inputInt;
                if (int.TryParse(cmd.Content, out inputInt) && inputInt > 0)
                {
                    matchSettings.AutoStartMinPlayers = inputInt;
                    SaveMatchSettings();
                    SendToAll(new SettingsChangedMessage(matchSettings));
                    Log("Match auto start minimum players set to " + inputInt);
                }
                else
                {
                    Log("Usage: setAutoStartMinPlayers [>0]");
                }
            });
            AddCommandHandler("setStageRotationMode",
            "If not set to None, stages will change either randomly or in sequence every time the server returns to lobby.",
            cmd =>
            {
                try {
                    StageRotationMode rotMode = (StageRotationMode)Enum.Parse(typeof(StageRotationMode), cmd.Content);
                    matchSettings.StageRotationMode = rotMode;
                    SaveMatchSettings();
                    SendToAll(new SettingsChangedMessage(matchSettings));
                    Log("Stage rotation mode set to " + rotMode);
                }
                catch (Exception) {
                    string[] modes = Enum.GetNames(typeof(StageRotationMode));
                    string modesStr = string.Join("|", modes);
                    Log("Usage: setStageRotationMode [" + modesStr + "]");
                }
            });
            AddCommandHandler("setAllowedTiers",
            "Controls what ball tiers players can use.",
            cmd =>
            {
                try {
                    AllowedTiers tiers = (AllowedTiers)Enum.Parse(typeof(AllowedTiers), cmd.Content);
                    matchSettings.AllowedTiers = tiers;
                    SaveMatchSettings();
                    SendToAll(new SettingsChangedMessage(matchSettings));
                    CorrectPlayerTiers();
                    Broadcast(GetAllowedTiersText());
                    Log("Allowed tiers set to " + tiers);
                }
                catch (Exception) {
                    string[] modes = Enum.GetNames(typeof(AllowedTiers));
                    string modesStr = string.Join("|", modes);
                    Log("Usage: setAllowedTiers [" + modesStr + "]");
                }
            });
            AddCommandHandler("setTierRotationMode",
            "If not None, allowed ball tiers will change (To either NormalOnly, OddOnly or HyperspeedOnly) each time the server returns to lobby. WeightedRandom has a 10/14 chance of picking NormalOnly, 3/14 of OddOnly and 1/14 of HyperspeedOnly.",
            cmd =>
            {
                try {
                    TierRotationMode mode = (TierRotationMode)Enum.Parse(typeof(TierRotationMode), cmd.Content);
                    matchSettings.TierRotationMode = mode;
                    SaveMatchSettings();
                    SendToAll(new SettingsChangedMessage(matchSettings));
                    Log("Tier rotation mode set to " + mode);
                }
                catch (Exception) {
                    string[] modes = Enum.GetNames(typeof(TierRotationMode));
                    string modesStr = string.Join("|", modes);
                    Log("Usage: setTierRotationMode [" + modesStr + "]");
                }
            });
            AddCommandHandler("setVoteRatio",
            "Sets the fraction of players required to select 'return to lobby' before the server returns to lobby. 1.0, the default, requires all players. Something like 0.8 would be good for a very big server.",
            cmd =>
            {
                float newVoteRatio;
                if (float.TryParse(cmd.Content, out newVoteRatio) && newVoteRatio >= 0f && newVoteRatio <= 1f)
                {
                    matchSettings.VoteRatio = newVoteRatio;
                    SaveMatchSettings();
                    SendToAll(new SettingsChangedMessage(matchSettings));
                    Log("Match vote ratio set to " + newVoteRatio);
                }
                else
                {
                    Log("Usage: setVoteRatio [0.0-1.0]");
                }
            });
            AddCommandHandler("setDisqualificationTime",
            "Sets the time a player needs to loiter around without passing any checkpoints before they are disqualified from a race. If too low, players might get DQ'd just for being slow. 0 disables disqualifying.",
            cmd =>
            {
                int inputInt;
                if (int.TryParse(cmd.Content, out inputInt) && inputInt >= 0)
                {
                    matchSettings.DisqualificationTime = inputInt;
                    SaveMatchSettings();
                    SendToAll(new SettingsChangedMessage(matchSettings));
                    Log("Disqualification time set to " + inputInt);
                }
                else
                {
                    Log("Usage: setDisqualificationTime [>=0]");
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
                    Console.Write("Show this server on one or more server lists? If no, players can still connect by IP (yes|no): ");
                    string input = Console.ReadLine();
                    if (input == "yes")
                    {
						newConfig.ShowOnList = true;
                        inputCorrect = true;
                    }
                    if (input == "no")
                    {
						newConfig.ShowOnList = false;
                        inputCorrect = true;
                    }
                }

				if (newConfig.ShowOnList)
                {
					while (newConfig.ServerListURLs == null) {
						Console.Write("Enter one or more URL(s) for server lists this server should appear on, seperated by space (Leave blank to use the default list, '" + DEFAULT_SERVER_LIST_URL + "'):");
						string input = Console.ReadLine().Trim();

						if (string.IsNullOrEmpty(input))
						{
							newConfig.ServerListURLs = new string[] { DEFAULT_SERVER_LIST_URL };
						}
						else 
						{
							var urls = input.Split(' ');
							newConfig.ServerListURLs = urls;
						}
					}

                    while (string.IsNullOrEmpty(newConfig.PublicIP))
                    {
                        Console.Write("Enter the public IP adress to use when connecting from the server browser (If behind a router, remember to port forward): ");
                        newConfig.PublicIP = Console.ReadLine().Trim();
                    }

                    while (newConfig.PublicPort == 0)
                    {
                        Console.Write("Enter the public port use when connecting from the server browser (Leave blank to use private port - if you are unsure just do this): ");

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
                    Console.WriteLine("Config saved! If you want to modify match settings, this is done using the commands listed when entering 'help'.");
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

            LoadMOTD();

            //Welcome message
            Log("Welcome! Type 'help' for a list of commands. Type 'stop' to shut down the server.");

            running = true;

#if DEBUG
            debugMode = true;
#endif

            NetPeerConfiguration config = new NetPeerConfiguration(Consts.APP_ID);
            config.Port = this.config.PrivatePort;
            config.MaximumConnections = this.config.MaxPlayers;
            config.EnableMessageType(NetIncomingMessageType.ConnectionApproval);
            config.EnableMessageType(NetIncomingMessageType.DiscoveryRequest);

            netServer = new NetServer(config);
            netServer.Start();

            Log("Server started on port " + this.config.PrivatePort + "!");

			if (this.config.ShowOnList)
            {
                AddToServerLists();
                serverListPingTimer.Start();
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
                        Log("Failed to parse server config from " + CONFIG_FILENAME + ", server cannot start. Please fix or delete the file. (" + ex.Message + ")", LogType.Error);
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
                        Log("Failed to parse settings from " + path + ", using default settings instead. (" + ex.Message + ")", LogType.Warning);
                    }
                }
            }
            else
            {
                Log("Match settings at " + path + " not found, using default settings", LogType.Warning);
            }
            return false;
        }

        private void LoadMOTD(string path = MOTD_FILENAME) {
            if (File.Exists(path))
            {
                using (StreamReader sr = new StreamReader(path))
                {
                    motd = sr.ReadToEnd();
                    Log("Loaded message of the day from " + path);
                }
            }
            else
            {
		if (path == MOTD_FILENAME)
		{
                Log("No message of the day found. You can display a message to joining players by creating a file named '" + MOTD_FILENAME + "' next to the server executable.",LogType.Warning);
		}
		else
		{
		Log("Could not load MOTD from this file.",LogType.Error);
		}
            }
        }

        private void AddToServerLists()
        {
            Thread addThread = new Thread(() =>
            {
                foreach (string listURL in config.ServerListURLs) {
                    try
                    {
                        using (var client = new WebClient())
                        {
                            var values = new NameValueCollection();
                            values["ip"] = config.PublicIP;
                            values["port"] = config.PublicPort.ToString();

                            var response = client.UploadValues(listURL + "/add/", values);
                            string responseString = Encoding.Default.GetString(response);

                            Log("Server list at '" + listURL +"' said: " + responseString, LogType.Debug);
                        }
                    }
                    catch (WebException ex)
                    {
                        Log("Failed adding server to server list at '" + listURL + "': " + ex.Message, LogType.Warning);
                    }
                }
            });
            addThread.Start();
        }

        private void MessageLoop()
        {
            while (running)
            {
                Thread.Sleep(1000 / TICKRATE);

                //Check server browser ping timer
                if (serverListPingTimer.IsRunning)
                {
                    if (serverListPingTimer.Elapsed.TotalSeconds >= SERVER_BROWSER_PING_INTERVAL)
                    {
                        AddToServerLists();
                        serverListPingTimer.Reset();
                        serverListPingTimer.Start();
                    }
                }

                //Check lobby timer
                if (lobbyTimer.IsRunning)
                {
                    if (lobbyTimer.Elapsed.TotalSeconds >= LOBBY_MATCH_START_TIME)
                    {
                        Log("The race has been started by all players being ready.",LogType.Debug);
                        LoadRace();
                    }
                }

                //Check stage loading timer
                if (stageLoadingTimeoutTimer.IsRunning)
                {
                    if (stageLoadingTimeoutTimer.Elapsed.TotalSeconds >= STAGE_LOADING_TIMEOUT)
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
                        Log("The race has been automatically started.",LogType.Debug);
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
                    if (matchSettings.DisqualificationTime > 0) {
                        if (!p.TimeoutMessageSent && p.RacingTimeout.Elapsed.TotalSeconds > matchSettings.DisqualificationTime / 2.0f)
                        {
                            SendToAll(new RaceTimeoutMessage(p.ClientGuid, p.CtrlType, matchSettings.DisqualificationTime / 2.0f));
                            p.TimeoutMessageSent = true;
                        }
                        if (p.RacingTimeout.Elapsed.TotalSeconds > matchSettings.DisqualificationTime)
                        {
                            Log("A player was too slow to race and has been disqualified.");
                            FinishRace(p);

                            SendToAll(new DoneRacingMessage(p.ClientGuid, p.CtrlType, 0, true));
                        }
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

                                    state.WriteToMessage(stateMsg);

                                    Log("State message size in bytes: " + stateMsg.LengthBytes, LogType.Debug);

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
                                            Log("No players left in race!",LogType.Debug);
                                            ReturnToLobby();
                                        }

                                        //If there are now less players than AutoStartMinPlayers, stop the auto start timer
                                        if (players.Count < matchSettings.AutoStartMinPlayers && autoStartTimer.IsRunning)
                                        {
                                            Log("Too few players, match auto start timer stopped",LogType.Debug);
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
                                        Log("Unknown client disconnected (Client was most likely not done connecting)",LogType.Debug);
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
                                case MessageType.PlayerMovementMessage:
                                    //Forward the message to all other clients
                                    NetOutgoingMessage outgoingMsg = netServer.CreateMessage();
                                    outgoingMsg.Write(msg.Data);
                                    List<NetConnection> recipients = netServer.Connections.Where(a => a != msg.SenderConnection).ToList();
                                    if (recipients.Count > 0)
                                        netServer.SendMessage(outgoingMsg, recipients, NetDeliveryMethod.Unreliable, 0);
                                    break;

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

                                        if (motd != null) {
                                            Whisper(newClient, "Server's message of the day:");
                                            Whisper(newClient, motd);
                                        }
                                        else {
                                            Whisper(newClient, "Welcome to the server!");
                                        }
                                        Whisper(newClient, GetAllowedTiersText());
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
                                            if (VerifyCharacterTier(castedMsg.InitialCharacter))
                                            {
                                                players.Add(new ServPlayer(castedMsg.ClientGuid, castedMsg.CtrlType, castedMsg.InitialCharacter));
                                                Log("Player " + client.Name + " (" + castedMsg.CtrlType + ") joined", LogType.Debug);
                                                SendToAll(matchMessage);

                                                if (players.Count >= matchSettings.AutoStartMinPlayers && !autoStartTimer.IsRunning && matchSettings.AutoStartTime > 0)
                                                {
                                                    Log("Match will auto start in " + matchSettings.AutoStartTime + " seconds.",LogType.Debug);
                                                    StartAutoStartTimer();
                                                }
                                            }
                                            else
                                            {
                                                Whisper(client, "You cannot join with this character - " + GetAllowedTiersText());
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
                                                Log("Too few players, match auto start timer stopped",LogType.Debug);
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
                                                if (VerifyCharacterTier(castedMsg.NewCharacter))
                                                {
                                                    player.CharacterId = castedMsg.NewCharacter;
                                                    Log("Player " + client.Name + " (" + castedMsg.CtrlType + ") set character to " + castedMsg.NewCharacter, LogType.Debug);
                                                    SendToAll(matchMessage);
                                                }
                                                else
                                                {
                                                    Whisper(client, "You can't use this character - " + GetAllowedTiersText());
                                                    Log("Player " + client.Name + " (" + castedMsg.CtrlType + ") tried to set character to " + castedMsg.NewCharacter + " but the character's tier is not allowed", LogType.Debug);
                                                }
                                            }
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
                                                Log("Waiting for " + clientsLoadingStage + " client(s) to load",LogType.Debug);
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

                                        if (castedMsg.Text.ToLower().Contains("shrek") && VerifyCharacterTier(15))
                                        {
                                            ServClient client = clients.FirstOrDefault(a => a.Connection == msg.SenderConnection);
                                            ServPlayer[] playersFromClient = players.Where(a => a.ClientGuid == client.Guid).ToArray();
                                            foreach (ServPlayer p in playersFromClient)
                                            {
                                                p.CharacterId = 15;
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

                                            int clientsRequiredToReturn = (int)(clients.Count * matchSettings.VoteRatio);

                                            if (clients.Count(a => a.WantsToReturnToLobby) >= clientsRequiredToReturn)
                                            {
                                                Broadcast("Returning to lobby by user vote.");
                                                ReturnToLobby();
                                            }
                                            else
                                            {
                                                int clientsNeeded = clientsRequiredToReturn - clients.Count(a => a.WantsToReturnToLobby);
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
                                                player.RacingTimeout.Reset();
                                                player.RacingTimeout.Start();
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

                backToLobbyTimer.Reset();

                players.ForEach(a =>
                {
                    a.CurrentlyRacing = false;
                    a.RacingTimeout.Reset();
                    a.TimeoutMessageSent = false;
                });
                clients.ForEach(a => a.WantsToReturnToLobby = false);

                bool matchSettingsChanged = false;

                //Stage rotation
                switch (matchSettings.StageRotationMode)
                {
                    case StageRotationMode.Random:
                        Log("Picking random stage", LogType.Debug);
                        int newStage = matchSettings.StageId;

                        while (newStage == matchSettings.StageId)
                            newStage = random.Next(STAGE_COUNT);

                        matchSettings.StageId = newStage;
                        matchSettingsChanged = true;
                        break;

                    case StageRotationMode.Sequenced:
                        Log("Picking next stage", LogType.Debug);
                        int nextStage = matchSettings.StageId + 1;
                        if (nextStage >= STAGE_COUNT) nextStage = 0;
                        matchSettings.StageId = nextStage;
                        matchSettingsChanged = true;
                        break;
                }

                //Tier rotation
                AllowedTiers newTiers = matchSettings.AllowedTiers;
                switch(matchSettings.TierRotationMode)
                {
                    case TierRotationMode.Cycle:
                        switch (matchSettings.AllowedTiers)
                        {
                            case AllowedTiers.NormalOnly:
                                newTiers = AllowedTiers.OddOnly;
                                break;
                            case AllowedTiers.OddOnly:
                                newTiers = AllowedTiers.HyperspeedOnly;
                                break;
                            default:
                                newTiers = AllowedTiers.NormalOnly;
                                break;
                        }
                        break;
                    case TierRotationMode.Random:
                        int rand = random.Next() % 3;
                        switch (rand) {
                            case 0:
                                newTiers = AllowedTiers.NormalOnly;
                                break;
                            case 1:
                                newTiers = AllowedTiers.OddOnly;
                                break;
                            case 2:
                                newTiers = AllowedTiers.HyperspeedOnly;
                                break;
                        }
                        break;
                    case TierRotationMode.WeightedRandom:
                        int[] choices = new[] {0,0,0,0,0,0,0,0,0,0,1,1,1,2};
                        int choice = choices[random.Next() % choices.Length];
                        switch (choice) {
                            case 0:
                                newTiers = AllowedTiers.NormalOnly;
                                break;
                            case 1:
                                newTiers = AllowedTiers.OddOnly;
                                break;
                            case 2:
                                newTiers = AllowedTiers.HyperspeedOnly;
                                break;
                        }
                        break;
                }
                if (newTiers != matchSettings.AllowedTiers) {
                    matchSettings.AllowedTiers = newTiers;
                    matchSettingsChanged = true;
                    CorrectPlayerTiers();
                    Broadcast(GetAllowedTiersText());
                }

                if (matchSettingsChanged)
                {
                    SendToAll(new SettingsChangedMessage(matchSettings));
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
            SendToAll(new RaceTimeoutMessage(p.ClientGuid, p.CtrlType, 0));

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
            Log("Sending message of type " + matchMsg.GetType() + " to " + netServer.Connections.Count + " connection(s)", LogType.Debug);
            if (netServer.ConnectionsCount == 0) return;
            string matchMsgSerialized = JsonConvert.SerializeObject(matchMsg, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });

            NetOutgoingMessage netMsg = netServer.CreateMessage();
            netMsg.Write(MessageType.MatchMessage);
            netMsg.WriteTime(false);
            netMsg.Write(matchMsgSerialized);
            netServer.SendMessage(netMsg, netServer.Connections, NetDeliveryMethod.ReliableOrdered, 0);
        }

        private void SendTo(MatchMessage matchMsg, ServClient reciever) {
            Log("Sending message of type " + matchMsg.GetType() + " to client " + reciever.Name, LogType.Debug);
            string matchMsgSerialized = JsonConvert.SerializeObject(matchMsg, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });

            NetOutgoingMessage netMsg = netServer.CreateMessage();
            netMsg.Write(MessageType.MatchMessage);
            netMsg.WriteTime(false);
            netMsg.Write(matchMsgSerialized);
            netServer.SendMessage(netMsg, reciever.Connection, NetDeliveryMethod.ReliableOrdered, 0);
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

        private void Whisper(ServClient reciever, string text) {
            Log("Sending whisper to client " + reciever.Name + "(Text: " + text + ")", LogType.Debug);
            SendTo(new ChatMessage("Server", ChatMessageType.System, text), reciever);
        }

        /// <summary>
        /// Writes a message to the server log.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="type"></param>
        public void Log(object message, LogType type = LogType.Normal)
        {
            lock (log)
            {
                if (!debugMode && type == LogType.Debug)
                    return;
                LogEntry entry = new LogEntry(DateTime.Now, "[" + DateTime.Now.ToString("HH:mm:ss") + "] " + message.ToString(), type);
                OnLog?.Invoke(this, new LogArgs(entry));
                log.Add(entry);
            }
        }

        private List<ServClient> SearchClients(string name)
        {
            return clients.Where(a => a.Name.Contains(name)).ToList();
        }

        public void AddCommandHandler(string commandName, string help, CommandHandler handler)
        {
            commandHandlers.Add(commandName, handler);
            commandHelp.Add(commandName, help);
        }

        public void Kick(ServClient client, string reason)
        {
            client.Connection.Disconnect(reason);
        }

        private void CorrectPlayerTiers() {
            foreach(ServPlayer player in players)
            {
                if (!VerifyCharacterTier(player.CharacterId))
                {
                    ServClient client = clients.FirstOrDefault(a => a.Guid == player.ClientGuid);
                    if (client != null)
                    {
                        for (int i=0; i < characterTiers.Length; i++)
                        {
                            if (VerifyCharacterTier(i))
                            {
                                player.CharacterId = i;
                                SendToAll(new CharacterChangedMessage(player.ClientGuid, player.CtrlType, i));
                                Whisper(client, "Your character is not allowed and has been automatically changed.");
                                break;
                            }
                        }
                    }
                }
            }
        }

        private bool VerifyCharacterTier(int id) {
            CharacterTier t = characterTiers[id];
            switch (matchSettings.AllowedTiers) {
                case AllowedTiers.All:
                    return true;
                case AllowedTiers.NormalOnly:
                    return t == CharacterTier.Normal;
                case AllowedTiers.OddOnly:
                    return t == CharacterTier.Odd;
                case AllowedTiers.HyperspeedOnly:
                    return t == CharacterTier.Hyperspeed;
                case AllowedTiers.NoHyperspeed:
                    return t != CharacterTier.Hyperspeed;
                default:
                    return true;
            }
        }

        private string GetAllowedTiersText() {
            switch(matchSettings.AllowedTiers){
                case AllowedTiers.NormalOnly:
                    return "Only characters from the Normal tier are allowed.";
                case AllowedTiers.OddOnly:
                    return "Only characters from the Odd tier are allowed.";
                case AllowedTiers.HyperspeedOnly:
                    return "Only characters from the Hyperspeed tier are allowed.";
                case AllowedTiers.NoHyperspeed:
                    return "Any character NOT from the Hyperspeed tier is allowed.";
                default:
                    return "All characters are allowed.";
            }
        }
        #endregion Utility methods

        private void SaveMatchSettings() {
            using (StreamWriter sw = new StreamWriter(SETTINGS_FILENAME))
            {
                sw.Write(JsonConvert.SerializeObject(matchSettings));
            }
        }

        public void Dispose()
        {
            Log("Saving match settings...");
            SaveMatchSettings();

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
