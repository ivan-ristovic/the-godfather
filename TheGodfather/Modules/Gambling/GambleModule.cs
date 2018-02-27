#region USING_DIRECTIVES
using TheGodfather.Attributes;
using TheGodfather.Services;

using DSharpPlus.CommandsNext.Attributes;
#endregion

namespace TheGodfather.Modules.Gambling
{
    [Group("gamble")]
    [Description("Betting and gambling commands.")]
    [Aliases("bet")]
    [Cooldown(2, 3, CooldownBucketType.User), Cooldown(5, 3, CooldownBucketType.Channel)]
    [ListeningCheck]
    public partial class GambleModule : TheGodfatherBaseModule
    {
        public GambleModule(DBService db) : base(db: db) { }
    }
}
