#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

using Humanizer;
using System.Text;
using System.Threading.Tasks;

using TheGodfather.Common.Attributes;
using TheGodfather.Database;
using TheGodfather.Extensions;
using TheGodfather.Modules.Search.Common;
using TheGodfather.Modules.Search.Extensions;
using TheGodfather.Modules.Search.Services;
#endregion

namespace TheGodfather.Modules.Search
{
    [Group("urbandict"), Module(ModuleType.Searches), NotBlocked]
    [Description("Urban Dictionary commands. Group call searches Urban Dictionary for a given query.")]
    [Aliases("ud", "urban")]
    [UsageExamples("!urbandict blonde")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public class UrbanDictModule : TheGodfatherModule
    {

        public UrbanDictModule(SharedData shared, DatabaseContextBuilder db)
            : base(shared, db)
        {
            this.ModuleColor = DiscordColor.CornflowerBlue;
        }


        [GroupCommand]
        public async Task ExecuteGroupAsync(CommandContext ctx,
                                           [RemainingText, Description("Query.")] string query)
        {
            UrbanDictData data = await UrbanDictService.GetDefinitionForTermAsync(query);

            if (data is null) {
                await this.InformFailureAsync(ctx, "No results found!");
                return;
            }

            await ctx.SendCollectionInPagesAsync(
                $"Urban Dictionary search results for \"{query}\"",
                data.List,
                res => res.ToInfoString(),
                this.ModuleColor,
                1
            );
        }
    }
}