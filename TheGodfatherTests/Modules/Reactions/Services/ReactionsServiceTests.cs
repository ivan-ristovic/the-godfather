using NUnit.Framework;
using TheGodfather.Common;
using TheGodfather.Modules.Reactions.Services;
using TheGodfatherTests.Services;

namespace TheGodfatherTests.Modules.Reactions.Services
{
    [TestFixture]
    public class ReactionsServiceTestsBase : IServiceTest<ReactionsService>
    {
        public ReactionsService Service { get; private set; }


        [SetUp]
        public void InitializeService()
        {
            this.Service = new ReactionsService(TestDatabaseProvider.Database, new Logger(BotConfig.Default), loadDataFromDatabase: false);
        }
    }
}
