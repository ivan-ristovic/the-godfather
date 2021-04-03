using TheGodfather.Database.Models;

namespace TheGodfather.Modules.Administration.Extensions
{
    public static class ActionHistoryEntryExtensions
    {
        public static string ToLocalizedKey(this ActionHistoryEntry.ActionType actionType)
        {
            return actionType switch {
                ActionHistoryEntry.ActionType.CustomNote => "str-aht-cn",
                ActionHistoryEntry.ActionType.ForbiddenName => "str-aht-fn",
                ActionHistoryEntry.ActionType.TemporaryMute => "str-aht-tm",
                ActionHistoryEntry.ActionType.IndefiniteMute => "str-aht-pm",
                ActionHistoryEntry.ActionType.Kick => "str-aht-k",
                ActionHistoryEntry.ActionType.TemporaryBan => "str-aht-tb",
                ActionHistoryEntry.ActionType.PermanentBan => "str-aht-pb",
                ActionHistoryEntry.ActionType.Warning => "str-aht-w",
                _ => "str-404",
            };
        }
    }
}
