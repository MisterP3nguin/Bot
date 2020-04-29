
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Timers;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Newtonsoft.Json;
using System.Threading;
using HtmlAgilityPack;

namespace Bot
{
    public class Program
    {
        public DiscordClient Client { get; set; }
        public static JsonConfig JsonConfig { get; set; }
        private static List<System.Timers.Timer> TimedFunctions { get; set; }

        public static void Main()
        {
            //Must pass to async method because running the discord Client is an async task.
            var prog = new Program();
            prog.RunBotAsync().GetAwaiter().GetResult();
        }
        public async Task RunBotAsync()
        {
            //Read config
            string json = "";
            using (FileStream fs = File.OpenRead("config.json"))
            using (StreamReader sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = await sr.ReadToEndAsync();

            JsonConfig jsonConfig = JsonConvert.DeserializeObject<JsonConfig>(json);
            Program.JsonConfig = jsonConfig;
            DiscordConfiguration Config = new DiscordConfiguration
            {
                Token = jsonConfig.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                LogLevel = LogLevel.Debug,
                UseInternalLogHandler = true
            };
            
            TimedFunctions = new List<System.Timers.Timer>();
            
            //Create Client.
            this.Client = new DiscordClient(Config);

            //Assign callback methods.
            this.Client.Ready += this.OnReady;
            this.Client.GuildAvailable += this.OnGuildAvailable;
            this.Client.ClientErrored += this.OnClientError;
            this.Client.MessageCreated += this.OnMessageCreated;

            //Link callback functions to commands in dictionary.
            CommandManager.Init();

            //Connect and do stuff because functions dont do what they say they do anymore.
            await this.Client.ConnectAsync();

            //This timer runs the Elapsed event every 24 hours (argument is in miliseconds).
            SetupTimerForNewsUpdate(new TimeSpan(9,0,0));

            //Seperate for condole debugging.
            Thread consoleHandler= new Thread(ReadConsoleInput);
            consoleHandler.Start();

            //I assume a thread is created prior to this abysmal line of code. This pains me in ways i cant express.
            await Task.Delay(-1);
            //I dont know if this is even ever executed. IF it is, great. If it isnt, who cares, the threads should exit automatically right...
            consoleHandler.Abort();
        }
        private Task OnReady(ReadyEventArgs e)
        {
            e.Client.DebugLogger.LogMessage(LogLevel.Info, "ExampleBot", "Client is ready to process events.", DateTime.Now);
            return Task.CompletedTask;
        }
        private Task OnGuildAvailable(GuildCreateEventArgs e)
        {
            e.Client.DebugLogger.LogMessage(LogLevel.Info, "ExampleBot", $"Guild available: {e.Guild.Name}", DateTime.Now);

            return Task.CompletedTask;
        }
        private Task OnClientError(ClientErrorEventArgs e)
        {
            e.Client.DebugLogger.LogMessage(LogLevel.Error, "ExampleBot", $"Exception occured: {e.Exception.GetType()}: {e.Exception.Message}", DateTime.Now);

            return Task.CompletedTask;
        }
        private Task OnMessageCreated(MessageCreateEventArgs e)
        {
            //Check if the sender is the bot, then check if the message starts with the command prefix.
            if (e.Message.Author != this.Client.CurrentUser
                && e.Message.Content.Substring(0,Program.JsonConfig.CommandPrefix.Length) == Program.JsonConfig.CommandPrefix)
            {
                Command command = CommandManager.GenerateCommandFromMessage(e.Message.Content);
                CommandManager.RunCommand(command,e);
            }    
            return Task.CompletedTask;
        }
        private void ReadConsoleInput ()
        {
            bool isRunning = true;
            while (isRunning)
            {
                string commandString = Program.JsonConfig.CommandPrefix + Console.ReadLine();
                Command command = CommandManager.GenerateCommandFromMessage(commandString);
                switch (command.commandString)
                {
                    case "help":
                        Console.WriteLine("hello");
                        break;
                    case "news":
                        GetNews();
                        break;
                    case "sendnews":
                        UpdateNewsToDiscordChannel(null, null);
                        break;
                    case "q":
                        isRunning = false;
                        break;
                    default:
                        Console.WriteLine("That is not a recognised command");
                        break;
                }                
            }
        }
        private void UpdateNewsToDiscordChannel(Object source, ElapsedEventArgs e)
        {
            string news = GetNews();            
            Task<DiscordChannel> getChanneltask =  Client.GetChannelAsync(699586797159841882);
            getChanneltask.Wait();
            DiscordChannel newsChannel = getChanneltask.Result;            
            newsChannel.SendMessageAsync(news).Wait();
        }
        private void SetupTimerForNewsUpdate(TimeSpan alertTime)
        {
            DateTime current = DateTime.Now;
            TimeSpan timeToGo = alertTime - current.TimeOfDay;
            System.Timers.Timer timer = null;
            //Set the timer to create the timer for the news update.
            if (timeToGo.TotalMilliseconds >= 0)    
                timer = new System.Timers.Timer(timeToGo.TotalMilliseconds);
            else //86400000 is 24 hours in miliseconds.
                timer = new System.Timers.Timer(86400000 - timeToGo.TotalMilliseconds);
            
            timer.Elapsed += CreateTimer;
            timer.Enabled = true;
        }
        private void CreateTimer (Object source, ElapsedEventArgs e)
        {
            System.Timers.Timer timer = new System.Timers.Timer(86400000);
            timer.Elapsed += UpdateNewsToDiscordChannel;
            timer.AutoReset = true;
            timer.Enabled = true;
            TimedFunctions.Add(timer);
        }
        private string GetNews ()
        {
            string news = "Corona virus news as of " + 
                DateTime.Now.Day + "/" +
                DateTime.Now.Month + "/" +
                DateTime.Now.Year + Environment.NewLine;
            //---Ireland Corona news.
            string url = "https://www.gov.ie/en/news/7e0924-latest-updates-on-covid-19-coronavirus/";
            var web = new HtmlAgilityPack.HtmlWeb();
            HtmlDocument doc = web.Load(url);
            HtmlNode govInfo = doc.DocumentNode.SelectSingleNode("/html/body/div[8]/div/div/div/div[1]/div/div/p[1]");
            if (govInfo != null)
            {
                string coronadeaths = govInfo.InnerText.Trim();
                news += ("Ireland : " + coronadeaths + Environment.NewLine);
            }
            //---England Corona news.
            url = "https://www.gov.uk/guidance/coronavirus-covid-19-information-for-the-public";
            web = new HtmlWeb();
            doc = web.Load(url);
            HtmlNode tests = doc.DocumentNode.SelectSingleNode("//*[@id=\"contents\"]/div[2]/div/table[1]/tbody/tr[1]/td[2]");
            HtmlNode peopleTested = doc.DocumentNode.SelectSingleNode("//*[@id=\"contents\"]/div[2]/div/table[1]/tbody/tr[1]/td[3]");
            HtmlNode positive = doc.DocumentNode.SelectSingleNode("//*[@id=\"contents\"]/div[2]/div/table[1]/tbody/tr[1]/td[4]");
            HtmlNode deaths = doc.DocumentNode.SelectSingleNode("//*[@id=\"contents\"]/div[2]/div/table[1]/tbody/tr[1]/td[5]");
            if (tests != null &&
                peopleTested != null &&
                positive != null &&
                deaths != null)
            {
                news += ("England : Daily tests | People tested | Positive | Deaths = " 
                    + tests.InnerText + " | " 
                    + peopleTested.InnerText + " | " 
                    + positive.InnerText + " | " 
                    + deaths.InnerText + Environment.NewLine); 
            }

            Console.Write(news);
            return news;
        }
    }
    public struct JsonConfig
    {
        [JsonProperty("token")]
        public string Token { get; private set; }

        [JsonProperty("prefix")]
        public string CommandPrefix { get; private set; }
    }
}