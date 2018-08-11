﻿#region USING_DIRECTIVES
using Npgsql;

using NpgsqlTypes;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

using TheGodfather.Common;
#endregion

namespace TheGodfather.Services
{
    internal static class DBServiceSavedTaskExtensions
    {
        public static async Task<int> AddSavedTaskAsync(this DBService db, SavedTaskInfo info)
        {
            int id = 0;

            await db.ExecuteCommandAsync(async (cmd) => {
                switch (info) {
                    case SendMessageTaskInfo smi:
                        cmd.CommandText = "INSERT INTO gf.saved_tasks(type, cid, uid, execution_time, comment) VALUES (@type, @cid, @uid, @execution_time, @comment) RETURNING id;";
                        cmd.Parameters.Add(new NpgsqlParameter<short>("type", (short)SavedTaskType.SendMessage));
                        cmd.Parameters.Add(new NpgsqlParameter<long>("cid", (long)smi.ChannelId));
                        cmd.Parameters.Add(new NpgsqlParameter<long>("uid", (long)smi.InitiatorId));
                        cmd.Parameters.Add(new NpgsqlParameter<string>("comment", smi.Message));
                        break;
                    case UnbanTaskInfo ubi:
                        cmd.CommandText = "INSERT INTO gf.saved_tasks(type, uid, gid, execution_time) VALUES (@type, @uid, @gid, @execution_time) RETURNING id;";
                        cmd.Parameters.Add(new NpgsqlParameter<short>("type", (short)SavedTaskType.Unban));
                        cmd.Parameters.Add(new NpgsqlParameter<long>("gid", (long)ubi.GuildId));
                        cmd.Parameters.Add(new NpgsqlParameter<long>("uid", (long)ubi.UnbanId));
                        break;
                    case UnmuteTaskInfo umi:
                        cmd.CommandText = "INSERT INTO gf.saved_tasks(type, uid, gid, rid, execution_time) VALUES (@type, @uid, @gid, @rid, @execution_time) RETURNING id;";
                        cmd.Parameters.Add(new NpgsqlParameter<short>("type", (short)SavedTaskType.Unmute));
                        cmd.Parameters.Add(new NpgsqlParameter<long>("gid", (long)umi.GuildId));
                        cmd.Parameters.Add(new NpgsqlParameter<long>("uid", (long)umi.UserId));
                        cmd.Parameters.Add(new NpgsqlParameter<long>("rid", (long)umi.MuteRoleId));
                        break;
                    default:
                        throw new ArgumentException("Unknown saved task info type!", nameof(info));
                }
                cmd.Parameters.AddWithValue("execution_time", NpgsqlDbType.TimestampTz, info.ExecutionTime);

                object res = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                if (res != null && !(res is DBNull))
                    id = (int)res;
            });

            return id;
        }

        public static async Task<IReadOnlyDictionary<int, SavedTaskInfo>> GetAllSavedTasksAsync(this DBService db)
        {
            var tasks = new Dictionary<int, SavedTaskInfo>();

            await db.ExecuteCommandAsync(async (cmd) => {
                cmd.CommandText = "SELECT * FROM gf.saved_tasks;";

                using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                    while (await reader.ReadAsync().ConfigureAwait(false)) {
                        int id = (int)reader["id"];
                        ulong gid = reader["gid"] is DBNull ? 0 : (ulong)(long)reader["gid"];
                        ulong cid = reader["cid"] is DBNull ? 0 : (ulong)(long)reader["cid"];
                        ulong uid = reader["uid"] is DBNull ? 0 : (ulong)(long)reader["uid"];
                        ulong rid = reader["rid"] is DBNull ? 0 : (ulong)(long)reader["rid"];
                        string message = reader["comment"] is DBNull ? null : (string)reader["comment"];
                        var exectime = (DateTime)reader["execution_time"];
                        switch ((SavedTaskType)(short)reader["type"]) {
                            case SavedTaskType.SendMessage:
                                tasks.Add(id, new SendMessageTaskInfo(cid, uid, message, exectime));
                                break;
                            case SavedTaskType.Unban:
                                tasks.Add(id, new UnbanTaskInfo(gid, uid, exectime));
                                break;
                            case SavedTaskType.Unmute:
                                tasks.Add(id, new UnmuteTaskInfo(gid, uid, rid, exectime));
                                break;
                        }
                    }
                }
            });

            return new ReadOnlyDictionary<int, SavedTaskInfo>(tasks);
        }

        public static Task RemoveSavedTaskAsync(this DBService db, int id)
        {
            return db.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "DELETE FROM gf.saved_tasks WHERE id = @id;";
                cmd.Parameters.Add(new NpgsqlParameter<int>("id", id));

                return cmd.ExecuteNonQueryAsync();
            });
        }
    }
}
