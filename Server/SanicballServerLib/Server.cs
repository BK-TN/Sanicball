using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Lidgren.Network;
using Newtonsoft.Json;
using Sanicball.Data;
using Sanicball.Net;

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
        private NetServer netServer;
        private bool running;
        private MatchSettings matchSettings;
        private CommandQueue commandQueue;

        public bool Running { get { return running; } }

        public Server(CommandQueue commandQueue)
        {
            this.commandQueue = commandQueue;
        }

        public void Start(int port)
        {
            bool defaultSettings = true;
            if (File.Exists(SETTINGS_FILENAME))
            {
                Log("Loading match settings");
                using (StreamReader sr = new StreamReader(SETTINGS_FILENAME))
                {
                    try
                    {
                        matchSettings = JsonConvert.DeserializeObject<MatchSettings>(sr.ReadToEnd());
                        defaultSettings = false;
                    }
                    catch (JsonException ex)
                    {
                        Log("Failed to load " + SETTINGS_FILENAME + ": " + ex.Message);
                    }
                }
            }
            if (defaultSettings)
                matchSettings = new MatchSettings();

            running = true;
            NetPeerConfiguration config = new NetPeerConfiguration(Sanicball.Match.OnlineMatchMessenger.APP_ID);
            config.Port = 25000;
            config.EnableMessageType(NetIncomingMessageType.ConnectionApproval);

            netServer = new NetServer(config);
            netServer.Start();

            Log("Server started on port " + port + "!");

            //Thread messageThread = new Thread(MessageLoop);
            MessageLoop();
        }

        private void MessageLoop()
        {
            while (running)
            {
                Thread.Sleep(1000 / TICKRATE);

                //Check command queue
                Command cmd;
                while ((cmd = commandQueue.ReadNext()) != null)
                {
                    Log("Entered command: " + cmd.Name + ", " + cmd.ArgCount + " arguments", LogType.Debug);

                    if (cmd.Name == "stop")
                    {
                        running = false;
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

                        case NetIncomingMessageType.StatusChanged:
                            byte status = msg.ReadByte();
                            string statusMsg = msg.ReadString();
                            Log("Status change recieved: " + (NetConnectionStatus)status + " - Message: " + statusMsg, LogType.Debug);
                            break;

                        case NetIncomingMessageType.ConnectionApproval:
                            string text = msg.ReadString();
                            if (text.Contains("please"))
                            {
                                //Approve for being nice
                                NetOutgoingMessage hailMsg = netServer.CreateMessage();
                                hailMsg.Write("Thank you!");
                                msg.SenderConnection.Approve(hailMsg);
                            }
                            else
                            {
                                msg.SenderConnection.Deny();
                            }
                            break;

                        case NetIncomingMessageType.Data:
                            byte messageType = msg.ReadByte();
                            switch (messageType)
                            {
                                case MessageType.MatchMessage:

                                    //Send the exact same message back again
                                    NetOutgoingMessage testMsg = netServer.CreateMessage();
                                    testMsg.Write(MessageType.MatchMessage);
                                    testMsg.Write(msg.ReadString());
                                    netServer.SendMessage(testMsg, msg.SenderConnection, NetDeliveryMethod.ReliableOrdered);
                                    Log("Forwarded message", LogType.Debug);
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

        public void Dispose()
        {
            Log("Saving match settings");
            using (StreamWriter sw = new StreamWriter(SETTINGS_FILENAME))
            {
                sw.Write(JsonConvert.SerializeObject(matchSettings));
            }
            netServer.Shutdown("Server was closed.");
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
    }
}