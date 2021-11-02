using ConsoleApp1.Commands;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public class Bot
    {  
        public DiscordClient Client { get; private set; }
        public CommandsNextExtension Commands { get; private set; }
        public InteractivityExtension Interactivity { get; private set; }

        //Run Async task
        public async Task RunAsync()
        {
            //Convert JSON to readible object
            var json = string.Empty;
            using (var fs = File.OpenRead("config.json"))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = await sr.ReadToEndAsync().ConfigureAwait(false);

            var configJson = JsonConvert.DeserializeObject<ConfigJson>(json);

            var config = new DiscordConfiguration
            {
                Token = configJson.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                MinimumLogLevel = Microsoft.Extensions.Logging.LogLevel.Debug,
                Intents = DiscordIntents.All
                
            };
            
            Client = new DiscordClient(config);

            Client.Ready += OnClientReady;

            //Logger
            //Log when user deletes a message
            Client.MessageDeleted += async (s, e) =>
            {
                var author = e.Message.Author;
                var message = author.Username.ToString() + " Deleted this message: " + e.Message.Content;
                DiscordChannel channel = e.Guild.GetChannel(904511634834341938);
                await Client.SendMessageAsync(channel, message).ConfigureAwait(false);
            };

            //Log when user modifies a message
            Client.MessageUpdated += async (s, e) =>
            {
                var author = e.Message.Author;
                var oldMessage =  "Original Post by " + author.Username.ToString() + " At " + e.MessageBefore.Timestamp + ": \n" + e.MessageBefore.Content;
                var message = "Edited Post by " + author.Username.ToString() + " At " + e.Message.Timestamp + ": \n" + e.Message.Content;
                DiscordChannel channel = e.Guild.GetChannel(904511634834341938);
                await Client.SendMessageAsync(channel, oldMessage).ConfigureAwait(false);
                await Client.SendMessageAsync(channel, message).ConfigureAwait(false);
            };

            //Interactivity config
            Client.UseInteractivity(new InteractivityConfiguration
            {
                Timeout = TimeSpan.FromDays(8)
            }); ;

            var commandsConfig = new CommandsNextConfiguration
            {
                StringPrefixes = new string[] { configJson.Prefix },
                EnableMentionPrefix = true,
                EnableDms = true
            };

            //Commands
            Commands = Client.UseCommandsNext(commandsConfig);

            Commands.RegisterCommands<General>();
            Commands.RegisterCommands<EasterEggs>();
            Commands.RegisterCommands<Moderator>();

            await Client.ConnectAsync();
            await Task.Delay(-1);

        }
        
        //Command returns
        private Task OnClientReady(object sender, ReadyEventArgs e)
        {
            return Task.CompletedTask;
        }
    }
}
