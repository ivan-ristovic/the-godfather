using System.ServiceModel.Syndication;
using Discord.Commands;
using TheGodfather.Attributes;
using TheGodfather.Modules.Search.Common;
using TheGodfather.Modules.Search.Services;

namespace TheGodfather.Modules.Search;

[Cooldown(3, 5, CooldownBucketType.Channel)]
public sealed class RssFeedsModule : TheGodfatherModule
{
    private readonly RssFeedsService rss;


    public RssFeedsModule(LocalizationService localization, RssFeedsService rss)
        : base(localization, new Color(0x1abc9c))
    {
        this.rss = rss;
    }


    [Command("rss")]
    [Alias("feed")]
    public Task RssAsync(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? uri) || !RssFeedsService.IsValidFeedURL(uri.AbsoluteUri))
            throw new LocalizedCommandException(TranslationKey.cmd_err_rss);

        IReadOnlyList<SyndicationItem>? res = RssFeedsService.GetFeedResults(uri.AbsoluteUri);
        if (res is null)
            throw new LocalizedCommandException(TranslationKey.cmd_err_sub_fail(uri));

        return this.RespondPagedAsync(res, (emb, item) => {
            emb.WithTitle(item.Title.Text);
            emb.WithDescription(item.Summary?.Text, false);
            if (item.Links.Any())
                emb.WithUrl(item.Links.First().Uri.ToString());
            if (item.Authors.Any())
                emb.AddLocalizedField(TranslationKey.str_author, item.Authors.First().Name, true, false);
            emb.WithLocalizedTimestamp(item.LastUpdatedTime);
            return emb;
        });
    }
}

[Group("subscribe")]
[Alias("sub", "subscriptions", "subscription")]
[RequireContext(ContextType.Guild)]
[RequireUserPermission(GuildPermission.ManageGuild)]
public sealed class SubscribeModule : TheGodfatherModule
{
    private readonly RssFeedsService rss;
    private readonly RedditService reddit;
    private readonly YtService youtube;


    public SubscribeModule(LocalizationService localization, RssFeedsService rss, RedditService reddit, YtService youtube)
        : base(localization, new Color(0x1abc9c))
    {
        this.rss = rss;
        this.reddit = reddit;
        this.youtube = youtube;
    }


    [Command("")]
    [Priority(2)]
    public Task ExecuteGroupAsync(ITextChannel channel, string url, [Remainder] string? name = null)
        => this.SubscribeRssAsync(channel, url, name);

    [Command("")]
    [Priority(1)]
    public Task ExecuteGroupAsync(string url, [Remainder] string? name = null)
        => this.SubscribeRssAsync((ITextChannel)this.Context.Channel, url, name);

    [Command("")]
    [Priority(0)]
    public Task ExecuteGroupAsync()
        => this.ListAsync();

    [Command("list")]
    [Alias("ls", "listsubs", "listfeeds")]
    public async Task ListAsync(ITextChannel? channel = null)
    {
        channel ??= (ITextChannel)this.Context.Channel;

        IReadOnlyList<RssSubscription> subs = await this.rss.Subscriptions.GetAllAsync((this.Context.Guild.Id, channel.Id));
        if (!subs.Any())
            throw new LocalizedCommandException(TranslationKey.cmd_err_subs_none(channel.Name));

        await this.RespondPagedAsync(
            TranslationKey.str_subs(channel.Mention),
            subs,
            sub => $"{InlineCode($"{sub.Id:D4}")} | {(string.IsNullOrWhiteSpace(sub.Name) ? sub.Feed.Url : sub.Name)}",
            5
        );
    }

    [Command("reddit")]
    [Alias("r")]
    public async Task RedditAsync(string sub, ITextChannel? channel = null)
    {
        channel ??= (ITextChannel)this.Context.Channel;
        string? url = this.reddit.GetFeedUrlForSubreddit(sub, RedditCategory.New, out string? normalizedSub);
        if (url is null || normalizedSub is null)
            throw new LocalizedCommandException(normalizedSub is null ? TranslationKey.cmd_err_sub_format : TranslationKey.cmd_err_sub_404(normalizedSub));

        if (await this.rss.SubscribeAsync(this.Context.Guild.Id, channel.Id, url, normalizedSub))
            await this.InfoAsync(this.ModuleColor);
        else
            await this.FailAsync(TranslationKey.cmd_err_sub(url));
    }

