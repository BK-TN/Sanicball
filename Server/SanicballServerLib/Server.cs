using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using Sanicball.Data;

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
        private const string APP_ID = "Sanicball";

        public event EventHandler<LogArgs> OnLog;

        private List<LogEntry> log = new List<LogEntry>();
        private NetServer netServer;
        private bool running;
        private MatchSettings matchSettings;

        public Server()
        {
        }

        public void Start(int port)
        {
            running = true;
            NetPeerConfiguration config = new NetPeerConfiguration(APP_ID);
            config.Port = 25000;
            config.EnableMessageType(NetIncomingMessageType.ConnectionApproval);

            netServer = new NetServer(config);
            netServer.Start();

            Log("Server started on port " + port + "!");
            MessageLoop();
        }

        private void MessageLoop()
        {
            while (running)
            {
                netServer.MessageReceivedEvent.WaitOne();

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
                                msg.SenderConnection.Approve();
                            }
                            else
                            {
                                msg.SenderConnection.Deny();
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
            netServer.Shutdown("Server was closed.");
        }

        private void Log(object message, LogType type = LogType.Normal)
        {
            LogEntry entry = new LogEntry(DateTime.Now, message.ToString(), type);
            OnLog?.Invoke(this, new LogArgs(entry));
            log.Add(entry);
        }
    }
}