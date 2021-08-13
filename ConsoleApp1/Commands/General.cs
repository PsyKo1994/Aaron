using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.Build.Tasks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        //Test SQL
        /*
        [Command("SQL")]
        public async Task SQL(CommandContext ctx)
        {
            string result = SQLConnection.Connection();
            await ctx.Channel.SendMessageAsync(result).ConfigureAwait(false);
        }
        */

        //Add Driver
        [Command("Add")]
        [Description("Add a driver")]
        public async Task Add(CommandContext ctx, [Description("Driver to add")] DiscordMember driver, [Description("Tier to add driver to")] string tier, [Description("Team to add driver to")] int team)
        {
            //Get driver
            int driverID = (int)driver.Id;
            string driverName = driver.Username;

            await ctx.Channel.SendMessageAsync(driverID + " " + driverName + " has not been added to the database yet but I found him so that's a good start").ConfigureAwait(false);


            //Call SP

        }

        //Read Teams
        //[Command("Listteams")]
        //[Description("List all teams")]

        //Add Team
        [Command("Addteam")]
        [Description("Add a Team")]
        public async Task AddTeam(CommandContext ctx, [Description("Team ID")] int id, [Description("Team name")] string teamname)
        {
            //call SP
            StringBuilder strBuilder = new StringBuilder();
            strBuilder.Append("EXECUTE AddTeam @id = " + id + ", @teamName = " + teamname);
            string sqlQuery = strBuilder.ToString();
            //SqlConnection conn = new SqlConnection(connString);
            using (SqlCommand command = new SqlCommand(sqlQuery, SQLConnection.Connection()))
            {
                command.ExecuteNonQuery(); //execute the Query
                Console.WriteLine("Query Executed.");
            }
        }

        //Update Team

        //Delete Team

        /*
        //create a new SQL Query using StringBuilder
        StringBuilder strBuilder = new StringBuilder();
        strBuilder.Append("INSERT INTO Student_details (Name, Email, Class) VALUES ");
                strBuilder.Append("(N'Harsh', N'harsh@gmail.com', N'Class X'), ");
                strBuilder.Append("(N'Ronak', N'ronak@gmail.com', N'Class X') ");

                string sqlQuery = strBuilder.ToString();
                using (SqlCommand command = new SqlCommand(sqlQuery, conn)) //pass SQL query created above and connection
                {
                    command.ExecuteNonQuery(); //execute the Query
                    Console.WriteLine("Query Executed.");
                }
        */
    }
}