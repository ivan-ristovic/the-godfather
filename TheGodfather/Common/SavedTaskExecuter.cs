#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using TheGodfather.Extensions;
using TheGodfather.Services;
using TheGodfather.Services.Common;
#endregion

namespace TheGodfather.Common
{
    public sealed class SavedTaskExecuter
    {
        public int Id { get; private set; }
        public SavedTask SavedTask { get; }

        private DiscordClient Client { get; }
        private SharedData Shared { get; }
        private DBService Database { get; }
        private CancellationTokenSource CTS { get; }
        private Task SystemTask { get; set; }


        public static async Task<bool> TryScheduleAsync(CommandContext ctx, SavedTask task)
        {
            var shared = ctx.Services.GetService<SharedData>();
            var db = ctx.Services.GetService<DBService>();
            try {
                int id = await db.AddSavedTaskAsync(task);
                var texec = new SavedTaskExecuter(id, ctx.Client, task, shared, db);
                texec.ScheduleExecution();
            } catch (Exception e) {
                shared.LogProvider.LogException(LogLevel.Warning, e);
                return false;
            }

            return true;
        }


        public SavedTaskExecuter(int id, DiscordClient client, SavedTask task, SharedData data, DBService db)
        {
            this.Id = id;
            this.Client = client;
            this.SavedTask = task;
            this.Shared = data;
            this.Database = db;
            this.CTS = new CancellationTokenSource();
        }


        public async Task DisposeAsync()
        {
            this.CTS.Cancel();
            await this.SystemTask;
        }

        private async Task ExecuteAsync()
        {
            try {
                await Task.Delay(this.SavedTask.TimeUntilExecution, this.CTS.Token);
                switch (this.SavedTask.Type) {
                    case SavedTaskType.SendMessage:
                        await SendMessageAsync();
                        break;
                    case SavedTaskType.Unban:
                        await UnbanUserAsync();
                        break;
                }
                this.Shared.LogProvider.LogMessage(LogLevel.Info,
                    $"| Saved task executed: {this.SavedTask.Type.ToTypeString()}\n" +
                    $"| Task comment: {this.SavedTask.Comment}]n" +
                    $"| User ID: {this.SavedTask.UserId}\n" +
                    $"| Guild ID: {this.SavedTask.GuildId}\n" +
                    $"| Channel ID: {this.SavedTask.ChannelId}"
                );
            } catch (TaskCanceledException) {

            } catch (Exception e) {
                this.Shared.LogProvider.LogException(LogLevel.Warning, e);
            } finally {
                await UnscheduleTask();
            }
        }

        public async Task HandleMissedExecutionAsync()
        {
            try {
                switch (this.SavedTask.Type) {
                    case SavedTaskType.SendMessage:
                        var channel = await this.Client.GetChannelAsync(this.SavedTask.ChannelId);
                        var user = await this.Client.GetUserAsync(this.SavedTask.UserId);
                        await channel.SendFailedEmbedAsync($"I have been asleep and failed to remind {user.Mention} to:\n\n{Formatter.Italic(this.SavedTask.Comment)}\n\nat {this.SavedTask.ExecutionTime.ToLongTimeString()} UTC");
                        break;
                    case SavedTaskType.Unban:
                        await UnbanUserAsync();
                        break;
                    default:
                        break;
                }
                this.Shared.LogProvider.LogMessage(LogLevel.Warning,
                    $"| Executed missed task: {this.SavedTask.Type.ToTypeString()}\n" +
                    $"| Task comment: {this.SavedTask.Comment}\n" +
                    $"| User ID: {this.SavedTask.UserId}\n" +
                    $"| Guild ID: {this.SavedTask.GuildId}\n" +
                    $"| Channel ID: {this.SavedTask.ChannelId}"
                );
            } catch (Exception e) {
                this.Shared.LogProvider.LogException(LogLevel.Warning, e);
            } finally {
                await UnscheduleTask();
            }
        }

        public void ScheduleExecution()
        {
            this.Shared.TaskExecuters.TryAdd(this.Id, this);
            this.SystemTask = ExecuteAsync();
        }


        #region CALLBACKS
        public async Task UnscheduleTask()
        {
            if (this.Shared.TaskExecuters.ContainsKey(this.Id))
                this.Shared.TaskExecuters.TryRemove(this.Id, out var _);
            await this.Database.RemoveSavedTaskAsync(this.Id);
        }

        private async Task SendMessageAsync()
        {
            var channel = await this.Client.GetChannelAsync(this.SavedTask.ChannelId);
            var user = await this.Client.GetUserAsync(this.SavedTask.UserId);
            await channel.SendIconEmbedAsync($"{user.Mention}'s reminder:\n\n{Formatter.Italic(this.SavedTask.Comment)}", DiscordEmoji.FromName(this.Client, ":alarm_clock:"));
        }

        private async Task UnbanUserAsync()
        {
            var guild = await this.Client.GetGuildAsync(this.SavedTask.GuildId);
            var user = await this.Client.GetUserAsync(this.SavedTask.UserId);
            await guild.UnbanMemberAsync(user, $"Temporary ban time expired");
        }
        #endregion
    }
}
