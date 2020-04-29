using System;
using System.Collections.Generic;
using System.Text;
using DSharpPlus.EventArgs;
using DSharpPlus;

namespace Bot
{
    public delegate void OperationToPerform(MessageCreateEventArgs e, Command command);
}
