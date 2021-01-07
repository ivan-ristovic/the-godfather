using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using TheGodfather.Common;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Music.Common;

namespace TheGodfather.Modules.Music
{
    public sealed partial class MusicModule
    {
        #region music remove
        [Command("remove")]
        [Aliases("dequeue", "delete", "rm", "del", "d", "-", "-=")]
        public Task RemoveAsync(CommandContext ctx,
                               [Description("desc-index-1")] int index)
        {
            if (index < 1 || index > this.Player.Queue.Count)
                throw new CommandFailedException(ctx, "cmd-err-index", 1, this.Player.Queue.Count);

            Song? removed = this.Player.Remove(index - 1);
            if (removed is null)
                throw new CommandFailedException(ctx);

            Song song = removed.Value;
            return ctx.InfoAsync(this.ModuleColor, Emojis.Headphones, "fmt-music-del", Formatter.Sanitize(song.Track.Title), Formatter.Sanitize(song.Track.Author));
        }
        #endregion
    }
}
