#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

using Humanizer;

using Microsoft.Extensions.DependencyInjection;

using System;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Common.Collections;
using TheGodfather.Database;
using TheGodfather.Database.Entities;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Common;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Modules.Reactions.Common;
#endregion

namespace TheGodfather.EventListeners
{
    internal static partial class Listeners
    {
        [AsyncEventListener(DiscordEventType.MessagesBulkDeleted)]
        public static async Task BulkDeleteEventHandlerAsync(TheGodfatherShard shard, MessageBulkDeleteEventArgs e)
        {
            if (e.Channel.IsPrivate)
                return;

            DiscordChannel logchn = shard.SharedData.GetLogChannelForGuild(shard.Client, e.Channel.Guild);
            if (logchn is null)
                return;

            using (DatabaseContext db = shard.Database.CreateContext())
                if (db.LoggingExempts.Any(ee => ee.Type == ExemptedEntityType.Channel && ee.Id == e.Channel.Id))
                    return;

            DiscordEmbedBuilder emb = FormEmbedBuilder(EventOrigin.Message, $"Bulk message deletion occured ({e.Messages.Count} total)", $"In channel {e.Channel.Mention}");
            await logchn.SendMessageAsync(embed: emb.Build());
        }

        [AsyncEventListener(DiscordEventType.MessageCreated)]
        public static async Task MessageCreateEventHandlerAsync(TheGodfatherShard shard, MessageCreateEventArgs e)
        {
            if (e.Author.IsBot || e.Channel.IsPrivate)
                return;

            if (shard.SharedData.BlockedChannels.Contains(e.Channel.Id) || shard.SharedData.BlockedUsers.Contains(e.Author.Id))
                return;

            if (!e.Channel.PermissionsFor(e.Guild.CurrentMember).HasFlag(Permissions.SendMessages))
                return;

            if (!string.IsNullOrWhiteSpace(e.Message?.Content) && !e.Message.Content.StartsWith(shard.SharedData.GetGuildPrefix(e.Guild.Id))) {
                short rank = shard.SharedData.IncrementMessageCountForUser(e.Author.Id);
                if (rank != 0) {
                    DatabaseGuildRank rankInfo;
                    using (DatabaseContext db = shard.Database.CreateContext())
                        rankInfo = db.GuildRanks.SingleOrDefault(r => r.GuildId == e.Guild.Id && r.Rank == rank);
                    await e.Channel.EmbedAsync($"GG {e.Author.Mention}! You have advanced to level {Formatter.Bold(rank.ToString())} {(rankInfo is null ? "" : $": {Formatter.Italic(rankInfo.Name)}")} !", StaticDiscordEmoji.Medal);
                }
            }
        }

        [AsyncEventListener(DiscordEventType.MessageCreated)]
        public static async Task MessageCreateProtectionHandlerAsync(TheGodfatherShard shard, MessageCreateEventArgs e)
        {
            if (e.Author.IsBot || e.Channel.IsPrivate || string.IsNullOrWhiteSpace(e.Message?.Content))
                return;

            if (shard.SharedData.BlockedChannels.Contains(e.Channel.Id))
                return;

            CachedGuildConfig gcfg = shard.SharedData.GetGuildConfig(e.Guild.Id);
            if (gcfg.RatelimitSettings.Enabled)
                await shard.CNext.Services.GetService<RatelimitService>().HandleNewMessageAsync(e, gcfg.RatelimitSettings);

            if (gcfg.AntispamSettings.Enabled)
                await shard.CNext.Services.GetService<AntispamService>().HandleNewMessageAsync(e, gcfg.AntispamSettings);
        }

        [AsyncEventListener(DiscordEventType.MessageCreated)]
        public static async Task MessageFilterEventHandlerAsync(TheGodfatherShard shard, MessageCreateEventArgs e)
        {
            if (e.Author.IsBot || e.Channel.IsPrivate || string.IsNullOrWhiteSpace(e.Message?.Content))
                return;

            if (shard.SharedData.BlockedChannels.Contains(e.Channel.Id))
                return;

            CachedGuildConfig gcfg = shard.SharedData.GetGuildConfig(e.Guild.Id);
            if (gcfg.LinkfilterSettings.Enabled) {
                if (await shard.CNext.Services.GetService<LinkfilterService>().HandleNewMessageAsync(e, gcfg.LinkfilterSettings))
                    return;
            }

            if (!shard.SharedData.MessageContainsFilter(e.Guild.Id, e.Message.Content))
                return;

            if (!e.Channel.PermissionsFor(e.Guild.CurrentMember).HasFlag(Permissions.ManageMessages))
                return;

            await e.Message.DeleteAsync("_gf: Filter hit");
            await e.Channel.SendMessageAsync($"{e.Author.Mention} said: {FormatterExtensions.Spoiler(Formatter.BlockCode(Formatter.Strip(e.Message.Content)))}");
        }

