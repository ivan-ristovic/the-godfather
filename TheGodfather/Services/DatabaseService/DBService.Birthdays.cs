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
        public async Task<IReadOnlyList<Birthday>> GetAllBirthdaysAsync()
        {
            var birthdays = new List<Birthday>();

            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = "SELECT * FROM gf.birthdays;";

                    using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                        while (await reader.ReadAsync().ConfigureAwait(false)) {
                            birthdays.Add(new Birthday(
                                (ulong)(long)reader["uid"],
                                (ulong)(long)reader["cid"],
                                (DateTime)reader["bday"]
                            ));
                        }
                    }
                }
            } finally {
                _sem.Release();
            }

            return birthdays.AsReadOnly();
        }

        public async Task<IReadOnlyList<Birthday>> GetTodayBirthdaysAsync()
        {
            var birthdays = new List<Birthday>();

            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = "SELECT * FROM gf.birthdays WHERE date_part('day', bday) = @today_day AND date_part('month', bday) = @today_month AND last_updated != date_part('year', CURRENT_DATE);";
                    cmd.Parameters.AddWithValue("today_day", NpgsqlDbType.Integer, DateTime.UtcNow.Day);
                    cmd.Parameters.AddWithValue("today_month", NpgsqlDbType.Integer, DateTime.UtcNow.Month);

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

        public async Task AddBirthdayAsync(ulong uid, ulong cid, DateTime? date = null)
        {
            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = "INSERT INTO gf.birthdays VALUES (@uid, @cid, @date, date_part('year', CURRENT_DATE)) ON CONFLICT DO NOTHING;";
                    cmd.Parameters.AddWithValue("uid", NpgsqlDbType.Bigint, uid);
                    cmd.Parameters.AddWithValue("cid", NpgsqlDbType.Bigint, cid);
                    cmd.Parameters.AddWithValue("date", NpgsqlDbType.Date, date?.Date ?? DateTime.Now.Date);

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                _sem.Release();
            }
        }

        public async Task RemoveBirthdayAsync(ulong uid)
        {
            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = "DELETE FROM gf.birthdays WHERE uid = @uid;";
                    cmd.Parameters.AddWithValue("uid", NpgsqlDbType.Bigint, uid);

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                _sem.Release();
            }
        }

        public async Task UpdateBirthdayLastNotifiedDateAsync(ulong uid, ulong cid)
        {
            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = "UPDATE gf.birthdays SET last_updated = date_part('year', CURRENT_DATE) WHERE uid = @uid AND cid = @cid;";
                    cmd.Parameters.AddWithValue("uid", NpgsqlDbType.Bigint, uid);
                    cmd.Parameters.AddWithValue("cid", NpgsqlDbType.Bigint, cid);

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                _sem.Release();
            }
        }
    }
}
