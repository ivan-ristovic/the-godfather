#region USING_DIRECTIVES
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

using TheGodfather.Helpers.DataManagers;
using TheGodfather.Exceptions;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Commands.Messages
{
    [Group("insult", CanInvokeWithoutSubcommand = true)]
    [Description("Burns a user!")]
    [Aliases("burn", "insults")]
    [Cooldown(2, 3, CooldownBucketType.User), Cooldown(5, 3, CooldownBucketType.Channel)]
    public class CommandsInsult
    {

        public async Task ExecuteGroupAsync(CommandContext ctx, 
                                           [Description("User.")] DiscordUser u = null)
        {
            if (u == null)
                u = ctx.User;

            if (u.Id == ctx.Client.CurrentUser.Id) {
                await ctx.RespondAsync("How original, trying to make me insult myself. Sadly it won't work.");
                return;
            }

            string insult = ctx.Dependencies.GetDependency<InsultManager>().GetRandomInsult();
            if (insult == null)
                throw new CommandFailedException("No available insults.");

            await ctx.RespondAsync(insult.Replace("%user%", u.Mention));
        }


        #region COMMAND_INSULTS_ADD
        [Command("add")]
        [Description("Add insult to list (Use % to code mention).")]
        [Aliases("+", "new")]
        public async Task AddInsult(CommandContext ctx,
                                   [RemainingText, Description("Response.")] string insult = null)
        {
            if (string.IsNullOrWhiteSpace(insult))
                throw new InvalidCommandUsageException("Missing insult string.");

            if (insult.Length >= 200)
                throw new CommandFailedException("Too long insult. I know it is hard, but keep it shorter than 200 please.");

            if (insult.Split(new string[] { "%user%" }, StringSplitOptions.None).Count() < 2)
                throw new InvalidCommandUsageException($"Insult not in correct format (missing {Formatter.Bold("%user%")})!");

            ctx.Dependencies.GetDependency<InsultManager>().Add(insult);

            await ctx.RespondAsync("Insult added.");
        }
        #endregion
        
        #region COMMAND_INSULTS_CLEAR
        [Command("clear")]
        [Description("Delete all insults.")]
        [Aliases("clearall")]
        [RequireOwner]
        public async Task ClearAllInsults(CommandContext ctx)
        {
            ctx.Dependencies.GetDependency<InsultManager>().ClearInsults();
            await ctx.RespondAsync("All insults successfully removed.");
        }
        #endregion

        #region COMMAND_INSULTS_DELETE
        [Command("delete")]
        [Description("Remove insult with a given index from list. (use ``!insults list`` to view indexes)")]
        [Aliases("-", "remove", "del")]
        [RequireOwner]
        public async Task DeleteInsult(CommandContext ctx, 
                                      [Description("Index.")] int i = 0)
        {
            if (ctx.Dependencies.GetDependency<InsultManager>().RemoveAt(i))
                await ctx.RespondAsync("Insult successfully removed.");
            else
                throw new CommandFailedException("No insults at such index.");
        }
        #endregion

        #region COMMAND_INSULTS_LIST
        [Command("list")]
        [Description("Show all insults.")]
        public async Task ListInsults(CommandContext ctx,
                                     [Description("Page.")] int page = 1)
        {
            var insults = ctx.Dependencies.GetDependency<InsultManager>().Insults;

            if (page < 1 || page > insults.Count / 10 + 1)
                throw new CommandFailedException("No insults on that page.", new ArgumentOutOfRangeException());

            string s = "";
            int starti = (page - 1) * 10;
            int endi = starti + 10 < insults.Count ? starti + 10 : insults.Count;
            for (int i = starti; i < endi; i++)
                s += $"{Formatter.Bold(i.ToString())} : {insults[i]}\n";

            await ctx.RespondAsync(embed: new DiscordEmbedBuilder() {
                Title = $"Available insults (page {page}/{insults.Count / 10 + 1}) :",
                Description = s,
                Color = DiscordColor.Turquoise
            });
        }
        #endregion

        #region COMMAND_INSULTS_SAVE
        [Command("save")]
        [Description("Save insults to file.")]
        [RequireOwner]
        public async Task SaveInsults(CommandContext ctx)
        {
            if (ctx.Dependencies.GetDependency<InsultManager>().Save(ctx.Client.DebugLogger))
                await ctx.RespondAsync("Insults successfully saved.");
            else
                throw new CommandFailedException("Failed saving insults.", new IOException());
        }
        #endregion
    }
}
