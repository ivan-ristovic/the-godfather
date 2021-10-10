using TheGodfather.Database.Models;

namespace TheGodfather.Modules.Administration.Extensions
{
    public static class ActionHistoryEntryExtensions
    {
        public static string ToLocalizedKey(this ActionHistoryEntry.Action actionType)
        {
            return actionType switch {
                ActionHistoryEntry.Action.CustomNote => "str-aht-cn",
                ActionHistoryEntry.Action.ForbiddenName => "str-aht-fn",
                ActionHistoryEntry.Action.TemporaryMute => "str-aht-tm",
                ActionHistoryEntry.Action.IndefiniteMute => "str-aht-pm",
                ActionHistoryEntry.Action.Kick => "str-aht-k",
                ActionHistoryEntry.Action.TemporaryBan => "str-aht-tb",
                ActionHistoryEntry.Action.PermanentBan => "str-aht-pb",
                ActionHistoryEntry.Action.Warning => "str-aht-w",
                _ => "str-404",
            };
        }
    }
}
