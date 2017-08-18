using System;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.VoiceNext;


namespace TheGodfatherBot
{
    [Description("Voice & music commands.")]
    public class CommandsVoice
    {
        [Command("join")]
        [Description("Connects me to your voice channel.")]
        [Aliases("connect", "voice")]
        public async Task Join(CommandContext ctx)
        {
            var vnext = ctx.Client.GetVoiceNextClient();

            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc != null)
            {
                await ctx.RespondAsync("Already connected in this guild.");
                throw new InvalidOperationException("Already connected in this guild.");
            }
                
            var chn = ctx.Member?.VoiceState?.Channel;
            if (chn == null)
            {
                await ctx.RespondAsync("You need to be in a voice channel.");
                throw new InvalidOperationException("You need to be in a voice channel.");
            }

            vnc = await vnext.ConnectAsync(chn);
            await ctx.RespondAsync("Connected.");
        }

        [Command("leave")]
        [Description("Disconnects from voice channel.")]
        [Aliases("disconnect")]
        public async Task Leave(CommandContext ctx)
        {
            var vnext = ctx.Client.GetVoiceNextClient();

            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc == null)
            {
                await ctx.RespondAsync("Not connected in this guild.");
                throw new InvalidOperationException("Not connected in this guild.");
            }

            vnc.Disconnect();
            await ctx.RespondAsync("Disconnected.");
        }
    }
}
