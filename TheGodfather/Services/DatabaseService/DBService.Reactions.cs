#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using TheGodfather.Modules.Reactions.Common;

using Npgsql;
using NpgsqlTypes;
#endregion

namespace TheGodfather.Services
{
    public partial class DBService
    {
        #region TEXT_REACTION_SERVICES
        public async Task<Dictionary<ulong, List<TextReaction>>> GetAllTextReactionsAsync()
        {
            var treactions = new Dictionary<ulong, List<TextReaction>>();

            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = "SELECT * FROM gf.text_reactions;";

                    using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                        while (await reader.ReadAsync().ConfigureAwait(false)) {
                            ulong gid = (ulong)(long)reader["gid"];
                            if (treactions.ContainsKey(gid)) {
                                if (treactions[gid] == null)
                                    treactions[gid] = new List<TextReaction>();
                            } else {
                                treactions.Add(gid, new List<TextReaction>());
                            }

                            string response = (string)reader["response"];
                            var r = new TextReaction((int)reader["id"], (string)reader["trigger"], (string)reader["response"], is_regex_trigger: true);
                            var reaction = treactions[gid].FirstOrDefault(tr => tr.Response == response);
                            if (reaction != null)
                                reaction.AddTrigger((string)reader["trigger"], is_regex_trigger: true);
                            else
                                treactions[gid].Add(r);
                        }
                    }
                }
            } finally {
                _sem.Release();
            }

            return treactions;
        }

        public async Task<int> AddTextReactionAsync(ulong gid, string trigger, string response, bool is_regex_trigger = false)
        {
            if (!is_regex_trigger)
                trigger = Regex.Escape(trigger);

            int id = 0;

            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = "INSERT INTO gf.text_reactions VALUES (@gid, @trigger, @response) RETURNING id;";
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, gid);
                    cmd.Parameters.AddWithValue("trigger", NpgsqlDbType.Varchar, trigger);
                    cmd.Parameters.AddWithValue("response", NpgsqlDbType.Varchar, response);

                    var res = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                    if (res != null && !(res is DBNull))
                        id = (int)res;
                }
            } finally {
                _sem.Release();
            }

            return id;
        }

        public async Task RemoveTextReactionsAsync(ulong gid, params int[] ids)
        {
            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = "DELETE FROM gf.text_reactions WHERE gid = @gid AND id = ANY(:ids);";
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, gid);
                    cmd.Parameters.Add("ids", NpgsqlDbType.Array | NpgsqlDbType.Integer).Value = ids;

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                _sem.Release();
            }
        }

        public async Task RemoveTextReactionTriggersAsync(ulong gid, string[] triggers)
        {
            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);
                    
                    cmd.CommandText = "DELETE FROM gf.text_reactions WHERE gid = @gid AND trigger = ANY(:triggers);";
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, gid);
                    cmd.Parameters.Add("triggers", NpgsqlDbType.Array | NpgsqlDbType.Varchar).Value = triggers;

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                _sem.Release();
            }
        }

        public async Task DeleteAllGuildTextReactionsAsync(ulong gid)
        {
            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = "DELETE FROM gf.text_reactions WHERE gid = @gid;";
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, gid);

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                _sem.Release();
            }
        }
        #endregion

        #region EMOJI_REACTION_SERVICES
        public async Task<Dictionary<ulong, List<EmojiReaction>>> GetAllEmojiReactionsAsync()
        {
            var ereactions = new Dictionary<ulong, List<EmojiReaction>>();

            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = "SELECT * FROM gf.emoji_reactions;";

                    using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                        while (await reader.ReadAsync().ConfigureAwait(false)) {
                            ulong gid = (ulong)(long)reader["gid"];

                            if (ereactions.ContainsKey(gid)) {
                                if (ereactions[gid] == null)
                                    ereactions[gid] = new List<EmojiReaction>();
                            } else {
                                ereactions.Add(gid, new List<EmojiReaction>());
                            }

                            string emoji = (string)reader["reaction"];
                            var reaction = ereactions[gid].FirstOrDefault(tr => tr.Response == emoji);
                            if (reaction != null)
                                reaction.AddTrigger((string)reader["trigger"], is_regex_trigger: true);
                            else
                                ereactions[gid].Add(new EmojiReaction((int)reader["id"], (string)reader["trigger"], emoji, is_regex_trigger: true));
                        }
                    }
                }
            } finally {
                _sem.Release();
            }

            return ereactions;
        }

        public async Task<int> AddEmojiReactionAsync(ulong gid, string trigger, string reaction, bool is_regex_trigger = false)
        {
            if (!is_regex_trigger)
                trigger = Regex.Escape(trigger);

            int id = 0;

            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = "INSERT INTO gf.emoji_reactions VALUES (@gid, @trigger, @reaction) RETURNING id;";
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, gid);
                    cmd.Parameters.AddWithValue("trigger", NpgsqlDbType.Varchar, trigger);
                    cmd.Parameters.AddWithValue("reaction", NpgsqlDbType.Varchar, reaction);

                    var res = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                    if (res != null && !(res is DBNull))
                        id = (int)res;
                }
            } finally {
                _sem.Release();
            }

            return id;
        }

        public async Task RemoveEmojiReactionTriggersAsync(ulong gid, string[] triggers)
        {
            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = "DELETE FROM gf.emoji_reactions WHERE gid = @gid AND trigger = ANY(:triggers);";
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, gid);
                    cmd.Parameters.Add("triggers", NpgsqlDbType.Array | NpgsqlDbType.Varchar).Value = triggers;

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                _sem.Release();
            }
        }

        public async Task RemoveEmojiReactionsAsync(ulong gid, params int[] ids)
        {
            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = "DELETE FROM gf.emoji_reactions WHERE gid = @gid AND id = ANY(:ids);";
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, gid);
                    cmd.Parameters.Add("ids", NpgsqlDbType.Array | NpgsqlDbType.Integer).Value = ids;

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                _sem.Release();
            }
        }

        public async Task RemoveAllEmojiReactionTriggersForReactionAsync(ulong gid, string reaction)
        {
            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = "DELETE FROM gf.emoji_reactions WHERE gid = @gid AND reaction = @reaction;";
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, gid);
                    cmd.Parameters.AddWithValue("reaction", NpgsqlDbType.Varchar, reaction);

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                _sem.Release();
            }
        }

        public async Task DeleteAllGuildEmojiReactionsAsync(ulong gid)
        {
            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = "DELETE FROM gf.emoji_reactions WHERE gid = @gid;";
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, gid);

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                _sem.Release();
            }
        }
        #endregion
    }
}
