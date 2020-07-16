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
    [Group("insult"), Module(ModuleType.Miscellaneous), NotBlocked]
    [Description("Insults manipulation. Group call insults a given user.")]
    [Aliases("burn", "insults", "ins", "roast")]

    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public class InsultModule : TheGodfatherModule
    {

        public InsultModule(DbContextBuilder db)
            : base(db)
        {

        }


        [GroupCommand]
        public async Task ExecuteGroupAsync(CommandContext ctx,
                                           [Description("User to insult.")] DiscordUser user = null)
        {
            user = user ?? ctx.User;

            if (user.Id == ctx.Client.CurrentUser.Id) {
                await this.InformAsync(ctx, "How original, trying to make me insult myself. Sadly it won't work.", ":middle_finger:");
                return;
            }

            Insult insult;
            using (TheGodfatherDbContext db = this.Database.CreateContext()) {
                if (!db.Insults.Any())
                    throw new CommandFailedException("No available insults.");
                insult = db.Insults.Shuffle().First();
            }

            await this.InformAsync(ctx, insult.Content.Replace("%user%", user.Mention), ":middle_finger:");
        }


        #region COMMAND_INSULTS_ADD
        [Command("add")]
        [Description("Add insult to list (use %user% instead of user mention).")]
        [Aliases("new", "a", "+", "+=", "<", "<<")]

        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task AddInsultAsync(CommandContext ctx,
                                        [RemainingText, Description("Insult (must contain ``%user%``).")] string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                throw new InvalidCommandUsageException("Missing insult string.");

            if (content.Length >= 120)
                throw new CommandFailedException("Too long insult. I know it is hard, but keep it shorter than 120 characters please.");

            if (!content.Contains("%user%"))
                throw new InvalidCommandUsageException($"Insult string is not in correct format (missing {Formatter.Bold("%user%")} in the content)!");

            using (TheGodfatherDbContext db = this.Database.CreateContext()) {
                if (db.Insults.Any(i => string.Compare(i.Content, content, StringComparison.InvariantCultureIgnoreCase) == 0))
                    throw new CommandFailedException("The given insult string already exists!");
                db.Insults.Add(new Insult {
                    Content = content
                });
                await db.SaveChangesAsync();
            }

            await this.InformAsync(ctx, $"Successfully added insult: {Formatter.Italic(content)}", important: false);
        }
        #endregion

        #region COMMAND_INSULTS_DELETE
        [Command("delete")]
        [Description("Remove insult with a given ID from list. (use command ``insults list`` to view insult indexes).")]
        [Aliases("-", "remove", "del", "rm", "rem", "d", ">", ">>", "-=")]

        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task DeleteInsultAsync(CommandContext ctx,
                                           [Description("ID of the insult to remove.")] int id)
        {
            using (TheGodfatherDbContext db = this.Database.CreateContext()) {
                db.Insults.Remove(new Insult { Id = id });
                await db.SaveChangesAsync();
            }

            await this.InformAsync(ctx, $"Removed insult with ID: {Formatter.Bold(id.ToString())}");
        }
        #endregion

        #region COMMAND_INSULTS_DELETEALL
        [Command("deleteall"), UsesInteractivity]
        [Description("Delete all insults.")]
        [Aliases("clear", "da", "c", "ca", "cl", "clearall", ">>>")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task ClearAllInsultsAsync(CommandContext ctx)
        {
            if (!await ctx.WaitForBoolReplyAsync("Are you sure you want to delete all insults?"))
                return;

            using (TheGodfatherDbContext db = this.Database.CreateContext()) {
                db.Insults.RemoveRange(db.Insults);
                await db.SaveChangesAsync();
            }

            await this.InformAsync(ctx, "All insults successfully removed.", important: false);
        }
        #endregion

        #region COMMAND_INSULTS_LIST
        [Command("list")]
        [Description("Show all insults.")]
        [Aliases("ls", "l")]
        public async Task ListInsultsAsync(CommandContext ctx)
        {
            List<Insult> insults;
            using (TheGodfatherDbContext db = this.Database.CreateContext())
                insults = await db.Insults.Where(i => i.GuildIdDb == (long)ctx.Guild.Id).ToListAsync();

            if (!insults.Any())
                throw new CommandFailedException("No insults registered.");

            await ctx.SendCollectionInPagesAsync(
                "Available insults",
                insults,
                insult => $"{Formatter.InlineCode($"{insult.Id:D3}")} | {Formatter.Italic(insult.Content)}",
                this.ModuleColor
            ).ConfigureAwait(false);
        }
        #endregion
    }
}