        [AsyncEventListener(DiscordEventType.MessageCreated)]
        public static async Task MessageEmojiReactionEventHandlerAsync(TheGodfatherShard shard, MessageCreateEventArgs e)
        {
            if (e.Author.IsBot || e.Channel.IsPrivate || string.IsNullOrWhiteSpace(e.Message?.Content))
                return;

            if (shard.SharedData.BlockedChannels.Contains(e.Channel.Id) || shard.SharedData.BlockedUsers.Contains(e.Author.Id))
                return;

            if (!e.Channel.PermissionsFor(e.Guild.CurrentMember).HasFlag(Permissions.AddReactions))
                return;

            if (!shard.SharedData.EmojiReactions.TryGetValue(e.Guild.Id, out ConcurrentHashSet<EmojiReaction> ereactions))
                return;

            EmojiReaction ereaction = ereactions?
                .Where(er => er.IsMatch(e.Message?.Content ?? ""))
                .Shuffle()
                .FirstOrDefault();
            if (!(ereaction is null)) {
                try {
                    var emoji = DiscordEmoji.FromName(shard.Client, ereaction.Response);
                    await e.Message.CreateReactionAsync(emoji);
                } catch (ArgumentException) {
                    using (DatabaseContext db = shard.Database.CreateContext()) {
                        db.EmojiReactions.RemoveRange(db.EmojiReactions.Where(er => er.GuildId == e.Guild.Id && er.Reaction == ereaction.Response));
                        await db.SaveChangesAsync();
                    }
                }
            }
        }

        [AsyncEventListener(DiscordEventType.MessageCreated)]
        public static async Task MessageTextReactionEventHandlerAsync(TheGodfatherShard shard, MessageCreateEventArgs e)
        {
            if (e.Author.IsBot || e.Channel.IsPrivate || string.IsNullOrWhiteSpace(e.Message?.Content))
                return;

            if (e.Message.Content.StartsWith(shard.SharedData.GetGuildPrefix(e.Guild.Id)))
                return;

            if (shard.SharedData.BlockedChannels.Contains(e.Channel.Id) || shard.SharedData.BlockedUsers.Contains(e.Author.Id))
                return;

            if (!e.Channel.PermissionsFor(e.Guild.CurrentMember).HasFlag(Permissions.SendMessages))
                return;

            if (!shard.SharedData.TextReactions.TryGetValue(e.Guild.Id, out ConcurrentHashSet<TextReaction> treactions))
                return;

            TextReaction tr = treactions?.FirstOrDefault(r => r.IsMatch(e.Message.Content));
            if (!tr?.IsCooldownActive() ?? false)
                await e.Channel.SendMessageAsync(tr.Response.Replace("%user%", e.Author.Mention));
        }

