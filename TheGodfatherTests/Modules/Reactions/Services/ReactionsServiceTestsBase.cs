using NUnit.Framework;
using TheGodfather.Modules.Reactions.Services;

namespace TheGodfather.Tests.Modules.Reactions.Services
{
    [TestFixture]
    public class ReactionsServiceTestsBase : ITheGodfatherServiceTest<ReactionsService>
    {
        public ReactionsService Service { get; private set; }


        [SetUp]
        public void InitializeService()
        {
            this.Service = new ReactionsService(TestDatabaseProvider.Database, loadData: false);
        }
    }
}
