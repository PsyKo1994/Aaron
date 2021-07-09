using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.Build.Tasks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ConsoleApp1.Commands
{
    public class General : BaseCommandModule
    {
        //Command to test ping
        [Command("Band")]
        public async Task Ping(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync("Band is a dirty driver").ConfigureAwait(false);
        }

        //Command to respond to thank you
        [Command("Good_bot")]
        public async Task PingReply(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync("Woof Woof").ConfigureAwait(false);
        }

        //Add Race
        [Command("Add")]
        [Description("Add a race")]
        [RequireRoles(RoleCheckMode.Any, "Moderator", "Admins")]
        public async Task Add(CommandContext ctx,
            [Description("Add a tier")] int tier
            , [Description("Add a round")] int round
            , [Description("Add a season")] int season
            , [Description("Add a date")] string raceDate
            , [Description("Add a track")] string track)
        {
            await ctx.Channel.SendMessageAsync("Tier" + tier + " Attendance Form for Round " + round + " Season " + season + " | " + raceDate + " | Track: " + track + " 7pm Melbourne / 9pm NZ").ConfigureAwait(false);
        }

        //Message Response
        [Command("Response")]
        public async Task Response(CommandContext ctx)
        {
            var interactivity = ctx.Client.GetInteractivity();

            var message = await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel);
            await ctx.Channel.SendMessageAsync(message.Result.Content).ConfigureAwait(false);
        }

        //Emoji Response
        [Command("ResponseReaction")]
        public async Task ResponseReaction(CommandContext ctx)
        {
            var interactivity = ctx.Client.GetInteractivity();

            var message = await interactivity.WaitForReactionAsync(x => x.Channel == ctx.Channel);
            await ctx.Channel.SendMessageAsync(message.Result.Emoji).ConfigureAwait(false);
        }

        //Race
        [Command("Race")]
        [Description("Set up race")]
        [RequireRoles(RoleCheckMode.Any, "Moderator", "Admins")]
        public async Task Race(CommandContext ctx, [Description("Add all race info")] string raceInfo)
        {
            //Edit last message sent and add emoji's
            var Message = await ctx.Channel.SendMessageAsync(raceInfo).ConfigureAwait(false);
            await Message.ModifyAsync(msg => msg.Content = "test [edited]");
            await Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":attendanceyes:"));
            await Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":attendancenope:"));
            await Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":attendancemaybe:"));
            await Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":regional_indicator_r:"));
            await Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":speaker:"));
            await Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":teamRacingPoint:"));

        }

        //Poll
        [Command("pollTest")]
        [RequireRoles(RoleCheckMode.Any, "Moderator", "Admins")]
        public async Task PollTest(CommandContext ctx, TimeSpan duration, string pollID)//, params DiscordEmoji[] emojiOptions)
        {
            var interactivity = ctx.Client.GetInteractivity();

            var embed = new DiscordEmbedBuilder
            {
                Title = "PollTest"//,
                //Description = string.Join("", options)
            };

            var pollMessage = await ctx.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);

            string[] emojiOptions = new string[]
            {
                ":attendanceyes:"
                , ":attendancenope:"
                , ":attendancemaybe:"
                , ":regional_indicator_r:"
                , ":speaker:"
            };

            for (int i = 0; i < emojiOptions.Length; i++)
            {
                await pollMessage.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, emojiOptions[i])).ConfigureAwait(false);
            }

            var result = await interactivity.CollectReactionsAsync(pollMessage, duration).ConfigureAwait(false);
            var distinctResult = result.Distinct();

            foreach (var elm in distinctResult)
            {
                var emoji = elm.Emoji.Name.ToString();
                var user = elm.Users.ToList();
                var username = user[0].Username;

                string reaction = "<" + username + ">" + emoji + "|" + pollMessage.Id.ToString();
                Console.WriteLine(reaction);
                ReadWrite.Write(reaction);
            }



            var results = result.Select(x => $"{x.Emoji}: {x.Total}");

            await ctx.Channel.SendMessageAsync(string.Join("\n", results)).ConfigureAwait(false);
        }

        //Poll Command
        [Command("Poll")]
        [RequireRoles(RoleCheckMode.Any, "Moderator", "Admins")]
        public async Task Poll(CommandContext ctx)
        {
            var Message = await ctx.Channel.SendMessageAsync("Poll").ConfigureAwait(false);
            await Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":attendanceyes:"));
            await Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":attendancenope:"));
            await Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":attendancemaybe:"));
            await Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":regional_indicator_r:"));
            await Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":speaker:"));

            //Drivers
            List<string> drivers = new List<string>();

            //Run Poll for 7 Days
            DateTime startDate = DateTime.Now;
            DateTime endDate = DateTime.Now.AddDays(7);


            while (startDate < endDate)
            {
                var interactivity = ctx.Client.GetInteractivity();
                var message = await interactivity.WaitForReactionAsync(x => x.Message.Id == Message.Id);

                //string reaction = "<" + message.Result.User.Username + ">" + message.Result.Emoji + "|" + message.Result.Message.Id.ToString();
                string reaction = "<" + message.Result.User.Username + ">" + message.Result.Emoji;
                string reactionToAdd = message.Result.Emoji + "   " + message.Result.User.Username;

                //Delete user reaction
                if (message.Result.User.IsBot == false)
                {
                    await Message.DeleteReactionAsync(message.Result.Emoji, message.Result.User);
                }

                //Add reaction to drivers
                if (drivers.Count == 0)
                {
                    drivers.Add(reactionToAdd);
                }

                bool reactionAdded = false;
                for (int i = 0; i < drivers.Count; i++)
                {
                    var match = Regex.Match(reaction, @"<(.+?)>").Groups[1].Value;
                    if (drivers[i].Contains(match))
                    {
                        drivers[i] = reactionToAdd;
                        reactionAdded = true;
                    }
                }

                if (reactionAdded == false)
                {
                    drivers.Add(reactionToAdd);
                }

                //Sort array of drivers
                drivers.Sort();

                //Add new reaction to message and write message
                string combindedString = string.Join("\n", drivers.ToArray());
                await Message.ModifyAsync(combindedString);
            }

        }

        //Mute User
        [Command("Mute")]
        [Description("Mute a user for a set time")]
        [RequireRoles(RoleCheckMode.Any, "Moderator", "Admins")]
        public async Task Mute(CommandContext ctx, [Description("User to mute")] DiscordMember user, [Description("Duration of mute time in minutes")] int time)
        {
            //Get role
            DiscordRole muteRole = ctx.Guild.GetRole(862770194475646996);

            await user.GrantRoleAsync(muteRole);
            await ctx.Channel.SendMessageAsync(user.Username + " has been sent to Principle O’Shaughnessy office for " + time + " minutes").ConfigureAwait(false);

            //Sleep for x amount of minutes
            System.Threading.Thread.Sleep(time * 60 * 1000);

            //Unmute user
            await user.RevokeRoleAsync(muteRole).ConfigureAwait(false);
        }
    }

    /*
    class Drivers
    {
        public string driver { get; set; }
        public string driverReaction { get; set; }
    }
    */
}



