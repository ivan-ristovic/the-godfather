#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using TheGodfather.Services;
using TheGodfather.Exceptions;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Commands.Main
{
    [Group("insult", CanInvokeWithoutSubcommand = true)]
    [Description("Burns a user!")]
    [Aliases("burn", "insults")]
    [Cooldown(2, 3, CooldownBucketType.User), Cooldown(5, 3, CooldownBucketType.Channel)]
    [PreExecutionCheck]
    public class CommandsInsult
    {

        public async Task ExecuteGroupAsync(CommandContext ctx, 
                                           [Description("User.")] DiscordUser u = null)
        {
            if (u == null)
                u = ctx.User;

            if (u.Id == ctx.Client.CurrentUser.Id) {
                await ctx.RespondAsync("How original, trying to make me insult myself. Sadly it won't work.")
                    .ConfigureAwait(false);
                return;
            }

            string insult = await ctx.Dependencies.GetDependency<DatabaseService>().GetRandomInsultAsync()
                .ConfigureAwait(false);
            if (insult == null)
                throw new CommandFailedException("No available insults.");

            await ctx.RespondAsync(insult.Replace("%user%", u.Mention))
                .ConfigureAwait(false);
        }


        #region COMMAND_INSULTS_ADD
        [Command("add")]
        [Description("Add insult to list (Use % to code mention).")]
        [Aliases("+", "new")]
        [RequireOwner]
        public async Task AddInsultAsync(CommandContext ctx,
                                        [RemainingText, Description("Response.")] string insult)
        {
            if (string.IsNullOrWhiteSpace(insult))
                throw new InvalidCommandUsageException("Missing insult string.");

            if (insult.Length >= 120)
                throw new CommandFailedException("Too long insult. I know it is hard, but keep it shorter than 120 characters please.");

            if (insult.Split(new string[] { "%user%" }, StringSplitOptions.None).Count() < 2)
                throw new InvalidCommandUsageException($"Insult not in correct format (missing {Formatter.Bold("%user%")} in the insult)!");

            await ctx.Dependencies.GetDependency<DatabaseService>().AddInsultAsync(insult)
                .ConfigureAwait(false);

            await ctx.RespondAsync("Insult added.")
                .ConfigureAwait(false);
        }
        #endregion
        
        #region COMMAND_INSULTS_CLEAR
        [Command("clear")]
        [Description("Delete all insults.")]
        [Aliases("clearall")]
        [RequireOwner]
        public async Task ClearAllInsultsAsync(CommandContext ctx)
        {
            await ctx.Dependencies.GetDependency<DatabaseService>().DeleteAllInsultsAsync()
                .ConfigureAwait(false);
            await ctx.RespondAsync("All insults successfully removed.")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_INSULTS_DELETE
        [Command("delete")]
        [Description("Remove insult with a given index from list. (use ``!insults list`` to view indexes)")]
        [Aliases("-", "remove", "del", "rm")]
        [RequireOwner]
        public async Task DeleteInsultAsync(CommandContext ctx, 
                                           [Description("Index.")] int i)
        {
            await ctx.Dependencies.GetDependency<DatabaseService>().DeleteInsultByIdAsync(i)
                .ConfigureAwait(false);
            await ctx.RespondAsync("Insult successfully removed.").ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_INSULTS_LIST
        [Command("list")]
        [Description("Show all insults.")]
        public async Task ListInsultsAsync(CommandContext ctx,
                                          [Description("Page.")] int page = 1)
        {
            var insults = await ctx.Dependencies.GetDependency<DatabaseService>().GetAllInsultsAsync()
                .ConfigureAwait(false);

            if (insults == null || !insults.Any()) {
                await ctx.RespondAsync("No insults registered.")
                    .ConfigureAwait(false);
                return;
            }

            if (page < 1 || page > insults.Count / 20 + 1)
                throw new CommandFailedException("No insults on that page.");

            int starti = (page - 1) * 20;
            int len = starti + 20 < insults.Count ? 20 : insults.Count - starti;

            await ctx.RespondAsync(embed: new DiscordEmbedBuilder() {
                Title = $"Available insults (page {page}/{insults.Count / 20 + 1}) :",
                Description = string.Join("\n", insults.Select(kvp => $"{kvp.Key} : {kvp.Value}").ToList().GetRange(starti, len)),
                Color = DiscordColor.Green
            }.Build()).ConfigureAwait(false);
        }
        #endregion
    }
}
