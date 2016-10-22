using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SanicballCore.Server;

namespace SanicballServer
{
    internal class Program
    {
        private static CommandQueue commandQueue = new CommandQueue();

        private static void Main(string[] args)
        {
            bool serverClosed = false;
            while (!serverClosed)
            {
                using (Server serv = new Server(commandQueue))
                {
                    serv.OnLog += (sender, e) =>
                    {
                        switch (e.Entry.Type)
                        {
                            case LogType.Normal:
                                Console.ForegroundColor = ConsoleColor.White;
                                break;

                            case LogType.Debug:
                                Console.ForegroundColor = ConsoleColor.Gray;
                                break;

                            case LogType.Warning:
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                break;

                            case LogType.Error:
                                Console.ForegroundColor = ConsoleColor.Red;
                                break;
                        }
                        Console.WriteLine(e.Entry.Message);

                        //Reset console color to not mess with the color of input text
                        Console.ForegroundColor = ConsoleColor.White;
                    };

                    Thread inputThread = new Thread(InputLoop);
                    inputThread.Start();

#if DEBUG
                    serv.Start();
                    serverClosed = true;
#else
                    try
                    {
                        serv.Start();

                        //Wait until server closes

                        serverClosed = true;
                    }
                    catch (Exception ex)
                    {
                        serv.Log("Server encountered an exception and will restart.", LogType.Error);
                        string exText = ex.GetType() + ": " + ex.Message + Environment.NewLine + ex.StackTrace;
                        serv.Log(exText, LogType.Normal);
                        Thread.Sleep(1000);
                    }
#endif

                    inputThread.Abort();
                    inputThread.Join();
                }
            }

            Console.WriteLine("Press any key to close this window.");
            Console.ReadKey(true);
        }

        private static void InputLoop()
        {
            string input;
            while (true)
            {
                input = Console.ReadLine();
                if (!string.IsNullOrEmpty(input))
                {
                    commandQueue.Add(new Command(input.ToString()));
                }
            }
        }
    }
}