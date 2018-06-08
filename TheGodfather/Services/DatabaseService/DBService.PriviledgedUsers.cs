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
        public async Task AddPriviledgedUserAsync(ulong uid)
        {
            await _sem.WaitAsync();
            try {
                using (var cmd = await OpenConnectionAndCreateCommandAsync()) {
                    cmd.CommandText = "INSERT INTO gf.priviledged (uid) VALUES (@uid);";
                    cmd.Parameters.AddWithValue("uid", NpgsqlDbType.Bigint, uid);

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                _sem.Release();
            }
        }

        public async Task<IReadOnlyList<ulong>> GetAllPriviledgedUsersAsync()
        {
            var priviledged = new List<ulong>();

            await _sem.WaitAsync();
            try {
                using (var cmd = await OpenConnectionAndCreateCommandAsync()) {
                    cmd.CommandText = "SELECT uid FROM gf.priviledged;";

                    using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                        while (await reader.ReadAsync().ConfigureAwait(false))
                            priviledged.Add((ulong)(long)reader["uid"]);
                    }
                }
            } finally {
                _sem.Release();
            }

            return priviledged.AsReadOnly();
        }

        public async Task<bool> IsPriviledgedUserAsync(ulong uid)
        {
            await _sem.WaitAsync();
            try {
                using (var cmd = await OpenConnectionAndCreateCommandAsync()) {
                    cmd.CommandText = "SELECT uid FROM gf.priviledged WHERE uid = @uid LIMIT 1;";
                    cmd.Parameters.AddWithValue("uid", NpgsqlDbType.Bigint, uid);

                    var res = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                    if (res != null && !(res is DBNull))
                        return true;
                }
            } finally {
                _sem.Release();
            }

            return false;
        }

        public async Task RemovePrivilegedUserAsync(ulong uid)
        {
            await _sem.WaitAsync();
            try {
                using (var cmd = await OpenConnectionAndCreateCommandAsync()) {
                    cmd.CommandText = "DELETE FROM gf.priviledged WHERE uid = @uid;";
                    cmd.Parameters.AddWithValue("uid", NpgsqlDbType.Bigint, uid);

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                _sem.Release();
            }
        }
    }
}
