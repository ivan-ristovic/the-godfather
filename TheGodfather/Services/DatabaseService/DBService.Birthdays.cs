#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

using TheGodfather.Services.Common;

using Npgsql;
using NpgsqlTypes;
#endregion

namespace TheGodfather.Services
{
    public partial class DBService
    {
        public async Task<IReadOnlyList<Birthday>> GetTodayBirthdaysAsync()
        {
            var birthdays = new List<Birthday>();

            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = "SELECT * FROM gf.birthdays WHERE bday = @today;";
                    cmd.Parameters.AddWithValue("today", NpgsqlDbType.Date, DateTime.UtcNow.Date);

                    using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                        while (await reader.ReadAsync().ConfigureAwait(false))
                            birthdays.Add(new Birthday((ulong)(long)reader["uid"], (ulong)(long)reader["cid"]));
                    }
                }
            } finally {
                _sem.Release();
            }

            return birthdays.AsReadOnly();
        }

        public async Task<int> AddBirthdayAsync(ulong uid, ulong cid)
        {
            int id = 0;

            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = "INSERT INTO gf.birthdays VALUES (@uid, @cid, CURRENT_DATE, date_part('year', CURRENT_DATE));";
                    cmd.Parameters.AddWithValue("uid", NpgsqlDbType.Bigint, uid);
                    cmd.Parameters.AddWithValue("cid", NpgsqlDbType.Bigint, cid);

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                _sem.Release();
            }

            return id;
        }

        public async Task RemoveBirthdayAsync(int uid)
        {
            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = "DELETE FROM gf.birthdays WHERE uid = @uid;";
                    cmd.Parameters.AddWithValue("uid", NpgsqlDbType.Integer, uid);

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                _sem.Release();
            }
        }
    }
}
