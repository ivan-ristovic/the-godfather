using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using TheGodfather.Attributes;
using TheGodfather.Common;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Misc.Services;

namespace TheGodfather.Modules.Misc
{
    [Group("random"), Module(ModuleType.Misc), NotBlocked]
    [Description("Random gibberish.")]
    [Aliases("rnd", "rand")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public sealed class RandomModule : TheGodfatherServiceModule<RandomService>
    {
        #region random choose
        [Command("choice")]
        [Aliases("select", "choose")]
        public Task ChooseAsync(CommandContext ctx,
                               [RemainingText, Description(TranslationKey.desc_choice_list)] string list)
        {
            if (string.IsNullOrWhiteSpace(list))
                throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_choice);

            string choice = this.Service.Choice(list);
            return ctx.RespondAsync(embed: new DiscordEmbedBuilder {
                Description = $"{Emojis.Dice} {Formatter.Strip(choice)}",
                Color = this.ModuleColor,
            });
        }
        #endregion

        #region random raffle
        [Command("raffle")]
        [Aliases("chooseuser")]
        public Task RaffleAsync(CommandContext ctx,
                               [Description(TranslationKey.desc_role)] DiscordRole? role = null)
        {
            IEnumerable<DiscordMember> members = ctx.Guild.Members.Values;
            if (role is { })
                members = members.Where(m => m.Roles.Contains(role));

            if (!members.Any())
                throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_choice_none);

            DiscordMember raffled = new SecureRandom().ChooseRandomElement(members);
            return ctx.RespondWithLocalizedEmbedAsync(emb => {
                emb.WithColor(this.ModuleColor);
                emb.WithLocalizedDescription(TranslationKey.fmt_raffle(raffled.Mention));
            });
        }
        #endregion
    }
}
