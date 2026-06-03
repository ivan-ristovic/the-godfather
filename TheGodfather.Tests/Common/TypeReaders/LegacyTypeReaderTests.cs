using Discord;
using Discord.Commands;
using Serilog.Events;
using TheGodfather.Attributes;
using TheGodfather.Common.TypeReaders;
using TheGodfather.Modules.Search.Common;

namespace TheGodfather.Tests.Common.TypeReaders;

[TestFixture]
public sealed class LegacyTypeReaderTests
{
    [Test]
    public void BooleanParserKeepsLegacyForms()
    {
        Assert.That(LegacyArgumentParsers.TryParseBool("yup", out bool value), Is.True);
        Assert.That(value, Is.True);
        Assert.That(LegacyArgumentParsers.TryParseBool("diSable", out value), Is.True);
        Assert.That(value, Is.False);
        Assert.That(LegacyArgumentParsers.TryParseBool("yesno", out _), Is.False);
    }

    [Test]
    public void ActivityTypeParserKeepsLegacyForms()
    {
        Assert.That(LegacyArgumentParsers.TryParseActivityType("plaaaay", out ActivityType activity), Is.True);
        Assert.That(activity, Is.EqualTo(ActivityType.Playing));
        Assert.That(LegacyArgumentParsers.TryParseActivityType("watchez", out activity), Is.True);
        Assert.That(activity, Is.EqualTo(ActivityType.Watching));
        Assert.That(LegacyArgumentParsers.TryParseActivityType("listensto", out activity), Is.True);
        Assert.That(activity, Is.EqualTo(ActivityType.Listening));
        Assert.That(LegacyArgumentParsers.TryParseActivityType("notwatching", out _), Is.False);
    }

    [Test]
    public void PunishmentAndFilterActionParsersKeepLegacyForms()
    {
        Assert.That(LegacyArgumentParsers.TryParsePunishmentAction("tempbaaaaannnnnned", out Punishment.Action punishment), Is.True);
        Assert.That(punishment, Is.EqualTo(Punishment.Action.TemporaryBan));
        Assert.That(LegacyArgumentParsers.TryParsePunishmentAction("permamute", out punishment), Is.True);
        Assert.That(punishment, Is.EqualTo(Punishment.Action.PermanentMute));

        Assert.That(LegacyArgumentParsers.TryParseFilterAction("spoiler", out Filter.Action filter), Is.True);
        Assert.That(filter, Is.EqualTo(Filter.Action.Sanitize));
        Assert.That(LegacyArgumentParsers.TryParseFilterAction("kickedd", out filter), Is.True);
        Assert.That(filter, Is.EqualTo(Filter.Action.Kick));
        Assert.That(LegacyArgumentParsers.TryParseFilterAction("b@n", out _), Is.False);
    }

    [Test]
    public void MiscParsersKeepLegacyForms()
    {
        Assert.That(LegacyArgumentParsers.TryParseTimeWindow("month", out TimeWindow timeWindow), Is.True);
        Assert.That(timeWindow, Is.EqualTo(TimeWindow.Month));
        Assert.That(LegacyArgumentParsers.TryParseLogEventLevel("warn", out LogEventLevel level), Is.True);
        Assert.That(level, Is.EqualTo(LogEventLevel.Warning));
        Assert.That(LegacyArgumentParsers.TryParseModuleType("bank", out ModuleType module), Is.True);
        Assert.That(module, Is.EqualTo(ModuleType.Currency));
    }

    [Test]
    public async Task UriTypeReaderRequiresAbsoluteUri()
    {
        var reader = new UriTypeReader();

        TypeReaderResult valid = await reader.ReadAsync(null!, "https://example.com/image.png", null!);
        Assert.That(valid.IsSuccess, Is.True);
        Assert.That(valid.Values.Single().Value, Is.EqualTo(new Uri("https://example.com/image.png")));

        TypeReaderResult invalid = await reader.ReadAsync(null!, "not/a/url", null!);
        Assert.That(invalid.IsSuccess, Is.False);
    }
}
