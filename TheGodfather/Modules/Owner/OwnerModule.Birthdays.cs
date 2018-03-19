#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

using TheGodfather.Attributes;
using TheGodfather.Entities;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Owner.Common;
using TheGodfather.Services;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
#endregion

namespace TheGodfather.Modules.Owner
{
    public partial class OwnerModule
    {
        [Group("birthdays")]
        [Description("Birthday notifications management.")]
        [Aliases("birthday", "bday", "bd", "bdays")]
        [ListeningCheck]
        public class BirthdayModule : TheGodfatherBaseModule
        {

            public BirthdayModule(DBService db) : base(db: db) { }


            #region COMMAND_BIRTHDAY_ADD
            [Command("add")]
            [Description("Add a birthday to the database.")]
            [Aliases("+", "a")]
            [UsageExample("!owner birthday add @Someone")]
            [UsageExample("!owner birthday add @Someone #channel_to_send_message_to")]
            public async Task AddAsync(CommandContext ctx,
                                      [Description("Birthday boy/girl.")] DiscordUser user,
                                      [Description("Channel to send a greeting message to.")] DiscordChannel channel = null)
            {
                if (channel == null)
                    channel = ctx.Channel;

                await Database.AddBirthdayAsync(user.Id, channel.Id)
                    .ConfigureAwait(false);
                await ctx.RespondWithIconEmbedAsync()
                    .ConfigureAwait(false);
            }
            #endregion

            #region COMMAND_BIRTHDAY_DELETE
            [Command("delete")]
            [Description("Remove status from running queue.")]
            [Aliases("-", "remove", "rm", "del")]
            [UsageExample("!owner birthday delete @Someone")]
            public async Task DeleteAsync(CommandContext ctx,
                                         [Description("User whose birthday to remove.")] DiscordUser user)
            {
                await Database.RemoveBirthdayAsync(user.Id)
                    .ConfigureAwait(false);
                await ctx.RespondWithIconEmbedAsync()
                    .ConfigureAwait(false);
            }
            #endregion

            #region COMMAND_BIRTHDAY_LIST
            [Command("list")]
            [Description("List all bot statuses.")]
            [Aliases("ls")]
            [UsageExample("!owner status list")]
            public async Task ListAsync(CommandContext ctx)
            {
                var birthdays = await Database.GetAllBirthdaysAsync()
                    .ConfigureAwait(false);

                List<string> lines = new List<string>();
                foreach (var birthday in birthdays) {
                    try {
                        var channel = await ctx.Client.GetChannelAsync(birthday.ChannelId)
                            .ConfigureAwait(false);
                        var user = await ctx.Client.GetUserAsync(birthday.UserId)
                            .ConfigureAwait(false);
                        lines.Add($"{Formatter.Bold(user.Username)} | {birthday.Date.ToShortDateString()} | {channel.Name}");
                    } catch (NotFoundException) {
                        await Database.RemoveBirthdayAsync(birthday.UserId)
                            .ConfigureAwait(false);
                    }
                }

                await ctx.SendPaginatedCollectionAsync(
                    "Birthdays:",
                    lines,
                    line => line,
                    DiscordColor.Azure,
                    5
                ).ConfigureAwait(false);
            }
            #endregion
        }
    }
}
