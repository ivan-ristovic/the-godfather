#region USING_DIRECTIVES
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;

using System.Threading.Tasks;

using TheGodfather.Modules.Administration.Common;
#endregion

namespace TheGodfather.Common.Converters
{
    public class CustomPunishmentActionTypeConverter : IArgumentConverter<PunishmentActionType>
    {
        public Task<Optional<PunishmentActionType>> ConvertAsync(string value, CommandContext ctx)
        {
            PunishmentActionType result = PunishmentActionType.Kick;
            bool parses = true;
            switch (value.ToLowerInvariant()) {
                case "silence":
                case "mute":
                case "m":
                    result = PunishmentActionType.Mute;
                    break;
                case "temporarymute":
                case "tempmute":
                case "tm":
                    result = PunishmentActionType.TemporaryMute;
                    break;
                case "ban":
                case "b":
                    result = PunishmentActionType.PermanentBan;
                    break;
                case "tempban":
                case "tb":
                    result = PunishmentActionType.TemporaryBan;
                    break;
                case "remove":
                case "kick":
                case "k":
                    result = PunishmentActionType.Kick;
                    break;
                default:
                    parses = false;
                    break;
            }

            if (parses)
                return Task.FromResult(new Optional<PunishmentActionType>(result));
            else
                return Task.FromResult(new Optional<PunishmentActionType>());
        }
    }
}