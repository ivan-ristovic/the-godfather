using Discord.Commands;
using TheGodfather.Attributes;
using TheGodfather.Modules.Chickens.Common;
using TheGodfather.Modules.Chickens.Services;
using TheGodfather.Modules.Currency.Services;

namespace TheGodfather.Modules.Chickens;

[Group("chicken")]
[Alias("chickens", "cock", "hen", "chick", "coc", "cc")]
[RequireContext(ContextType.Guild)]
[Cooldown(3, 5, CooldownBucketType.Channel)]
public sealed class ChickenModule : TheGodfatherModule
{
    private readonly ChickenService chickens;
    private readonly ChickenUpgradeService upgrades;
    private readonly ChickenBoughtUpgradeService boughtUpgrades;
    private readonly BankAccountService bank;
    private readonly GuildConfigService guildConfig;


    public ChickenModule(
        LocalizationService localization,
        ChickenService chickens,
        ChickenUpgradeService upgrades,
        ChickenBoughtUpgradeService boughtUpgrades,
        BankAccountService bank,
        GuildConfigService guildConfig)
        : base(localization, new Color(0xE67E22))
    {
        this.chickens = chickens;
        this.upgrades = upgrades;
        this.boughtUpgrades = boughtUpgrades;
        this.bank = bank;
        this.guildConfig = guildConfig;
    }


    [Command("")]
    [Priority(1)]
    public Task ExecuteGroupAsync(IGuildUser? member = null)
        => this.InfoAsync(member);

    [Command("")]
    [Priority(0)]
    public Task ExecuteGroupAsync([Remainder] string chickenName)
        => this.InfoByNameAsync(chickenName);

    [Command("buy")]
    [Alias("buy default", "b", "shop")]
    public Task BuyDefaultAsync([Remainder] string name)
        => this.BuyInternalAsync(ChickenType.Default, name);

    [Command("buy wellfed")]
    [Alias("buy wf", "buy fed")]
    public Task BuyWellFedAsync([Remainder] string name)
        => this.BuyInternalAsync(ChickenType.WellFed, name);

    [Command("buy trained")]
    [Alias("buy tr", "buy train")]
    public Task BuyTrainedAsync([Remainder] string name)
        => this.BuyInternalAsync(ChickenType.Trained, name);

    [Command("buy steroidempowered")]
    [Alias("buy s", "buy steroid", "buy empowered")]
    public Task BuyEmpoweredAsync([Remainder] string name)
        => this.BuyInternalAsync(ChickenType.SteroidEmpowered, name);

    [Command("buy alien")]
    [Alias("buy a", "buy extraterrestrial")]
    public Task BuyAlienAsync([Remainder] string name)
        => this.BuyInternalAsync(ChickenType.Alien, name);

    [Command("buy list")]
    [Alias("buy ls", "buy view")]
    public Task BuyListAsync()
    {
        string currency = this.guildConfig.GetCachedConfig(this.Context.Guild.Id).Currency;
        return this.RespondWithLocalizedEmbedAsync(emb => {
            emb.WithTitle(this.Localization.GetString(this.Context.Guild.Id, TranslationKey.str_chicken_types));
            emb.WithColor(this.ModuleColor);
            foreach ((ChickenType type, ChickenStats stats) in Chicken.StartingStats)
                emb.AddField($"{type.Humanize()} ({Chicken.Price(type):n0} {currency})", stats.ToString());
        });
    }

