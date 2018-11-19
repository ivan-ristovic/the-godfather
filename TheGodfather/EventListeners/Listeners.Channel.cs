#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

using Humanizer;

using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Database;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Common;
#endregion

namespace TheGodfather.EventListeners
{
    internal static partial class Listeners
    {
        [AsyncEventListener(DiscordEventType.ChannelCreated)]
        public static async Task ChannelCreateEventHandlerAsync(TheGodfatherShard shard, ChannelCreateEventArgs e)
        {
            DiscordChannel logchn = shard.SharedData.GetLogChannelForGuild(shard.Client, e.Guild);
            if (logchn is null)
                return;

            DiscordEmbedBuilder emb = FormEmbedBuilder(EventOrigin.Channel, "Channel created", e.Channel.ToString());

            var entry = await e.Guild.GetLatestAuditLogEntryAsync(AuditLogActionType.ChannelCreate);
            if (entry is null || !(entry is DiscordAuditLogChannelEntry centry)) {
                emb.AddField("Error", "Failed to read audit log information. Please check my permissions");
            } else {
                emb.AddField("User responsible", centry.UserResponsible?.Mention ?? _unknown, inline: true);
                emb.AddField("Channel type", centry.Target.Type.ToString(), inline: true);
                if (!string.IsNullOrWhiteSpace(centry.Reason))
                    emb.AddField("Reason", centry.Reason);
                emb.WithFooter(centry.CreationTimestamp.ToUtcTimestamp(), centry.UserResponsible.AvatarUrl);
            }

            await logchn.SendMessageAsync(embed: emb.Build());
        }

        [AsyncEventListener(DiscordEventType.ChannelDeleted)]
        public static async Task ChannelDeleteEventHandlerAsync(TheGodfatherShard shard, ChannelDeleteEventArgs e)
        {
            DiscordChannel logchn = shard.SharedData.GetLogChannelForGuild(shard.Client, e.Guild);
            if (logchn is null)
                return;

            using (DatabaseContext db = shard.Database.CreateContext())
                if (db.LoggingExempts.Any(ee => ee.Type == ExemptedEntityType.Channel && ee.Id == e.Channel.Id))
                    return;

            DiscordEmbedBuilder emb = FormEmbedBuilder(EventOrigin.Channel, "Channel deleted", e.Channel.ToString());

            emb.AddField("Channel type", e.Channel?.Type.ToString() ?? _unknown, inline: true);

            var entry = await e.Guild.GetLatestAuditLogEntryAsync(AuditLogActionType.ChannelDelete);
            if (entry is null || !(entry is DiscordAuditLogChannelEntry centry)) {
                emb.AddField("Error", "Failed to read audit log information. Please check my permissions");
            } else {
                emb.AddField("User responsible", centry.UserResponsible?.Mention ?? _unknown, inline: true);
                if (!string.IsNullOrWhiteSpace(centry.Reason))
                    emb.AddField("Reason", centry.Reason);
                emb.WithFooter(centry.CreationTimestamp.ToUtcTimestamp(), centry.UserResponsible.AvatarUrl);
            }

            await logchn.SendMessageAsync(embed: emb.Build());
        }

        [AsyncEventListener(DiscordEventType.ChannelPinsUpdated)]
        public static async Task ChannelPinsUpdateEventHandlerAsync(TheGodfatherShard shard, ChannelPinsUpdateEventArgs e)
        {
            DiscordChannel logchn = shard.SharedData.GetLogChannelForGuild(shard.Client, e.Channel.Guild);
            if (logchn is null)
                return;

            using (DatabaseContext db = shard.Database.CreateContext())
                if (db.LoggingExempts.Any(ee => ee.Type == ExemptedEntityType.Channel && ee.Id == e.Channel.Id))
                    return;

            DiscordEmbedBuilder emb = FormEmbedBuilder(EventOrigin.Channel, "Channel pins updated", e.Channel.ToString());
            emb.AddField("Channel", e.Channel.Mention, inline: true);

            var pinned = await e.Channel.GetPinnedMessagesAsync();
            if (pinned.Any()) {
                emb.WithDescription(Formatter.MaskedUrl("Jump to top pin", pinned.First().JumpLink));
                string content = string.IsNullOrWhiteSpace(pinned.First().Content) ? "<embedded message>" : pinned.First().Content;
                emb.AddField("Top pin content", Formatter.BlockCode(Formatter.Sanitize(content.Truncate(1000))));
            }
            if (!(e.LastPinTimestamp is null))
                emb.AddField("Last pin timestamp", e.LastPinTimestamp.Value.ToUtcTimestamp(), inline: true);

            await logchn.SendMessageAsync(embed: emb.Build());
        }

