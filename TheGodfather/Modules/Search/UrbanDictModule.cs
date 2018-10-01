#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

using Humanizer;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TheGodfather.Common.Attributes;
using TheGodfather.Extensions;
using TheGodfather.Modules.Search.Common;
using TheGodfather.Modules.Search.Services;
using TheGodfather.Services;
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

        public UrbanDictModule(SharedData shared, DBService db)
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
                res => {
                    var sb = new StringBuilder("Definition by ");
                    sb.Append(Formatter.Bold(res.Author)).AppendLine().AppendLine();
                    sb.Append(Formatter.Bold(res.Word)).Append(" :");
                    sb.AppendLine(Formatter.BlockCode(res.Definition.Trim().Truncate(1000)));
                    if (!string.IsNullOrWhiteSpace(res.Example))
                        sb.Append("Examples:").AppendLine(Formatter.BlockCode(res.Example.Trim().Truncate(250)));
                    sb.Append(res.Permalink);
                    return sb.ToString();
                },
                this.ModuleColor,
                1
            );
        }
    }
}