    [Command("fight")]
    [Priority(1)]
    [Alias("f", "duel", "attack")]
    public async Task FightAsync(IGuildUser member)
    {
        if (member.Id == this.Context.User.Id)
            throw new LocalizedCommandException(TranslationKey.cmd_err_chicken_self);

        Chicken? mine = await this.chickens.GetCompleteAsync(this.Context.Guild.Id, this.Context.User.Id);
        if (mine is null)
            throw new LocalizedCommandException(TranslationKey.cmd_err_chicken_none);
        mine.Owner = this.Context.User;

        Chicken? theirs = await this.chickens.GetCompleteAsync(this.Context.Guild.Id, member.Id);
        if (theirs is null)
            throw new LocalizedCommandException(TranslationKey.cmd_err_chicken_404(member.Mention));
        theirs.Owner = member;

        if (mine.Stats.TotalVitality < Chicken.MinVitalityToFight)
            throw new LocalizedCommandException(TranslationKey.cmd_err_chicken_weak(this.Context.User.Mention));

        if (mine.IsTooStrongFor(theirs))
            throw new LocalizedCommandException(TranslationKey.cmd_err_chicken_strdiff(Chicken.MaxFightStrDiff));

        ChickenFightResult result = ChickenFightResult.Fight(mine, theirs);
        await this.chickens.UpdateAsync(result);
        await this.bank.IncreaseBankAccountAsync(this.Context.Guild.Id, result.Winner.UserId, result.Reward);

        string currency = this.guildConfig.GetCachedConfig(this.Context.Guild.Id).Currency;
        await this.RespondWithLocalizedEmbedAsync(emb => {
            emb.WithColor(this.ModuleColor);
            emb.WithDescription(
                $"{Emojis.Chicken} {mine.Name} {mine.Stats.ToShortString()} {Emojis.DuelSwords} {theirs.Name} {theirs.Stats.ToShortString()} {Emojis.Chicken}\n\n" +
                $"{Emojis.Trophy} {result.Winner.Name} wins.\n" +
                $"{result.Winner.Name} gained {result.StrGain} STR.\n" +
                (result.IsLoserDead ? $"{result.Loser.Name} died.\n" : $"{result.Loser.Name} lost {ChickenFightResult.VitLoss} VIT.\n") +
                $"{result.Winner.Owner?.Mention} earned {result.Reward:n0} {currency}."
            );
        });
    }

    [Command("fight")]
    [Priority(0)]
    public async Task FightByNameAsync([Remainder] string chickenName)
    {
        Chicken? chicken = this.chickens.GetByName(this.Context.Guild.Id, chickenName);
        if (chicken is null)
            throw new LocalizedCommandException(TranslationKey.cmd_err_chicken_name_404);
        SocketGuildUser? owner = this.Context.Guild.GetUser(chicken.UserId);
        if (owner is null) {
            await this.chickens.RemoveAsync(chicken);
            throw new LocalizedCommandException(TranslationKey.cmd_err_chicken_owner);
        }
        await this.FightAsync(owner);
    }

    [Command("heal")]
    [Alias("+hp", "hp")]
    [Cooldown(1, 300, CooldownBucketType.Guild)]
    public async Task HealAsync()
    {
        if (!await this.chickens.HealAsync(this.Context.Guild.Id, this.Context.User.Id, 100))
            throw new LocalizedCommandException(TranslationKey.cmd_err_chicken_none);

        await this.ImpInfoAsync(this.ModuleColor, Emojis.Chicken, TranslationKey.fmt_chicken_heal(this.Context.User.Mention, 100));
    }

    [Command("info")]
    [Priority(1)]
    [Alias("information", "stats")]
    public async Task InfoAsync(IGuildUser? member = null)
    {
        member ??= (IGuildUser)this.Context.User;
        Chicken? chicken = await this.chickens.GetCompleteAsync(this.Context.Guild.Id, member.Id);
        if (chicken is null)
            throw new LocalizedCommandException(TranslationKey.cmd_err_chicken_none);

        chicken.Owner = member;
        await PrintChickenAsync(chicken);
    }

    [Command("info")]
    [Priority(0)]
    public async Task InfoByNameAsync([Remainder] string chickenName)
    {
        Chicken? chicken = this.chickens.GetByName(this.Context.Guild.Id, chickenName);
        if (chicken is null)
            throw new LocalizedCommandException(TranslationKey.cmd_err_chicken_name_404);
        chicken.Owner = this.Context.Guild.GetUser(chicken.UserId);
        await PrintChickenAsync(chicken);
    }

