using System.Text;
using Discord.Commands;
using TheGodfather.Attributes;
using TheGodfather.Modules.Games.Services;

namespace TheGodfather.Modules.Games;

[Group("game")]
[Alias("games", "gm")]
[Cooldown(3, 5, CooldownBucketType.Channel)]
public sealed class GamesModule : TheGodfatherModule
{
    private static readonly string[] GameNames = {
        "animalrace", "caro", "connect4", "duel", "hangman", "numberrace", "othello",
        "quiz", "rps", "russianroulette", "tictactoe", "typingrace", "minesweeper"
    };

    private readonly GameStatsService stats;
    private readonly SecureRandom rng = new();


    public GamesModule(LocalizationService localization, GameStatsService stats)
        : base(localization, new Color(0x9B59B6))
    {
        this.stats = stats;
    }


    [Command("")]
    public Task ExecuteGroupAsync()
    {
        var sb = new StringBuilder();
        foreach (string game in GameNames)
            sb.Append(Emojis.SmallBlueDiamond).Append(' ').AppendLine(game);

        return this.RespondWithLocalizedEmbedAsync(emb => {
            emb.WithLocalizedTitle(TranslationKey.str_games);
            emb.WithDescription(sb.ToString());
            emb.WithColor(this.ModuleColor);
            emb.WithLocalizedFooter(TranslationKey.str_games_help, null);
        });
    }

    [Command("leaderboard")]
    [Alias("globalstats")]
    public async Task LeaderboardAsync()
    {
        await this.RespondWithLocalizedEmbedAsync(emb => {
            emb.WithLocalizedTitle(TranslationKey.str_hall_of_fame(Emojis.Trophy, Emojis.Trophy));
            emb.WithColor(this.ModuleColor);
            emb.AddField("Animal race", BuildTopString(this.stats.GetTopAnimalRaceStatsAsync().GetAwaiter().GetResult(), s => $"{s.AnimalRacesWon} wins"), true);
            emb.AddField("Caro", BuildTopString(this.stats.GetTopCaroStatsAsync().GetAwaiter().GetResult(), s => $"{GameStats.WinPercentage(s.CaroWon, s.CaroLost)}% ({s.CaroWon}/{s.CaroLost})"), true);
            emb.AddField("Connect 4", BuildTopString(this.stats.GetTopConnect4StatsAsync().GetAwaiter().GetResult(), s => $"{GameStats.WinPercentage(s.Connect4Won, s.Connect4Lost)}% ({s.Connect4Won}/{s.Connect4Lost})"), true);
            emb.AddField("Duel", BuildTopString(this.stats.GetTopDuelStatsAsync().GetAwaiter().GetResult(), s => $"{GameStats.WinPercentage(s.DuelsWon, s.DuelsLost)}% ({s.DuelsWon}/{s.DuelsLost})"), true);
            emb.AddField("Quiz", BuildTopString(this.stats.GetTopQuizStatsAsync().GetAwaiter().GetResult(), s => $"{s.QuizWon} wins"), true);
            emb.AddField("Typing race", BuildTopString(this.stats.GetTopTypingRaceStatsAsync().GetAwaiter().GetResult(), s => $"{s.TypingRacesWon} wins"), true);
        });
    }

    [Command("stats")]
    [Priority(1)]
    [Alias("s", "st")]
    public async Task StatsAsync(IUser? user = null)
    {
        user ??= this.Context.User;
        GameStats? s = await this.stats.GetAsync(user.Id);
        await this.RespondWithLocalizedEmbedAsync(emb => {
            emb.WithLocalizedTitle(TranslationKey.fmt_game_stats(UserDiscriminatorString(user)));
            emb.WithColor(this.ModuleColor);
            emb.WithThumbnail(user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl());
            if (s is null) {
                emb.WithLocalizedDescription(TranslationKey.str_game_stats_none);
                return;
            }

            emb.AddLocalizedField(TranslationKey.str_game_stats_duel, WinLoss(s.DuelsWon, s.DuelsLost));
            emb.AddLocalizedField(TranslationKey.str_game_stats_ttt, WinLoss(s.TicTacToeWon, s.TicTacToeLost));
            emb.AddLocalizedField(TranslationKey.str_game_stats_c4, WinLoss(s.Connect4Won, s.Connect4Lost));
            emb.AddLocalizedField(TranslationKey.str_game_stats_caro, WinLoss(s.CaroWon, s.CaroLost));
            emb.AddLocalizedField(TranslationKey.str_game_stats_ot, WinLoss(s.OthelloWon, s.OthelloLost));
            emb.AddLocalizedField(TranslationKey.str_game_stats_nr, s.NumberRacesWon, true);
            emb.AddLocalizedField(TranslationKey.str_game_stats_quiz, s.QuizWon, true);
            emb.AddLocalizedField(TranslationKey.str_game_stats_ar, s.AnimalRacesWon, true);
            emb.AddLocalizedField(TranslationKey.str_game_stats_rr, s.RussianRoulettesWon, true);
            emb.AddLocalizedField(TranslationKey.str_game_stats_tr, s.TypingRacesWon, true);
            emb.AddLocalizedField(TranslationKey.str_game_stats_hm, s.HangmanWon, true);
        });
    }

