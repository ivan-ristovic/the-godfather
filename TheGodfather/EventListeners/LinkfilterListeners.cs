#region USING_DIRECTIVES
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
#endregion

namespace TheGodfather.EventListeners
{
    internal static class LinkfilterListeners
    {
        public static readonly Regex InviteRegex = new Regex(@"discord(?:\.gg|app\.com\/invite)\/([\w\-]+)", RegexOptions.Compiled);


        [AsyncExecuter(EventTypes.MessageCreated)]
        public static async Task Client_MessageCreatedLinkfilters(TheGodfatherShard shard, MessageCreateEventArgs e)
        {
            if (e.Author.IsBot || !TheGodfather.Listening || e.Channel.IsPrivate || shard.Shared.BlockedChannels.Contains(e.Channel.Id))
                return;

            if (e.Message?.Content == null)
                return;

            var gcfg = shard.Shared.GetGuildConfig(e.Guild.Id);
            if (!gcfg.LinkfilterEnabled)
                return;
            
            if ((e.Channel.PermissionsFor(e.Author as DiscordMember).HasPermission(Permissions.ManageMessages)))
                return;
            
            if (gcfg.BlockInvites && await DeleteInvitesAsync(shard, e).ConfigureAwait(false))
                return;
        }


        #region HELPER_FUNCTIONS
        private static async Task<bool> DeleteInvitesAsync(TheGodfatherShard shard, MessageCreateEventArgs e)
        {
            if (!InviteRegex.IsMatch(e.Message.Content))
                return false;

            try {
                await e.Channel.DeleteMessageAsync(e.Message, "_gf: Invite linkfilter")
                    .ConfigureAwait(false);
                shard.Log(LogLevel.Debug,
                    $"Linkfilter (invites) triggered in message: {e.Message.Content.Replace('\n', ' ')}<br>" +
                    $"{e.Message.Author.ToString()}<br>" +
                    $"{e.Guild.ToString()} | {e.Channel.ToString()}"
                );
            } catch (UnauthorizedException) {
                shard.Log(LogLevel.Debug,
                    $"Linkfilter (invites) triggered in message but missing permissions to delete!<br>" +
                    $"Message: {e.Message.Content.Replace('\n', ' ')}<br>" +
                    $"{e.Message.Author.ToString()}<br>" +
                    $"{e.Guild.ToString()} | {e.Channel.ToString()}"
                );
            }
            return true;
        }
        #endregion
    }
}
