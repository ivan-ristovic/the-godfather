using System.Reflection;
using Discord.Commands;
using TheGodfather.Modules.Administration;

namespace TheGodfather.Tests.Modules.Administration;

[TestFixture]
public sealed class ConfigModuleCommandSurfaceTests
{
    [TestCase("timezone")]
    [TestCase("timezone current")]
    [TestCase("timezone list")]
    [TestCase("timezone reset")]
    [TestCase("localization list")]
    [TestCase("logging")]
    [TestCase("backup")]
    [TestCase("welcome")]
    [TestCase("welcome channel")]
    [TestCase("welcome message")]
    [TestCase("leave")]
    [TestCase("leave channel")]
    [TestCase("leave message")]
    [TestCase("linkfilter")]
    [TestCase("linkfilter invites")]
    [TestCase("linkfilter booters")]
    [TestCase("linkfilter shocksites")]
    [TestCase("linkfilter iploggers")]
    [TestCase("linkfilter shorteners")]
    [TestCase("antiflood")]
    [TestCase("antiflood action")]
    [TestCase("antiflood sensitivity")]
    [TestCase("antiflood cooldown")]
    [TestCase("instantleave")]
    [TestCase("instantleave cooldown")]
    [TestCase("antispam")]
    [TestCase("antispam action")]
    [TestCase("antispam sensitivity")]
    [TestCase("antispam exempt channel")]
    [TestCase("antispam exempt member")]
    [TestCase("antispam exempt role")]
    [TestCase("antispam unexempt channel")]
    [TestCase("antispam unexempt member")]
    [TestCase("antispam unexempt role")]
    [TestCase("antimention")]
    [TestCase("antimention action")]
    [TestCase("antimention sensitivity")]
    [TestCase("antimention exempt channel")]
    [TestCase("antimention exempt member")]
    [TestCase("antimention exempt role")]
    [TestCase("antimention unexempt channel")]
    [TestCase("antimention unexempt member")]
    [TestCase("antimention unexempt role")]
    [TestCase("ratelimit")]
    [TestCase("ratelimit action")]
    [TestCase("ratelimit sensitivity")]
    [TestCase("ratelimit exempt channel")]
    [TestCase("ratelimit exempt member")]
    [TestCase("ratelimit exempt role")]
    [TestCase("ratelimit unexempt channel")]
    [TestCase("ratelimit unexempt member")]
    [TestCase("ratelimit unexempt role")]
    public void ConfigModuleExposesExpectedPortedCommand(string command)
    {
        IReadOnlySet<string> commands = typeof(ConfigModule)
            .GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .SelectMany(method => method.GetCustomAttributes<CommandAttribute>())
            .Select(attribute => attribute.Text)
            .ToHashSet();

        Assert.That(commands, Does.Contain(command));
    }
}
