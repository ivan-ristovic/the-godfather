using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Services;

namespace TheGodfather.Modules.Administration.Extensions
{
    public static class BackupServiceExtensions
    {
        public static Task BackupAsync(this BackupService bs, DiscordMessage msg)
        {
            ulong gid = msg.Channel.GuildId;
            ulong cid = msg.ChannelId;

            if (!bs.IsBackupEnabledFor(gid) || bs.IsChannelExempted(gid, cid))
                return Task.CompletedTask;

            var sb = new StringBuilder();
            sb.Append('[').Append(msg.CreationTimestamp).Append("] ").Append(msg.Author.ToDiscriminatorString()).AppendLine(":");

            if (!string.IsNullOrWhiteSpace(msg.Content))
                sb.AppendLine(msg.Content);
            
            foreach (DiscordEmbed e in msg.Embeds) {
                sb.Append("----- ").Append(e.Title).AppendLine(" -----");
                sb.AppendLine(e.Description);
                if (e.Fields is { }) {
                    foreach (DiscordEmbedField f in e.Fields) {
                        sb.Append("# ").Append(f.Name).AppendLine(" #");
                        sb.AppendLine(f.Value);
                    }
                }
                if (e.Image is { })
                    sb.AppendLine(e.Image.Url.ToString());
                if (e.Footer is { })
                    sb.AppendLine("---").AppendLine(e.Footer.Text);
                sb.Append('-', 12 + (e.Title?.Length ?? 0));
                sb.AppendLine();
            }

            foreach (DiscordAttachment att in msg.Attachments)
                sb.Append("(A): ").Append(att.FileName).Append(" (").Append(att.FileSize).Append("): ").AppendLine(att.Url);

            return bs.BackupAsync(gid, cid, sb.ToString());
        }
    }
}
