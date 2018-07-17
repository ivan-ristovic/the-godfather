#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Services.Database.Insults;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using TheGodfather.Services.Database;
#endregion

namespace TheGodfather.Modules.Misc
{
    [Group("insult"), Module(ModuleType.Miscellaneous)]
    [Description("Insults manipulation. If invoked without subcommands, insults a given user.")]
    [Aliases("burn", "insults", "ins", "roast")]
    [UsageExamples("!insult @Someone")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    [NotBlocked]
    public class InsultModule : TheGodfatherBaseModule
    {

        public InsultModule(DBService db) : base(db: db) { }


        [GroupCommand]
        public async Task ExecuteGroupAsync(CommandContext ctx, 
                                           [Description("User to insult.")] DiscordUser user = null)
        {
            if (user == null)
                user = ctx.User;

            if (user.Id == ctx.Client.CurrentUser.Id) {
                await ctx.InformSuccessAsync("How original, trying to make me insult myself. Sadly it won't work.", ":middle_finger:")
                    .ConfigureAwait(false);
                return;
            }

            string insult = await Database.GetRandomInsultAsync()
                .ConfigureAwait(false);
            if (insult == null)
                throw new CommandFailedException("No available insults.");

            await ctx.InformSuccessAsync(insult.Replace("%user%", user.Mention), ":middle_finger:")
                .ConfigureAwait(false);
        }


        #region COMMAND_INSULTS_ADD
        [Command("add"), Module(ModuleType.Miscellaneous)]
        [Description("Add insult to list (use %user% instead of user mention).")]
        [Aliases("+", "new", "a")]
        [UsageExamples("!insult add You are so dumb, %user%!")]
        [RequireOwner]
        public async Task AddInsultAsync(CommandContext ctx,
                                        [RemainingText, Description("Insult (must contain ``%user%``).")] string insult)
        {
            if (string.IsNullOrWhiteSpace(insult))
                throw new InvalidCommandUsageException("Missing insult string.");

            if (insult.Length >= 120)
                throw new CommandFailedException("Too long insult. I know it is hard, but keep it shorter than 120 characters please.");

            if (insult.Split(new string[] { "%user%" }, StringSplitOptions.None).Count() < 2)
                throw new InvalidCommandUsageException($"Insult not in correct format (missing {Formatter.Bold("%user%")} in the insult)!");

            await Database.AddInsultAsync(insult)
                .ConfigureAwait(false);

            await ctx.InformSuccessAsync()
                .ConfigureAwait(false);
        }
        #endregion
        
        #region COMMAND_INSULTS_CLEAR
        [Command("clear"), Module(ModuleType.Miscellaneous)]
        [Description("Delete all insults.")]
        [Aliases("da", "c", "ca", "cl", "clearall")]
        [UsageExamples("!insults clear")]
        [RequireOwner]
        [UsesInteractivity]
        public async Task ClearAllInsultsAsync(CommandContext ctx)
        {
            if (!await ctx.WaitForBoolReplyAsync("Are you sure you want to delete all insults?").ConfigureAwait(false))
                return;

            await Database.RemoveAllInsultsAsync()
                .ConfigureAwait(false);
            await ctx.InformSuccessAsync("All insults successfully removed.")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_INSULTS_DELETE
        [Command("delete"), Module(ModuleType.Miscellaneous)]
        [Description("Remove insult with a given index from list. (use command ``insults list`` to view insult indexes).")]
        [Aliases("-", "remove", "del", "rm", "rem", "d")]
        [UsageExamples("!insult delete 2")]
        [RequireOwner]
        public async Task DeleteInsultAsync(CommandContext ctx, 
                                           [Description("Index of the insult to remove.")] int index)
        {
            await Database.RemoveInsultAsync(index)
                .ConfigureAwait(false);
            await ctx.InformSuccessAsync()
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_INSULTS_LIST
        [Command("list"), Module(ModuleType.Miscellaneous)]
        [Description("Show all insults.")]
        [Aliases("ls", "l")]
        [UsageExamples("!insult list")]
        public async Task ListInsultsAsync(CommandContext ctx)
        {
            var insults = await Database.GetAllInsultsAsync()
                .ConfigureAwait(false);

            if (insults == null || !insults.Any())
                throw new CommandFailedException("No insults registered.");

            await ctx.SendCollectionInPagesAsync(
                "Available insults",
                insults.Values,
                i => Formatter.Italic(i),
                DiscordColor.Green
            ).ConfigureAwait(false);
        }
        #endregion
    }
}
