using DSharpPlus;
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
    public class Moderator : BaseCommandModule
    {
        //Mute User
        [Command("Mute")]
        [Description("Mute a user for a set time")]
        [RequireRoles(RoleCheckMode.Any, "Moderator", "Admins")]
        public async Task Mute(CommandContext ctx, [Description("User to mute")] DiscordMember user, [Description("Duration of mute time in minutes")] int time)
        {
            await ctx.Message.DeleteAsync();
            //Get role
            DiscordRole muteRole = ctx.Guild.GetRole(862770194475646996);

            await user.GrantRoleAsync(muteRole);
            await ctx.Channel.SendMessageAsync(user.Username + " has been sent to Principal O’Shaughnessy's office for " + time + " minutes").ConfigureAwait(false);
            await user.SendMessageAsync("https://tenor.com/view/key-and-peele-messed-up-aaron-gif-9848731").ConfigureAwait(false);
            await user.SendMessageAsync("Your actions have resulted in a temporary mute in text channels in AFR. Please take this time to think about how you respond").ConfigureAwait(false);

            //Sleep for x amount of minutes
            System.Threading.Thread.Sleep(time * 60 * 1000);

            //Unmute user
            await user.RevokeRoleAsync(muteRole).ConfigureAwait(false);
        }

        //Delete all messaged in a channel, can't delete messages older than 14 days
        [Command("DeleteLast")]
        [Description("Deletes all messaged in a channel, can't delete messages older than 14 days")]
        [RequireRoles(RoleCheckMode.Any, "Admins")]
        public async Task DeleteLast(CommandContext ctx, [Description("How many messages to delete. Limit is 100")] int amount)
        {
            var messages = await ctx.Channel.GetMessagesAsync(amount + 1);
            await ctx.Channel.DeleteMessagesAsync(messages);
        }

        //Lock Incidents
        [Command("LockIncidents")]
        [Description("Locks incident channels from Tier X roles")]
        [RequireRoles(RoleCheckMode.Any, "Admins", "Head Steward")]
        public async Task LockIncidents(CommandContext ctx)
        {
            await ctx.Message.DeleteAsync();
            //Lock Tier 1 incident channel
            var channelToLock = ctx.Guild.GetChannel(683482799780528128);
            var roleToLock = ctx.Guild.GetRole(491762196775305227);
            var reserveRole = ctx.Guild.GetRole(360955418870022144);
            await channelToLock.AddOverwriteAsync(roleToLock, Permissions.None);
            await channelToLock.AddOverwriteAsync(reserveRole, Permissions.None);
            //await channelToLock.SendMessageAsync("Channel locked until Monday morning. If you have an incident to report, the stewards recommend discussing with the other party during the cooldown period and reconsider the need to report.");

            //Lock Tier 2 incident channel
            var channelToLock2 = ctx.Guild.GetChannel(658120861299245057);
            var roleToLock2 = ctx.Guild.GetRole(595188325569265664);
            await channelToLock2.AddOverwriteAsync(roleToLock2, Permissions.None);
            await channelToLock2.AddOverwriteAsync(reserveRole, Permissions.None);
            //await channelToLock2.SendMessageAsync("Channel locked until Monday morning. If you have an incident to report, the stewards recommend discussing with the other party during the cooldown period and reconsider the need to report.");

            //Lock Tier 3 incident channel
            var channelToLock3 = ctx.Guild.GetChannel(861893292715409408);
            var roleToLock3 = ctx.Guild.GetRole(866464597707849760);
            await channelToLock3.AddOverwriteAsync(roleToLock3, Permissions.None);
            await channelToLock3.AddOverwriteAsync(reserveRole, Permissions.None);
            //await channelToLock3.SendMessageAsync("Channel locked until Monday morning. If you have an incident to report, the stewards recommend discussing with the other party during the cooldown period and reconsider the need to report.");
        }

        //Unlock Incidents
        [Command("UnlockIncidents")]
        [Description("Unlocks incident channels from Tier X roles")]
        [RequireRoles(RoleCheckMode.Any, "Admins", "Head Steward")]
        public async Task UnlockIncidents(CommandContext ctx)
        {
            await ctx.Message.DeleteAsync();
            //Unlock Tier 1 incident channel
            var channelToUnlock = ctx.Guild.GetChannel(683482799780528128);
            var roleToUnlock = ctx.Guild.GetRole(491762196775305227);
            var reserveRole = ctx.Guild.GetRole(360955418870022144);
            await channelToUnlock.AddOverwriteAsync(roleToUnlock, Permissions.SendMessages);
            await channelToUnlock.AddOverwriteAsync(reserveRole, Permissions.SendMessages);
            await channelToUnlock.SendMessageAsync($"<@&{roleToUnlock.Id}>" + ", " + $"<@&{reserveRole.Id}>" + " Channel unlocked");

            //Unlock Tier 1 incident channel
            var channelToUnlock2 = ctx.Guild.GetChannel(658120861299245057);
            var roleToUnlock2 = ctx.Guild.GetRole(595188325569265664);
            await channelToUnlock2.AddOverwriteAsync(roleToUnlock2, Permissions.SendMessages);
            await channelToUnlock2.AddOverwriteAsync(reserveRole, Permissions.SendMessages);
            await channelToUnlock2.SendMessageAsync($"<@&{roleToUnlock2.Id}>" + ", " + $"<@&{reserveRole.Id}>" + " Channel unlocked");

            //Unlock Tier 1 incident channel
            var channelToUnlock3 = ctx.Guild.GetChannel(861893292715409408);
            var roleToUnlock3 = ctx.Guild.GetRole(866464597707849760);
            await channelToUnlock3.AddOverwriteAsync(roleToUnlock3, Permissions.SendMessages);
            await channelToUnlock3.AddOverwriteAsync(reserveRole, Permissions.SendMessages);
            await channelToUnlock3.SendMessageAsync($"<@&{roleToUnlock3.Id}>" + ", " + $"<@&{reserveRole.Id}>" + " Channel unlocked");
        }

        //Create adhoc voice channel on a timer
        [Command("CreateVoice")]
        [Description("Create adhoc voice channel on a timer")]
        [RequireRoles(RoleCheckMode.Any, "Moderator", "Admins")]
        public async Task CreateVoice(CommandContext ctx, [Description("Name of channel")] string name, [Description("time in minutes until channel is nuked")] int time)
        {
            await ctx.Message.DeleteAsync();
            var tempChannel = await ctx.Guild.CreateVoiceChannelAsync(name, null, null, 20);
            await ctx.Channel.SendMessageAsync(name + " voice channel has been created and will be deleted in " + time + " minutes").ConfigureAwait(false);
            System.Threading.Thread.Sleep(time * 60 * 1000);
            await tempChannel.DeleteAsync();
        }
    }
}