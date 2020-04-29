using System;
using System.Collections.Generic;
using System.Text;

namespace Bot
{
    public struct Command
    {
        public string commandString;
        public string[] Args;
        public OperationToPerform Operation;
    }
}
