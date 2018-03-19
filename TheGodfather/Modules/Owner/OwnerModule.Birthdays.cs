#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
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
        [Description("Birthday notifications management. If invoked without subcommand, lists all birthdays registered.")]
        [Aliases("birthday", "bday", "bd", "bdays")]
        [ListeningCheck]
        public class BirthdayModule : TheGodfatherBaseModule
        {

            public BirthdayModule(DBService db) : base(db: db) { }


            [GroupCommand]
            public async Task ExecuteGroupAsync(CommandContext ctx)
                => await ListAsync(ctx).ConfigureAwait(false);


            #region COMMAND_BIRTHDAY_ADD
            [Command("add"), Priority(1)]
            [Description("Add a birthday to the database. If date is not specified, uses the current date as a birthday date. If the channel is not specified, uses the current channel.")]
            [Aliases("+", "a")]
            [UsageExample("!owner birthday add @Someone")]
            [UsageExample("!owner birthday add @Someone #channel_to_send_message_to")]
            [UsageExample("!owner birthday add @Someone 15.2.1990")]
            [UsageExample("!owner birthday add @Someone #channel_to_send_message_to 15.2.1990")]
            [UsageExample("!owner birthday add @Someone 15.2.1990 #channel_to_send_message_to")]
            public async Task AddAsync(CommandContext ctx,
                                      [Description("Birthday boy/girl.")] DiscordUser user,
                                      [Description("Birth date.")] string date_str = null,
                                      [Description("Channel to send a greeting message to.")] DiscordChannel channel = null)
            {
                var date = DateTime.UtcNow.Date;
                if (!string.IsNullOrWhiteSpace(date_str))
                    DateTime.TryParse(date_str, out date);

                if (channel == null)
                    channel = ctx.Channel;

                await Database.AddBirthdayAsync(user.Id, channel.Id, date)
                    .ConfigureAwait(false);
                await ctx.RespondWithIconEmbedAsync()
                    .ConfigureAwait(false);
            }

            [Command("add"), Priority(0)]
            public async Task AddAsync(CommandContext ctx,
                                      [Description("Birthday boy/girl.")] DiscordUser user,
                                      [Description("Channel to send a greeting message to.")] DiscordChannel channel = null,
                                      [Description("Birth date.")] string date_str = null)
                => await AddAsync(ctx, user, date_str, channel).ConfigureAwait(false);
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
            [Description("List all registered birthdays.")]
            [Aliases("ls")]
            [UsageExample("!owner birthday list")]
            public async Task ListAsync(CommandContext ctx)
            {
                var birthdays = await Database.GetAllBirthdaysAsync()
                    .ConfigureAwait(false);

                if (!birthdays.Any())
                    throw new CommandFailedException("No birthdays registered!");

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
