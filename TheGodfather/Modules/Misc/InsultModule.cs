#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TheGodfather.Common.Attributes;
using TheGodfather.Database;
using TheGodfather.Database.Entities;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
#endregion

namespace TheGodfather.Modules.Misc
{
    [Group("insult"), Module(ModuleType.Miscellaneous), NotBlocked]
    [Description("Insults manipulation. Group call insults a given user.")]
    [Aliases("burn", "insults", "ins", "roast")]
    [UsageExampleArgs("@Someone")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public class InsultModule : TheGodfatherModule
    {

        public InsultModule(SharedData shared, DatabaseContextBuilder db)
            : base(shared, db)
        {
            this.ModuleColor = DiscordColor.Brown;
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

            DatabaseInsult insult;
            using (DatabaseContext db = this.Database.CreateContext()) {
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
        [UsageExampleArgs("%user% is lowering the IQ of the entire street!")]
        [RequirePrivilegedUser]
        public async Task AddInsultAsync(CommandContext ctx,
                                        [RemainingText, Description("Insult (must contain ``%user%``).")] string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                throw new InvalidCommandUsageException("Missing insult string.");

            if (content.Length >= 120)
                throw new CommandFailedException("Too long insult. I know it is hard, but keep it shorter than 120 characters please.");

            if (!content.Contains("%user%"))
                throw new InvalidCommandUsageException($"Insult string is not in correct format (missing {Formatter.Bold("%user%")} in the content)!");

            using (DatabaseContext db = this.Database.CreateContext()) {
                if (db.Insults.Any(i => string.Compare(i.Content, content, StringComparison.InvariantCultureIgnoreCase) == 0))
                    throw new CommandFailedException("The given insult string already exists!");
                db.Insults.Add(new DatabaseInsult() {
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
        [UsageExampleArgs("!insult delete 2")]
        [RequirePrivilegedUser]
        public async Task DeleteInsultAsync(CommandContext ctx,
                                           [Description("ID of the insult to remove.")] int id)
        {
            using (DatabaseContext db = this.Database.CreateContext()) {
                db.Insults.Remove(new DatabaseInsult() { Id = id });
                await db.SaveChangesAsync();
            }

            await this.InformAsync(ctx, $"Removed insult with ID: {Formatter.Bold(id.ToString())}");
        }
        #endregion

        #region COMMAND_INSULTS_DELETEALL
        [Command("deleteall"), UsesInteractivity]
        [Description("Delete all insults.")]
        [Aliases("clear", "da", "c", "ca", "cl", "clearall", ">>>")]
        [RequirePrivilegedUser]
        public async Task ClearAllInsultsAsync(CommandContext ctx)
        {
            if (!await ctx.WaitForBoolReplyAsync("Are you sure you want to delete all insults?"))
                return;

            using (DatabaseContext db = this.Database.CreateContext()) {
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
            List<DatabaseInsult> insults;
            using (DatabaseContext db = this.Database.CreateContext())
                insults = await db.Insults.ToListAsync();

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
