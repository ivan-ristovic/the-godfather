using Discord.Commands;
using TheGodfather.Attributes;
using TheGodfather.Modules.Search.Common;
using TheGodfather.Modules.Search.Exceptions;
using TheGodfather.Modules.Search.Services;

namespace TheGodfather.Modules.Search;

[Group("reddit")]
[Alias("r")]
[Cooldown(3, 5, CooldownBucketType.Channel)]
public sealed class RedditModule : TheGodfatherModule
{
    private readonly RedditService reddit;
    private readonly RssFeedsService rss;


    public RedditModule(LocalizationService localization, RedditService reddit, RssFeedsService rss)
        : base(localization, new Color(0x1abc9c))
    {
        this.reddit = reddit;
        this.rss = rss;
    }


    [Command("")]
    public Task ExecuteGroupAsync(string sub = "all")
        => this.InternalSearchAsync(sub, RedditCategory.Hot);

    [Command("controversial")]
    [Alias("c")]
    public Task ControversialAsync(string sub)
        => this.InternalSearchAsync(sub, RedditCategory.Controversial);

    [Command("gilded")]
    [Alias("g")]
    public Task GildedAsync(string sub)
        => this.InternalSearchAsync(sub, RedditCategory.Gilded);

    [Command("hot")]
    [Alias("h")]
    public Task HotAsync(string sub)
        => this.InternalSearchAsync(sub, RedditCategory.Hot);

    [Command("new")]
    [Alias("n", "newest", "latest")]
    public Task NewAsync(string sub)
        => this.InternalSearchAsync(sub, RedditCategory.New);

    [Command("rising")]
    [Alias("r")]
    public Task RisingAsync(string sub)
        => this.InternalSearchAsync(sub, RedditCategory.Rising);

    [Command("top")]
    [Alias("t")]
    public Task TopAsync(string sub)
        => this.InternalSearchAsync(sub, RedditCategory.Top);

    [Command("subscribe")]
    [Priority(1)]
    [Alias("sub", "follow")]
    [RequireContext(ContextType.Guild)]
    [RequireUserPermission(GuildPermission.ManageGuild)]
    public Task SubscribeAsync(ITextChannel channel, string sub)
        => this.SubscribeInternalAsync(channel, sub);

    [Command("subscribe")]
    [Priority(0)]
    [Alias("sub", "follow")]
    [RequireContext(ContextType.Guild)]
    [RequireUserPermission(GuildPermission.ManageGuild)]
    public Task SubscribeAsync(string sub)
        => this.SubscribeInternalAsync((ITextChannel)this.Context.Channel, sub);

    [Command("unsubscribe")]
    [Alias("unfollow", "unsub")]
    [RequireContext(ContextType.Guild)]
    [RequireUserPermission(GuildPermission.ManageGuild)]
    public async Task UnsubscribeAsync(string sub)
    {
        if (string.IsNullOrWhiteSpace(sub))
            throw new LocalizedCommandException(TranslationKey.cmd_err_sub_none);

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

    private async Task SubscribeInternalAsync(ITextChannel channel, string sub)
    {
        if (string.IsNullOrWhiteSpace(sub))
            throw new LocalizedCommandException(TranslationKey.cmd_err_sub_none);

        string? url = this.reddit.GetFeedUrlForSubreddit(sub, RedditCategory.New, out string? normalizedSub);
        if (url is null || normalizedSub is null)
            throw new LocalizedCommandException(normalizedSub is null ? TranslationKey.cmd_err_sub_format : TranslationKey.cmd_err_sub_404(normalizedSub));

        if (await this.rss.SubscribeAsync(this.Context.Guild.Id, channel.Id, url, normalizedSub))
            await this.InfoAsync(this.ModuleColor);
        else
            await this.FailAsync(TranslationKey.cmd_err_sub(url));
    }

    private async Task<IReadOnlyList<RedditPost>> InternalRetrieveResultsAsync(string sub, RedditCategory category)
    {
        if (string.IsNullOrWhiteSpace(sub))
            throw new LocalizedCommandException(TranslationKey.cmd_err_sub_none);

        IEnumerable<RedditPost>? res;
        try {
            res = await this.reddit.GetPostsAsync(sub, category);
        } catch (SearchServiceException<RedditError> e) {
            if (e.Details.ErrorCode == 404)
                throw new LocalizedCommandException(TranslationKey.cmd_err_sub_404(sub));
            throw new LocalizedCommandException(TranslationKey.cmd_err_sub_fail_det(sub, e.Details.ErrorMessage));
        }

        if (res is null)
            throw new LocalizedCommandException(TranslationKey.cmd_err_sub_format);

        return res.ToList().AsReadOnly();
    }

    private async Task InternalSearchAsync(string sub, RedditCategory category)
    {
        IReadOnlyList<RedditPost> res = await this.InternalRetrieveResultsAsync(sub, category);

        if (!this.IsNsfwChannel() && res.Any(post => post.IsNsfw)) {
            await this.ImpInfoAsync(this.ModuleColor, Emojis.Information, TranslationKey.cmd_err_sfw_only);
            res = res.Where(post => !post.IsNsfw).ToList().AsReadOnly();
        }

        if (!res.Any()) {
            await this.FailAsync(TranslationKey.cmd_err_res_none);
            return;
        }

        await this.RespondPagedAsync(res, (emb, post) => this.AddRedditPost(emb, post));
    }

    private LocalizedEmbedBuilder AddRedditPost(LocalizedEmbedBuilder emb, RedditPost post)
    {
        string title = post.Title;
        if (post.IsLocked)
            title = $"[LOCKED] {title}";
        else if (post.IsArchived)
            title = $"[ARCHIVED] {title}";
        else if (post.IsNsfw)
            title = $"[NSFW] {title}";
        else if (post.IsPinned)
            title = $"[PINNED] {title}";
        else if (post.IsSpoiler)
            title = $"[SPOILER] {title}";

        emb.WithTitle(title);
        emb.WithDescription(Format.Sanitize(post.MarkdownText), false);

        if (string.Equals(post.PostType, "image", StringComparison.InvariantCultureIgnoreCase) && Uri.TryCreate(post.Url, UriKind.Absolute, out Uri? imageUri))
            emb.WithImageUrl(imageUri.ToString());
        else if (Uri.TryCreate(post.ThumbnailUrl, UriKind.Absolute, out Uri? thumbnailUri))
            emb.WithThumbnail(thumbnailUri.ToString());
        emb.WithUrl(post.Url);
        emb.AddLocalizedField(TranslationKey.str_type, post.PostType, true, false);
        emb.AddLocalizedField(TranslationKey.str_author, post.Author, true);
        emb.AddLocalizedField(TranslationKey.str_comments, post.CommentCount, true);
        emb.AddLocalizedField(TranslationKey.str_upvotes, post.UpvoteCount, true);
        emb.AddLocalizedField(TranslationKey.str_upvote_ratio, post.UpvoteRatio, true);
        if (post.AwardCount > 0)
            emb.AddLocalizedField(TranslationKey.str_awards, post.AwardCount, true);
        return emb;
    }
}
