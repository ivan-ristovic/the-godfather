#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using TheGodfather.Common.Attributes;
using TheGodfather.Database;
using TheGodfather.Database.Models;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
#endregion

namespace TheGodfather.Modules.Misc
{
    [Group("birthdays"), Module(ModuleType.Miscellaneous), NotBlocked]
    [Description("Birthday notifications commands. Group call either lists or adds birthday depending if argument is given.")]
    [Aliases("birthday", "bday", "bd", "bdays")]

    [RequireUserPermissions(Permissions.ManageGuild)]
    [Cooldown(3, 5, CooldownBucketType.Guild)]
    public class BirthdayModule : TheGodfatherModule
    {

        public BirthdayModule(DbContextBuilder db)
            : base(db)
        {

        }


        [GroupCommand, Priority(3)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("Whose birthday to search for.")] DiscordUser user)
            => this.ListAsync(ctx, user);

        [GroupCommand, Priority(2)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("Channel for which to list the birthdays.")] DiscordChannel channel = null)
            => this.ListAsync(ctx, channel);

        [GroupCommand, Priority(1)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("Birthday boy/girl.")] DiscordUser user,
                                     [Description("Channel to send a greeting message to.")] DiscordChannel channel,
                                     [Description("Birth date.")] string date = null)
            => this.AddAsync(ctx, user, date, channel);

        [GroupCommand, Priority(0)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("Birthday boy/girl.")] DiscordUser user,
                                     [Description("Birth date.")] string date,
                                     [Description("Channel to send a greeting message to.")] DiscordChannel channel = null)
            => this.AddAsync(ctx, user, date, channel);


        #region COMMAND_BIRTHDAY_ADD
        [Command("add"), Priority(0)]
        [Description("Schedule a birthday notification. If the date is not specified, uses the current date as a birthday date. If the channel is not specified, uses the current channel.")]
        [Aliases("new", "+", "a", "+=", "<", "<<")]

        public async Task AddAsync(CommandContext ctx,
                                  [Description("Birthday boy/girl.")] DiscordUser user,
                                  [Description("Birth date.")] string date_str = null,
                                  [Description("Channel to send a greeting message to.")] DiscordChannel channel = null)
        {
            DateTime date = DateTime.UtcNow.Date;
            if (!string.IsNullOrWhiteSpace(date_str) && !DateTime.TryParse(date_str, out date))
                throw new CommandFailedException("The given date is not valid!");

            channel = channel ?? ctx.Channel;

            if (channel.Type != ChannelType.Text)
                throw new CommandFailedException("I can only send birthday notifications in a text channel.");

            using (TheGodfatherDbContext db = this.Database.CreateContext()) {
                db.Birthdays.Add(new Birthday {
                    ChannelId = channel.Id,
                    Date = date,
                    GuildId = ctx.Guild.Id,
                    LastUpdateYear = DateTime.Now.Year,
                    UserId = user.Id
                });

                await db.SaveChangesAsync();
            }

            await this.InformAsync(ctx, $"Added a new birthday in channel {Formatter.Bold(channel.Name)} for {Formatter.Bold(user.Username)}", important: false);
        }

        [Command("add"), Priority(1)]
        public Task AddAsync(CommandContext ctx,
                            [Description("Birthday boy/girl.")] DiscordUser user,
                            [Description("Channel to send a greeting message to.")] DiscordChannel channel = null,
                            [Description("Birth date.")] string date_str = null)
            => this.AddAsync(ctx, user, date_str, channel);
        #endregion

        #region COMMAND_BIRTHDAY_DELETE
        [Command("delete"), Priority(1), UsesInteractivity]
        [Description("Remove status from running queue.")]
        [Aliases("-", "remove", "rm", "del", "-=", ">", ">>")]

        public async Task DeleteAsync(CommandContext ctx,
                                     [Description("User whose birthday to remove.")] DiscordUser user)
        {

            using (TheGodfatherDbContext db = this.Database.CreateContext()) {
                db.Birthdays.RemoveRange(db.Birthdays.Where(b => b.GuildId == ctx.Guild.Id && b.UserId == user.Id));
                await db.SaveChangesAsync();
            }

            await this.InformAsync(ctx, $"Removed birthday for {Formatter.Bold(user.Username)}", important: false);
        }

        [Command("delete"), Priority(0)]
        public async Task DeleteAsync(CommandContext ctx,
                                     [Description("Channel for which to remove birthdays.")] DiscordChannel channel)
        {
            if (!await ctx.WaitForBoolReplyAsync("Are you sure you want to delete all birthdays in this channel?"))
                return;

            using (TheGodfatherDbContext db = this.Database.CreateContext()) {
                db.Birthdays.RemoveRange(db.Birthdays.Where(b => b.GuildId == ctx.Guild.Id && b.ChannelId == channel.Id));
                await db.SaveChangesAsync();
            }

            await this.InformAsync(ctx, $"Removed birthday notifications in channel {Formatter.Bold(channel.Mention)}", important: false);
        }
        #endregion

        #region COMMAND_BIRTHDAY_LIST
        [Command("list"), Priority(1)]
        public async Task ListAsync(CommandContext ctx,
                                   [Description("Whose birthday to search for.")] DiscordUser user)
        {
            List<Birthday> birthdays;
            using (TheGodfatherDbContext db = this.Database.CreateContext()) {
                birthdays = await db.Birthdays
                    .Where(b => b.GuildId == ctx.Guild.Id && b.UserId == user.Id)
                    .ToListAsync();
            }

            await ctx.SendCollectionInPagesAsync(
                $"Birthdays registered for {user.Username}:",
                birthdays,
                b => $"{Formatter.InlineCode(b.Date.ToShortDateString())} | Channel: {b.ChannelId}",
                this.ModuleColor,
                5
            );
        }

        [Command("list"), Priority(0)]
        [Description("List registered birthday notifications for this channel.")]
        [Aliases("ls")]
        public async Task ListAsync(CommandContext ctx,
                                   [Description("Channel for which to list the birthdays.")] DiscordChannel channel = null)
        {
            channel = channel ?? ctx.Channel;

            if (channel.Type != ChannelType.Text)
                throw new CommandFailedException("Birthday notifications are only posted in text channels");

            List<Birthday> birthdays;
            using (TheGodfatherDbContext db = this.Database.CreateContext()) {
                birthdays = await db.Birthdays
                    .Where(b => b.GuildId == ctx.Guild.Id && b.ChannelId == channel.Id)
                    .ToListAsync();
            }

            if (!birthdays.Any())
                throw new CommandFailedException("No birthdays registered!");

            var lines = new List<string>();
            foreach (Birthday birthday in birthdays) {
                try {
                    DiscordUser user = await ctx.Client.GetUserAsync(birthday.UserId);
                    lines.Add($"{Formatter.InlineCode(birthday.Date.ToShortDateString())} | {Formatter.Bold(user.Username)} | {channel.Name}");
                } catch {

                    using (TheGodfatherDbContext db = this.Database.CreateContext()) {
                        db.Birthdays.RemoveRange(db.Birthdays.Where(b => b.UserId == birthday.UserId));
                        await db.SaveChangesAsync();
                    }
                }
            }

            await ctx.SendCollectionInPagesAsync(
                $"Birthdays registered in channel {ctx.Channel.Name}:",
                lines,
                line => line,
                this.ModuleColor,
                5
            );
        }
        #endregion

        #region COMMAND_BIRTHDAY_LISTALL
        [Command("listall")]
        [Description("List all registered birthdays.")]
        [Aliases("lsa")]
        [RequirePrivilegedUser]
        public async Task ListAllAsync(CommandContext ctx)
        {
            List<Birthday> birthdays;
            using (TheGodfatherDbContext db = this.Database.CreateContext()) {
                birthdays = await db.Birthdays
                    .Where(b => b.GuildId == ctx.Guild.Id)
                    .ToListAsync();
            }

            if (!birthdays.Any())
                throw new CommandFailedException("No birthdays registered!");

            var lines = new List<string>();
            foreach (Birthday birthday in birthdays) {
                try {
                    DiscordChannel channel = await ctx.Client.GetChannelAsync(birthday.ChannelId);
                    DiscordUser user = await ctx.Client.GetUserAsync(birthday.UserId);
                    lines.Add($"{Formatter.InlineCode(birthday.Date.ToShortDateString())} | {Formatter.Bold(user.Username)} | {channel.Name}");
                } catch {

                    using (TheGodfatherDbContext db = this.Database.CreateContext()) {
                        db.Birthdays.RemoveRange(db.Birthdays.Where(b => b.UserId == birthday.UserId));
                        await db.SaveChangesAsync();
                    }
                }
            }

            await ctx.SendCollectionInPagesAsync(
                "Birthdays:",
                lines,
                line => line,
                this.ModuleColor,
                5
            );
        }
        #endregion
    }
}
