using TheGodfather.Database.Models;

namespace TheGodfather.Modules.Misc.Common
{
    public enum StarboardActionType
    {
        None,
        Add,
        Update,
        Remove,
    }

    public readonly struct StarboardModificationResult
    {
        public StarboardMessage Entry { get; }
        public StarboardActionType ActionType { get; }

        public StarboardModificationResult(StarboardMessage entry, int starsPre, int minStars)
        {
            this.Entry = entry;
            if (starsPre < minStars && entry.Stars >= minStars)
                this.ActionType = StarboardActionType.Add;
            else if (starsPre >= minStars)
                this.ActionType = entry.Stars < minStars ? StarboardActionType.Remove : StarboardActionType.Update;
            else
                this.ActionType = StarboardActionType.None;
        }
    }
}