    [Command("rps")]
    [Alias("rockpaperscissors", "rockpaper")]
    public async Task RockPaperScissorsAsync(string pick)
    {
        string[] choices = { "rock", "paper", "scissors" };
        string user = pick.ToLowerInvariant();
        if (!choices.Contains(user)) {
            await this.FailAsync("Pick rock, paper, or scissors.");
            return;
        }

        string bot = choices[this.rng.Next(choices.Length)];
        string result = user == bot ? "Draw" :
            (user, bot) is ("rock", "scissors") or ("paper", "rock") or ("scissors", "paper") ? "You win" : "You lose";
        await this.InfoAsync(this.ModuleColor, $"{result}. You picked {user}; I picked {bot}.");
    }

    [Command("minesweeper")]
    [Alias("ms")]
    public Task MinesweeperAsync(int width = 8, int height = 8, int mines = 10)
    {
        width = Math.Clamp(width, 4, 14);
        height = Math.Clamp(height, 4, 14);
        mines = Math.Clamp(mines, 1, width * height - 1);
        bool[] field = Enumerable.Range(0, width * height).Select(i => i < mines).OrderBy(_ => this.rng.Next()).ToArray();
        var sb = new StringBuilder();
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                int idx = y * width + x;
                sb.Append(field[idx] ? "||*||" : "||.||");
            }
            sb.AppendLine();
        }
        return this.ReplyAsync(sb.ToString());
    }

    [Command("animalrace")]
    [Alias("animal", "ar")]
    public async Task AnimalRaceAsync()
    {
        string[] racers = Emojis.Animals.All.Take(6).ToArray();
        int winner = this.rng.Next(racers.Length);
        bool userWon = winner == 0;
        if (userWon)
            await this.stats.UpdateStatsAsync(this.Context.User.Id, s => s.AnimalRacesWon++);

        await this.InfoAsync(this.ModuleColor, $"{racers.JoinWith(" ")}\nWinner: {racers[winner]} {(userWon ? this.Context.User.Mention : "house")}");
    }

    [Command("caro")]
    public Task CaroAsync(IUser? opponent = null)
        => this.ResolveBoardGameAsync("Caro", opponent, (s, won) => {
            if (won) s.CaroWon++;
            else s.CaroLost++;
        });

    [Command("connect4")]
    [Alias("connectfour", "c4")]
    public Task Connect4Async(IUser? opponent = null)
        => this.ResolveBoardGameAsync("Connect 4", opponent, (s, won) => {
            if (won) s.Connect4Won++;
            else s.Connect4Lost++;
        });

    [Command("duel")]
    public async Task DuelAsync(IUser? opponent = null)
    {
        opponent = NormalizeOpponent(opponent, this.Context.User, this.Context.Client.CurrentUser);
        bool userWon = this.rng.Next(2) == 0;
        await this.stats.UpdateStatsAsync(this.Context.User.Id, s => {
            if (userWon) s.DuelsWon++;
            else s.DuelsLost++;
        });
        if (!opponent.IsBot)
            await this.stats.UpdateStatsAsync(opponent.Id, s => {
                if (userWon) s.DuelsLost++;
                else s.DuelsWon++;
            });

        string weapon = Emojis.Weapons.GetRandomDuelWeapon();
        IUser winner = userWon ? this.Context.User : opponent;
        await this.InfoAsync(this.ModuleColor, weapon, $"{this.Context.User.Mention} vs {opponent.Mention}\nWinner: {winner.Mention}");
    }

    [Command("hangman")]
    [Alias("hm")]
    public async Task HangmanAsync()
    {
        string[] words = { "discord", "godfather", "migration", "command", "module", "service" };
        string word = words[this.rng.Next(words.Length)];
        await this.InfoAsync(this.ModuleColor, $"Hangman: `{new string('_', word.Length)}`. Guess the word.");
        SocketUserMessage? guess = await this.WaitForMessageAsync(
            msg => msg.Author.Id == this.Context.User.Id && msg.Channel.Id == this.Context.Channel.Id,
            TimeSpan.FromSeconds(30)
        );
        bool won = string.Equals(guess?.Content.Trim(), word, StringComparison.OrdinalIgnoreCase);
        if (won)
            await this.stats.UpdateStatsAsync(this.Context.User.Id, s => s.HangmanWon++);
        await this.InfoAsync(this.ModuleColor, won ? $"Correct: `{word}`" : $"Nope. The word was `{word}`.");
    }

    [Command("numberrace")]
    [Alias("number", "nr")]
    public async Task NumberRaceAsync()
    {
        int target = this.rng.Next(1000, 9999);
        await this.InfoAsync(this.ModuleColor, $"Type `{target}` first.");
        SocketUserMessage? reply = await this.WaitForMessageAsync(
            msg => msg.Channel.Id == this.Context.Channel.Id && msg.Content.Trim() == target.ToString(CultureInfo.InvariantCulture),
            TimeSpan.FromSeconds(15)
        );
        if (reply is not null) {
            await this.stats.UpdateStatsAsync(reply.Author.Id, s => s.NumberRacesWon++);
            await this.InfoAsync(this.ModuleColor, $"{reply.Author.Mention} wins.");
        } else {
            await this.FailAsync("No winner.");
        }
    }

    [Command("othello")]
    [Alias("reversi", "ot")]
    public Task OthelloAsync(IUser? opponent = null)
        => this.ResolveBoardGameAsync("Othello", opponent, (s, won) => {
            if (won) s.OthelloWon++;
            else s.OthelloLost++;
        });

    [Command("quiz")]
    public async Task QuizAsync()
    {
        int a = this.rng.Next(10, 100);
        int b = this.rng.Next(10, 100);
        int answer = a + b;
        await this.InfoAsync(this.ModuleColor, $"Quiz: `{a} + {b}`");
        SocketUserMessage? reply = await this.WaitForMessageAsync(
            msg => msg.Channel.Id == this.Context.Channel.Id && int.TryParse(msg.Content.Trim(), out int value) && value == answer,
            TimeSpan.FromSeconds(20)
        );
        if (reply is not null) {
            await this.stats.UpdateStatsAsync(reply.Author.Id, s => s.QuizWon++);
            await this.InfoAsync(this.ModuleColor, $"{reply.Author.Mention} wins.");
        } else {
            await this.FailAsync($"No correct answer. It was {answer}.");
        }
    }

    [Command("russianroulette")]
    [Alias("rr")]
    public async Task RussianRouletteAsync()
    {
        bool survived = this.rng.Next(6) != 0;
        if (survived)
            await this.stats.UpdateStatsAsync(this.Context.User.Id, s => s.RussianRoulettesWon++);
        await this.InfoAsync(this.ModuleColor, survived ? $"{Emojis.Gun} Click. {this.Context.User.Mention} survives." : $"{Emojis.Blast} Bang.");
    }

    [Command("tictactoe")]
    [Alias("ttt")]
    public Task TicTacToeAsync(IUser? opponent = null)
        => this.ResolveBoardGameAsync("Tic-tac-toe", opponent, (s, won) => {
            if (won) s.TicTacToeWon++;
            else s.TicTacToeLost++;
        });

    [Command("typingrace")]
    [Alias("typing", "tr")]
    public async Task TypingRaceAsync()
    {
        string[] phrases = {
            "sleeping with the fishes",
            "leave the gun take the cannoli",
            "discord net migration"
        };
        string phrase = phrases[this.rng.Next(phrases.Length)];
        await this.InfoAsync(this.ModuleColor, $"Type: `{phrase}`");
        SocketUserMessage? reply = await this.WaitForMessageAsync(
            msg => msg.Channel.Id == this.Context.Channel.Id && string.Equals(msg.Content.Trim(), phrase, StringComparison.OrdinalIgnoreCase),
            TimeSpan.FromSeconds(30)
        );
        if (reply is not null) {
            await this.stats.UpdateStatsAsync(reply.Author.Id, s => s.TypingRacesWon++);
            await this.InfoAsync(this.ModuleColor, $"{reply.Author.Mention} wins.");
        } else {
            await this.FailAsync("No winner.");
        }
    }

    [Command("rules")]
    public Task RulesAsync([Remainder] string? game = null)
        => string.IsNullOrWhiteSpace(game)
            ? this.ExecuteGroupAsync()
            : this.InfoAsync(this.ModuleColor, $"{game}: play with the command name; text prompts expire automatically.");


    private async Task ResolveBoardGameAsync(string name, IUser? opponent, Action<GameStats, bool> update)
    {
        opponent = NormalizeOpponent(opponent, this.Context.User, this.Context.Client.CurrentUser);
        int roll = this.rng.Next(opponent.IsBot ? 2 : 3);
        if (roll == 2) {
            await this.InfoAsync(this.ModuleColor, $"{name}: {this.Context.User.Mention} draws with {opponent.Mention}.");
            return;
        }

        bool userWon = roll == 0;
        await this.stats.UpdateStatsAsync(this.Context.User.Id, s => update(s, userWon));
        if (!opponent.IsBot)
            await this.stats.UpdateStatsAsync(opponent.Id, s => update(s, !userWon));

        await this.InfoAsync(this.ModuleColor, $"{name}: {(userWon ? this.Context.User.Mention : opponent.Mention)} wins.");
    }

    private static IUser NormalizeOpponent(IUser? opponent, IUser user, IUser bot)
        => opponent is null || opponent.Id == user.Id ? bot : opponent;

    private string BuildTopString(IEnumerable<GameStats> top, Func<GameStats, string> selector)
        => top.Any()
            ? top.Select((s, i) => $"{i + 1}. {MentionUtils.MentionUser(s.UserId)} - {selector(s)}").JoinWith("\n")
            : "-";

    private static string WinLoss(int won, int lost)
        => $"{GameStats.WinPercentage(won, lost)}% ({won}/{lost})";
}
