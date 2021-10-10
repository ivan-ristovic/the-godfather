using DSharpPlus.Entities;
using NUnit.Framework;
using TheGodfather.Common.Converters;
using TheGodfather.Modules.Administration.Common;

namespace TheGodfather.Tests.Common.Converters
{
    [TestFixture]
    public sealed class ConverterTests
    {
        [Test]
        public void ActivityTypeConverterTests()
        {
            var converter = new ActivityTypeConverter();

            this.AssertConvertSuccess(converter, "Playing", ActivityType.Playing);
            this.AssertConvertSuccess(converter, "plAys", ActivityType.Playing);
            this.AssertConvertSuccess(converter, "play", ActivityType.Playing);
            this.AssertConvertSuccess(converter, "plaaaay", ActivityType.Playing);
            this.AssertConvertSuccess(converter, "playyyyyy", ActivityType.Playing);
            this.AssertConvertSuccess(converter, "playz", ActivityType.Playing);
            this.AssertConvertSuccess(converter, "p", ActivityType.Playing);
            this.AssertConvertSuccess(converter, "P", ActivityType.Playing);
            this.AssertConvertFail<ActivityTypeConverter, ActivityType>(converter, "pl");
            this.AssertConvertFail<ActivityTypeConverter, ActivityType>(converter, "pl1ayy");
            this.AssertConvertFail<ActivityTypeConverter, ActivityType>(converter, "pl1ayz");
            this.AssertConvertFail<ActivityTypeConverter, ActivityType>(converter, "p1playing");
            this.AssertConvertFail<ActivityTypeConverter, ActivityType>(converter, "notplaying");

            this.AssertConvertSuccess(converter, "watcHing", ActivityType.Watching);
            this.AssertConvertSuccess(converter, "watches", ActivityType.Watching);
            this.AssertConvertSuccess(converter, "watchez", ActivityType.Watching);
            this.AssertConvertSuccess(converter, "watchs", ActivityType.Watching);
            this.AssertConvertSuccess(converter, "watchz", ActivityType.Watching);
            this.AssertConvertSuccess(converter, "waaaatch", ActivityType.Watching);
            this.AssertConvertSuccess(converter, "waaaatcccch", ActivityType.Watching);
            this.AssertConvertSuccess(converter, "watch", ActivityType.Watching);
            this.AssertConvertSuccess(converter, "w", ActivityType.Watching);
            this.AssertConvertSuccess(converter, "W", ActivityType.Watching);
            this.AssertConvertFail<ActivityTypeConverter, ActivityType>(converter, "wt");
            this.AssertConvertFail<ActivityTypeConverter, ActivityType>(converter, "wat");
            this.AssertConvertFail<ActivityTypeConverter, ActivityType>(converter, "watche");
            this.AssertConvertFail<ActivityTypeConverter, ActivityType>(converter, "wwatch3s");
            this.AssertConvertFail<ActivityTypeConverter, ActivityType>(converter, "notwatching");

            this.AssertConvertSuccess(converter, "streaming", ActivityType.Streaming);
            this.AssertConvertSuccess(converter, "Streams", ActivityType.Streaming);
            this.AssertConvertSuccess(converter, "streAm", ActivityType.Streaming);
            this.AssertConvertSuccess(converter, "streeeam", ActivityType.Streaming);
            this.AssertConvertSuccess(converter, "s", ActivityType.Streaming);
            this.AssertConvertSuccess(converter, "S", ActivityType.Streaming);
            this.AssertConvertFail<ActivityTypeConverter, ActivityType>(converter, "st");
            this.AssertConvertFail<ActivityTypeConverter, ActivityType>(converter, "str");
            this.AssertConvertFail<ActivityTypeConverter, ActivityType>(converter, "streming");
            this.AssertConvertFail<ActivityTypeConverter, ActivityType>(converter, "notstreaming");

            this.AssertConvertSuccess(converter, "lisTeningto", ActivityType.ListeningTo);
            this.AssertConvertSuccess(converter, "listensto", ActivityType.ListeningTo);
            this.AssertConvertSuccess(converter, "ListenTo", ActivityType.ListeningTo);
            this.AssertConvertSuccess(converter, "listens", ActivityType.ListeningTo);
            this.AssertConvertSuccess(converter, "listenz", ActivityType.ListeningTo);
            this.AssertConvertSuccess(converter, "listening", ActivityType.ListeningTo);
            this.AssertConvertSuccess(converter, "l", ActivityType.ListeningTo);
            this.AssertConvertSuccess(converter, "L", ActivityType.ListeningTo);
            this.AssertConvertFail<ActivityTypeConverter, ActivityType>(converter, "listnin");
            this.AssertConvertFail<ActivityTypeConverter, ActivityType>(converter, "listn");
            this.AssertConvertFail<ActivityTypeConverter, ActivityType>(converter, "lis");
            this.AssertConvertFail<ActivityTypeConverter, ActivityType>(converter, "notlistening");
        }

