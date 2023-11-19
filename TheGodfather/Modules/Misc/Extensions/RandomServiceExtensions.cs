using DSharpPlus.Entities;
using TheGodfather.Modules.Misc.Services;

namespace TheGodfather.Modules.Misc.Extensions;

public static class RandomServiceExtensions
{
    public static TranslationKey EightBall(this RandomService service, DiscordChannel channel, string question)
    {
        if (question.StartsWith("when", StringComparison.InvariantCultureIgnoreCase) ||
            question.StartsWith("how long", StringComparison.InvariantCultureIgnoreCase)) {
            return service.GetRandomTimeAnswer();
        } else if (question.StartsWith("who", StringComparison.InvariantCultureIgnoreCase) && channel.Guild is not null) {
            var rng = new SecureRandom();
            DiscordMember member = rng.ChooseRandomElement(rng.NextBool(3)
                ? channel.Users.Where(m => IsOnline(m))
                : channel.Users.Where(m => !IsOnline(m))
            );
            return TranslationKey.wrap_1(member.Mention);
        } else if (question.StartsWith("how much", StringComparison.InvariantCultureIgnoreCase) ||
                   question.StartsWith("how many", StringComparison.InvariantCultureIgnoreCase)) {
            return service.GetRandomQuantityAnswer();
        } else {
            return service.GetRandomYesNoAnswer();
        }

        static bool IsOnline(DiscordMember m)
            => (m?.Presence?.Status ?? UserStatus.Offline) >= UserStatus.Online;
    }
}