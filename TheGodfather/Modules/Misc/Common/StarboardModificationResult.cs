using TheGodfather.Database.Models;

namespace TheGodfather.Modules.Misc.Common
{
    public enum StarboardActionType
    {
        None,
        Send,
        Modify,
        Delete,
    }

    public readonly struct StarboardModificationResult
    {
        public StarboardMessage? Entry { get; }
        public StarboardActionType ActionType { get; }

        public StarboardModificationResult(StarboardMessage? entry, StarboardActionType action)
        {
            this.Entry = entry;
            this.ActionType = action;
        }
    }
}
