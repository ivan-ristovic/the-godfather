using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using TheGodfather.Modules.Reactions.Services;
using TheGodfatherTests.Services;

namespace TheGodfatherTests.Modules.Reactions.Services
{
    [TestFixture]
    public sealed class ReactionsServiceTests : IServiceTest<ReactionsService>
    {
        public ReactionsService Service { get; private set; }


        [SetUp]
        public void InitializeService()
        {

        }
    }
}
