#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Misc.Extensions;
using TheGodfather.Services;
#endregion

namespace TheGodfather.Modules.Misc
{
    [Group("insult"), Module(ModuleType.Miscellaneous), NotBlocked]
    [Description("Insults manipulation. Group call insults a given user.")]
    [Aliases("burn", "insults", "ins", "roast")]
    [UsageExamples("!insult @Someone")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public class InsultModule : TheGodfatherModule
    {

        public InsultModule(SharedData shared, DBService db)
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

            string insult = await this.Database.GetRandomInsultAsync();
            if (insult is null)
                throw new CommandFailedException("No available insults.");

            await this.InformAsync(ctx, insult.Replace("%user%", user.Mention), ":middle_finger:");
        }


        #region COMMAND_INSULTS_ADD
        [Command("add")]
        [Description("Add insult to list (use %user% instead of user mention).")]
        [Aliases("new", "a", "+", "+=", "<", "<<")]
        [UsageExamples("!insult add %user% is lowering the IQ of the entire street!")]
        [RequirePrivilegedUser]
        public async Task AddInsultAsync(CommandContext ctx,
                                        [RemainingText, Description("Insult (must contain ``%user%``).")] string insult)
        {
            if (string.IsNullOrWhiteSpace(insult))
                throw new InvalidCommandUsageException("Missing insult string.");

            if (insult.Length >= 120)
                throw new CommandFailedException("Too long insult. I know it is hard, but keep it shorter than 120 characters please.");

            if (insult.Split(new string[] { "%user%" }, StringSplitOptions.None).Count() < 2)
                throw new InvalidCommandUsageException($"Insult not in correct format (missing {Formatter.Bold("%user%")} in the insult)!");

            await this.Database.AddInsultAsync(insult);
            await this.InformAsync(ctx, $"Successfully added insult: {Formatter.Italic(insult)}", important: false);
        }
        #endregion
        
        #region COMMAND_INSULTS_DELETEALL
        [Command("deleteall"), UsesInteractivity]
        [Description("Delete all insults.")]
        [Aliases("clear", "da", "c", "ca", "cl", "clearall", ">>>")]
        [UsageExamples("!insults clear")]
        [RequirePrivilegedUser]
        public async Task ClearAllInsultsAsync(CommandContext ctx)
        {
            if (!await ctx.WaitForBoolReplyAsync("Are you sure you want to delete all insults?"))
                return;

            await this.Database.RemoveAllInsultsAsync();
            await this.InformAsync(ctx, "All insults successfully removed.", important: false);
        }
        #endregion

        #region COMMAND_INSULTS_DELETE
        [Command("delete")]
        [Description("Remove insult with a given index from list. (use command ``insults list`` to view insult indexes).")]
        [Aliases("-", "remove", "del", "rm", "rem", "d", ">", ">>", "-=")]
        [UsageExamples("!insult delete 2")]
        [RequirePrivilegedUser]
        public async Task DeleteInsultAsync(CommandContext ctx, 
                                           [Description("Index of the insult to remove.")] int index)
        {
            await this.Database.RemoveInsultAsync(index);
            await this.InformAsync(ctx, $"Removed insult with ID: {Formatter.Bold(index.ToString())}");
        }
        #endregion

        #region COMMAND_INSULTS_LIST
        [Command("list")]
        [Description("Show all insults.")]
        [Aliases("ls", "l")]
        [UsageExamples("!insult list")]
        public async Task ListInsultsAsync(CommandContext ctx)
        {
            IReadOnlyDictionary<int, string> insults = await this.Database.GetAllInsultsAsync();
            if (insults is null || !insults.Any())
                throw new CommandFailedException("No insults registered.");

            await ctx.SendCollectionInPagesAsync(
                "Available insults",
                insults,
                kvp => $"{Formatter.InlineCode($"{kvp.Key:D3}")} | {Formatter.Italic(kvp.Value)}",
                this.ModuleColor
            ).ConfigureAwait(false);
        }
        #endregion
    }
}
