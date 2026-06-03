using Discord.Commands;
using Google.Apis.YouTube.v3.Data;
using TheGodfather.Attributes;
using TheGodfather.Modules.Search.Services;

namespace TheGodfather.Modules.Search;

[Group("youtube")]
[Alias("y", "yt", "ytube")]
[Cooldown(3, 5, CooldownBucketType.Channel)]
public sealed class YoutubeModule : TheGodfatherModule
{
    private readonly YtService youtube;
    private readonly RssFeedsService rss;


    public YoutubeModule(LocalizationService localization, YtService youtube, RssFeedsService rss)
        : base(localization, Color.Red)
    {
        this.youtube = youtube;
        this.rss = rss;
    }


    [Command("")]
    public Task ExecuteGroupAsync([Remainder] string query)
        => this.SearchAndSendResultsAsync(5, query);

    [Command("search")]
    [Alias("s")]
    public Task AdvancedSearchAsync(int amount, [Remainder] string query)
        => this.SearchAndSendResultsAsync(amount, query);

    [Command("searchvideo")]
    [Alias("searchvideos", "sv", "searchv", "video")]
    public Task SearchVideoAsync([Remainder] string query)
        => this.SearchAndSendResultsAsync(5, query, "video");

    [Command("searchchannel")]
    [Alias("searchchannels", "sc", "searchc", "channel")]
    public Task SearchChannelAsync([Remainder] string query)
        => this.SearchAndSendResultsAsync(5, query, "channel");

    [Command("searchplaylist")]
    [Alias("searchplaylists", "sp", "searchp", "playlist")]
    public Task SearchPlaylistAsync([Remainder] string query)
        => this.SearchAndSendResultsAsync(5, query, "playlist");

    [Command("subscribe")]
    [Priority(1)]
    [Alias("sub", "follow")]
    [RequireContext(ContextType.Guild)]
    [RequireUserPermission(GuildPermission.ManageGuild)]
    public Task SubscribeAsync(ITextChannel channel, string usernameOrUrl, [Remainder] string? name = null)
        => this.SubscribeInternalAsync(channel, usernameOrUrl, name);

    [Command("subscribe")]
    [Priority(0)]
    [Alias("sub", "follow")]
    [RequireContext(ContextType.Guild)]
    [RequireUserPermission(GuildPermission.ManageGuild)]
    public Task SubscribeAsync(string usernameOrUrl, [Remainder] string? name = null)
        => this.SubscribeInternalAsync((ITextChannel)this.Context.Channel, usernameOrUrl, name);

    [Command("unsubscribe")]
    [Alias("unfollow", "unsub")]
    [RequireContext(ContextType.Guild)]
    [RequireUserPermission(GuildPermission.ManageGuild)]
    public async Task UnsubscribeAsync([Remainder] string subscription)
    {
        string? url = await this.youtube.GetRssUrlForChannel(subscription);
        if (url is null)
            throw new LocalizedCommandException(TranslationKey.cmd_err_sub_yt);

        RssSubscription? sub = await this.rss.Subscriptions.GetByNameAsync((this.Context.Guild.Id, this.Context.Channel.Id), subscription);
        if (sub is null) {
            RssFeed? feed = await this.rss.GetByUrlAsync(url);
            if (feed is null)
                throw new LocalizedCommandException(TranslationKey.cmd_err_sub_fail(subscription));
            sub = await this.rss.Subscriptions.GetAsync((this.Context.Guild.Id, this.Context.Channel.Id), feed.Id);
        }

        if (sub is null)
            throw new LocalizedCommandException(TranslationKey.cmd_err_sub_fail(subscription));

        await this.rss.Subscriptions.RemoveAsync(sub);
        await this.InfoAsync(this.ModuleColor);
    }

    private async Task SubscribeInternalAsync(ITextChannel channel, string usernameOrUrl, string? name)
    {
        string? feed = await this.youtube.GetRssUrlForChannel(usernameOrUrl);
        if (feed is null)
            throw new LocalizedCommandException(TranslationKey.cmd_err_sub_yt);

        string subscriptionName = string.IsNullOrWhiteSpace(name) ? usernameOrUrl : name;
        if (!await this.rss.SubscribeAsync(this.Context.Guild.Id, channel.Id, feed, subscriptionName))
            throw new LocalizedCommandException(TranslationKey.cmd_err_sub_yt);

        await this.InfoAsync(this.ModuleColor);
    }

    private async Task SearchAndSendResultsAsync(int amount, string query, string? type = null)
    {
        if (string.IsNullOrWhiteSpace(query))
            throw new LocalizedCommandException(TranslationKey.cmd_err_query);

        IReadOnlyList<Google.Apis.YouTube.v3.Data.SearchResult>? res = await this.youtube.SearchAsync(query, amount, type);
        if (res is null) {
            await this.FailAsync(TranslationKey.cmd_err_res_none);
            return;
        }

        await this.RespondPagedAsync(res, (emb, result) => {
            emb.WithTitle(result.Snippet.Title);
            emb.WithColor(Color.Red);
            emb.WithDescription(result.Snippet.Description, false);
            if (result.Snippet.Thumbnails is not null)
                emb.WithThumbnail(result.Snippet.Thumbnails.Default__.Url);
            emb.AddLocalizedField(TranslationKey.str_chn, result.Snippet.ChannelTitle, true);
            emb.AddLocalizedField(TranslationKey.str_published, result.Snippet.PublishedAtDateTimeOffset, true);

            string? url = this.youtube.GetUrlForResourceId(result.Id);
            if (url is not null)
                emb.WithUrl(url);
            return emb;
        });
    }
}