/*
Read and update messages
var Message = await ctx.Channel.GetMessageAsync(854885329332076554);
await Message.ModifyAsync(msg => msg.Content = "test [edited]");


old code
            await ctx.Channel.SendMessageAsync(message.Result.Emoji).ConfigureAwait(false);
            await ctx.Channel.SendMessageAsync(message.Result.User.Username).ConfigureAwait(false);
            await ctx.Channel.SendMessageAsync(message.Result.Message.Id.ToString()).ConfigureAwait(false);

Ways to get members
            var member = ctx.Message.MentionedUsers.First();
            await DiscordMember dmember = ctx.Guild.GetMemberAsync(member.Id);
            var userToMute = ctx.Client.GetUserAsync((ulong)Convert.ToInt64(Regex.Replace(user, "[^0-9]", "")));

Roles


            var role = "";
            var userid = message.Result.User.Id;
            var test = ctx.Client.GetUserAsync(userid);
            ctx.Client.getus
            //ctx.Client.Get

            
            
            //message.Result.User
            var user = ctx.Member.Roles.ToArray(); //This lists all channel roles
            var currentuser = ctx.Client.CurrentUser.Id;

            string[] userRoles = (from o in user
                                  select o.ToString()).ToArray();

            if (userRoles.Contains("Role 283726114436677633; BWT Racing Point F1 Team"))
            {
                role = "BWT Racing Point F1 Team";
            }
            else if (role == "" && userRoles.Contains("Role 283725844524826635; Scuderia Ferrari"))
            {
                role = "Scuderia Ferrari";
            }
            else if (role == "" && userRoles.Contains("Role 283726027702665216; Williams Racing"))
            {
                role = "Williams Racing";
            }
            else if (role == "" && userRoles.Contains("Role 283726063316762624; Haas F1 Team"))
            {
                role = "Haas F1 Team";
            }
            else if (role == "" && userRoles.Contains("Role 283725451942428673; Mercedes AMG Petronas F1 Team"))
            {
                role = "Mercedes AMG Petronas F1 Team";
            }
            else if (role == "" && userRoles.Contains("Role 283726208846266379; Scuderia Alpha Tauri Honda"))
            {
                role = "Scuderia Alpha Tauri Honda";
            }
            else if (role == "" && userRoles.Contains("Role 283726296083595265; McLaren F1 Team"))
            {
                role = "McLaren F1 Team";
            }
            else if (role == "" && userRoles.Contains("Role 283726376820015105; Renault DP World F1 Team"))
            {
                role = "Renault DP World F1 Team";
            }
            else if (role == "" && userRoles.Contains("Role 283725954264596481; Aston Martin Red Bull Racing"))
            {
                role = "Aston Martin Red Bull Racing";
            }
            else if (role == "" && userRoles.Contains("Role 283726652549234689; Alfa Romeo Racing ORLEN"))
            {
                role = "Alfa Romeo Racing ORLEN";
            }
            else if (role == "" && userRoles.Contains("Role 360955418870022144; Reserve Drivers"))
            {
                role = "Reserve Drivers";
            }
            else if (role == "" && userRoles.Contains("Role 604927082316693515; Commentator"))
            {
                role = "Commentator";
            }
            else
            {
                role = "IMPOSTER!!!";
            }
 */