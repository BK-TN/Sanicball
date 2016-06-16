using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SanicballServerLib;

namespace SanicballServer
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            using (Server serv = new Server())
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
                };

                serv.Start(25000);
            }
            Console.Write("Press any key to close this window.");
            Console.ReadLine();
        }
    }
}