        [AsyncEventListener(DiscordEventType.MessageDeleted)]
        public static async Task MessageDeleteEventHandlerAsync(TheGodfatherShard shard, MessageDeleteEventArgs e)
        {
            if (e.Channel.IsPrivate || e.Message is null)
                return;

            DiscordChannel logchn = shard.SharedData.GetLogChannelForGuild(shard.Client, e.Guild);
            if (logchn is null)
                return;

            using (DatabaseContext db = shard.Database.CreateContext())
                if (db.LoggingExempts.Any(ee => ee.Type == ExemptedEntityType.Channel && ee.Id == e.Channel.Id))
                    return;

            DiscordEmbedBuilder emb = FormEmbedBuilder(EventOrigin.Message, "Message deleted");
            emb.AddField("Location", e.Channel.Mention, inline: true);
            emb.AddField("Author", e.Message.Author?.Mention ?? _unknown, inline: true);

            DiscordAuditLogEntry entry = await e.Guild.GetLatestAuditLogEntryAsync(AuditLogActionType.MessageDelete);
            if (!(entry is null) && entry is DiscordAuditLogMessageEntry mentry) {
                DiscordMember member = await e.Guild.GetMemberAsync(mentry.UserResponsible.Id);

                using (DatabaseContext db = shard.Database.CreateContext()) {
                    if (db.LoggingExempts.Any(ee => ee.Type == ExemptedEntityType.Member && ee.Id == mentry.UserResponsible.Id))
                        return;
                    if (member?.Roles.Any(r => db.LoggingExempts.Any(ee => ee.Type == ExemptedEntityType.Role && ee.Id == r.Id)) ?? false)
                        return;
                }

                emb.AddField("User responsible", mentry.UserResponsible.Mention, inline: true);
                if (!string.IsNullOrWhiteSpace(mentry.Reason))
                    emb.AddField("Reason", mentry.Reason);
                emb.WithFooter(mentry.CreationTimestamp.ToUtcTimestamp(), mentry.UserResponsible.AvatarUrl);
            }

            if (!string.IsNullOrWhiteSpace(e.Message.Content)) {
                emb.AddField("Content", $"{Formatter.BlockCode(string.IsNullOrWhiteSpace(e.Message.Content) ? "<empty content>" : Formatter.Strip(e.Message.Content.Truncate(1000)))}");
                if (shard.SharedData.MessageContainsFilter(e.Guild.Id, e.Message.Content))
                    emb.WithDescription(Formatter.Italic("Message contained a filter."));
            }
            if (e.Message.Embeds.Any())
                emb.AddField("Embeds", e.Message.Embeds.Count.ToString(), inline: true);
            if (e.Message.Reactions.Any())
                emb.AddField("Reactions", string.Join(" ", e.Message.Reactions.Select(r => r.Emoji.GetDiscordName())), inline: true);
            if (e.Message.Attachments.Any())
                emb.AddField("Attachments", string.Join("\n", e.Message.Attachments.Select(a => a.FileName)), inline: true);
            if (e.Message.CreationTimestamp != null)
                emb.AddField("Message creation time", e.Message.CreationTimestamp.ToUtcTimestamp(), inline: true);

            await logchn.SendMessageAsync(embed: emb.Build());
        }

        [AsyncEventListener(DiscordEventType.MessageUpdated)]
        public static async Task MessageUpdateEventHandlerAsync(TheGodfatherShard shard, MessageUpdateEventArgs e)
        {

            if (e.Author is null || e.Author.IsBot || e.Channel is null || e.Channel.IsPrivate || e.Message is null)
                return;

            if (shard.SharedData.BlockedChannels.Contains(e.Channel.Id))
                return;

            if (!(e.Message.Content is null) && shard.SharedData.MessageContainsFilter(e.Guild.Id, e.Message.Content)) {
                try {
                    await e.Message.DeleteAsync("_gf: Filter hit after update");
                    await e.Channel.SendMessageAsync($"{e.Author.Mention} said: {FormatterExtensions.Spoiler(Formatter.BlockCode(Formatter.Strip(e.Message.Content)))}");
                } catch {

                }
            }

            DiscordChannel logchn = shard.SharedData.GetLogChannelForGuild(shard.Client, e.Guild);
            if (logchn is null || !e.Message.IsEdited)
                return;

            DiscordMember member = await e.Guild.GetMemberAsync(e.Author.Id);

            using (DatabaseContext db = shard.Database.CreateContext()) {
                if (db.LoggingExempts.Any(ee => ee.Type == ExemptedEntityType.Channel && ee.Id == e.Channel.Id))
                    return;
                if (db.LoggingExempts.Any(ee => ee.Type == ExemptedEntityType.Member && ee.Id == e.Author.Id))
                    return;
                if (member?.Roles.Any(r => db.LoggingExempts.Any(ee => ee.Type == ExemptedEntityType.Role && ee.Id == r.Id)) ?? false)
                    return;
            }

            string pcontent = string.IsNullOrWhiteSpace(e.MessageBefore?.Content) ? "" : e.MessageBefore.Content.Truncate(700);
            string acontent = string.IsNullOrWhiteSpace(e.Message?.Content) ? "" : e.Message.Content.Truncate(700);
            string ctime = e.Message.CreationTimestamp == null ? _unknown : e.Message.CreationTimestamp.ToUtcTimestamp();
            string etime = e.Message.EditedTimestamp is null ? _unknown : e.Message.EditedTimestamp.Value.ToUtcTimestamp();
            string bextra = $"Embeds: {e.MessageBefore?.Embeds?.Count ?? 0}, Reactions: {e.MessageBefore?.Reactions?.Count ?? 0}, Attachments: {e.MessageBefore?.Attachments?.Count ?? 0}";
            string aextra = $"Embeds: {e.Message.Embeds.Count}, Reactions: {e.Message.Reactions.Count}, Attachments: {e.Message.Attachments.Count}";

            DiscordEmbedBuilder emb = FormEmbedBuilder(EventOrigin.Message, "Message updated");
            emb.WithDescription(Formatter.MaskedUrl("Jump to message", e.Message.JumpLink));
            emb.AddField("Location", e.Channel.Mention, inline: true);
            emb.AddField("Author", e.Message.Author?.Mention ?? _unknown, inline: true);
            emb.AddField("Before update", $"Created {ctime}\n{bextra}\nContent:{Formatter.BlockCode(Formatter.Strip(pcontent))}");
            emb.AddField("After update", $"Edited {etime}\n{aextra}\nContent:{Formatter.BlockCode(Formatter.Strip(acontent))}");

            await logchn.SendMessageAsync(embed: emb.Build());
        }



