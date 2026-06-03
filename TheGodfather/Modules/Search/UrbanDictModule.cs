using System.Text;
using Discord.Commands;
using TheGodfather.Modules.Search.Common;
using TheGodfather.Modules.Search.Services;

namespace TheGodfather.Modules.Search;

[Group("urbandict")]
[Alias("ud", "urban", "urbandictionary")]
public sealed class UrbanDictModule : TheGodfatherModule
{
    public UrbanDictModule(LocalizationService localization)
        : base(localization, new Color(0x1abc9c))
    {
    }


    [Command("")]
    public async Task ExecuteGroupAsync([Remainder] string query)
    {
        UrbanDictData? data = await UrbanDictService.GetDefinitionForTermAsync(query);
        UrbanDictList? result = data?.List.FirstOrDefault();
        if (result is null) {
            await this.FailAsync(TranslationKey.cmd_err_res_none);
            return;
        }

        var sb = new StringBuilder(this.Localization.GetString(this.Context.Guild?.Id, TranslationKey.str_def_by));
        sb.Append(Format.Bold(result.Author)).AppendLine().AppendLine();
        sb.Append(Format.Bold(result.Word)).Append(" :");
        sb.AppendLine(Format.Code(result.Definition.Trim().Truncate(1000)));
        if (!string.IsNullOrWhiteSpace(result.Example))
            sb.Append(this.Localization.GetString(this.Context.Guild?.Id, TranslationKey.str_examples))
              .AppendLine(Format.Code(result.Example.Trim().Truncate(250)));
        sb.Append(result.Permalink);

        await this.ReplyAsync(embed: new EmbedBuilder {
            Color = this.ModuleColor,
            Description = sb.ToString().Truncate(DiscordLimits.EmbedDescriptionLimit - 3, "...")
        }.Build());
    }
}
