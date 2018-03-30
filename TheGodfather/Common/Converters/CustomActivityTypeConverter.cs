#region USING_DIRECTIVES
using System.Threading.Tasks;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Common.Converters
{
    public class CustomActivityTypeConverter : IArgumentConverter<ActivityType>
    {
        public async Task<Optional<ActivityType>> ConvertAsync(string value, CommandContext ctx)
        {
            await Task.Delay(0);

            ActivityType activity = ActivityType.Playing;
            bool parses = true;
            switch (value.ToLowerInvariant()) {
                case "playing":
                case "plays":
                case "play":
                case "p":
                    activity = ActivityType.Playing;
                    break;
                case "watching":
                case "watches":
                case "watch":
                case "w":
                    activity = ActivityType.Watching;
                    break;
                case "streaming":
                case "streams":
                case "stream":
                case "s":
                    activity = ActivityType.Streaming;
                    break;
                case "listeningto":
                case "listensto":
                case "listento":
                case "listens":
                case "listening":
                case "l":
                    activity = ActivityType.ListeningTo;
                    break;
                default:
                    parses = false;
                    break;
            }

            if (parses)
                return new Optional<ActivityType>(activity);
            else
                return new Optional<ActivityType>();
        }
    }
}