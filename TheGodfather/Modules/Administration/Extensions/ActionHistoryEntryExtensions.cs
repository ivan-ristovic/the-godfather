using TheGodfather.Database.Models;
using TheGodfather.Translations;

namespace TheGodfather.Modules.Administration.Extensions
{
    public static class ActionHistoryEntryExtensions
    {
        public static TranslationKey ToLocalizedKey(this ActionHistoryEntry.Action actionType)
        {
            return actionType switch {
                ActionHistoryEntry.Action.CustomNote => TranslationKey.str_aht_cn,
                ActionHistoryEntry.Action.ForbiddenName => TranslationKey.str_aht_fn,
                ActionHistoryEntry.Action.TemporaryMute => TranslationKey.str_aht_tm,
                ActionHistoryEntry.Action.IndefiniteMute => TranslationKey.str_aht_pm,
                ActionHistoryEntry.Action.Kick => TranslationKey.str_aht_k,
                ActionHistoryEntry.Action.TemporaryBan => TranslationKey.str_aht_tb,
                ActionHistoryEntry.Action.PermanentBan => TranslationKey.str_aht_pb,
                ActionHistoryEntry.Action.Warning => TranslationKey.str_aht_w,
                _ => TranslationKey.str_404,
            };
        }
    }
}
