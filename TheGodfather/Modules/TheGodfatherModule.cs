using Discord.Commands;
using System.Text;

namespace TheGodfather.Modules;

public abstract class TheGodfatherModule : ModuleBase<GodfatherCommandContext>
{
    protected LocalizationService Localization { get; }
    protected Color ModuleColor { get; }
    protected InteractivityService Interactivity => this.Context.Services.GetRequiredService<InteractivityService>();


    protected TheGodfatherModule(LocalizationService localization, Color moduleColor)
    {
        this.Localization = localization;
        this.ModuleColor = moduleColor;
    }


    protected Task ImpInfoAsync(Color color, string emoji, TranslationKey key)
    {
        string response = this.Localization.GetString(this.Context.Guild?.Id, key);
        var embed = new EmbedBuilder {
            Description = $"{emoji} {response}",
            Color = color
        };

        return this.ReplyAsync(embed: embed.Build());
    }

    protected Task ImpInfoAsync(Color color, TranslationKey? key = null)
        => this.ImpInfoAsync(color, Emojis.CheckMarkSuccess, key ?? TranslationKey.str_done);

    protected Task InfoAsync(Color color, TranslationKey? key = null)
        => this.ImpInfoAsync(color, Emojis.CheckMarkSuccess, key ?? TranslationKey.str_done);

    protected Task InfoAsync(Color color, string emoji, TranslationKey key)
        => this.ImpInfoAsync(color, emoji, key);

    protected Task InfoAsync(Color color, object? message)
        => this.ReplyAsync(embed: new EmbedBuilder {
            Description = $"{Emojis.CheckMarkSuccess} {message}",
            Color = color
        }.Build());

    protected Task InfoAsync(Color color, string emoji, object? message)
        => this.ReplyAsync(embed: new EmbedBuilder {
            Description = $"{emoji} {message}",
            Color = color
        }.Build());

    protected Task FailAsync(TranslationKey key)
        => this.FailAsync(Emojis.X, key);

    protected Task FailAsync(string emoji, TranslationKey key)
    {
        string response = this.Localization.GetString(this.Context.Guild?.Id, key);
        var embed = new EmbedBuilder {
            Description = $"{emoji} {response}",
            Color = Color.Red
        };

        return this.ReplyAsync(embed: embed.Build());
    }

    protected Task FailAsync(object? message)
        => this.ReplyAsync(embed: new EmbedBuilder {
            Description = $"{Emojis.X} {message}",
            Color = Color.Red
        }.Build());

    protected Task RespondWithLocalizedEmbedAsync(Action<LocalizedEmbedBuilder> action)
    {
        var embed = new LocalizedEmbedBuilder(this.Localization, this.Context.Guild?.Id);
        action(embed);
        return this.ReplyAsync(embed: embed.Build());
    }

    protected Task RespondPagedAsync<T>(
        TranslationKey title,
        IEnumerable<T> collection,
        Func<T, string> selector,
        int pageSize = 10)
    {
        T[] items = collection.ToArray();
        if (!items.Any())
            return this.RespondWithLocalizedEmbedAsync(emb => emb
                .WithColor(this.ModuleColor)
                .WithLocalizedTitle(title)
                .WithLocalizedDescription(TranslationKey.NotFound));

        string localizedTitle = this.Localization.GetString(this.Context.Guild?.Id, title);
        int pageCount = (items.Length - 1) / pageSize + 1;

        // Discord.Net.Commands has no built-in paginator, so this delegates controls to the shared interactivity service.
        var pages = new List<Embed>();
        for (int page = 0; page < pageCount; page++) {
            int from = page * pageSize;
            int to = Math.Min(from + pageSize, items.Length);
            string body = items[from..to].Select(selector).JoinWith();
            var embed = new EmbedBuilder {
                Color = this.ModuleColor,
                Title = localizedTitle,
                Description = body,
                Footer = new EmbedFooterBuilder {
                    Text = this.Localization.GetString(
                        this.Context.Guild?.Id,
                        TranslationKey.fmt_page_footer(from + 1, to, items.Length, page + 1, pageCount)
                    )
                }
            };
            pages.Add(embed.Build());
        }

        return this.Interactivity.SendPaginatedMessageAsync(this.Context.Channel, this.Context.User, pages);
    }

    protected Task RespondPagedAsync<T>(
        IEnumerable<T> collection,
        Func<LocalizedEmbedBuilder, T, LocalizedEmbedBuilder> formatter)
    {
        T[] items = collection.ToArray();
        if (!items.Any())
            return this.FailAsync(TranslationKey.cmd_err_res_none);

        var pages = new List<Embed>();
        for (int i = 0; i < items.Length; i++) {
            var embed = new LocalizedEmbedBuilder(this.Localization, this.Context.Guild?.Id)
                .WithColor(this.ModuleColor)
                .WithLocalizedFooter(TranslationKey.fmt_page_footer_single(i + 1, items.Length), null);
            embed = formatter(embed, items[i]);
            pages.Add(embed.Build());
        }

        return this.Interactivity.SendPaginatedMessageAsync(this.Context.Channel, this.Context.User, pages);
    }

    protected async Task<bool> ConfirmAsync(TranslationKey question, bool replyOnCancel = true)
    {
        await this.ReplyAsync(embed: new EmbedBuilder {
            Color = Color.Gold,
            Description = $"{Emojis.Question} {this.Localization.GetString(this.Context.Guild?.Id, question)} (y/n)"
        }.Build());

        bool accepted = await this.Interactivity.WaitForBooleanReplyAsync(this.Context.Channel, this.Context.User);

        if (accepted)
            return true;

        if (replyOnCancel)
            await this.FailAsync(TranslationKey.str_aborting);
        return false;
    }

    protected async Task<SocketUserMessage?> WaitForMessageAsync(Func<SocketUserMessage, bool> predicate, TimeSpan? timeout = null)
    {
        InteractivityResult<SocketUserMessage> result = await this.Interactivity.WaitForMessageAsync(
            msg => !msg.Author.IsBot && predicate(msg),
            timeout
        );
        return result.TimedOut ? null : result.Result;
    }

    protected static string UserDiscriminatorString(IUser user)
    {
        if (!string.IsNullOrWhiteSpace(user.Discriminator) && user.Discriminator != "0000")
            return $"{user.Username}#{user.Discriminator}";

        return user.GlobalName ?? user.Username;
    }

    protected static string UserDisplayName(IUser user)
        => user is IGuildUser guildUser ? guildUser.DisplayName : user.GlobalName ?? user.Username;

    protected static string InlineCode(object? value)
        => Format.Code(value?.ToString() ?? string.Empty);

    protected static string Bold(object? value)
        => Format.Bold(value?.ToString() ?? string.Empty);

    protected static string JoinLines(IEnumerable<string> lines)
    {
        var sb = new StringBuilder();
        foreach (string line in lines)
            sb.AppendLine(line);
        return sb.ToString();
    }

    protected bool IsNsfwChannel()
        => this.Context.Channel switch {
            ITextChannel text => text.IsNsfw || text.Name.Contains("nsfw", StringComparison.OrdinalIgnoreCase),
            _ => false
        };
}