        [Test]
        public void BoolConverterTests()
        {
            var converter = new BoolConverter();

            this.AssertConvertSuccess(converter, "t", true);
            this.AssertConvertSuccess(converter, "y", true);
            this.AssertConvertSuccess(converter, "Y", true);
            this.AssertConvertSuccess(converter, "yy", true);
            this.AssertConvertSuccess(converter, "yY", true);
            this.AssertConvertSuccess(converter, "Ye", true);
            this.AssertConvertSuccess(converter, "yA", true);
            this.AssertConvertSuccess(converter, "yEs", true);
            this.AssertConvertSuccess(converter, "yea", true);
            this.AssertConvertSuccess(converter, "yup", true);
            this.AssertConvertSuccess(converter, "yEe", true);
            this.AssertConvertSuccess(converter, "yeah", true);
            this.AssertConvertSuccess(converter, "true", true);
            this.AssertConvertSuccess(converter, "True", true);
            this.AssertConvertSuccess(converter, "On", true);
            this.AssertConvertSuccess(converter, "enable", true);
            this.AssertConvertSuccess(converter, "1", true);
            this.AssertConvertFail<BoolConverter, bool>(converter, "yq");
            this.AssertConvertFail<BoolConverter, bool>(converter, "ohy");
            this.AssertConvertFail<BoolConverter, bool>(converter, "4");
            this.AssertConvertFail<BoolConverter, bool>(converter, "+");

            this.AssertConvertSuccess(converter, "f", false);
            this.AssertConvertSuccess(converter, "n", false);
            this.AssertConvertSuccess(converter, "no", false);
            this.AssertConvertSuccess(converter, "nn", false);
            this.AssertConvertSuccess(converter, "nAh", false);
            this.AssertConvertSuccess(converter, "nope", false);
            this.AssertConvertSuccess(converter, "false", false);
            this.AssertConvertSuccess(converter, "False", false);
            this.AssertConvertSuccess(converter, "of", false);
            this.AssertConvertSuccess(converter, "off", false);
            this.AssertConvertSuccess(converter, "diSable", false);
            this.AssertConvertSuccess(converter, "0", false);
            this.AssertConvertFail<BoolConverter, bool>(converter, "nwh");
            this.AssertConvertFail<BoolConverter, bool>(converter, "ohn");
            this.AssertConvertFail<BoolConverter, bool>(converter, "-10");
            this.AssertConvertFail<BoolConverter, bool>(converter, "-");

            this.AssertConvertFail<BoolConverter, bool>(converter, "yesno");
        }