    [Command("rename")]
    [Alias("rn", "name")]
    public async Task RenameAsync([Remainder] string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new LocalizedCommandException(TranslationKey.cmd_err_missing_name);
        if (newName.Length > Chicken.NameLimit)
            throw new LocalizedCommandException(TranslationKey.cmd_err_name(Chicken.NameLimit));
        if (!newName.All(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c)))
            throw new LocalizedCommandException(TranslationKey.cmd_err_name_alnum);

        if (!await this.chickens.RenameAsync(this.Context.Guild.Id, this.Context.User.Id, newName))
            throw new LocalizedCommandException(TranslationKey.cmd_err_chicken_none);
        await this.InfoAsync(this.ModuleColor);
    }

    [Command("sell")]
    [Alias("s")]
    public async Task SellAsync()
    {
        Chicken? chicken = await this.chickens.GetCompleteAsync(this.Context.Guild.Id, this.Context.User.Id);
        if (chicken is null)
            throw new LocalizedCommandException(TranslationKey.cmd_err_chicken_none);

        string currency = this.guildConfig.GetCachedConfig(this.Context.Guild.Id).Currency;
        if (!await this.ConfirmAsync(TranslationKey.q_chicken_sell(this.Context.User.Mention, chicken.SellPrice, currency)))
            return;

        await this.chickens.RemoveAsync(chicken);
        await this.bank.IncreaseBankAccountAsync(this.Context.Guild.Id, this.Context.User.Id, chicken.SellPrice);
        await this.ImpInfoAsync(this.ModuleColor, Emojis.Chicken, TranslationKey.fmt_chicken_sell(this.Context.User.Mention, chicken.Name, chicken.SellPrice, currency));
    }

    [Command("list")]
    [Alias("print", "show", "view", "ls", "l", "p")]
    public async Task ListAsync()
    {
        IReadOnlyList<Chicken> all = await this.chickens.GetAllAsync(this.Context.Guild.Id);
        if (!all.Any())
            throw new LocalizedCommandException(TranslationKey.cmd_err_chickens_none);

        await this.RespondPagedAsync(
            TranslationKey.str_chicken_list,
            all,
            c => $"{Format.Bold(c.Name)} | {MentionUtils.MentionUser(c.UserId)} | {c.Stats.TotalStrength} ({c.BareStrength}) STR"
        );
    }

    [Command("top")]
    [Alias("best", "strongest")]
    public async Task TopAsync()
        => await this.PrintChickensAsync(await this.chickens.GetTopAsync(this.Context.Guild.Id));

    [Command("topglobal")]
    [Alias("bestglobally", "globallystrongest", "globaltop", "topg", "gtop", "globalbest", "bestglobal")]
    public async Task GlobalTopAsync()
        => await this.PrintChickensAsync(await this.chickens.GetGlobalTopAsync());

    [Command("train")]
    [Alias("tr", "t", "exercise")]
    public Task TrainAsync()
        => this.TrainStrengthAsync();

    [Command("train strength")]
    [Alias("train str", "train st", "train s")]
    public Task TrainStrengthAsync()
        => this.TrainInternalAsync("STR");

    [Command("train vitality")]
    [Alias("train vit", "train vi", "train v")]
    public Task TrainVitalityAsync()
        => this.TrainInternalAsync("VIT");

    [Command("upgrade")]
    [Priority(1)]
    [Alias("perks", "upgrades", "upg", "u")]
    public Task UpgradeListGroupAsync()
        => this.UpgradeListAsync();

    [Command("upgrade")]
    [Priority(0)]
    public async Task UpgradeAsync(params int[] ids)
    {
        if (ids is null || !ids.Any())
            throw new LocalizedCommandException(TranslationKey.cmd_err_chicken_upg_ids_none);

        Chicken? chicken = await this.chickens.GetCompleteAsync(this.Context.Guild.Id, this.Context.User.Id);
        if (chicken is null)
            throw new LocalizedCommandException(TranslationKey.cmd_err_chicken_none);

        if (chicken.Stats.Upgrades?.Any(u => ids.Contains(u.Id)) ?? false)
            throw new LocalizedCommandException(TranslationKey.cmd_err_chicken_upg_dup);

        IReadOnlyList<ChickenUpgrade> allUpgrades = await this.upgrades.GetAsync();
        List<ChickenUpgrade> toBuy = allUpgrades.Where(u => ids.Contains(u.Id)).ToList();
        long totalCost = toBuy.Sum(u => u.Cost);
        string currency = this.guildConfig.GetCachedConfig(this.Context.Guild.Id).Currency;

        if (!await this.ConfirmAsync(TranslationKey.q_chicken_upg(this.Context.User.Mention, totalCost, currency, toBuy.Select(u => u.Name).JoinWith(", "))))
            return;

        if (!await this.bank.TryDecreaseBankAccountAsync(this.Context.Guild.Id, this.Context.User.Id, totalCost))
            throw new LocalizedCommandException(TranslationKey.cmd_err_funds(currency, totalCost));

        await this.boughtUpgrades.AddAsync(toBuy.Select(u => new ChickenBoughtUpgrade {
            Id = u.Id,
            GuildId = chicken.GuildId,
            UserId = chicken.UserId
        }));

        int addedStr = toBuy.Where(u => u.UpgradesStat == ChickenStatUpgrade.Str).Sum(u => u.Modifier);
        int addedVit = toBuy.Where(u => u.UpgradesStat == ChickenStatUpgrade.MaxVit).Sum(u => u.Modifier);
        await this.ImpInfoAsync(this.ModuleColor, Emojis.Chicken, TranslationKey.fmt_chicken_upg(this.Context.User.Mention, chicken.Name, toBuy.Count, addedStr, addedVit));
    }

    [Command("upgrade list")]
    [Alias("upgrades list", "upg list", "u list")]
    public async Task UpgradeListAsync()
    {
        IReadOnlyList<ChickenUpgrade> allUpgrades = await this.upgrades.GetAsync();
        if (!allUpgrades.Any())
            throw new LocalizedCommandException(TranslationKey.cmd_err_res_none);

        string currency = this.guildConfig.GetCachedConfig(this.Context.Guild.Id).Currency;
        await this.RespondPagedAsync(
            allUpgrades.OrderBy(u => u.Cost),
            (emb, upgrade) => {
                emb.WithTitle(upgrade.Name);
                emb.WithColor(this.ModuleColor);
                emb.AddLocalizedField(TranslationKey.str_id, upgrade.Id, true);
                emb.AddLocalizedField(TranslationKey.str_cost, $"{upgrade.Cost:n0} {currency}", true);
                emb.AddLocalizedField(TranslationKey.str_stats, $"+{upgrade.Modifier} {upgrade.UpgradesStat.Humanize(LetterCasing.AllCaps)}", true);
                return emb;
            }
        );
    }

    [Command("ambush")]
    [Priority(1)]
    [Alias("gangattack", "poison")]
    public Task AmbushAsync(IGuildUser member)
        => this.FightAsync(member);

    [Command("ambush")]
    [Priority(0)]
    public Task AmbushByNameAsync([Remainder] string chickenName)
        => this.FightByNameAsync(chickenName);

    [Command("war")]
    [Alias("gangwar", "battle")]
    public async Task WarAsync()
    {
        Chicken[] fighters = (await this.chickens.GetAllAsync(this.Context.Guild.Id))
            .Where(c => c.Stats.TotalVitality >= Chicken.MinVitalityToFight)
            .OrderBy(_ => Guid.NewGuid())
            .Take(6)
            .ToArray();

        if (fighters.Length < 2)
            throw new LocalizedCommandException(TranslationKey.str_chicken_war_none);

        Chicken[] team1 = fighters.Where((_, i) => i % 2 == 0).ToArray();
        Chicken[] team2 = fighters.Where((_, i) => i % 2 != 0).ToArray();
        var aggregate1 = new Chicken { BareStrength = team1.Sum(c => c.Stats.TotalStrength) };
        var aggregate2 = new Chicken { BareStrength = team2.Sum(c => c.Stats.TotalStrength) };
        ChickenFightResult result = ChickenFightResult.Fight(aggregate1, aggregate2);
        bool team1Won = result.Winner == aggregate1;
        Chicken[] winners = team1Won ? team1 : team2;
        Chicken[] losers = team1Won ? team2 : team1;
        int gain = Math.Max(1, result.StrGain / winners.Length);

        foreach (Chicken chicken in winners) {
            chicken.Stats.BareStrength += gain;
            chicken.Stats.BareVitality -= 20;
            await this.bank.IncreaseBankAccountAsync(chicken.GuildId, chicken.UserId, result.Reward);
        }
        foreach (Chicken chicken in losers)
            chicken.Stats.BareVitality -= 50;

        await this.chickens.UpdateAsync(winners.Where(c => c.Stats.TotalVitality > 0));
        await this.chickens.UpdateAsync(losers.Where(c => c.Stats.TotalVitality > 0));
        await this.chickens.RemoveAsync(losers.Where(c => c.Stats.TotalVitality <= 0));

        string currency = this.guildConfig.GetCachedConfig(this.Context.Guild.Id).Currency;
        await this.RespondWithLocalizedEmbedAsync(emb => {
            emb.WithLocalizedTitle(TranslationKey.fmt_chicken_war_won(Emojis.Chicken, team1Won ? "Team 1" : "Team 2"));
            emb.WithColor(this.ModuleColor);
            emb.AddField("Team 1", team1.Select(c => c.Name).JoinWith(", "), true);
            emb.AddField("Team 2", team2.Select(c => c.Name).JoinWith(", "), true);
            emb.AddLocalizedField(TranslationKey.str_result, winners.Select(c => $"{c.Name} +{gain} STR").JoinWith("\n"));
            emb.WithLocalizedFooter(TranslationKey.fmt_chicken_war_rew(result.Reward, currency), null);
        });
    }


    private async Task BuyInternalAsync(ChickenType type, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new LocalizedCommandException(TranslationKey.cmd_err_missing_name);
        if (name.Length > Chicken.NameLimit)
            throw new LocalizedCommandException(TranslationKey.cmd_err_name(Chicken.NameLimit));
        if (!name.All(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c)))
            throw new LocalizedCommandException(TranslationKey.cmd_err_name_alnum);
        if (await this.chickens.ContainsAsync(this.Context.Guild.Id, this.Context.User.Id))
            throw new LocalizedCommandException(TranslationKey.cmd_err_chicken_dup);

        string currency = this.guildConfig.GetCachedConfig(this.Context.Guild.Id).Currency;
        long price = Chicken.Price(type);
        if (!await this.ConfirmAsync(TranslationKey.q_chicken_buy(this.Context.User.Mention, price, currency)))
            return;

        if (!await this.bank.TryDecreaseBankAccountAsync(this.Context.Guild.Id, this.Context.User.Id, price))
            throw new LocalizedCommandException(TranslationKey.cmd_err_funds(currency, price));

        await this.chickens.AddAsync(new Chicken(type) {
            GuildId = this.Context.Guild.Id,
            Name = name,
            UserId = this.Context.User.Id
        });

        await this.ImpInfoAsync(this.ModuleColor, Emojis.Chicken, TranslationKey.fmt_chicken_buy(this.Context.User.Mention, type.Humanize(LetterCasing.LowerCase), name));
    }

    private async Task TrainInternalAsync(string stat)
    {
        Chicken? chicken = await this.chickens.GetCompleteAsync(this.Context.Guild.Id, this.Context.User.Id);
        if (chicken is null)
            throw new LocalizedCommandException(TranslationKey.cmd_err_chicken_none);
        if (chicken.Stats.TotalVitality < Chicken.MinVitalityToFight)
            throw new LocalizedCommandException(TranslationKey.cmd_err_chicken_weak(this.Context.User.Mention));

        long price = stat == "STR" ? chicken.TrainStrengthPrice : chicken.TrainVitalityPrice;
        string currency = this.guildConfig.GetCachedConfig(this.Context.Guild.Id).Currency;
        if (!await this.ConfirmAsync(TranslationKey.q_chicken_train(this.Context.User.Mention, stat, price, currency)))
            return;

        if (!await this.bank.TryDecreaseBankAccountAsync(this.Context.Guild.Id, this.Context.User.Id, price))
            throw new LocalizedCommandException(TranslationKey.cmd_err_funds(currency, price));

        bool success = stat == "STR" ? chicken.TrainStrength() : chicken.TrainVitality();
        chicken.Vitality--;
        await this.chickens.UpdateAsync(chicken);

        TranslationKey msg = success
            ? TranslationKey.fmt_chicken_train_succ(this.Context.User.Mention, stat, stat == "STR" ? chicken.Stats.TotalStrength : chicken.Stats.TotalMaxVitality)
            : TranslationKey.fmt_chicken_train_fail(this.Context.User.Mention, stat, stat == "STR" ? chicken.Stats.TotalStrength : chicken.Stats.TotalMaxVitality);
        await this.ImpInfoAsync(this.ModuleColor, Emojis.Chicken, msg);
    }

    private Task PrintChickenAsync(Chicken chicken)
        => this.RespondWithLocalizedEmbedAsync(emb => {
            emb.WithTitle($"{Emojis.Chicken} {chicken.Name}");
            emb.WithColor(this.ModuleColor);
            emb.AddLocalizedField(TranslationKey.str_owner, chicken.Owner?.Mention ?? chicken.UserId.ToString(), true);
            emb.AddLocalizedField(TranslationKey.str_value, $"{chicken.SellPrice:n0}", true);
            emb.AddLocalizedField(TranslationKey.str_stats, chicken.Stats.ToString(), true);
            if (chicken.Stats.Upgrades?.Any() ?? false)
                emb.AddLocalizedField(TranslationKey.str_upgrades, chicken.Stats.Upgrades.Select(u => u.Upgrade.Name).JoinWith(", "), true);
            emb.WithLocalizedFooter(TranslationKey.str_chickens, chicken.Owner?.GetAvatarUrl());
        });

    private async Task PrintChickensAsync(IEnumerable<Chicken> chickenList)
    {
        Chicken[] items = chickenList.ToArray();
        if (!items.Any())
            throw new LocalizedCommandException(TranslationKey.cmd_err_chickens_none);

        await this.RespondPagedAsync(
            items,
            (emb, chicken) => {
                emb.WithTitle($"{Emojis.Chicken} {chicken.Name} {Emojis.Chicken}");
                emb.WithDescription(this.Localization.GetString(this.Context.Guild.Id, TranslationKey.fmt_chicken_owned_by(MentionUtils.MentionUser(chicken.UserId))));
                emb.AddLocalizedField(TranslationKey.str_chicken_str, $"{chicken.BareStrength} ({chicken.Stats.TotalStrength})", true);
                emb.AddLocalizedField(TranslationKey.str_chicken_vit, $"{chicken.Vitality}/{chicken.BareMaxVitality} ({chicken.Stats.TotalMaxVitality})", true);
                emb.AddLocalizedField(TranslationKey.str_chicken_upg, chicken.Upgrades.Select(u => u.Upgrade.Name).JoinWith(", "), unknown: false);
                return emb;
            }
        );
    }
}
