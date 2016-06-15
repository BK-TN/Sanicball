using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;

namespace SanicballServerLib
{
    public class LogArgs : EventArgs
    {
        public string Message { get; }

        public LogArgs(string message)
        {
            Message = message;
        }
    }

    public struct LogEntry
    {
        private DateTime Timestamp { get; }
        private string Message { get; }

        public LogEntry(DateTime timestamp, string message)
        {
            Timestamp = timestamp;
            Message = message;
        }
    }

    public class Server : IDisposable
    {
        private const string APP_ID = "Sanicball";

        public event EventHandler<LogArgs> OnLog;

        private List<LogEntry> log = new List<LogEntry>();
        private NetServer netServer;
        private bool running;

        public Server()
        {
        }

        public void Start(int port)
        {
            running = true;
            netServer = new NetServer(new NetPeerConfiguration(APP_ID)
            { Port = 25000 });
            netServer.Start();

            Log("Server started on port " + port + "!");
            MessageLoop();
        }

        private void MessageLoop()
        {
            while (running)
            {
                netServer.MessageReceivedEvent.WaitOne();

                var msg = netServer.ReadMessage();
                switch (msg.MessageType)
                {
                    case NetIncomingMessageType.StatusChanged:
                        byte status = msg.ReadByte();
                        string statusMsg = msg.ReadString();
                        Log("Status changed to " + (NetConnectionStatus)status + " - Message: " + statusMsg);
                        break;

                    default:
                        Log("Recieved unhandled message of type " + msg.MessageType);
                        break;
                }
            }
        }

        public void Dispose()
        {
            netServer.Shutdown("Server was closed.");
            Log("Goodbye world!");
        }

        private void Log(object message)
        {
            string stringMessage = message.ToString();
            OnLog?.Invoke(this, new LogArgs(stringMessage));
            log.Add(new LogEntry(DateTime.Now, stringMessage));
        }
    }
}