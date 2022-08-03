using ConsoleApp1.Commands;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Threading;
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
            //Channels for the logger to ignore
            List<ulong> channelsToIgnore = new List<ulong>();
            channelsToIgnore.Add(448739263580864522);   //General Announcements
            channelsToIgnore.Add(448739662438072330);   //T1 Attendance Form
            channelsToIgnore.Add(862977325640581120);   //T1 Verdicts
            channelsToIgnore.Add(595178511908732929);   //T2 Attendance Form
            channelsToIgnore.Add(862977561834422292);   //T2 Verdicts
            channelsToIgnore.Add(861893058152103966);   //T3 Attendnace Form
            channelsToIgnore.Add(862978452335099945);   //T3 Verdicts
            channelsToIgnore.Add(632100047508471809);   //Friday Attendnace Form
            channelsToIgnore.Add(871696178339786792);   //Friday Verdicts
            channelsToIgnore.Add(815540017974738986);   //iRacing Attendance Form
            channelsToIgnore.Add(637979193212141569);   //Hall of Fame
            channelsToIgnore.Add(604627899542274083);   //T1 Standings
            channelsToIgnore.Add(604627940503846913);   //T2 Standings
            channelsToIgnore.Add(873762279575932988);   //T3 Standings
            channelsToIgnore.Add(597768404002275338);   //Combined Constructors
            channelsToIgnore.Add(627379648665223168);   //Friday Series Standings
            channelsToIgnore.Add(755624038772113459);   //License Points
            channelsToIgnore.Add(682156056301797397);   //Race Incidents Archive
            channelsToIgnore.Add(874271605864419359);   //Friday Voting
            channelsToIgnore.Add(749583731701841991);   //T1 Voting
            channelsToIgnore.Add(874271486892974121);   //T2 Voting
            channelsToIgnore.Add(874271555415322685);   //T3 Voting

            //Log when user deletes a message
            Client.MessageDeleted += async (s, e) =>
            {
                var author = e.Message.Author;

                //Only log messages not in list
                if (!channelsToIgnore.Contains(e.Message.ChannelId))
                {
                    var message = author.Username.ToString() + " Deleted this message: " + e.Message.Content + "\nFrom this channel: " + e.Message.Channel.Name;
                    DiscordChannel channel = e.Guild.GetChannel(904511634834341938);
                    await Client.SendMessageAsync(channel, message).ConfigureAwait(false);
                }

            };

            //Log when user modifies a message
            Client.MessageUpdated += async (s, e) =>
            {
                var author = e.Message.Author;
                
                //Only log messages not in list
                if (!channelsToIgnore.Contains(e.Message.ChannelId))
                {
                    //Only log messages that have actually changed
                    if (e.Message.Content != e.MessageBefore.Content)
                    {
                        var oldMessage = "Original Post by " + author.Username.ToString() + " At " + e.MessageBefore.Timestamp + ": \n" + e.MessageBefore.Content;
                        var message = "Edited Post by " + author.Username.ToString() + " At " + e.Message.Timestamp + ": \n" + e.Message.Content + "\nIn this channel: " + e.Message.Channel.Name;
                        DiscordChannel channel = e.Guild.GetChannel(904511634834341938);
                        await Client.SendMessageAsync(channel, oldMessage).ConfigureAwait(false);
                        await Client.SendMessageAsync(channel, message).ConfigureAwait(false);
                    }
                }
            };

            //Update reactions for Attendnace V2
            Client.MessageReactionAdded += async (s, e) =>
            {
                //Get Values
                string reaction = "'" + e.Emoji + "'";
                ulong driverID = e.User.Id;
                string driver = e.User.Username;
                ulong channel = e.Channel.Id;
                string tier;

                //Get correct Tier from channel where the reaction is used
                switch (channel)
                {
                    case 448739662438072330:
                        tier = "T1";
                        break;
                    case 595178511908732929:
                        tier = "T2";
                        break;
                    case 861893058152103966:
                        tier = "T3";
                        break;
                    default:
                        tier = null;
                        break;
                }

                //If Tier is still blank then the reaction isn't for attendance and we can end it here
                if (tier == null)
                {
                    return;
                }

                //Build string and call SP
                StringBuilder strBuilder = new StringBuilder();
                strBuilder.Append("EXEC " + "UpdateAttendance " + "@reaction = " + reaction + ", @driverID = " + driverID + ", @driver = " + "'" + driver + "'" + ", @tier = " + tier);
                string sqlQuery = strBuilder.ToString();
                using (SqlCommand command = new SqlCommand(sqlQuery, SQLConnection.Connection()))
                {
                    command.ExecuteNonQuery();
                    using (SqlDataReader dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            string result = dr[0].ToString();
                            var Message = await e.Channel.SendMessageAsync(result.ToString()).ConfigureAwait(false);
                            Thread.Sleep(5000);
                            await e.Channel.DeleteMessageAsync(Message).ConfigureAwait(false);
                        }
                    }
                }
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
