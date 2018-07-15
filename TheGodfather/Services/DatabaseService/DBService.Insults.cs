#region USING_DIRECTIVES
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
#endregion

namespace TheGodfather.Services.Database.Insults
{
    internal static class DBServiceInsultExtensions
    {
        public static Task AddInsultAsync(this DBService db, string insult)
        {
            return db.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "INSERT INTO gf.insults(insult) VALUES (@insult);";
                cmd.Parameters.Add(new NpgsqlParameter("insult", insult));

                return cmd.ExecuteNonQueryAsync();
            });
        }

        public static async Task<IReadOnlyDictionary<int, string>> GetAllInsultsAsync(this DBService db)
        {
            var insults = new Dictionary<int, string>();

            await db.ExecuteCommandAsync(async (cmd) => {
                cmd.CommandText = "SELECT * FROM gf.insults;";

                using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                        insults.Add((int)reader["id"], (string)reader["insult"]);
                }
            });

            return new ReadOnlyDictionary<int, string>(insults);
        }

        public static async Task<string> GetRandomInsultAsync(this DBService db)
        {
            string insult = null;

            await db.ExecuteCommandAsync(async (cmd) => {
                cmd.CommandText = "SELECT insult FROM gf.insults LIMIT 1 OFFSET floor(random() * (SELECT count(*) FROM gf.insults));";

                object res = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                if (res != null && !(res is DBNull))
                    insult = (string)res;
            });

            return insult;
        }

        public static Task RemoveInsultAsync(this DBService db, int id)
        {
            return db.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "DELETE FROM gf.insults WHERE id = @id;";
                cmd.Parameters.Add(new NpgsqlParameter("id", id));

                return cmd.ExecuteNonQueryAsync();
            });
        }

        public static Task RemoveAllInsultsAsync(this DBService db)
        {
            return db.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "DELETE FROM gf.insults;";

                return cmd.ExecuteNonQueryAsync();
            });
        }
    }
}
