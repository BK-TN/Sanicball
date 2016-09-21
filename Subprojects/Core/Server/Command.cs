using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SanicballCore.Server
{
    public delegate void CommandHandler(Command cmd);

    public class Command
    {
        public string Name { get; }
        public string Content { get; }

        public Command(string text)
        {
            Name = "";
            Content = "";

            text = text.Trim();

            int split = text.IndexOf(' ');
            if (split > -1)
            {
                Name = text.Substring(0, split);
                if (text.Length > split + 1)
                {
                    Content = text.Substring(split + 1, text.Length - (split + 1));
                }
            }
            else
            {
                Name = text;
            }
        }
    }
}