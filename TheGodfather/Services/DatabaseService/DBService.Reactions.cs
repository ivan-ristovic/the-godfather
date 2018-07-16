#region USING_DIRECTIVES
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TheGodfather.Modules.Reactions.Common;
#endregion

namespace TheGodfather.Services.Database.Reactions
{
    internal static class DBServiceReactionExtensions
    {
        public static async Task<int> AddEmojiReactionAsync(this DBService db, ulong gid, string trigger, string reaction, bool regex = false)
        {
            if (!regex)
                trigger = Regex.Escape(trigger);

            int id = 0;

            await db.ExecuteCommandAsync(async (cmd) => {
                cmd.CommandText = "INSERT INTO gf.emoji_reactions VALUES (@gid, @trigger, @reaction) RETURNING id;";
                cmd.Parameters.Add(new NpgsqlParameter<long>("gid", (long)gid));
                cmd.Parameters.Add(new NpgsqlParameter<string>("trigger", trigger));
                cmd.Parameters.Add(new NpgsqlParameter<string>("reaction", reaction));

                object res = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                if (res != null && !(res is DBNull))
                    id = (int)res;
            });

            return id;
        }

        public static async Task<int> AddTextReactionAsync(this DBService db, ulong gid, string trigger, string response, bool regex = false)
        {
            if (!regex)
                trigger = Regex.Escape(trigger);

            int id = 0;

            await db.ExecuteCommandAsync(async (cmd) => {
                cmd.CommandText = "INSERT INTO gf.text_reactions VALUES (@gid, @trigger, @response) RETURNING id;";
                cmd.Parameters.Add(new NpgsqlParameter<long>("gid", (long)gid));
                cmd.Parameters.Add(new NpgsqlParameter<string>("trigger", trigger));
                cmd.Parameters.Add(new NpgsqlParameter<string>("response", response));

                object res = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                if (res != null && !(res is DBNull))
                    id = (int)res;
            });

            return id;
        }

        public static async Task<IReadOnlyDictionary<ulong, List<EmojiReaction>>> GetEmojiReactionsForAllGuildsAsync(this DBService db)
        {
            var ereactions = new Dictionary<ulong, List<EmojiReaction>>();

            await db.ExecuteCommandAsync(async (cmd) => {
                cmd.CommandText = "SELECT id, gid, trigger, reaction FROM gf.emoji_reactions;";

                using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                    while (await reader.ReadAsync().ConfigureAwait(false)) {
                        ulong gid = (ulong)(long)reader["gid"];

                        if (ereactions.ContainsKey(gid)) {
                            if (ereactions[gid] == null)
                                ereactions[gid] = new List<EmojiReaction>();
                        } else {
                            ereactions.Add(gid, new List<EmojiReaction>());
                        }

                        int id = (int)reader["id"];
                        string trigger = (string)reader["trigger"];
                        string emoji = (string)reader["reaction"];

                        EmojiReaction conflict = ereactions[gid].FirstOrDefault(tr => tr.Response == emoji);
                        if (conflict != null) {
                            conflict.AddTrigger(trigger, regex: true);
                        } else {
                            ereactions[gid].Add(new EmojiReaction(id, trigger, emoji, regex: true));
                        }
                    }
                }
            });

            return new ReadOnlyDictionary<ulong, List<EmojiReaction>>(ereactions);
        }

        public static async Task<IReadOnlyDictionary<ulong, List<TextReaction>>> GetTextReactionsForAllGuildsAsync(this DBService db)
        {
            var treactions = new Dictionary<ulong, List<TextReaction>>();

            await db.ExecuteCommandAsync(async (cmd) => {
                cmd.CommandText = "SELECT id, gid, trigger, response FROM gf.text_reactions;";

                using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                    while (await reader.ReadAsync().ConfigureAwait(false)) {
                        ulong gid = (ulong)(long)reader["gid"];
                        if (treactions.ContainsKey(gid)) {
                            if (treactions[gid] == null)
                                treactions[gid] = new List<TextReaction>();
                        } else {
                            treactions.Add(gid, new List<TextReaction>());
                        }

                        int id = (int)reader["id"];
                        string trigger = (string)reader["trigger"];
                        string response = (string)reader["response"];

                        TextReaction conflict = treactions[gid].FirstOrDefault(tr => tr.Response == response);
                        if (conflict != null) {
                            conflict.AddTrigger(trigger, regex: true);
                        } else {
                            treactions[gid].Add(new TextReaction(id, trigger, response, regex: true));
                        }
                    }
                }
            });

