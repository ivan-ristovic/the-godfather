using NUnit.Framework;
using TheGodfather.Common;
using TheGodfather.Modules.Chickens.Common;
using TheGodfather.Modules.Currency.Common;
using TheGodfather.Modules.Games.Common;
using TheGodfather.Services;

namespace TheGodfatherTests.Services
{
    [TestFixture]
    public sealed class ChannelEventServiceTests : ITheGodfatherServiceTest<ChannelEventService>
    {
        public ChannelEventService Service { get; private set; }


        [SetUp]
        public void InitializeService()
        {
            this.Service = new ChannelEventService();
        }


        [Test]
        public void GetEventInChannelTests()
        {
            Assert.That(EventIn(MockData.Ids[0]), Is.Null);
            Assert.That(EventIn(MockData.Ids[1]), Is.Null);
            Assert.That(EventIn(MockData.Ids[2]), Is.Null);

            IChannelEvent caro = new CaroGame(null, null, null, null);
            IChannelEvent holdem = new HoldemGame(null, null, 0);

            this.Service.RegisterEventInChannel(caro, MockData.Ids[0]);
            this.Service.RegisterEventInChannel(holdem, MockData.Ids[1]);

            Assert.That(EventIn(MockData.Ids[0]), Is.SameAs(caro));
            Assert.That(EventIn(MockData.Ids[1]), Is.SameAs(holdem));
            Assert.That(EventIn(MockData.Ids[2]), Is.Null);

            Assert.That(EventOfTypeIn<CaroGame>(MockData.Ids[0]), Is.SameAs(caro));
            Assert.That(EventOfTypeIn<HoldemGame>(MockData.Ids[0]), Is.Null);

            Assert.That(EventOfTypeIn<HoldemGame>(MockData.Ids[1]), Is.SameAs(holdem));
            Assert.That(EventOfTypeIn<CaroGame>(MockData.Ids[1]), Is.Null);


            IChannelEvent EventIn(ulong cid)
                => this.Service.GetEventInChannel(cid);

            T EventOfTypeIn<T>(ulong cid) where T : class, IChannelEvent
                => this.Service.GetEventInChannel<T>(cid);
        }

        [Test]
        public void IsEventRunningInChannelTests()
        {
            Assert.That(IsEventRunningIn(MockData.Ids[0]), Is.False);
            Assert.That(IsEventRunningIn(MockData.Ids[1]), Is.False);
            Assert.That(IsEventRunningIn(MockData.Ids[2]), Is.False);

            IChannelEvent caro = new CaroGame(null, null, null, null);
            IChannelEvent holdem = new HoldemGame(null, null, 0);

            this.Service.RegisterEventInChannel(caro, MockData.Ids[0]);
            this.Service.RegisterEventInChannel(holdem, MockData.Ids[1]);

            Assert.That(IsEventRunningIn(MockData.Ids[0]), Is.True);
            Assert.That(IsEventRunningIn(MockData.Ids[1]), Is.True);
            Assert.That(IsEventRunningIn(MockData.Ids[2]), Is.False);

            Assert.That(IsOutEventRunningIn(MockData.Ids[0], out IChannelEvent outEvent), Is.True);
            Assert.That(outEvent, Is.SameAs(caro));
            Assert.That(IsOutEventRunningIn(MockData.Ids[1], out outEvent), Is.True);
            Assert.That(outEvent, Is.SameAs(holdem));
            Assert.That(IsOutEventRunningIn(MockData.Ids[2], out outEvent), Is.False);
            Assert.That(outEvent, Is.Null);

            Assert.That(IsEventOfTypeRunningIn(MockData.Ids[0], out CaroGame outCaro), Is.True);
            Assert.That(outCaro, Is.SameAs(caro));
            Assert.That(IsEventOfTypeRunningIn(MockData.Ids[0], out HoldemGame outHoldem), Is.False);
            Assert.That(outHoldem, Is.Null);
            Assert.That(IsEventOfTypeRunningIn(MockData.Ids[1], out outHoldem), Is.True);
            Assert.That(outHoldem, Is.SameAs(holdem));
            Assert.That(IsEventOfTypeRunningIn(MockData.Ids[2], out IChannelEvent ret3), Is.False);
            Assert.That(ret3, Is.Null);


            bool IsEventRunningIn(ulong cid)
                => this.Service.IsEventRunningInChannel(cid);

            bool IsOutEventRunningIn(ulong cid, out IChannelEvent @event)
                => this.Service.IsEventRunningInChannel(cid, out @event);

            bool IsEventOfTypeRunningIn<T>(ulong cid, out T @event) where T : class, IChannelEvent
                => this.Service.IsEventRunningInChannel<T>(cid, out @event);
        }

