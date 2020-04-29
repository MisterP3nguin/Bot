using System;
using System.Collections.Generic;
using System.Text;
using DSharpPlus.EventArgs;
using DSharpPlus;

namespace Bot
{
    public class CommandManager
    {
        private static Dictionary<string, OperationToPerform> CallbackFunctions { get; set; }
        public static void Init()
        {
            CallbackFunctions = new Dictionary<string, OperationToPerform>
            {
                { "ping", Ping },
                { "help", Help },
                { "judge", Judge },
                { "based?", Based }
            };
        }
        public static Command GenerateCommandFromMessage(string Raw)
        {
            //Remove the command prefix and get the first word from Raw and store it in commandString.
            string commandString;
            Raw = Raw.Remove(0, 1);
            string[] words = Raw.Split(" ".ToCharArray());
            commandString = words[0];
                     
            //build an array of arguments if arguments exist. Subcommands are handled by the given function.
            string[] argsString = new string[words.Length - 1];
            
            if (argsString.Length > 0)
            {
                for (int i = 0; i < argsString.Length; i++)
                {
                    argsString[i] = words[i + 1];
                    Console.WriteLine(argsString[i]);
                }
            }
            else
                argsString = null;
            
            Command Command = new Command
            {
                commandString = commandString,
                Args = argsString              
            };

            CallbackFunctions.TryGetValue(commandString,out Command.Operation);
            if (Command.Operation == null)            
                Command.Operation += PrintError;
            
            return Command;
        }        
        public static void RunCommand(Command command,MessageCreateEventArgs e)
        {
            command.Operation.Invoke(e,command);

            Logger.Log("User "+ e.Author.Username + "#" + e.Author.Discriminator + " ran the command "+ command.commandString);
        }
        public static void PrintError (MessageCreateEventArgs e, Command command)
        {
            e.Channel.SendMessageAsync("That is not a recognised command.");
        }
        static void Ping(MessageCreateEventArgs e, Command command)
        {
            e.Channel.SendMessageAsync("!Pong");
        }
        static void Help(MessageCreateEventArgs e, Command command)
        {
            if (command.Args != null)
            {
                switch(command.Args[0])
                {
                    case "ping":
                        e.Channel.SendMessageAsync("This command spits out \"Pong!\"");
                        break;
                    case "help":
                        e.Channel.SendMessageAsync("Are you stupid?");
                        break;
                    case "based?":
                        e.Channel.SendMessageAsync("Checks the greatness of contnet.");
                        break;
                    case "judge":
                        e.Channel.SendMessageAsync("Art thou worthy?");
                        break;
                    default: 
                        e.Channel.SendMessageAsync("The specified sub-command does not exist");
                        break;
                }
            }
            else
                e.Channel.SendMessageAsync("Commands :" + Environment.NewLine +
                    "!ping" + Environment.NewLine +
                    "!based?" + Environment.NewLine +
                    "!judge");           

        }
        static void Judge(MessageCreateEventArgs e,Command command)
        {
            int num = new Random().Next(0, 2);
            if (num == 1)
                e.Channel.SendMessageAsync("Yay");
            else
                e.Channel.SendMessageAsync("Nay");
        }
        static void Based(MessageCreateEventArgs e, Command command)
        {
            //e.Channel.SendMessageAsync("Hello, this is pretty based!");
            int num = new Random().Next(0, 4);
            if (num == 0)
                e.Channel.SendMessageAsync("Cringe.");
            else if(num == 1)
                e.Channel.SendMessageAsync("This is awful.");
            else if (num == 2)
                e.Channel.SendMessageAsync("Totally based bro.");
            else if (num == 3)
                e.Channel.SendMessageAsync("Get this shit off my screen.");
        }
    }
}
