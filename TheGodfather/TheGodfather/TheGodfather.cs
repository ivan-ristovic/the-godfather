using System;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.VoiceNext;
using System.IO;

namespace TheGodfatherBot
{
    class TheGodfather
    {
        static DiscordClient _client { get; set; }
        static CommandsNextModule _commands { get; set; }
        static InteractivityModule _interactivity { get; set; }
        static VoiceNextClient _voice { get; set; }


        public static void Main(string[] args) =>
            new TheGodfather().MainAsync(args).ConfigureAwait(false).GetAwaiter().GetResult();


        public async Task MainAsync(string[] args)
        {
            _client = new DiscordClient(new DiscordConfig {
                DiscordBranch = Branch.Stable,
                LargeThreshold = 250,
                Token = GetToken("token.txt"),
                TokenType = TokenType.Bot,
                UseInternalLogHandler = true,
                LogLevel = LogLevel.Debug
            });
            _client.Ready += Client_Ready;
            _client.GuildAvailable += Client_GuildAvailable;
            _client.ClientError += Client_ClientError;

            _commands = _client.UseCommandsNext(new CommandsNextConfiguration {
                StringPrefix = "!",
                EnableDms = false,
                CaseSensitive = false,
                EnableMentionPrefix = true
            });
            _commands.RegisterCommands<CommandsAdmin>();
            _commands.RegisterCommands<CommandsBase>();
            _commands.RegisterCommands<CommandsGamble>();
            _commands.RegisterCommands<CommandsImgur>();
            _commands.RegisterCommands<CommandsMemes>();
            _commands.RegisterCommands<CommandsVoice>();
            _commands.RegisterCommands<CommandsSwat>();
            CommandsSwat.LoadServers();
            _commands.CommandExecuted += Commands_CommandExecuted;
            _commands.CommandErrored += Commands_CommandErrored;

            _interactivity = _client.UseInteractivity();

            _voice = _client.UseVoiceNext();

            _client.SetWebSocketClient<WebSocket4NetClient>();
            await _client.ConnectAsync();
            await Task.Delay(-1);
        }

        private string GetToken(string filename)
        {
            if (!File.Exists(filename))
                return null;
            else
                return File.ReadAllLines("filename")[0].Trim();
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