        [Test]
        public void RegisterEventInChannelTests()
        {
            Assert.That(EventIn(MockData.Ids[0]), Is.Null);
            Assert.That(EventIn(MockData.Ids[1]), Is.Null);
            Assert.That(EventIn(MockData.Ids[2]), Is.Null);

            IChannelEvent war = new ChickenWar(null, null, null, null);
            IChannelEvent ttt = new TicTacToeGame(null, null, null, null);

            Assert.That(() => this.Service.RegisterEventInChannel(war, MockData.Ids[0]), Throws.Nothing);
            Assert.That(() => this.Service.RegisterEventInChannel(ttt, MockData.Ids[1]), Throws.Nothing);

            Assert.That(() => this.Service.RegisterEventInChannel(war, MockData.Ids[0]), Throws.InvalidOperationException);
            Assert.That(() => this.Service.RegisterEventInChannel(ttt, MockData.Ids[0]), Throws.InvalidOperationException);
            Assert.That(() => this.Service.RegisterEventInChannel(ttt, MockData.Ids[1]), Throws.InvalidOperationException);
            Assert.That(() => this.Service.RegisterEventInChannel(war, MockData.Ids[1]), Throws.InvalidOperationException);

            Assert.That(EventIn(MockData.Ids[0]), Is.SameAs(war));
            Assert.That(EventIn(MockData.Ids[1]), Is.SameAs(ttt));
            Assert.That(EventIn(MockData.Ids[2]), Is.Null);


            IChannelEvent EventIn(ulong cid)
                => this.Service.GetEventInChannel(cid);
        }

        [Test]
        public void UnregisterEventInChannelTests()
        {
            Assert.That(EventIn(MockData.Ids[0]), Is.Null);
            Assert.That(EventIn(MockData.Ids[1]), Is.Null);
            Assert.That(EventIn(MockData.Ids[2]), Is.Null);

            IChannelEvent war = new ChickenWar(null, null, null, null);
            IChannelEvent ttt = new TicTacToeGame(null, null, null, null);

            this.Service.RegisterEventInChannel(war, MockData.Ids[0]);
            this.Service.RegisterEventInChannel(ttt, MockData.Ids[1]);

            Assert.That(() => this.Service.UnregisterEventInChannel(MockData.Ids[0]), Throws.Nothing);

            Assert.That(EventIn(MockData.Ids[0]), Is.Null);
            Assert.That(EventIn(MockData.Ids[1]), Is.SameAs(ttt));

            Assert.That(() => this.Service.UnregisterEventInChannel(MockData.Ids[1]), Throws.Nothing);

            Assert.That(EventIn(MockData.Ids[0]), Is.Null);
            Assert.That(EventIn(MockData.Ids[1]), Is.Null);

            Assert.That(() => this.Service.UnregisterEventInChannel(MockData.Ids[0]), Throws.Nothing);
            Assert.That(() => this.Service.UnregisterEventInChannel(MockData.Ids[1]), Throws.Nothing);


            IChannelEvent EventIn(ulong cid)
                => this.Service.GetEventInChannel(cid);
        }
    }
}
