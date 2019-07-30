using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using TheGodfather;
using TheGodfather.Database;
using TheGodfather.Modules;

public class TestModule : TheGodfatherModule
{
    public TestModule(SharedData shared, DatabaseContextBuilder dbb) : base(shared, dbb)
    {
    }


    [Command("test")]
    public Task TestAsync(CommandContext ctx, bool success = true)
        => this.InformAsync(ctx, success ? "suc" : "errr");
}