            return new ReadOnlyDictionary<ulong, List<TextReaction>>(treactions);
        }

        public static Task RemoveAllGuildEmojiReactionsAsync(this DBService db, ulong gid)
        {
            return db.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "DELETE FROM gf.emoji_reactions WHERE gid = @gid;";
                cmd.Parameters.Add(new NpgsqlParameter<long>("gid", (long)gid));

                return cmd.ExecuteNonQueryAsync();
            });
        }

        public static Task RemoveAllGuildTextReactionsAsync(this DBService db, ulong gid)
        {
            return db.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "DELETE FROM gf.text_reactions WHERE gid = @gid;";
                cmd.Parameters.Add(new NpgsqlParameter<long>("gid", (long)gid));

                return cmd.ExecuteNonQueryAsync();
            });
        }

        public static Task RemoveAllTriggersForEmojiReactionAsync(this DBService db, ulong gid, string reaction)
        {
            return db.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "DELETE FROM gf.emoji_reactions WHERE gid = @gid AND reaction = @reaction;";
                cmd.Parameters.Add(new NpgsqlParameter<long>("gid", (long)gid));
                cmd.Parameters.Add(new NpgsqlParameter<string>("reaction", reaction));

                return cmd.ExecuteNonQueryAsync();
            });
        }

        public static Task RemoveEmojiReactionsAsync(this DBService db, ulong gid, params int[] ids)
        {
            return db.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "DELETE FROM gf.emoji_reactions WHERE gid = @gid AND id = ANY(:ids);";
                cmd.Parameters.Add(new NpgsqlParameter<long>("gid", (long)gid));
                cmd.Parameters.Add("ids", NpgsqlDbType.Array | NpgsqlDbType.Integer).Value = ids;

                return cmd.ExecuteNonQueryAsync();
            });
        }

        public static Task RemoveEmojiReactionTriggersAsync(this DBService db, ulong gid, string[] triggers)
        {
            return db.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "DELETE FROM gf.emoji_reactions WHERE gid = @gid AND trigger = ANY(:triggers);";
                cmd.Parameters.Add(new NpgsqlParameter<long>("gid", (long)gid));
                cmd.Parameters.Add("triggers", NpgsqlDbType.Array | NpgsqlDbType.Varchar).Value = triggers;

                return cmd.ExecuteNonQueryAsync();
            });
        }

        public static Task RemoveTextReactionsAsync(this DBService db, ulong gid, params int[] ids)
        {
            return db.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "DELETE FROM gf.text_reactions WHERE gid = @gid AND id = ANY(:ids);";
                cmd.Parameters.Add(new NpgsqlParameter<long>("gid", (long)gid));
                cmd.Parameters.Add("ids", NpgsqlDbType.Array | NpgsqlDbType.Integer).Value = ids;

                return cmd.ExecuteNonQueryAsync();
            });
        }

        public static Task RemoveTextReactionTriggersAsync(this DBService db, ulong gid, string[] triggers)
        {
            return db.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "DELETE FROM gf.text_reactions WHERE gid = @gid AND trigger = ANY(:triggers);";
                cmd.Parameters.Add(new NpgsqlParameter<long>("gid", (long)gid));
                cmd.Parameters.Add("triggers", NpgsqlDbType.Array | NpgsqlDbType.Varchar).Value = triggers;

                return cmd.ExecuteNonQueryAsync();
            });
        }
    }
}
