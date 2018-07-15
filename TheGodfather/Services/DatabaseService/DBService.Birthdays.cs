#region USING_DIRECTIVES
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TheGodfather.Modules.Misc.Common;
#endregion

namespace TheGodfather.Services.Database.Birthdays
{
    public static class DBServiceBirthdaysExtensions
    {
        public static Task AddBirthdayAsync(this DBService db, ulong uid, ulong cid, DateTime? date = null)
        {
            return db.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "INSERT INTO gf.birthdays VALUES (@uid, @cid, @date, date_part('year', CURRENT_DATE)) ON CONFLICT DO NOTHING;";
                cmd.Parameters.Add(new NpgsqlParameter("uid", (long)uid));
                cmd.Parameters.Add(new NpgsqlParameter("cid", (long)cid));
                cmd.Parameters.AddWithValue("date", NpgsqlDbType.Date, date?.Date ?? DateTime.UtcNow.Date);

                return cmd.ExecuteNonQueryAsync();
            });
        }

        public static async Task<IReadOnlyList<Birthday>> GetAllBirthdaysAsync(this DBService db)
        {
            var birthdays = new List<Birthday>();

            await db.ExecuteCommandAsync(async (cmd) => {
                cmd.CommandText = "SELECT uid, cid, bday FROM gf.birthdays;";

                using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                    while (await reader.ReadAsync().ConfigureAwait(false)) {
                        birthdays.Add(new Birthday() {
                            ChannelId = (ulong)(long)reader["cid"],
                            Date = (DateTime)reader["bday"],
                            UserId = (ulong)(long)reader["uid"]
                        });
                    }
                }
            });

            return birthdays.AsReadOnly();
        }

        public static async Task<IReadOnlyList<Birthday>> GetTodayBirthdaysAsync(this DBService db)
        {
            var birthdays = new List<Birthday>();

            await db.ExecuteCommandAsync(async (cmd) => {
                cmd.CommandText = "SELECT * FROM gf.birthdays WHERE date_part('day', bday) = @today_day AND date_part('month', bday) = @today_month AND last_updated != date_part('year', CURRENT_DATE);";
                cmd.Parameters.Add(new NpgsqlParameter("today_day", DateTime.UtcNow.Day));
                cmd.Parameters.Add(new NpgsqlParameter("today_month", DateTime.UtcNow.Month));

                using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                    while (await reader.ReadAsync().ConfigureAwait(false)) {
                        birthdays.Add(new Birthday() {
                            ChannelId = (ulong)(long)reader["cid"],
                            Date = (DateTime)reader["bday"],
                            UserId = (ulong)(long)reader["uid"]
                        });
                    }
                }
            });

            return birthdays.AsReadOnly();
        }

        public static Task RemoveBirthdayAsync(this DBService db, ulong uid)
        {
            return db.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "DELETE FROM gf.birthdays WHERE uid = @uid;";
                cmd.Parameters.Add(new NpgsqlParameter("uid", (long)uid));

                return cmd.ExecuteNonQueryAsync();
            });
        }

        public static Task UpdateBirthdayLastNotifiedDateAsync(this DBService db, ulong uid, ulong cid)
        {
            return db.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "UPDATE gf.birthdays SET last_updated = date_part('year', CURRENT_DATE) WHERE uid = @uid AND cid = @cid;";
                cmd.Parameters.Add(new NpgsqlParameter("uid", (long)uid));
                cmd.Parameters.Add(new NpgsqlParameter("cid", (long)cid));

                return cmd.ExecuteNonQueryAsync();
            });
        }
    }
}
