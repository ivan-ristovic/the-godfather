using DSharpPlus.Entities;
using NUnit.Framework;
using TheGodfather.Common.Converters;
using TheGodfather.Database.Models;

namespace TheGodfather.Tests.Common.Converters;

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

        this.AssertConvertSuccess(converter, "k", Punishment.Action.Kick);
        this.AssertConvertSuccess(converter, "kick", Punishment.Action.Kick);
        this.AssertConvertSuccess(converter, "Kick", Punishment.Action.Kick);
        this.AssertConvertSuccess(converter, "kiK", Punishment.Action.Kick);
        this.AssertConvertSuccess(converter, "kiiick", Punishment.Action.Kick);
        this.AssertConvertSuccess(converter, "kiiik", Punishment.Action.Kick);
        this.AssertConvertSuccess(converter, "kk", Punishment.Action.Kick);
        this.AssertConvertSuccess(converter, "kiccck", Punishment.Action.Kick);
        this.AssertConvertSuccess(converter, "KICK", Punishment.Action.Kick);
        this.AssertConvertSuccess(converter, "kickedd", Punishment.Action.Kick);
        this.AssertConvertFail<PunishmentActionConverter, Punishment.Action>(converter, "kek");
        this.AssertConvertFail<PunishmentActionConverter, Punishment.Action>(converter, "ick");
        this.AssertConvertFail<PunishmentActionConverter, Punishment.Action>(converter, "kekick");
        this.AssertConvertFail<PunishmentActionConverter, Punishment.Action>(converter, "kickek");

        this.AssertConvertSuccess(converter, "b", Punishment.Action.PermanentBan);
        this.AssertConvertSuccess(converter, "ban", Punishment.Action.PermanentBan);
        this.AssertConvertSuccess(converter, "Ban", Punishment.Action.PermanentBan);
        this.AssertConvertSuccess(converter, "ben", Punishment.Action.PermanentBan);
        this.AssertConvertSuccess(converter, "baN", Punishment.Action.PermanentBan);
        this.AssertConvertSuccess(converter, "baaannn", Punishment.Action.PermanentBan);
        this.AssertConvertSuccess(converter, "bb", Punishment.Action.PermanentBan);
        this.AssertConvertSuccess(converter, "bbban", Punishment.Action.PermanentBan);
        this.AssertConvertSuccess(converter, "BAN", Punishment.Action.PermanentBan);
        this.AssertConvertSuccess(converter, "BanNedd", Punishment.Action.PermanentBan);
        this.AssertConvertSuccess(converter, "pb", Punishment.Action.PermanentBan);
        this.AssertConvertSuccess(converter, "pban", Punishment.Action.PermanentBan);
        this.AssertConvertSuccess(converter, "pBan", Punishment.Action.PermanentBan);
        this.AssertConvertSuccess(converter, "pben", Punishment.Action.PermanentBan);
        this.AssertConvertSuccess(converter, "pbaN", Punishment.Action.PermanentBan);
        this.AssertConvertSuccess(converter, "pbaaannn", Punishment.Action.PermanentBan);
        this.AssertConvertSuccess(converter, "pbb", Punishment.Action.PermanentBan);
        this.AssertConvertSuccess(converter, "pbbban", Punishment.Action.PermanentBan);
        this.AssertConvertSuccess(converter, "pBAN", Punishment.Action.PermanentBan);
        this.AssertConvertSuccess(converter, "pBanNedd", Punishment.Action.PermanentBan);
        this.AssertConvertSuccess(converter, "permb", Punishment.Action.PermanentBan);
        this.AssertConvertSuccess(converter, "permban", Punishment.Action.PermanentBan);
        this.AssertConvertSuccess(converter, "permBan", Punishment.Action.PermanentBan);
        this.AssertConvertSuccess(converter, "permben", Punishment.Action.PermanentBan);
        this.AssertConvertSuccess(converter, "permbaN", Punishment.Action.PermanentBan);
        this.AssertConvertSuccess(converter, "permbaaannn", Punishment.Action.PermanentBan);
        this.AssertConvertSuccess(converter, "permbb", Punishment.Action.PermanentBan);
        this.AssertConvertSuccess(converter, "permbbban", Punishment.Action.PermanentBan);
        this.AssertConvertSuccess(converter, "permBAN", Punishment.Action.PermanentBan);
        this.AssertConvertSuccess(converter, "permBanNedd", Punishment.Action.PermanentBan);
        this.AssertConvertSuccess(converter, "permab", Punishment.Action.PermanentBan);
        this.AssertConvertSuccess(converter, "permaban", Punishment.Action.PermanentBan);
        this.AssertConvertSuccess(converter, "permanentban", Punishment.Action.PermanentBan);
        this.AssertConvertFail<PunishmentActionConverter, Punishment.Action>(converter, "b@n");
        this.AssertConvertFail<PunishmentActionConverter, Punishment.Action>(converter, "bon");
        this.AssertConvertFail<PunishmentActionConverter, Punishment.Action>(converter, "8an");
        this.AssertConvertFail<PunishmentActionConverter, Punishment.Action>(converter, "permab@n");
        this.AssertConvertFail<PunishmentActionConverter, Punishment.Action>(converter, "bonperm");
        this.AssertConvertFail<PunishmentActionConverter, Punishment.Action>(converter, "8anperm");

        this.AssertConvertSuccess(converter, "m", Punishment.Action.PermanentMute);
        this.AssertConvertSuccess(converter, "mute", Punishment.Action.PermanentMute);
        this.AssertConvertSuccess(converter, "Mute", Punishment.Action.PermanentMute);
        this.AssertConvertSuccess(converter, "muted", Punishment.Action.PermanentMute);
        this.AssertConvertSuccess(converter, "muT", Punishment.Action.PermanentMute);
        this.AssertConvertSuccess(converter, "muuute", Punishment.Action.PermanentMute);
        this.AssertConvertSuccess(converter, "mutteeeddd", Punishment.Action.PermanentMute);
        this.AssertConvertSuccess(converter, "mm", Punishment.Action.PermanentMute);
        this.AssertConvertSuccess(converter, "mmmute", Punishment.Action.PermanentMute);
        this.AssertConvertSuccess(converter, "MUTE", Punishment.Action.PermanentMute);
        this.AssertConvertSuccess(converter, "MuTedD", Punishment.Action.PermanentMute);
        this.AssertConvertSuccess(converter, "pm", Punishment.Action.PermanentMute);
        this.AssertConvertSuccess(converter, "pmute", Punishment.Action.PermanentMute);
        this.AssertConvertSuccess(converter, "pMute", Punishment.Action.PermanentMute);
        this.AssertConvertSuccess(converter, "pmut", Punishment.Action.PermanentMute);
        this.AssertConvertSuccess(converter, "pmUt", Punishment.Action.PermanentMute);
        this.AssertConvertSuccess(converter, "pmmuuuttteee", Punishment.Action.PermanentMute);
        this.AssertConvertSuccess(converter, "pMuUUUTED", Punishment.Action.PermanentMute);
        this.AssertConvertSuccess(converter, "pmm", Punishment.Action.PermanentMute);
        this.AssertConvertSuccess(converter, "pmmmute", Punishment.Action.PermanentMute);
        this.AssertConvertSuccess(converter, "pMUTE", Punishment.Action.PermanentMute);
        this.AssertConvertSuccess(converter, "pMuteedd", Punishment.Action.PermanentMute);
        this.AssertConvertSuccess(converter, "permm", Punishment.Action.PermanentMute);
        this.AssertConvertSuccess(converter, "permmute", Punishment.Action.PermanentMute);
        this.AssertConvertSuccess(converter, "permMute", Punishment.Action.PermanentMute);
        this.AssertConvertSuccess(converter, "permmut", Punishment.Action.PermanentMute);
        this.AssertConvertSuccess(converter, "permmuT", Punishment.Action.PermanentMute);
        this.AssertConvertSuccess(converter, "permmuuuute", Punishment.Action.PermanentMute);
        this.AssertConvertSuccess(converter, "permmuuuuuttteeee", Punishment.Action.PermanentMute);
        this.AssertConvertSuccess(converter, "permmm", Punishment.Action.PermanentMute);
        this.AssertConvertSuccess(converter, "permmmmute", Punishment.Action.PermanentMute);
        this.AssertConvertSuccess(converter, "permMUTE", Punishment.Action.PermanentMute);
        this.AssertConvertSuccess(converter, "permMuteeDd", Punishment.Action.PermanentMute);
        this.AssertConvertSuccess(converter, "permam", Punishment.Action.PermanentMute);
        this.AssertConvertSuccess(converter, "permamute", Punishment.Action.PermanentMute);
        this.AssertConvertSuccess(converter, "permanentmute", Punishment.Action.PermanentMute);
        this.AssertConvertFail<PunishmentActionConverter, Punishment.Action>(converter, "mu7");
        this.AssertConvertFail<PunishmentActionConverter, Punishment.Action>(converter, "mut3");
        this.AssertConvertFail<PunishmentActionConverter, Punishment.Action>(converter, "ute");
        this.AssertConvertFail<PunishmentActionConverter, Punishment.Action>(converter, "permamt");
        this.AssertConvertFail<PunishmentActionConverter, Punishment.Action>(converter, "mperm");
        this.AssertConvertFail<PunishmentActionConverter, Punishment.Action>(converter, "apermm");

        this.AssertConvertSuccess(converter, "tb", Punishment.Action.TemporaryBan);
        this.AssertConvertSuccess(converter, "tban", Punishment.Action.TemporaryBan);
        this.AssertConvertSuccess(converter, "tBan", Punishment.Action.TemporaryBan);
        this.AssertConvertSuccess(converter, "tben", Punishment.Action.TemporaryBan);
        this.AssertConvertSuccess(converter, "tBaN", Punishment.Action.TemporaryBan);
        this.AssertConvertSuccess(converter, "tBaaaaannn", Punishment.Action.TemporaryBan);
        this.AssertConvertSuccess(converter, "tBanNeD", Punishment.Action.TemporaryBan);
        this.AssertConvertSuccess(converter, "tbb", Punishment.Action.TemporaryBan);
        this.AssertConvertSuccess(converter, "tbbbban", Punishment.Action.TemporaryBan);
        this.AssertConvertSuccess(converter, "tBAN", Punishment.Action.TemporaryBan);
        this.AssertConvertSuccess(converter, "tBanedd", Punishment.Action.TemporaryBan);
        this.AssertConvertSuccess(converter, "tmpb", Punishment.Action.TemporaryBan);
        this.AssertConvertSuccess(converter, "tmpban", Punishment.Action.TemporaryBan);
        this.AssertConvertSuccess(converter, "tempb", Punishment.Action.TemporaryBan);
        this.AssertConvertSuccess(converter, "tempban", Punishment.Action.TemporaryBan);
        this.AssertConvertSuccess(converter, "tempBan", Punishment.Action.TemporaryBan);
        this.AssertConvertSuccess(converter, "tempbann", Punishment.Action.TemporaryBan);
        this.AssertConvertSuccess(converter, "tempBanN", Punishment.Action.TemporaryBan);
        this.AssertConvertSuccess(converter, "tempBaNNnne", Punishment.Action.TemporaryBan);
        this.AssertConvertSuccess(converter, "tempbaaaaannnnnned", Punishment.Action.TemporaryBan);
        this.AssertConvertSuccess(converter, "tempbb", Punishment.Action.TemporaryBan);
        this.AssertConvertSuccess(converter, "tempbbban", Punishment.Action.TemporaryBan);
        this.AssertConvertSuccess(converter, "tempBAN", Punishment.Action.TemporaryBan);
        this.AssertConvertSuccess(converter, "tempBaneDd", Punishment.Action.TemporaryBan);
        this.AssertConvertSuccess(converter, "temporalb", Punishment.Action.TemporaryBan);
        this.AssertConvertSuccess(converter, "temporalban", Punishment.Action.TemporaryBan);
        this.AssertConvertSuccess(converter, "temporaryban", Punishment.Action.TemporaryBan);
        this.AssertConvertFail<PunishmentActionConverter, Punishment.Action>(converter, "tmu7");
        this.AssertConvertFail<PunishmentActionConverter, Punishment.Action>(converter, "tmut3");
        this.AssertConvertFail<PunishmentActionConverter, Punishment.Action>(converter, "tute");
        this.AssertConvertFail<PunishmentActionConverter, Punishment.Action>(converter, "tmppermamt");
        this.AssertConvertFail<PunishmentActionConverter, Punishment.Action>(converter, "mtemp");
        this.AssertConvertFail<PunishmentActionConverter, Punishment.Action>(converter, "atempm");

        this.AssertConvertSuccess(converter, "tm", Punishment.Action.TemporaryMute);
        this.AssertConvertSuccess(converter, "tmute", Punishment.Action.TemporaryMute);
        this.AssertConvertSuccess(converter, "tMute", Punishment.Action.TemporaryMute);
        this.AssertConvertSuccess(converter, "tmut", Punishment.Action.TemporaryMute);
        this.AssertConvertSuccess(converter, "tmUt", Punishment.Action.TemporaryMute);
        this.AssertConvertSuccess(converter, "tmmuuuttteee", Punishment.Action.TemporaryMute);
        this.AssertConvertSuccess(converter, "tMuUUUTED", Punishment.Action.TemporaryMute);
        this.AssertConvertSuccess(converter, "tmm", Punishment.Action.TemporaryMute);
        this.AssertConvertSuccess(converter, "tmmmute", Punishment.Action.TemporaryMute);
        this.AssertConvertSuccess(converter, "tMUTE", Punishment.Action.TemporaryMute);
        this.AssertConvertSuccess(converter, "tMuteedd", Punishment.Action.TemporaryMute);
        this.AssertConvertSuccess(converter, "tmpm", Punishment.Action.TemporaryMute);
        this.AssertConvertSuccess(converter, "tmpmute", Punishment.Action.TemporaryMute);
        this.AssertConvertSuccess(converter, "tempm", Punishment.Action.TemporaryMute);
        this.AssertConvertSuccess(converter, "tempmute", Punishment.Action.TemporaryMute);
        this.AssertConvertSuccess(converter, "tempMute", Punishment.Action.TemporaryMute);
        this.AssertConvertSuccess(converter, "tempmut", Punishment.Action.TemporaryMute);
        this.AssertConvertSuccess(converter, "tempmuT", Punishment.Action.TemporaryMute);
        this.AssertConvertSuccess(converter, "tempmuuuute", Punishment.Action.TemporaryMute);
        this.AssertConvertSuccess(converter, "tempmuuuuuttteeee", Punishment.Action.TemporaryMute);
        this.AssertConvertSuccess(converter, "tempmm", Punishment.Action.TemporaryMute);
        this.AssertConvertSuccess(converter, "tempmmmute", Punishment.Action.TemporaryMute);
        this.AssertConvertSuccess(converter, "tempMUTE", Punishment.Action.TemporaryMute);
        this.AssertConvertSuccess(converter, "tempMuteeDd", Punishment.Action.TemporaryMute);
        this.AssertConvertSuccess(converter, "temporalm", Punishment.Action.TemporaryMute);
        this.AssertConvertSuccess(converter, "temporalmute", Punishment.Action.TemporaryMute);
        this.AssertConvertSuccess(converter, "temporarymute", Punishment.Action.TemporaryMute);
        this.AssertConvertFail<PunishmentActionConverter, Punishment.Action>(converter, "tmu7");
        this.AssertConvertFail<PunishmentActionConverter, Punishment.Action>(converter, "tmut3");
        this.AssertConvertFail<PunishmentActionConverter, Punishment.Action>(converter, "tute");
        this.AssertConvertFail<PunishmentActionConverter, Punishment.Action>(converter, "tmppermamt");
        this.AssertConvertFail<PunishmentActionConverter, Punishment.Action>(converter, "mtemp");
        this.AssertConvertFail<PunishmentActionConverter, Punishment.Action>(converter, "atempm");

        this.AssertConvertFail<PunishmentActionConverter, Punishment.Action>(converter, "sanitycheck");
        this.AssertConvertFail<PunishmentActionConverter, Punishment.Action>(converter, "dontkickme");
        this.AssertConvertFail<PunishmentActionConverter, Punishment.Action>(converter, "plskick");
        this.AssertConvertFail<PunishmentActionConverter, Punishment.Action>(converter, "permanent");
        this.AssertConvertFail<PunishmentActionConverter, Punishment.Action>(converter, "temporary");
        this.AssertConvertFail<PunishmentActionConverter, Punishment.Action>(converter, "qwertyuioplkjhgfdsazxcvbnm");
        this.AssertConvertFail<PunishmentActionConverter, Punishment.Action>(converter, "tempkickban");
        this.AssertConvertFail<PunishmentActionConverter, Punishment.Action>(converter, "permkickmute");
        this.AssertConvertFail<PunishmentActionConverter, Punishment.Action>(converter, "bankick");
        this.AssertConvertFail<PunishmentActionConverter, Punishment.Action>(converter, "kickban");
        this.AssertConvertFail<PunishmentActionConverter, Punishment.Action>(converter, "kickmute");
        this.AssertConvertFail<PunishmentActionConverter, Punishment.Action>(converter, "mutekickban");
        this.AssertConvertFail<PunishmentActionConverter, Punishment.Action>(converter, "permandtempban");
    }


    private void AssertConvertSuccess<TConverter, TValue>(TConverter converter, string text, TValue expected)
        where TConverter : BaseArgumentConverter<TValue>
    {
        Assert.That(converter.TryConvert(text, out TValue? parsed), Is.True);
        Assert.That(parsed, Is.Not.Null);
        Assert.That(parsed, Is.EqualTo(expected));
    }

    private void AssertConvertFail<TConverter, TValue>(TConverter converter, string text)
        where TConverter : BaseArgumentConverter<TValue>
        => Assert.That(converter.TryConvert(text, out _), Is.False);
}