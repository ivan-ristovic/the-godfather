using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using TheGodfather.Modules.Music.Common;

namespace TheGodfather.Modules.Music;

public sealed partial class MusicModule
{
    #region music queue
    [Command("queue")]
    [Aliases("q", "playlist")]
    public Task QueueAsync(CommandContext ctx)
    {
        if (this.Player.RepeatMode == RepeatMode.Single) {
            Song song = this.Player.NowPlaying;
            return ctx.ImpInfoAsync(this.ModuleColor, Emojis.Headphones, 
                TranslationKey.fmt_music_queue_rep(Formatter.Sanitize(song.Track.Title), Formatter.Sanitize(song.Track.Author))
            );
        }

        if (!this.Player.Queue.Any())
            return ctx.ImpInfoAsync(this.ModuleColor, Emojis.Headphones, TranslationKey.str_music_queue_none);

        return ctx.PaginateAsync(this.Player.Queue, (emb, s) => {
            emb.WithLocalizedTitle(TranslationKey.str_music_queue);
            emb.WithDescription(Formatter.Bold(Formatter.Sanitize(s.Track.Title)));
            emb.AddLocalizedField(TranslationKey.str_author, s.Track.Author, true);
            emb.AddLocalizedField(TranslationKey.str_duration, s.Track.Length.ToDurationString(), true);
            emb.AddLocalizedField(TranslationKey.str_requested_by, s.RequestedBy?.Mention, true);
            return emb;
        }, this.ModuleColor);
    }
    #endregion

    #region music remove
    [Command("remove")]
    [Aliases("dequeue", "delete", "rm", "del", "d", "-", "-=")]
    public Task RemoveAsync(CommandContext ctx,
        [Description(TranslationKey.desc_index_1)] int index)
    {
        if (index < 1 || index > this.Player.Queue.Count)
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_index(1, this.Player.Queue.Count));

        Song? removed = this.Player.Remove(index - 1);
        if (removed is null)
            throw new CommandFailedException(ctx);

        Song song = removed.Value;
        return ctx.InfoAsync(this.ModuleColor, Emojis.Headphones, TranslationKey.fmt_music_del(Formatter.Sanitize(song.Track.Title), Formatter.Sanitize(song.Track.Author)));
    }
    #endregion
    
    #region music clear
    [Command("clear")]
    [Aliases("removeall", "empty", "rmrf", "rma", "clearall", "delall", "da", "cl", "-a", "--", ">>>")]
    public Task ClearAsync(CommandContext ctx)
    {
        int removed = this.Player.EmptyQueue();
        if (removed == 0)
            throw new CommandFailedException(ctx);
    
        return ctx.InfoAsync(this.ModuleColor, Emojis.Headphones, TranslationKey.fmt_music_del_many(removed));
    }
    #endregion
}