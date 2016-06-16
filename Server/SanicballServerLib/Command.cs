using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SanicballServerLib
{
    public class Command
    {
        private string[] args;

        public string Name { get; }
        public int ArgCount { get { return args.Length; } }

        public Command(string text)
        {
            text = text.Trim();
            string[] parts = text.Split(' ');
            if (parts.Length > 0)
            {
                Name = parts[0];

                args = new string[parts.Length - 1];
                for (int i = 0; i < args.Length; i++)
                {
                    args[i] = parts[i + 1];
                }
            }
            else
            {
                Name = "";
                parts = new string[0];
            }
        }

        public string GetArg(int pos)
        {
            return args[pos];
        }
    }
}