        // APRIL FOOLS
        private static string[] tmp = {
            "I am gay.",
            "Fuck you.",
            "Your mom.",
            @"""I'd just like to interject for a moment.  What you're referring to as Linux,
is in fact, GNU/Linux, or as I've recently taken to calling it, GNU plus Linux.
Linux is not an operating system unto itself, but rather another free component
of a fully functioning GNU system made useful by the GNU corelibs, shell
utilities and vital system components comprising a full OS as defined by POSIX.

Many computer users run a modified version of the GNU system every day,
without realizing it.  Through a peculiar turn of events, the version of GNU
which is widely used today is often called ""Linux"", and many of its users are
not aware that it is basically the GNU system, developed by the GNU Project.

There really is a Linux, and these people are using it, but it is just a
part of the system they use.Linux is the kernel: the program in the system
that allocates the machine's resources to the other programs that you run.
The kernel is an essential part of an operating system, but useless by itself;
    it can only function in the context of a complete operating system.Linux is
normally used in combination with the GNU operating system: the whole system
is basically GNU with Linux added, or GNU/Linux.All the so-called ""Linux""
distributions are really distributions of GNU/Linux.""",
            "ayyy lmao",
            "xd",
        };

        [AsyncEventListener(DiscordEventType.MessageCreated)]
        public static async Task AprilFoolsAsync(TheGodfatherShard shard, MessageCreateEventArgs e)
        {
            // In case I forget to remove this tomorrow /shrug
            if (DateTime.Now.Date.Day > 1)
                return;

            if (e.Author.IsBot || e.Channel.IsPrivate)
                return;

            if (!new ulong[] { 201315884709576705, 482290212915904532, 509138796466405376, 420357097146810378 }.Contains(e.Guild.Id))
                return;

            if (new ulong[] { 224287575718756352, 515098985770385419 }.Contains(e.Channel.Id))
                return;

            DiscordMember sender = await e.Guild.GetMemberAsync(e.Author.Id);
            DiscordWebhook wh = (await e.Channel.GetWebhooksAsync()).FirstOrDefault();
            using (System.IO.Stream stream = await new System.Net.Http.HttpClient().GetStreamAsync(e.Author.AvatarUrl))
            using (var ms = new System.IO.MemoryStream()) {
                await stream.CopyToAsync(ms);
                if (wh is null)
                    wh = await e.Channel.CreateWebhookAsync(sender.DisplayName, ms, "April fools");
                else
                    wh = await wh.ModifyAsync(sender.DisplayName, ms);
            }

            string message = e.Message.Content;
            await e.Message.DeleteAsync();
            if (GFRandom.Generator.Next() % 10 == 0)
                await wh.ExecuteAsync(tmp[GFRandom.Generator.Next() % tmp.Length]);
            else
                await wh.ExecuteAsync(message);
        }
    }
}
}