        [Test]
        public void PunishmentActionConverterTests()
        {
            var converter = new PunishmentActionConverter();

            this.AssertConvertSuccess(converter, "k", PunishmentAction.Kick);
            this.AssertConvertSuccess(converter, "kick", PunishmentAction.Kick);
            this.AssertConvertSuccess(converter, "Kick", PunishmentAction.Kick);
            this.AssertConvertSuccess(converter, "kiK", PunishmentAction.Kick);
            this.AssertConvertSuccess(converter, "kiiick", PunishmentAction.Kick);
            this.AssertConvertSuccess(converter, "kiiik", PunishmentAction.Kick);
            this.AssertConvertSuccess(converter, "kk", PunishmentAction.Kick);
            this.AssertConvertSuccess(converter, "kiccck", PunishmentAction.Kick);
            this.AssertConvertSuccess(converter, "KICK", PunishmentAction.Kick);
            this.AssertConvertSuccess(converter, "kickedd", PunishmentAction.Kick);
            this.AssertConvertFail<PunishmentActionConverter, PunishmentAction>(converter, "kek");
            this.AssertConvertFail<PunishmentActionConverter, PunishmentAction>(converter, "ick");
            this.AssertConvertFail<PunishmentActionConverter, PunishmentAction>(converter, "kekick");
            this.AssertConvertFail<PunishmentActionConverter, PunishmentAction>(converter, "kickek");

            this.AssertConvertSuccess(converter, "b", PunishmentAction.PermanentBan);
            this.AssertConvertSuccess(converter, "ban", PunishmentAction.PermanentBan);
            this.AssertConvertSuccess(converter, "Ban", PunishmentAction.PermanentBan);
            this.AssertConvertSuccess(converter, "ben", PunishmentAction.PermanentBan);
            this.AssertConvertSuccess(converter, "baN", PunishmentAction.PermanentBan);
            this.AssertConvertSuccess(converter, "baaannn", PunishmentAction.PermanentBan);
            this.AssertConvertSuccess(converter, "bb", PunishmentAction.PermanentBan);
            this.AssertConvertSuccess(converter, "bbban", PunishmentAction.PermanentBan);
            this.AssertConvertSuccess(converter, "BAN", PunishmentAction.PermanentBan);
            this.AssertConvertSuccess(converter, "BanNedd", PunishmentAction.PermanentBan);
            this.AssertConvertSuccess(converter, "pb", PunishmentAction.PermanentBan);
            this.AssertConvertSuccess(converter, "pban", PunishmentAction.PermanentBan);
            this.AssertConvertSuccess(converter, "pBan", PunishmentAction.PermanentBan);
            this.AssertConvertSuccess(converter, "pben", PunishmentAction.PermanentBan);
            this.AssertConvertSuccess(converter, "pbaN", PunishmentAction.PermanentBan);
            this.AssertConvertSuccess(converter, "pbaaannn", PunishmentAction.PermanentBan);
            this.AssertConvertSuccess(converter, "pbb", PunishmentAction.PermanentBan);
            this.AssertConvertSuccess(converter, "pbbban", PunishmentAction.PermanentBan);
            this.AssertConvertSuccess(converter, "pBAN", PunishmentAction.PermanentBan);
            this.AssertConvertSuccess(converter, "pBanNedd", PunishmentAction.PermanentBan);
            this.AssertConvertSuccess(converter, "permb", PunishmentAction.PermanentBan);
            this.AssertConvertSuccess(converter, "permban", PunishmentAction.PermanentBan);
            this.AssertConvertSuccess(converter, "permBan", PunishmentAction.PermanentBan);
            this.AssertConvertSuccess(converter, "permben", PunishmentAction.PermanentBan);
            this.AssertConvertSuccess(converter, "permbaN", PunishmentAction.PermanentBan);
            this.AssertConvertSuccess(converter, "permbaaannn", PunishmentAction.PermanentBan);
            this.AssertConvertSuccess(converter, "permbb", PunishmentAction.PermanentBan);
            this.AssertConvertSuccess(converter, "permbbban", PunishmentAction.PermanentBan);
            this.AssertConvertSuccess(converter, "permBAN", PunishmentAction.PermanentBan);
            this.AssertConvertSuccess(converter, "permBanNedd", PunishmentAction.PermanentBan);
            this.AssertConvertSuccess(converter, "permab", PunishmentAction.PermanentBan);
            this.AssertConvertSuccess(converter, "permaban", PunishmentAction.PermanentBan);
            this.AssertConvertSuccess(converter, "permanentban", PunishmentAction.PermanentBan);
            this.AssertConvertFail<PunishmentActionConverter, PunishmentAction>(converter, "b@n");
            this.AssertConvertFail<PunishmentActionConverter, PunishmentAction>(converter, "bon");
            this.AssertConvertFail<PunishmentActionConverter, PunishmentAction>(converter, "8an");
            this.AssertConvertFail<PunishmentActionConverter, PunishmentAction>(converter, "permab@n");
            this.AssertConvertFail<PunishmentActionConverter, PunishmentAction>(converter, "bonperm");
            this.AssertConvertFail<PunishmentActionConverter, PunishmentAction>(converter, "8anperm");

            this.AssertConvertSuccess(converter, "m", PunishmentAction.PermanentMute);
            this.AssertConvertSuccess(converter, "mute", PunishmentAction.PermanentMute);
            this.AssertConvertSuccess(converter, "Mute", PunishmentAction.PermanentMute);
            this.AssertConvertSuccess(converter, "muted", PunishmentAction.PermanentMute);
            this.AssertConvertSuccess(converter, "muT", PunishmentAction.PermanentMute);
            this.AssertConvertSuccess(converter, "muuute", PunishmentAction.PermanentMute);
            this.AssertConvertSuccess(converter, "mutteeeddd", PunishmentAction.PermanentMute);
            this.AssertConvertSuccess(converter, "mm", PunishmentAction.PermanentMute);
            this.AssertConvertSuccess(converter, "mmmute", PunishmentAction.PermanentMute);
            this.AssertConvertSuccess(converter, "MUTE", PunishmentAction.PermanentMute);
            this.AssertConvertSuccess(converter, "MuTedD", PunishmentAction.PermanentMute);
            this.AssertConvertSuccess(converter, "pm", PunishmentAction.PermanentMute);
            this.AssertConvertSuccess(converter, "pmute", PunishmentAction.PermanentMute);
            this.AssertConvertSuccess(converter, "pMute", PunishmentAction.PermanentMute);
            this.AssertConvertSuccess(converter, "pmut", PunishmentAction.PermanentMute);
            this.AssertConvertSuccess(converter, "pmUt", PunishmentAction.PermanentMute);
            this.AssertConvertSuccess(converter, "pmmuuuttteee", PunishmentAction.PermanentMute);
            this.AssertConvertSuccess(converter, "pMuUUUTED", PunishmentAction.PermanentMute);
            this.AssertConvertSuccess(converter, "pmm", PunishmentAction.PermanentMute);
            this.AssertConvertSuccess(converter, "pmmmute", PunishmentAction.PermanentMute);
            this.AssertConvertSuccess(converter, "pMUTE", PunishmentAction.PermanentMute);
            this.AssertConvertSuccess(converter, "pMuteedd", PunishmentAction.PermanentMute);
            this.AssertConvertSuccess(converter, "permm", PunishmentAction.PermanentMute);
            this.AssertConvertSuccess(converter, "permmute", PunishmentAction.PermanentMute);
            this.AssertConvertSuccess(converter, "permMute", PunishmentAction.PermanentMute);
            this.AssertConvertSuccess(converter, "permmut", PunishmentAction.PermanentMute);
            this.AssertConvertSuccess(converter, "permmuT", PunishmentAction.PermanentMute);
            this.AssertConvertSuccess(converter, "permmuuuute", PunishmentAction.PermanentMute);
            this.AssertConvertSuccess(converter, "permmuuuuuttteeee", PunishmentAction.PermanentMute);
            this.AssertConvertSuccess(converter, "permmm", PunishmentAction.PermanentMute);
            this.AssertConvertSuccess(converter, "permmmmute", PunishmentAction.PermanentMute);
            this.AssertConvertSuccess(converter, "permMUTE", PunishmentAction.PermanentMute);
            this.AssertConvertSuccess(converter, "permMuteeDd", PunishmentAction.PermanentMute);
            this.AssertConvertSuccess(converter, "permam", PunishmentAction.PermanentMute);
            this.AssertConvertSuccess(converter, "permamute", PunishmentAction.PermanentMute);
            this.AssertConvertSuccess(converter, "permanentmute", PunishmentAction.PermanentMute);
            this.AssertConvertFail<PunishmentActionConverter, PunishmentAction>(converter, "mu7");
            this.AssertConvertFail<PunishmentActionConverter, PunishmentAction>(converter, "mut3");
            this.AssertConvertFail<PunishmentActionConverter, PunishmentAction>(converter, "ute");
            this.AssertConvertFail<PunishmentActionConverter, PunishmentAction>(converter, "permamt");
            this.AssertConvertFail<PunishmentActionConverter, PunishmentAction>(converter, "mperm");
            this.AssertConvertFail<PunishmentActionConverter, PunishmentAction>(converter, "apermm");

            this.AssertConvertSuccess(converter, "tb", PunishmentAction.TemporaryBan);
            this.AssertConvertSuccess(converter, "tban", PunishmentAction.TemporaryBan);
            this.AssertConvertSuccess(converter, "tBan", PunishmentAction.TemporaryBan);
            this.AssertConvertSuccess(converter, "tben", PunishmentAction.TemporaryBan);
            this.AssertConvertSuccess(converter, "tBaN", PunishmentAction.TemporaryBan);
            this.AssertConvertSuccess(converter, "tBaaaaannn", PunishmentAction.TemporaryBan);
            this.AssertConvertSuccess(converter, "tBanNeD", PunishmentAction.TemporaryBan);
            this.AssertConvertSuccess(converter, "tbb", PunishmentAction.TemporaryBan);
            this.AssertConvertSuccess(converter, "tbbbban", PunishmentAction.TemporaryBan);
            this.AssertConvertSuccess(converter, "tBAN", PunishmentAction.TemporaryBan);
            this.AssertConvertSuccess(converter, "tBanedd", PunishmentAction.TemporaryBan);
            this.AssertConvertSuccess(converter, "tmpb", PunishmentAction.TemporaryBan);
            this.AssertConvertSuccess(converter, "tmpban", PunishmentAction.TemporaryBan);
            this.AssertConvertSuccess(converter, "tempb", PunishmentAction.TemporaryBan);
            this.AssertConvertSuccess(converter, "tempban", PunishmentAction.TemporaryBan);
            this.AssertConvertSuccess(converter, "tempBan", PunishmentAction.TemporaryBan);
            this.AssertConvertSuccess(converter, "tempbann", PunishmentAction.TemporaryBan);
            this.AssertConvertSuccess(converter, "tempBanN", PunishmentAction.TemporaryBan);
            this.AssertConvertSuccess(converter, "tempBaNNnne", PunishmentAction.TemporaryBan);
            this.AssertConvertSuccess(converter, "tempbaaaaannnnnned", PunishmentAction.TemporaryBan);
            this.AssertConvertSuccess(converter, "tempbb", PunishmentAction.TemporaryBan);
            this.AssertConvertSuccess(converter, "tempbbban", PunishmentAction.TemporaryBan);
            this.AssertConvertSuccess(converter, "tempBAN", PunishmentAction.TemporaryBan);
            this.AssertConvertSuccess(converter, "tempBaneDd", PunishmentAction.TemporaryBan);
            this.AssertConvertSuccess(converter, "temporalb", PunishmentAction.TemporaryBan);
            this.AssertConvertSuccess(converter, "temporalban", PunishmentAction.TemporaryBan);
            this.AssertConvertSuccess(converter, "temporaryban", PunishmentAction.TemporaryBan);
            this.AssertConvertFail<PunishmentActionConverter, PunishmentAction>(converter, "tmu7");
            this.AssertConvertFail<PunishmentActionConverter, PunishmentAction>(converter, "tmut3");
            this.AssertConvertFail<PunishmentActionConverter, PunishmentAction>(converter, "tute");
            this.AssertConvertFail<PunishmentActionConverter, PunishmentAction>(converter, "tmppermamt");
            this.AssertConvertFail<PunishmentActionConverter, PunishmentAction>(converter, "mtemp");
            this.AssertConvertFail<PunishmentActionConverter, PunishmentAction>(converter, "atempm");

            this.AssertConvertSuccess(converter, "tm", PunishmentAction.TemporaryMute);
            this.AssertConvertSuccess(converter, "tmute", PunishmentAction.TemporaryMute);
            this.AssertConvertSuccess(converter, "tMute", PunishmentAction.TemporaryMute);
            this.AssertConvertSuccess(converter, "tmut", PunishmentAction.TemporaryMute);
            this.AssertConvertSuccess(converter, "tmUt", PunishmentAction.TemporaryMute);
            this.AssertConvertSuccess(converter, "tmmuuuttteee", PunishmentAction.TemporaryMute);
            this.AssertConvertSuccess(converter, "tMuUUUTED", PunishmentAction.TemporaryMute);
            this.AssertConvertSuccess(converter, "tmm", PunishmentAction.TemporaryMute);
            this.AssertConvertSuccess(converter, "tmmmute", PunishmentAction.TemporaryMute);
            this.AssertConvertSuccess(converter, "tMUTE", PunishmentAction.TemporaryMute);
            this.AssertConvertSuccess(converter, "tMuteedd", PunishmentAction.TemporaryMute);
            this.AssertConvertSuccess(converter, "tmpm", PunishmentAction.TemporaryMute);
            this.AssertConvertSuccess(converter, "tmpmute", PunishmentAction.TemporaryMute);
            this.AssertConvertSuccess(converter, "tempm", PunishmentAction.TemporaryMute);
            this.AssertConvertSuccess(converter, "tempmute", PunishmentAction.TemporaryMute);
            this.AssertConvertSuccess(converter, "tempMute", PunishmentAction.TemporaryMute);
            this.AssertConvertSuccess(converter, "tempmut", PunishmentAction.TemporaryMute);
            this.AssertConvertSuccess(converter, "tempmuT", PunishmentAction.TemporaryMute);
            this.AssertConvertSuccess(converter, "tempmuuuute", PunishmentAction.TemporaryMute);
            this.AssertConvertSuccess(converter, "tempmuuuuuttteeee", PunishmentAction.TemporaryMute);
            this.AssertConvertSuccess(converter, "tempmm", PunishmentAction.TemporaryMute);
            this.AssertConvertSuccess(converter, "tempmmmute", PunishmentAction.TemporaryMute);
            this.AssertConvertSuccess(converter, "tempMUTE", PunishmentAction.TemporaryMute);
            this.AssertConvertSuccess(converter, "tempMuteeDd", PunishmentAction.TemporaryMute);
            this.AssertConvertSuccess(converter, "temporalm", PunishmentAction.TemporaryMute);
            this.AssertConvertSuccess(converter, "temporalmute", PunishmentAction.TemporaryMute);
            this.AssertConvertSuccess(converter, "temporarymute", PunishmentAction.TemporaryMute);
            this.AssertConvertFail<PunishmentActionConverter, PunishmentAction>(converter, "tmu7");
            this.AssertConvertFail<PunishmentActionConverter, PunishmentAction>(converter, "tmut3");
            this.AssertConvertFail<PunishmentActionConverter, PunishmentAction>(converter, "tute");
            this.AssertConvertFail<PunishmentActionConverter, PunishmentAction>(converter, "tmppermamt");
            this.AssertConvertFail<PunishmentActionConverter, PunishmentAction>(converter, "mtemp");
            this.AssertConvertFail<PunishmentActionConverter, PunishmentAction>(converter, "atempm");

            this.AssertConvertFail<PunishmentActionConverter, PunishmentAction>(converter, "sanitycheck");
            this.AssertConvertFail<PunishmentActionConverter, PunishmentAction>(converter, "dontkickme");
            this.AssertConvertFail<PunishmentActionConverter, PunishmentAction>(converter, "plskick");
            this.AssertConvertFail<PunishmentActionConverter, PunishmentAction>(converter, "permanent");
            this.AssertConvertFail<PunishmentActionConverter, PunishmentAction>(converter, "temporary");
            this.AssertConvertFail<PunishmentActionConverter, PunishmentAction>(converter, "qwertyuioplkjhgfdsazxcvbnm");
            this.AssertConvertFail<PunishmentActionConverter, PunishmentAction>(converter, "tempkickban");
            this.AssertConvertFail<PunishmentActionConverter, PunishmentAction>(converter, "permkickmute");
            this.AssertConvertFail<PunishmentActionConverter, PunishmentAction>(converter, "bankick");
            this.AssertConvertFail<PunishmentActionConverter, PunishmentAction>(converter, "kickban");
            this.AssertConvertFail<PunishmentActionConverter, PunishmentAction>(converter, "kickmute");
            this.AssertConvertFail<PunishmentActionConverter, PunishmentAction>(converter, "mutekickban");
            this.AssertConvertFail<PunishmentActionConverter, PunishmentAction>(converter, "permandtempban");
        }


        private void AssertConvertSuccess<TConverter, TValue>(TConverter converter, string text, TValue expected) where TConverter : BaseArgumentConverter<TValue>
        {
            Assert.That(converter.TryConvert(text, out TValue? parsed), Is.True);
            Assert.That(parsed, Is.Not.Null);
            Assert.That(parsed, Is.EqualTo(expected));
        }

        private void AssertConvertFail<TConverter, TValue>(TConverter converter, string text) where TConverter : BaseArgumentConverter<TValue>
            => Assert.That(converter.TryConvert(text, out _), Is.False);
    }
}
