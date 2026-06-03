using Discord.Commands;
using TheGodfather.Attributes;
using TheGodfather.Modules.Misc.Services;

namespace TheGodfather.Modules.Misc;

[Group("rank")]
[Alias("ranks", "ranking", "level", "xp")]
[Cooldown(3, 5, CooldownBucketType.Channel)]
public sealed class RanksModule : TheGodfatherModule
{
    private readonly GuildRanksService guildRanks;
    private readonly UserRanksService userRanks;


    public RanksModule(LocalizationService localization, GuildRanksService guildRanks, UserRanksService userRanks)
        : base(localization, new Color(0x007FFF))
    {
        this.guildRanks = guildRanks;
        this.userRanks = userRanks;
    }


    [Command("")]
    public Task ExecuteGroupAsync(IUser? user = null)
    {
        user ??= this.Context.User;
        return this.RespondWithLocalizedEmbedAsync(async emb => {
            emb.WithColor(this.ModuleColor);
            emb.WithTitle(UserDiscriminatorString(user));
            emb.WithThumbnail(user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl());
            emb.AddLocalizedField(TranslationKey.str_xp, this.userRanks.GetUserXp(this.Context.Guild?.Id, user.Id), true);
            if (this.Context.Guild is not null) {
                short rank = this.userRanks.CalculateRankForUser(this.Context.Guild.Id, user.Id);
                XpRank? rankInfo = await this.guildRanks.GetAsync(this.Context.Guild.Id, rank);
                emb.AddLocalizedField(TranslationKey.str_rank, rank, true);
                emb.AddLocalizedField(TranslationKey.str_xp_next, UserRanksService.CalculateXpNeededForRank((short)(rank + 1)), true);
                if (rankInfo is not null)
                    emb.AddLocalizedField(TranslationKey.str_rank_name, Format.Italics(rankInfo.Name), true);
                else
                    emb.AddLocalizedField(TranslationKey.str_rank, TranslationKey.str_rank_noname, true);
            }
        });
    }

    [Command("add")]
    [Alias("register", "rename", "mv", "newname", "reg", "a", "+", "+=", "<<", "<", "<-", "<=")]
    [RequireContext(ContextType.Guild)]
    [RequireUserPermission(GuildPermission.ManageGuild)]
    public async Task AddAsync(short rank, [Remainder] string name)
    {
        if (rank is < 0 or > 150)
            throw new LocalizedCommandException(TranslationKey.cmd_err_rank(0, 150));

        if (string.IsNullOrWhiteSpace(name))
            throw new LocalizedCommandException(TranslationKey.cmd_err_name_404);

        if (name.Length > XpRank.NameLimit)
            throw new LocalizedCommandException(TranslationKey.cmd_err_name(XpRank.NameLimit));

        await this.guildRanks.RemoveAsync(this.Context.Guild.Id, rank);
        await this.guildRanks.AddAsync(new XpRank {
            GuildId = this.Context.Guild.Id,
            Name = name,
            Rank = rank
        });

        await this.InfoAsync(this.ModuleColor);
    }

    [Command("delete")]
    [Alias("unregister", "remove", "rm", "del", "d", "-", "-=", ">", ">>", "->", "=>")]
    [RequireContext(ContextType.Guild)]
    [RequireUserPermission(GuildPermission.ManageGuild)]
    public async Task DeleteAsync(short rank)
    {
        await this.guildRanks.RemoveAsync(this.Context.Guild.Id, rank);
        await this.InfoAsync(this.ModuleColor);
    }

    [Command("list")]
    [Alias("print", "show", "view", "ls", "l", "p")]
    [RequireContext(ContextType.Guild)]
    public async Task RankListAsync()
    {
        IReadOnlyList<XpRank> ranks = await this.guildRanks.GetAllAsync(this.Context.Guild.Id);
        if (!ranks.Any())
            throw new LocalizedCommandException(TranslationKey.cmd_err_rank_none);

        await this.RespondPagedAsync(
            TranslationKey.str_ranks,
            ranks.OrderBy(r => r.Rank),
            rank => $"{InlineCode($"{rank.Rank:D2}")} | XP: {InlineCode($"{UserRanksService.CalculateXpNeededForRank(rank.Rank):D5}")} | {Bold(rank.Name)}"
        );
    }

    [Command("top")]
    [RequireContext(ContextType.Guild)]
    public Task TopAsync()
        => this.InternalTopAsync(false);

    [Command("topglobal")]
    [Alias("bestglobally", "globallystrongest", "globaltop", "topg", "gtop", "globalbest", "bestglobal")]
    [RequireContext(ContextType.Guild)]
    public Task TopGlobalAsync()
        => this.InternalTopAsync(true);

    private async Task InternalTopAsync(bool global)
    {
        var embed = new LocalizedEmbedBuilder(this.Localization, this.Context.Guild.Id)
            .WithLocalizedTitle(global ? TranslationKey.str_rank_topg : TranslationKey.str_rank_top)
            .WithColor(this.ModuleColor);
        string unknown = this.Localization.GetString(this.Context.Guild.Id, TranslationKey.str_404);

        IReadOnlyList<XpCount> top = await this.userRanks.GetTopRankedUsersAsync(global ? null : this.Context.Guild.Id);
        var ranks = new Dictionary<short, string>();
        var notFoundUsers = new List<ulong>();
        foreach (XpCount xpc in top) {
            IUser? user = this.Context.Client.GetUser(xpc.UserId);
            if (user is null)
                notFoundUsers.Add(xpc.UserId);

            short rank = UserRanksService.CalculateRankForXp(xpc.Xp);
            if (!ranks.ContainsKey(rank)) {
                XpRank? gr = await this.guildRanks.GetAsync(this.Context.Guild.Id, rank);
                if (gr is not null)
                    ranks.Add(rank, gr.Name);
            }

            string title = user is null ? unknown : UserDiscriminatorString(user);
            if (ranks.TryGetValue(rank, out string? name))
                embed.AddField(title, $"{name} ({rank}) ({xpc.Xp} XP)");
            else
                embed.AddField(title, $"LVL {rank} ({xpc.Xp} XP)");
        }

        await this.ReplyAsync(embed: embed.Build());
        await this.userRanks.RemoveDeletedUsers(notFoundUsers);
    }
}
