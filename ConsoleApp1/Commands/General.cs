using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.Build.Tasks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp1.Commands
{
    public class General : BaseCommandModule
    {
        //Poll Command
        [Command("Poll")]
        [RequireRoles(RoleCheckMode.Any, "Moderator", "Admins")]
        public async Task Poll(CommandContext ctx)
        {
            //Delete the poll command
            await ctx.Message.DeleteAsync();
            
            //Create poll message
            var Message = await ctx.Channel.SendMessageAsync("Poll").ConfigureAwait(false);
            await Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":attendanceyes:"));
            await Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":attendancenope:"));
            await Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":attendancemaybe:"));
            await Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":regional_indicator_r:"));
            await Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":speaker:"));

            //Drivers
            List<string> drivers = new List<string>();

            //Run Poll for 7 Days
            //DateTime startDate = DateTime.Now;
            //DateTime endDate = DateTime.Now.AddDays(7);

            while (ctx.Channel.GetMessageAsync(Message.Id) != null)
            {
                var interactivity = ctx.Client.GetInteractivity();
                var message = await interactivity.WaitForReactionAsync(x => x.Message.Id == Message.Id);

                //Get timestamp of message and convert to Melbourne time
                var time = DateTime.Now;
                DateTime utcTime = time.ToUniversalTime();
                TimeZoneInfo timeInfo = TimeZoneInfo.FindSystemTimeZoneById("AUS Eastern Standard Time");
                DateTime userTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, timeInfo);

                string reaction = "<" + message.Result.User.Username + ">" + message.Result.Emoji;
                string reactionToAdd = message.Result.Emoji + "   " + message.Result.User.Username + "   " + userTime.ToString("MM/dd/yyyy HH:mm");

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

        //List Teams
        [Command("Listteams")]
        [Description("List all teams")]
        public async Task Listteams(CommandContext ctx)
        {
            await ctx.Message.DeleteAsync();
            StringBuilder strBuilder = new StringBuilder();
            strBuilder.Append("SELECT * FROM teams");
            string sqlQuery = strBuilder.ToString();
            using (SqlCommand command = new SqlCommand(sqlQuery, SQLConnection.Connection()))
            {
                command.ExecuteNonQuery();
                DataSet ds = new DataSet();
                DataTable table = new DataTable();
                table.Load(command.ExecuteReader());
                ds.Tables.Add(table);

                //Build list
                List<string> teamsList = new List<string>();
                for (int i = 0; i < table.Rows.Count; i++)
                {
                    teamsList.Add(table.Rows[i]["id"].ToString() + " " + table.Rows[i]["teamName"].ToString());
                }

                //Write out list
                string combindedString = string.Join("\n", teamsList.ToArray());
                await ctx.Channel.SendMessageAsync(combindedString).ConfigureAwait(false);
            }

        }

        //Add / Update Driver
        [Command("Add")]
        [Description("Adds or updates a driver")]
        public async Task Add(CommandContext ctx, [Description("Driver to add")] DiscordMember user, [Description("Team to add driver to")] int team, [Description("Tier to add driver to")] string tier)
        {
            await ctx.Message.DeleteAsync();

            //Get values
            ulong driverID = user.Id;
            string driver = user.Username;

            //Build string and call SP
            StringBuilder strBuilder = new StringBuilder();
            strBuilder.Append("EXEC AddDriver " + "@driverID = " + driverID + ", @driver = " + "'" + driver + "'" + ", @team = " + team + ", @tier = " + tier);
            string sqlQuery = strBuilder.ToString();
            using (SqlCommand command = new SqlCommand(sqlQuery, SQLConnection.Connection()))
            {
                command.ExecuteNonQuery();
                using (SqlDataReader dr = command.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        string result = dr[0].ToString();
                        var Message = await ctx.Channel.SendMessageAsync(result.ToString()).ConfigureAwait(false);
                        Thread.Sleep(60000);
                        await ctx.Channel.DeleteMessageAsync(Message).ConfigureAwait(false);
                    }
                }
            }

        }

        //Delete Driver
        [Command("Remove")]
        [Description("Removes a driver")]
        public async Task Remove(CommandContext ctx, [Description("DriverID to remove")] string driverID, [Description("Tier to remove driver from")] string tier)
        {
            await ctx.Message.DeleteAsync();

            //Build string and call SP
            StringBuilder strBuilder = new StringBuilder();
            strBuilder.Append("EXEC RemoveDriver " + "@driverID = " + driverID + ", @tier = " + tier);
            string sqlQuery = strBuilder.ToString();
            using (SqlCommand command = new SqlCommand(sqlQuery, SQLConnection.Connection()))
            {
                command.ExecuteNonQuery();
                using (SqlDataReader dr = command.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        string result = dr[0].ToString();
                        var Message = await ctx.Channel.SendMessageAsync(result.ToString()).ConfigureAwait(false);
                        Thread.Sleep(60000);
                        await ctx.Channel.DeleteMessageAsync(Message).ConfigureAwait(false);
                    }
                }
            }

        }
        //Read all Drivers
        [Command("ListAll")]
        [Description("List all drivers in all tiers")]
        public async Task ListAll(CommandContext ctx)
        {
            await ctx.Message.DeleteAsync();
            StringBuilder strBuilder = new StringBuilder();
            strBuilder.Append("SELECT * FROM attendance");
            string sqlQuery = strBuilder.ToString();
            using (SqlCommand command = new SqlCommand(sqlQuery, SQLConnection.Connection()))
            {
                command.ExecuteNonQuery();
                DataSet ds = new DataSet();
                DataTable table = new DataTable();
                table.Load(command.ExecuteReader());
                ds.Tables.Add(table);

                //Build list
                List<string> teamsList = new List<string>();
                for (int i = 0; i < table.Rows.Count; i++)
                {
                    teamsList.Add(table.Rows[i]["driverID"].ToString() + " " + table.Rows[i]["driver"].ToString() + " " + table.Rows[i]["team"].ToString() + " " + table.Rows[i]["tier"].ToString() + " " + table.Rows[i]["attendanceReaction"].ToString());
                }
                //driverID driver team tier attendanceReaction
                //Write out list
                string combindedString = string.Join("\n", teamsList.ToArray());
                await ctx.Channel.SendMessageAsync(combindedString).ConfigureAwait(false);
            }
        }

        //Read specific Tier
    }
}

//Test SQL
/*
[Command("SQL")]
public async Task SQL(CommandContext ctx)
{
    string result = SQLConnection.Connection();
    await ctx.Channel.SendMessageAsync(result).ConfigureAwait(false);
}
*/