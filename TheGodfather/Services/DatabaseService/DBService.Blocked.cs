#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Npgsql;
using NpgsqlTypes;
#endregion

namespace TheGodfather.Services
{
    public partial class DBService
    {
        public async Task<IReadOnlyList<ulong>> GetBlockedUsersAsync()
        {
            var blocked = new List<ulong>();

            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = "SELECT * FROM gf.blocked_users;";

                    using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                        while (await reader.ReadAsync().ConfigureAwait(false))
                            blocked.Add((ulong)(long)reader["uid"]);
                    }
                }
            } finally {
                _sem.Release();
            }

            return blocked.AsReadOnly();
        }

        public async Task AddBlockedUserAsync(ulong uid)
        {

        }

        public async Task RemoveBlockedUserAsync(ulong uid)
        {

        }

        public async Task<IReadOnlyList<ulong>> GetBlockedChannelsAsync()
        {
            return new List<ulong>();
        }

        public async Task AddBlockedChannelAsync(ulong cid)
        {

        }

        public async Task RemoveBlockedChannelAsync(ulong cid)
        {

        }
    }
}
