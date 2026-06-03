using Discord.Commands;
using TheGodfather.Attributes;
using TheGodfather.Modules.Misc.Services;

namespace TheGodfather.Modules.Misc;

[Group("starboard")]
[Alias("star", "stars")]
[RequireContext(ContextType.Guild)]
[RequireUserPermission(GuildPermission.ManageGuild)]
[Cooldown(3, 5, CooldownBucketType.Guild)]
public sealed class StarboardModule : TheGodfatherModule
{
    private readonly StarboardService starboard;


    public StarboardModule(LocalizationService localization, StarboardService starboard)
        : base(localization, new Color(0x007FFF))
    {
        this.starboard = starboard;
    }


    [Command("")]
    [Priority(2)]
    public Task ExecuteGroupAsync(bool enable, ITextChannel channel, string? emoji = null, int? sensitivity = null)
        => this.InternalStarboardAsync(enable, channel, emoji, sensitivity);

    [Command("")]
    [Priority(0)]
    public async Task ExecuteGroupAsync()
    {
        ITextChannel? starChannel = null;
        string? starEmoji = null;
        int? totalMessages = null;
        int? totalStars = null;
        bool enabled = this.starboard.IsStarboardEnabled(this.Context.Guild.Id, out ulong cid, out string emoji);
        if (enabled) {
            starChannel = this.Context.Guild.GetTextChannel(cid);
            starEmoji = emoji;
            IReadOnlyList<StarboardMessage> all = await this.starboard.GetAllAsync(this.Context.Guild.Id);
            totalMessages = all.Count;
            totalStars = all.Sum(sm => sm.Stars);
        }

        await this.RespondWithLocalizedEmbedAsync(emb => {
            emb.WithColor(this.ModuleColor);
            emb.WithLocalizedTitle(TranslationKey.str_starboard);
            emb.AddLocalizedField(TranslationKey.str_status, enabled ? TranslationKey.str_enabled : TranslationKey.str_disabled, true);
            emb.AddLocalizedField(TranslationKey.str_chn, starChannel?.Mention, true, false);
            emb.AddLocalizedField(TranslationKey.str_star, starEmoji, true, false);
            emb.AddLocalizedField(TranslationKey.str_sensitivity, this.starboard.GetStarboardSensitivity(this.Context.Guild.Id), true);
            emb.AddLocalizedField(TranslationKey.str_starmsgs, totalMessages, true, false);
            emb.AddLocalizedField(TranslationKey.str_stars_total, totalStars, true, false);
        });
    }

    [Command("channel")]
    [Alias("chn", "setchannel", "setchn", "setc", "location")]
    public async Task ChannelAsync(ITextChannel? channel = null)
    {
        if (channel is null) {
            ulong cid = this.starboard.GetStarboardChannel(this.Context.Guild.Id);
            SocketTextChannel? current = this.Context.Guild.GetTextChannel(cid);
            if (current is null)
                throw new LocalizedCommandException(TranslationKey.cmd_err_sb_chn(cid));
            await this.ImpInfoAsync(this.ModuleColor, Emojis.Star, TranslationKey.evt_sb_chn(current.Mention));
            return;
        }

        await this.starboard.SetStarboardChannelAsync(this.Context.Guild.Id, channel.Id);
        await this.InfoAsync(this.ModuleColor, TranslationKey.evt_sb_chn(channel.Mention));
    }

    [Command("sensitivity")]
    [Alias("setsensitivity", "setsens", "sens", "s")]
    public async Task SensitivityAsync(int? sensitivity = null)
    {
        if (sensitivity is null) {
            int current = this.starboard.GetStarboardSensitivity(this.Context.Guild.Id);
            await this.ImpInfoAsync(this.ModuleColor, Emojis.Star, TranslationKey.evt_sb_sens(current));
            return;
        }

        if (sensitivity < 1)
            throw new LocalizedCommandException(TranslationKey.cmd_err_range_sens_g(0));

        await this.starboard.SetStarboardSensitivityAsync(this.Context.Guild.Id, sensitivity.Value);
        await this.InfoAsync(this.ModuleColor, TranslationKey.evt_sb_sens(sensitivity.Value));
    }

    [Command("emoji")]
    [Alias("e", "star")]
    public async Task EmojiAsync(string? emoji = null)
    {
        if (emoji is null) {
            string current = this.starboard.GetStarboardEmoji(this.Context.Guild.Id);
            await this.ImpInfoAsync(this.ModuleColor, Emojis.Star, TranslationKey.evt_sb_emoji(current));
            return;
        }

        await this.starboard.SetStarboardEmojiAsync(this.Context.Guild.Id, emoji);
        await this.InfoAsync(this.ModuleColor, TranslationKey.evt_sb_emoji(emoji));
    }

    private async Task InternalStarboardAsync(bool enable, ITextChannel channel, string? emoji = null, int? sensitivity = null)
    {
        if (enable)
            await this.starboard.ModifySettingsAsync(this.Context.Guild.Id, channel.Id, emoji, sensitivity);
        else
            await this.starboard.ModifySettingsAsync(this.Context.Guild.Id, null);

        await this.ExecuteGroupAsync();
    }
}