    [Command("youtube")]
    [Alias("y", "yt", "ytube")]
    public async Task YoutubeAsync(string usernameOrUrl, [Remainder] string? name = null)
    {
        string? feed = await this.youtube.GetRssUrlForChannel(usernameOrUrl);
        if (feed is null)
            throw new LocalizedCommandException(TranslationKey.cmd_err_sub_yt);

        string subscriptionName = string.IsNullOrWhiteSpace(name) ? usernameOrUrl : name;
        if (!await this.rss.SubscribeAsync(this.Context.Guild.Id, this.Context.Channel.Id, feed, subscriptionName))
            throw new LocalizedCommandException(TranslationKey.cmd_err_sub_yt);

        await this.InfoAsync(this.ModuleColor);
    }

    private async Task SubscribeRssAsync(ITextChannel channel, string url, string? name)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? uri) || !RssFeedsService.IsValidFeedURL(uri.AbsoluteUri))
            throw new LocalizedCommandException(TranslationKey.cmd_err_rss);

        await this.rss.SubscribeAsync(this.Context.Guild.Id, channel.Id, uri.AbsoluteUri, name);
        await this.InfoAsync(this.ModuleColor);
    }
}

[Group("unsubscribe")]
[Alias("unsub")]
[RequireContext(ContextType.Guild)]
[RequireUserPermission(GuildPermission.ManageGuild)]
public sealed class UnsubscribeModule : TheGodfatherModule
{
    private readonly RssFeedsService rss;
    private readonly RedditService reddit;
    private readonly YtService youtube;


    public UnsubscribeModule(LocalizationService localization, RssFeedsService rss, RedditService reddit, YtService youtube)
        : base(localization, new Color(0x1abc9c))
    {
        this.rss = rss;
        this.reddit = reddit;
        this.youtube = youtube;
    }


    [Command("")]
    [Priority(1)]
    public async Task ExecuteGroupAsync(params int[] ids)
    {
        if (ids is null || !ids.Any())
            throw new LocalizedCommandException(TranslationKey.cmd_err_ids_none);

        int removed = await this.rss.Subscriptions.RemoveAsync((this.Context.Guild.Id, this.Context.Channel.Id), ids);
        await this.InfoAsync(this.ModuleColor, TranslationKey.fmt_unsub(removed));
    }

    [Command("")]
    [Priority(0)]
    public async Task ExecuteGroupAsync([Remainder] string name)
    {
        RssSubscription? sub = await this.rss.Subscriptions.GetByNameAsync((this.Context.Guild.Id, this.Context.Channel.Id), name);
        if (sub is null)
            throw new LocalizedCommandException(TranslationKey.cmd_err_sub_name(this.Context.Channel.Name));

        int removed = await this.rss.Subscriptions.RemoveAsync(sub);
        await this.InfoAsync(this.ModuleColor, TranslationKey.fmt_unsub(removed));
    }

    [Command("all")]
    [Alias("a")]
    public async Task AllAsync(ITextChannel? channel = null)
    {
        channel ??= (ITextChannel)this.Context.Channel;
        if (!await this.ConfirmAsync(TranslationKey.q_unsub(channel.Mention)))
            return;

        await this.rss.Subscriptions.ClearAsync((this.Context.Guild.Id, channel.Id));
        await this.InfoAsync(this.ModuleColor);
    }

    [Command("reddit")]
    [Alias("r")]
    public async Task RedditAsync(string sub)
    {
        string? url = this.reddit.GetFeedUrlForSubreddit(sub, RedditCategory.New, out string? normalizedSub);
        if (url is null || normalizedSub is null)
            throw new LocalizedCommandException(normalizedSub is null ? TranslationKey.cmd_err_sub_format : TranslationKey.cmd_err_sub_404(normalizedSub));

        RssFeed? feed = await this.rss.GetByUrlAsync(url);
        if (feed is null)
            throw new LocalizedCommandException(TranslationKey.cmd_err_sub_not(normalizedSub));

        RssSubscription? subscription = await this.rss.Subscriptions.GetAsync((this.Context.Guild.Id, this.Context.Channel.Id), feed.Id);
        if (subscription is null)
            throw new LocalizedCommandException(TranslationKey.cmd_err_sub_not(normalizedSub));

        await this.rss.Subscriptions.RemoveAsync((this.Context.Guild.Id, this.Context.Channel.Id), feed.Id);
        await this.InfoAsync(this.ModuleColor);
    }

    [Command("youtube")]
    [Alias("y", "yt", "ytube")]
    public async Task YoutubeAsync([Remainder] string subscription)
    {
        if (string.IsNullOrWhiteSpace(subscription))
            throw new LocalizedCommandException(TranslationKey.cmd_err_name_404);

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
}
