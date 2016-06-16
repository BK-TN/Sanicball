using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SanicballServerLib;

namespace SanicballServer
{
    internal class Program
    {
        private static CommandQueue commandQueue = new CommandQueue();

        private static void Main(string[] args)
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

                serv.Start(25000);

                Console.WriteLine("Press any key to close this window.");
                inputThread.Join();
            }
        }

        private static void InputLoop()
        {
            string input;
            while (true)
            {
                input = Console.ReadLine();
                commandQueue.Add(new Command(input.ToString()));
            }
        }
    }
}