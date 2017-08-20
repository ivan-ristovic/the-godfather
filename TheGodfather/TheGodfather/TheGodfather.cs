using System;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.VoiceNext;


namespace TheGodfatherBot
{
    class TheGodfather
    {
        static DiscordClient discord { get; set; }
        static CommandsNextModule commands { get; set; }
        static InteractivityModule interactivity { get; set; }
        static VoiceNextClient voice { get; set; }

        public static void Main(string[] args) =>
            new TheGodfather().MainAsync(args).ConfigureAwait(false).GetAwaiter().GetResult();

        public async Task MainAsync(string[] args)
        {
            discord = new DiscordClient(new DiscordConfig {
                DiscordBranch = Branch.Stable,
                LargeThreshold = 250,
                Token = "",
                TokenType = TokenType.Bot,
                UseInternalLogHandler = true,
                LogLevel = LogLevel.Debug
            });
            discord.Ready += Client_Ready;
            discord.GuildAvailable += Client_GuildAvailable;
            discord.ClientError += Client_ClientError;

            commands = discord.UseCommandsNext(new CommandsNextConfiguration {
                StringPrefix = "!",
                EnableDms = false,
                CaseSensitive = false,
                EnableMentionPrefix = true
            });
            commands.RegisterCommands<CommandsAdmin>();
            commands.RegisterCommands<CommandsBase>();
            commands.RegisterCommands<CommandsGamble>();
            commands.RegisterCommands<CommandsImgur>();
            commands.RegisterCommands<CommandsMemes>();
            commands.RegisterCommands<CommandsVoice>();
            commands.RegisterCommands<CommandsSwat>();
            CommandsSwat.LoadServers();
            commands.CommandExecuted += Commands_CommandExecuted;
            commands.CommandErrored += Commands_CommandErrored;

            interactivity = discord.UseInteractivity();

            voice = discord.UseVoiceNext();

            discord.SetWebSocketClient<WebSocket4NetClient>();
            await discord.ConnectAsync();
            await Task.Delay(-1);
        }

        private Task Client_Ready(ReadyEventArgs e)
        {
            e.Client.DebugLogger.LogMessage(LogLevel.Info, "TheGodfather", "Ready.", DateTime.Now);
            return Task.CompletedTask;
        }

        private Task Client_GuildAvailable(GuildCreateEventArgs e)
        {
            e.Client.DebugLogger.LogMessage(LogLevel.Info, "TheGodfather", $"Guild available: {e.Guild.Name}", DateTime.Now);
            return Task.CompletedTask;
        }

        private Task Client_ClientError(ClientErrorEventArgs e)
        {
            e.Client.DebugLogger.LogMessage(LogLevel.Error, "TheGodfather", $"Exception occured: {e.Exception.GetType()}: {e.Exception.Message}", DateTime.Now);
            return Task.CompletedTask;
        }

        private Task Commands_CommandExecuted(CommandExecutedEventArgs e)
        {
            e.Context.Client.DebugLogger.LogMessage(LogLevel.Info, "TheGodfather", $"{e.Context.User.Username} successfully executed '{e.Command.QualifiedName}'", DateTime.Now);
            return Task.CompletedTask;
        }

        private async Task Commands_CommandErrored(CommandErrorEventArgs e)
        {

            e.Context.Client.DebugLogger.LogMessage(LogLevel.Error, "TheGodfather", $"{e.Context.User.Username} tried executing '{e.Command?.QualifiedName ?? "<unknown command>"}' but it errored: {e.Exception.GetType()}: {e.Exception.Message ?? "<no message>"}", DateTime.Now);

            if (e.Exception is ChecksFailedException ex) {
                var emoji = DiscordEmoji.FromName(e.Context.Client, ":no_entry:");
                var embed = new DiscordEmbed {
                    Title = "Access denied",
                    Description = $"{emoji} You do not have the permissions required to execute this command.",
                    Color = 0xFF0000
                };
                await e.Context.RespondAsync("", embed: embed);
            }
        }
    }
}


