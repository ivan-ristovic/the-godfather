using System.Collections.Concurrent;
using System.IO;
using System.IO.Compression;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace TheGodfather.Modules.Administration.Services;

public sealed class BackupService : ITheGodfatherService, IDisposable
{
    public bool IsDisabled => false;

    private readonly DbContextBuilder dbb;
    private readonly GuildConfigService gcs;
    private readonly ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, TextWriter?>> streams;


    public BackupService(DbContextBuilder dbb, GuildConfigService gcs)
    {
        this.dbb = dbb;
        this.gcs = gcs;
        this.streams = new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, TextWriter?>>();
        this.LoadData();
    }


    public void Dispose()
    {
        foreach (ConcurrentDictionary<ulong, TextWriter?> sws in this.streams.Values)
        foreach (TextWriter? sw in sws.Values)
            sw?.Dispose();
    }

    public async Task EnableAsync(ulong gid)
    {
        this.streams.AddOrUpdate(gid, _ => new ConcurrentDictionary<ulong, TextWriter?>(), (_, sws) => sws);
        await this.gcs.ModifyConfigAsync(gid, gcfg => gcfg.BackupEnabled = true);
    }

    public async Task DisableAsync(ulong gid)
    {
        if (this.streams.TryRemove(gid, out ConcurrentDictionary<ulong, TextWriter?>? sws))
            foreach (TextWriter? sw in sws.Values)
                sw?.Dispose();
        await this.gcs.ModifyConfigAsync(gid, gcfg => gcfg.BackupEnabled = false);
    }

    public async Task AddChannelAsync(ulong gid, ulong cid)
    {
        if (this.streams.TryGetValue(gid, out ConcurrentDictionary<ulong, TextWriter?>? sws)) {
            await using TheGodfatherDbContext db = this.dbb.CreateContext();
            ExemptedBackupEntity? e = await db.ExemptsBackup.FindAsync((long)gid, (long)cid);
            if (e is null)
                sws.TryAdd(cid, this.CreateTextWriter(gid, cid));
            else
                sws.TryAdd(cid, null);
        }
    }

    public void RemoveChannel(ulong gid, ulong cid)
    {
        if (this.streams.TryGetValue(gid, out ConcurrentDictionary<ulong, TextWriter?>? sws))
            sws.TryRemove(cid, out _);
    }

    public bool IsBackupEnabledFor(ulong gid)
        => this.streams.ContainsKey(gid);

    public async Task<IReadOnlyList<ExemptedBackupEntity>> GetExemptsAsync(ulong gid)
    {
        List<ExemptedBackupEntity> exempts;
        await using TheGodfatherDbContext db = this.dbb.CreateContext();
        exempts = await db.ExemptsBackup.AsQueryable().Where(ex => ex.GuildIdDb == (long)gid).ToListAsync();
        return exempts.AsReadOnly();
    }

    public async Task ExemptAsync(ulong gid, IEnumerable<ulong> cids)
    {
        if (this.streams.TryGetValue(gid, out ConcurrentDictionary<ulong, TextWriter?>? sws))
            foreach (ulong cid in cids)
                if (sws.TryGetValue(cid, out TextWriter? sw) && sw is { }) {
                    try {
                        await sw.DisposeAsync();
                    } catch (IOException e) {
                        Log.Error(e, "Failed to close backup stream writer for channel: {ChannelId}", cid);
                    }
                    sws[cid] = null;
                }

        await using TheGodfatherDbContext db = this.dbb.CreateContext();
        await db.ExemptsBackup.SafeAddRangeAsync(cids.Select(cid => new ExemptedBackupEntity {
            GuildId = gid,
            ChannelId = cid
        }), e => new object[] { e.GuildIdDb, e.ChannelIdDb });
        await db.SaveChangesAsync();
    }

    public async Task UnexemptAsync(ulong gid, IEnumerable<ulong> cids)
    {
        if (this.streams.TryGetValue(gid, out ConcurrentDictionary<ulong, TextWriter?>? sws))
            foreach (ulong cid in cids)
                sws.TryUpdate(cid, this.CreateTextWriter(gid, cid), null);

        await using TheGodfatherDbContext db = this.dbb.CreateContext();
        await db.ExemptsBackup.SafeRemoveRangeAsync(cids.Select(cid => new ExemptedBackupEntity {
            GuildId = gid,
            ChannelId = cid
        }), e => new object[] { e.GuildIdDb, e.ChannelIdDb });
        await db.SaveChangesAsync();
    }

    public bool IsChannelExempted(ulong gid, ulong cid)
        => !this.streams.TryGetValue(gid, out ConcurrentDictionary<ulong, TextWriter?>? sws) || (sws.TryGetValue(cid, out TextWriter? sw) && sw is null);

    public async Task BackupAsync(ulong gid, ulong cid, string contents)
    {
        if (!this.streams.TryGetValue(gid, out ConcurrentDictionary<ulong, TextWriter?>? sws))
            return;

        if (!sws.TryGetValue(cid, out _))
            await this.AddChannelAsync(gid, cid);

        if (!sws.TryGetValue(cid, out TextWriter? swNew) || swNew is null)
            return;

        try {
            await swNew.WriteLineAsync(contents);
        } catch (IOException e) {
            Log.Error(e, "Failed to write to backup stream for channel {ChannelId} in guild {", cid);
        }
    }

    public async Task<bool> WithBackupZipAsync(ulong gid, Func<Stream, Task> action)
    {
        string dirPath = Path.Combine("backup", gid.ToString());
        string zipPath = Path.Combine("backup", $"{gid}.zip");

        bool succ = false;
        try {
            ZipFile.CreateFromDirectory(dirPath, zipPath, CompressionLevel.Optimal, false);
            await using (var fs = new FileStream(zipPath, FileMode.Open)) {
                await action(fs);
            }

            succ = true;
        } finally {
            try {
                if (File.Exists(zipPath))
                    File.Delete(zipPath);
            } catch (IOException e) {
                Log.Error(e, "Failed to delete backup file: {ZipPath}", zipPath);
            }
        }

        return succ;
    }


    private TextWriter CreateTextWriter(ulong gid, ulong cid)
    {
        string dirPath = Path.Combine("backup", gid.ToString());
        if (!Directory.Exists(dirPath))
            Directory.CreateDirectory(dirPath);
        return TextWriter.Synchronized(new StreamWriter(Path.Combine(dirPath, $"{cid}.txt"), true, Encoding.UTF8) { AutoFlush = true });
    }

    private void LoadData()
    {
        using TheGodfatherDbContext db = this.dbb.CreateContext();
        var gids = db.Configs.AsQueryable().Where(gcfg => gcfg.BackupEnabled).Select(gcfg => gcfg.GuildIdDb).ToList();
        foreach (long gid in gids)
            this.streams.TryAdd((ulong)gid, new ConcurrentDictionary<ulong, TextWriter?>());
    }
}