        [AsyncEventListener(DiscordEventType.ChannelUpdated)]
        public static async Task ChannelUpdateEventHandlerAsync(TheGodfatherShard shard, ChannelUpdateEventArgs e)
        {
            if (e.ChannelBefore.Position != e.ChannelAfter.Position)
                return;

            DiscordChannel logchn = shard.SharedData.GetLogChannelForGuild(shard.Client, e.Guild);
            if (logchn is null)
                return;

            using (DatabaseContext db = shard.Database.CreateContext())
                if (db.LoggingExempts.Any(ee => ee.Type == ExemptedEntityType.Channel && ee.Id == e.ChannelAfter.Id))
                    return;

            DiscordEmbedBuilder emb = FormEmbedBuilder(EventOrigin.Channel, "Channel updated");
            DiscordAuditLogEntry entry = await e.Guild.GetLatestAuditLogEntryAsync(AuditLogActionType.ChannelUpdate);            
            if (!(entry is null) && entry is DiscordAuditLogChannelEntry centry) {     // Regular update
                emb.WithDescription(centry.Target.ToString());
                emb.AddField("User responsible", centry.UserResponsible?.Mention ?? _unknown, inline: true);
                if (!(centry.BitrateChange is null))
                    emb.AddField("Bitrate changed to", centry.BitrateChange.After.ToString(), inline: true);
                if (!(centry.NameChange is null))
                    emb.AddField("Name changed to", centry.NameChange.After, inline: true);
                if (!(centry.NsfwChange is null))
                    emb.AddField("NSFW flag changed to", centry.NsfwChange.After.Value.ToString(), inline: true);
                if (!(centry.OverwriteChange is null))
                    emb.AddField("Permissions overwrites changed", $"{centry.OverwriteChange.After.Count} overwrites after changes");
                if (!(centry.TopicChange is null)) {
                    string ptopic = Formatter.BlockCode(Formatter.Sanitize(string.IsNullOrWhiteSpace(centry.TopicChange.Before) ? " " : centry.TopicChange.Before));
                    string ctopic = Formatter.BlockCode(Formatter.Sanitize(string.IsNullOrWhiteSpace(centry.TopicChange.After) ? " " : centry.TopicChange.After));
                    emb.AddField("Topic changed", $"From:{ptopic}\nTo:{ctopic}");
                }
                if (!(centry.TypeChange is null))
                    emb.AddField("Type changed to", centry.TypeChange.After.Value.ToString());
                if (!string.IsNullOrWhiteSpace(centry.Reason))
                    emb.AddField("Reason", centry.Reason);
                emb.WithFooter($"At {centry.CreationTimestamp.ToUniversalTime().ToString()} UTC", centry.UserResponsible.AvatarUrl);
            } else {    // Overwrite update
                AuditLogActionType type;
                if (e.ChannelBefore.PermissionOverwrites.Count > e.ChannelAfter.PermissionOverwrites.Count) {
                    type = AuditLogActionType.OverwriteCreate;
                    entry = await e.Guild.GetLatestAuditLogEntryAsync(AuditLogActionType.OverwriteCreate);
                } else if (e.ChannelBefore.PermissionOverwrites.Count < e.ChannelAfter.PermissionOverwrites.Count) {
                    type = AuditLogActionType.OverwriteDelete;
                    entry = await e.Guild.GetLatestAuditLogEntryAsync(AuditLogActionType.OverwriteDelete);
                } else {
                    if (e.ChannelBefore.PermissionOverwrites.Zip(e.ChannelAfter.PermissionOverwrites, (o1, o2) => o1.Allowed != o1.Allowed && o2.Denied != o2.Denied).Any()) {
                        type = AuditLogActionType.OverwriteUpdate;
                        entry = await e.Guild.GetLatestAuditLogEntryAsync(AuditLogActionType.OverwriteUpdate);
                    } else {
                        type = AuditLogActionType.ChannelUpdate;
                        entry = await e.Guild.GetLatestAuditLogEntryAsync(AuditLogActionType.ChannelUpdate);
                    }
                }

                if (!(entry is null) && entry is DiscordAuditLogOverwriteEntry owentry) {
                    emb.WithDescription($"{owentry.Channel.ToString()} ({type})");
                    emb.AddField("User responsible", owentry.UserResponsible?.Mention ?? _unknown, inline: true);

                    DiscordUser member = null;
                    DiscordRole role = null;

                    try {
                        bool isMemberUpdated = owentry.Target.Type.HasFlag(OverwriteType.Member);
                        if (isMemberUpdated)
                            member = await e.Client.GetUserAsync(owentry.Target.Id);
                        else
                            role = e.Guild.GetRole(owentry.Target.Id);
                        emb.AddField("Target", isMemberUpdated ? member.ToString() : role.ToString(), inline: true);
                        if (!(owentry.AllowChange is null))
                            emb.AddField("Allowed", $"{owentry.Target.Allowed.ToPermissionString() ?? _unknown}", inline: true);
                        if (!(owentry.DenyChange is null))
                            emb.AddField("Denied", $"{owentry.Target.Denied.ToPermissionString() ?? _unknown}", inline: true);
                    } catch {
                        emb.AddField("Target ID", owentry.Target.Id.ToString(), inline: true);
                    }

                    if (!string.IsNullOrWhiteSpace(owentry.Reason))
                        emb.AddField("Reason", owentry.Reason);
                    emb.WithFooter(owentry.CreationTimestamp.ToUtcTimestamp(), owentry.UserResponsible.AvatarUrl);
                } else {
                    return;
                }
            }

            await logchn.SendMessageAsync(embed: emb.Build());
        }
    }
}
