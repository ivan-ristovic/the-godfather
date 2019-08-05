using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using TheGodfather.Common.Attributes;
using TheGodfather.Database;

namespace TheGodfather.Modules
{
    [Module(ModuleType.Uncategorized)]
    public class TestModule : TheGodfatherModule
    {

        public TestModule(SharedData shared, DatabaseContextBuilder db)
            : base(shared, db)
        {

        }


        [Command("test")]
        public Task TestAsync(CommandContext ctx, bool succ = true)
        {
            return this.InformAsync(ctx, succ ? "msg-suc" : "msg-err");
        }
    }
}
