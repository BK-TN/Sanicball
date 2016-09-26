using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SanicballCore.Server
{
    /// <summary>
    /// Thread-safe command queue
    /// </summary>
    public class CommandQueue
    {
        private List<Command> commands = new List<Command>();

        public void Add(Command command)
        {
            lock (commands)
            {
                commands.Add(command);
            }
        }

        public Command ReadNext()
        {
            lock (commands)
            {
                if (commands.Count > 0)
                {
                    Command cmd = commands[0];
                    commands.RemoveAt(0);
                    return cmd;
                }
                else
                {
                    return null;
                }
            }
        }
    }
}