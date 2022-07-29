using System.Text;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using DSharpPlus.Interactivity;
using TheGodfather.Modules.Games.Extensions;
using TheGodfather.Services.Common;

namespace TheGodfather.Modules.Games.Common;

public sealed class DuelGame : BaseChannelGame
{
    public const int StartingHp = 5;
    public const int Damage = 1;

    public static string AgainstBot(string user, string bot)
    {
        return $"{user} {Enumerable.Repeat(Emojis.BlackSquare, 5).JoinWith("")} {Emojis.DuelSwords} " +
               $"{Enumerable.Repeat(Emojis.WhiteSquare, 5).JoinWith("")} {bot}\n\n" +
               $"{bot} {Emojis.Zap} {user}";
    }


    public string? FinishingMove { get; private set; }

    private int hp1;
    private int hp2;
    private string? hp1str;
    private string? hp2str;
    private bool potionUsed1;
    private bool potionUsed2;
    private DiscordMessage? msgHandle;
    private readonly DiscordUser player1;
    private readonly DiscordUser player2;
    private readonly StringBuilder eb;


    public DuelGame(InteractivityExtension interactivity, DiscordChannel channel, DiscordUser p1, DiscordUser p2)
        : base(interactivity, channel)
    {
        this.player1 = p1;
        this.player2 = p2;
        this.hp1 = this.hp2 = StartingHp;
        this.eb = new StringBuilder();
    }


    public override async Task RunAsync(LocalizationService lcs)
    {
        this.UpdateHpBars();

        this.msgHandle = await this.Channel.EmbedAsync($"{this.player1.Mention} {this.hp1str} {Emojis.DuelSwords} {this.hp2str} {this.player2.Mention}");

        while (this.hp1 > 0 && this.hp2 > 0)
            await this.AdvanceAsync(lcs);

        this.Winner = this.hp1 > 0 ? this.player1 : this.player2;
        this.FinishingMove = await this.WaitForFinishingMoveAsync(lcs);

        string toSend = string.IsNullOrWhiteSpace(this.FinishingMove)
            ? lcs.GetString(this.Channel.GuildId, TranslationKey.fmt_game_duel_win(Emojis.DuelSwords, this.Winner.Mention))
            : lcs.GetString(this.Channel.GuildId, TranslationKey.fmt_game_duel_winf(Emojis.DuelSwords, this.Winner.Mention, this.FinishingMove));
        await this.Channel.EmbedAsync(toSend);
    }

    private async Task AdvanceAsync(LocalizationService lcs)
    {
        this.DealDamage();
        await this.WaitForPotionUseAsync();
        this.UpdateHpBars();

        string header = $"{this.player1.Mention} {this.hp1str} {Emojis.DuelSwords} {this.hp2str} {this.player2.Mention}";
        var emb = new LocalizedEmbedBuilder(lcs, this.Channel.GuildId);
        emb.WithDescription($"{header}\n\n{this.eb}");
        emb.WithColor(DiscordColor.Teal);

        this.msgHandle = await this.msgHandle.ModifyOrResendAsync(this.Channel, emb.Build());
    }

    private void UpdateHpBars()
    {
        this.hp1str = Enumerable.Repeat(Emojis.WhiteSquare, this.hp1).JoinWith("") + Enumerable.Repeat(Emojis.BlackSquare, StartingHp - this.hp1).JoinWith("");
        this.hp2str = Enumerable.Repeat(Emojis.BlackSquare, StartingHp - this.hp2).JoinWith("") + Enumerable.Repeat(Emojis.WhiteSquare, this.hp2).JoinWith("");
    }

    private void DealDamage()
    {
        if (new SecureRandom().NextBool()) {
            this.eb.AppendLine($"{this.player1.Mention} {Emojis.Weapons.GetRandomDuelWeapon()} {this.player2.Mention}");
            this.hp2 -= Damage;
        } else {
            this.eb.AppendLine($"{this.player2.Mention} {Emojis.Weapons.GetRandomDuelWeapon()} {this.player1.Mention}");
            this.hp1 -= Damage;
        }
    }

    private async Task WaitForPotionUseAsync()
    {
        InteractivityResult<MessageReactionAddEventArgs> mctx = await this.Interactivity.WaitForReactionAsync(
            e => {
                if (e.Channel != this.Channel) return false;
                if (e.Emoji != Emojis.Syringe) return false;
                if (!this.potionUsed1 && e.User == this.player1) return true;
                if (!this.potionUsed2 && e.User == this.player2) return true;
                return false;
            },
            TimeSpan.FromSeconds(2)
        );
        if (!mctx.TimedOut) {
            if (mctx.Result.User == this.player1) {
                this.hp1++;
                this.hp1 = Math.Min(this.hp1, StartingHp);
                this.potionUsed1 = true;
            } else {
                this.hp2++;
                this.hp2 = Math.Min(this.hp2, StartingHp);
                this.potionUsed2 = true;
            }
            this.eb.AppendLine($"{mctx.Result.User.Mention} {Emojis.Syringe}");
        }
    }

    private async Task<string?> WaitForFinishingMoveAsync(LocalizationService lcs)
    {
        if (this.Winner is null)
            return null;

        string fstr = lcs.GetString(this.Channel.GuildId, TranslationKey.fmt_game_duel_f(Emojis.DuelSwords, this.Winner.Mention, Emojis.DuelSwords));
        DiscordMessage msg = await this.Channel.EmbedAsync(fstr);

        InteractivityResult<DiscordMessage> mctx = await this.Interactivity.WaitForMessageAsync(
            m => m.Channel == this.Channel && m.Author == this.Winner,
            TimeSpan.FromSeconds(15)
        );

        if (mctx.TimedOut || string.IsNullOrWhiteSpace(mctx.Result.Content))
            return null;

        return mctx.Result.Content.Trim();
    }
}