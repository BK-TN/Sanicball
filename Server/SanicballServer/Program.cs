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
                    Console.WriteLine(e.Message);
                };

                serv.Start(25000);
            }
            Console.Write("Press any key to close.");
            Console.ReadLine();
        }
    }
}