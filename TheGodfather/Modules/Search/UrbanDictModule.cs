using System.Text;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using TheGodfather.Modules.Search.Common;
using TheGodfather.Modules.Search.Services;

namespace TheGodfather.Modules.Search;

[Group("urbandict")][Module(ModuleType.Searches)][NotBlocked]
[Aliases("ud", "urban", "urbandictionary")]
[Cooldown(3, 5, CooldownBucketType.Channel)]
public class UrbanDictModule : TheGodfatherModule
{
    #region urbandict
    [GroupCommand]
    public async Task ExecuteGroupAsync(CommandContext ctx,
        [RemainingText][Description(TranslationKey.desc_query)] string query)
    {
        UrbanDictData? data = await UrbanDictService.GetDefinitionForTermAsync(query);
        if (data is null) {
            await ctx.FailAsync(TranslationKey.cmd_err_res_none);
            return;
        }

        await ctx.PaginateAsync(
            TranslationKey.fmt_ud(query),
            data.List,
            res => {
                var sb = new StringBuilder(this.Localization.GetString(ctx.Guild?.Id, TranslationKey.str_def_by));
                sb.Append(Formatter.Bold(res.Author)).AppendLine().AppendLine();
                sb.Append(Formatter.Bold(res.Word)).Append(" :");
                sb.AppendLine(Formatter.BlockCode(res.Definition.Trim().Truncate(1000)));
                if (!string.IsNullOrWhiteSpace(res.Example))
                    sb.Append(this.Localization.GetString(ctx.Guild?.Id, TranslationKey.str_examples)).AppendLine(Formatter.BlockCode(res.Example.Trim().Truncate(250)));
                sb.Append(res.Permalink);
                return sb.ToString();
            },
            this.ModuleColor,
            1
        );
    }
    #endregion
}