using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Serilog;
using TheGodfather.Database;
using TheGodfather.Database.Models;
using TheGodfather.Services;

namespace TheGodfather.Modules.Administration.Services
{
    public sealed class BackupService : ITheGodfatherService, IDisposable
    {
        public bool IsDisabled => false;

        private readonly DbContextBuilder dbb;
        private readonly GuildConfigService gcs;
        private readonly ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, StreamWriter?>> streams;


        public BackupService(DbContextBuilder dbb, GuildConfigService gcs)
        {
            this.dbb = dbb;
            this.gcs = gcs;
            this.streams = new();
            this.LoadData();
        }


        public void Dispose()
        {
            foreach (ConcurrentDictionary<ulong, StreamWriter?> sws in this.streams.Values) {
                foreach (StreamWriter? sw in sws.Values)
                    sw?.Dispose();
            }
        }

        public async Task EnableAsync(ulong gid)
        {
            this.streams.AddOrUpdate(gid, _ => new(), (_, sws) => sws);
            await gcs.ModifyConfigAsync(gid, gcfg => gcfg.BackupEnabled = true);
        }

        public async Task DisableAsync(ulong gid)
        {
            this.streams.TryRemove(gid, out _);
            await gcs.ModifyConfigAsync(gid, gcfg => gcfg.BackupEnabled = false);
        }

        public async Task AddChannel(ulong gid, ulong cid)
        {
            if (this.streams.TryGetValue(gid, out ConcurrentDictionary<ulong, StreamWriter?>? sws)) {

                using TheGodfatherDbContext db = this.dbb.CreateContext();
                ExemptedBackupEntity? e = await db.ExemptsBackup.FindAsync((long)gid, (long)cid);
                if (e is null)
                    sws.TryAdd(cid, this.CreateStreamWriter(gid, cid));
                else
                    sws.TryAdd(cid, null);
            }
        }

        public void RemoveChannel(ulong gid, ulong cid)
        {
            if (this.streams.TryGetValue(gid, out ConcurrentDictionary<ulong, StreamWriter?>? sws))
                sws.TryRemove(cid, out _);
        }

        public bool IsBackupEnabledFor(ulong gid)
            => this.streams.ContainsKey(gid);

        public async Task<IReadOnlyList<ExemptedBackupEntity>> GetExemptsAsync(ulong gid)
        {
            List<ExemptedBackupEntity> exempts;
            using TheGodfatherDbContext db = this.dbb.CreateContext();
            exempts = await db.ExemptsBackup.Where(ex => ex.GuildIdDb == (long)gid).ToListAsync();
            return exempts.AsReadOnly();
        }

        public async Task ExemptAsync(ulong gid, IEnumerable<ulong> cids)
        {
            if (this.streams.TryGetValue(gid, out ConcurrentDictionary<ulong, StreamWriter?>? sws)) {
                foreach (ulong cid in cids) {
                    if (sws.TryGetValue(cid, out StreamWriter? sw) && sw is { }) {
                        try {
                            await sw.DisposeAsync();
                        } catch (IOException e) {
                            Log.Error(e, "Failed to close backup stream writer for channel: {ChannelId}", cid);
                        }
                        sws[cid] = null;
                    }
                }
            }

            using TheGodfatherDbContext db = this.dbb.CreateContext();
            db.ExemptsBackup.AddRange(cids
                .Where(cid => !db.ExemptsBackup.Where(dbe => dbe.GuildIdDb == (long)gid).Any(dbe => dbe.ChannelIdDb == (long)cid))
                .Select(cid => new ExemptedBackupEntity {
                    GuildId = gid,
                    ChannelId = cid,
                })
            );
            await db.SaveChangesAsync();
        }

        public async Task UnexemptAsync(ulong gid, IEnumerable<ulong> cids)
        {
            if (this.streams.TryGetValue(gid, out ConcurrentDictionary<ulong, StreamWriter?>? sws)) {
                foreach (ulong cid in cids)
                    sws.TryUpdate(cid, this.CreateStreamWriter(gid, cid), null);
            }

            using TheGodfatherDbContext db = this.dbb.CreateContext();
            db.ExemptsBackup.RemoveRange(
                db.ExemptsBackup.Where(ex => ex.GuildId == gid && cids.Any(id => id == ex.ChannelId))
            );
            await db.SaveChangesAsync();
        }

        public bool IsChannelExempted(ulong gid, ulong cid)
            => !this.streams.TryGetValue(gid, out ConcurrentDictionary<ulong, StreamWriter?>? sws) || (sws.TryGetValue(cid, out StreamWriter? sw) && sw is null);

        public async Task BackupAsync(ulong gid, ulong cid, string contents)
        {
            if (!this.streams.TryGetValue(gid, out ConcurrentDictionary<ulong, StreamWriter?>? sws))
                return;

            if (!sws.TryGetValue(cid, out _))
                await this.AddChannel(gid, cid);

            if (!sws.TryGetValue(cid, out StreamWriter? swNew) || swNew is null)
                return;

            try {
                await swNew.WriteLineAsync(contents);
            } catch (IOException e) {
                Log.Error(e, "Failed to write to backup stream for channel {ChannelId} in guild {", cid);
            }
        }


        private StreamWriter CreateStreamWriter(ulong gid, ulong cid)
        {
            string dirPath = Path.Combine("backup", gid.ToString());
            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);
            return new StreamWriter(Path.Combine(dirPath, $"{cid}.txt"), true, Encoding.UTF8) { AutoFlush = true };
        }

        private void LoadData()
        {
            using TheGodfatherDbContext db = this.dbb.CreateContext();
            var gids = db.Configs.Where(gcfg => gcfg.BackupEnabled).Select(gcfg => gcfg.GuildIdDb).ToList();
            foreach (long gid in gids)
                this.streams.TryAdd((ulong)gid, new());
        }
    }
}
