using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;
using TheGodfather.Modules.Administration.Common;

namespace TheGodfather.Common.Converters
{
    public class PunishmentActionConverter : IArgumentConverter<PunishmentAction>
    {
        public static PunishmentAction? TryConvert(string value)
        {
            PunishmentAction result = PunishmentAction.Kick;
            bool parses = true;
            switch (value.ToLowerInvariant()) {
                case "silence":
                case "mute":
                case "m":
                    result = PunishmentAction.PermanentMute;
                    break;
                case "temporarymute":
                case "tempmute":
                case "tempm":
                case "tmpm":
                case "tm":
                    result = PunishmentAction.TemporaryMute;
                    break;
                case "ban":
                case "b":
                    result = PunishmentAction.PermanentBan;
                    break;
                case "temporaryban":
                case "tempban":
                case "tmpban":
                case "tempb":
                case "tmpb":
                case "tb":
                    result = PunishmentAction.TemporaryBan;
                    break;
                case "remove":
                case "kick":
                case "k":
                    result = PunishmentAction.Kick;
                    break;
                default:
                    parses = false;
                    break;
            }

            return parses ? result : (PunishmentAction?)null;
        }


        public Task<Optional<PunishmentAction>> ConvertAsync(string value, CommandContext ctx)
            => Task.FromResult(new Optional<PunishmentAction>(TryConvert(value).GetValueOrDefault()));
    }
}