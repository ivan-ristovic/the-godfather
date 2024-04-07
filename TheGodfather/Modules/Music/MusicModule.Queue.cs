using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Lavalink4NET.Players;
using Lavalink4NET.Players.Queued;
using Lavalink4NET.Tracks;
using TheGodfather.Modules.Music.Common;

namespace TheGodfather.Modules.Music;

public sealed partial class MusicModule
{
    #region music queue
    [Command("queue")]
    [Aliases("q", "playlist")]
    public Task QueueAsync(CommandContext ctx)
    {
        if (this.Player.RepeatMode == TrackRepeatMode.Track) {
            LavalinkTrack? song = this.Player.CurrentTrack;
            if (song is null) {
                return Task.CompletedTask;
            }
            return ctx.ImpInfoAsync(this.ModuleColor, Emojis.Headphones, 
                TranslationKey.fmt_music_queue_rep(Formatter.Sanitize(song.Title), Formatter.Sanitize(song.Author))
            );
        }

        if (this.Player.Queue.IsEmpty)
            return ctx.ImpInfoAsync(this.ModuleColor, Emojis.Headphones, TranslationKey.str_music_queue_none);

        return ctx.PaginateAsync(this.Player.Queue, (emb, s) => {
            emb.WithLocalizedTitle(TranslationKey.str_music_queue);
            if (s.Track is not null) {
                emb.WithDescription(Formatter.Bold(Formatter.Sanitize(s.Track.Title)));
                emb.AddLocalizedField(TranslationKey.str_author, s.Track.Author, true);
                emb.AddLocalizedField(TranslationKey.str_duration, s.Track.Duration.ToDurationString(), true);
            }
            return emb;
        }, this.ModuleColor);
    }
    #endregion

    #region music remove
    [Command("remove")]
    [Aliases("dequeue", "delete", "rm", "del", "d", "-", "-=")]
    public async Task RemoveAsync(CommandContext ctx,
        [Description(TranslationKey.desc_index_1)] int index)
    {
        if (index < 1 || index > this.Player.Queue.Count)
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_index(1, this.Player.Queue.Count));
        index--;

        ITrackQueueItem song = this.Player.Queue[index];
        bool removed = await this.Player.Queue.RemoveAtAsync(index);
        if (!removed)
            throw new CommandFailedException(ctx);

        if (song.Track is not null)
            await ctx.InfoAsync(this.ModuleColor, Emojis.Headphones, TranslationKey.fmt_music_del(Formatter.Sanitize(song.Track.Title), Formatter.Sanitize(song.Track.Author)));
    }
    #endregion
    
    #region music clear
    [Command("clear")]
    [Aliases("removeall", "empty", "rmrf", "rma", "clearall", "delall", "da", "cl", "-a", "--", ">>>")]
    public async Task ClearAsync(CommandContext ctx)
    {
        int removed = await this.Player.Queue.RemoveAllAsync(_ => true);
        if (removed == 0)
            throw new CommandFailedException(ctx, TranslationKey.str_music_queue_none);
    
        await ctx.InfoAsync(this.ModuleColor, Emojis.Headphones, TranslationKey.fmt_music_del_many(removed));
    }
    #endregion
}