#region USING_DIRECTIVES
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;

using System.Threading.Tasks;
#endregion

namespace TheGodfather.Common.Converters
{
    public class CustomActivityTypeConverter : IArgumentConverter<ActivityType>
    {
        public static ActivityType? TryConvert(string value)
        {
            ActivityType result = ActivityType.Playing;
            bool parses = true;
            switch (value.ToLowerInvariant()) {
                case "playing":
                case "plays":
                case "play":
                case "p":
                    result = ActivityType.Playing;
                    break;
                case "watching":
                case "watches":
                case "watch":
                case "w":
                    result = ActivityType.Watching;
                    break;
                case "streaming":
                case "streams":
                case "stream":
                case "s":
                    result = ActivityType.Streaming;
                    break;
                case "listeningto":
                case "listensto":
                case "listento":
                case "listens":
                case "listening":
                case "l":
                    result = ActivityType.ListeningTo;
                    break;
                default:
                    parses = false;
                    break;
            }

            return parses ? result : (ActivityType?)null;
        }


        public Task<Optional<ActivityType>> ConvertAsync(string value, CommandContext ctx)
            => Task.FromResult(new Optional<ActivityType>(TryConvert(value).GetValueOrDefault()));
    }
}