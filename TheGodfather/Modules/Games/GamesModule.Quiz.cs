#region USING_DIRECTIVES
using System.Threading.Tasks;

using TheGodfather.Attributes;
using TheGodfather.Extensions;
using TheGodfather.Services;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
#endregion

namespace TheGodfather.Modules.Games
{
    public partial class GamesModule
    {
        [Group("quiz")]
        [Description("List all available quiz categories.")]
        [Aliases("trivia", "q")]
        [UsageExample("!game quiz ")]
        [Cooldown(2, 5, CooldownBucketType.User), Cooldown(3, 5, CooldownBucketType.Channel)]
        [ListeningCheck]
        public partial class QuizModule : TheGodfatherBaseModule
        {

            public QuizModule(DBService db) : base(db: db) { }


            [GroupCommand]
            public async Task ExecuteGroupAsync(CommandContext ctx)
            {
                await ctx.RespondWithIconEmbedAsync(
                    "Available quiz categories:\n" +
                    $"- {Formatter.Bold("countries")}",
                    ":information_source:"
                ).ConfigureAwait(false);
            }
        }
    }
}
