using System;
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
        public void GetEventInChannelTest()
        {
            Assert.IsNull(EventIn(MockData.Ids[0]));
            Assert.IsNull(EventIn(MockData.Ids[1]));
            Assert.IsNull(EventIn(MockData.Ids[2]));

            IChannelEvent caro = new CaroGame(null, null, null, null);
            IChannelEvent holdem = new HoldemGame(null, null, 0);
            this.Service.RegisterEventInChannel(caro, MockData.Ids[0]);
            this.Service.RegisterEventInChannel(holdem, MockData.Ids[1]);

            Assert.AreSame(EventIn(MockData.Ids[0]), caro);
            Assert.AreSame(EventIn(MockData.Ids[1]), holdem);
            Assert.IsNull(EventIn(MockData.Ids[2]));

            Assert.AreSame(EventOfTypeIn<CaroGame>(MockData.Ids[0]), caro);
            Assert.IsNull(EventOfTypeIn<HoldemGame>(MockData.Ids[0]));

            Assert.AreSame(EventOfTypeIn<HoldemGame>(MockData.Ids[1]), holdem);
            Assert.IsNull(EventOfTypeIn<CaroGame>(MockData.Ids[1]));


            IChannelEvent EventIn(ulong cid)
                => this.Service.GetEventInChannel(cid);

            T EventOfTypeIn<T>(ulong cid) where T : class, IChannelEvent
                => this.Service.GetEventInChannel<T>(cid);
        }

        [Test]
        public void IsEventRunningInChannelTest()
        {
            Assert.False(IsEventRunningIn(MockData.Ids[0]));
            Assert.False(IsEventRunningIn(MockData.Ids[1]));
            Assert.False(IsEventRunningIn(MockData.Ids[2]));
            
            IChannelEvent caro = new CaroGame(null, null, null, null);
            IChannelEvent holdem = new HoldemGame(null, null, 0);

            this.Service.RegisterEventInChannel(caro, MockData.Ids[0]);
            this.Service.RegisterEventInChannel(holdem, MockData.Ids[1]);

            Assert.True(IsEventRunningIn(MockData.Ids[0]));
            Assert.True(IsEventRunningIn(MockData.Ids[1]));
            Assert.False(IsEventRunningIn(MockData.Ids[2]));

            Assert.True(IsOutEventRunningIn(MockData.Ids[0], out IChannelEvent outEvent));
            Assert.AreSame(outEvent, caro);
            Assert.True(IsOutEventRunningIn(MockData.Ids[1], out outEvent));
            Assert.AreSame(outEvent, holdem);
            Assert.False(IsOutEventRunningIn(MockData.Ids[2], out outEvent));
            Assert.IsNull(outEvent);

            Assert.True(IsEventOfTypeRunningIn(MockData.Ids[0], out CaroGame outCaro));
            Assert.AreSame(outCaro, caro);
            Assert.False(IsEventOfTypeRunningIn(MockData.Ids[0], out HoldemGame outHoldem));
            Assert.IsNull(outHoldem);
            Assert.True(IsEventOfTypeRunningIn(MockData.Ids[1], out outHoldem));
            Assert.AreSame(outHoldem, holdem);
            Assert.False(IsEventOfTypeRunningIn(MockData.Ids[2], out IChannelEvent ret3));
            Assert.IsNull(ret3);


            bool IsEventRunningIn(ulong cid)
                => this.Service.IsEventRunningInChannel(cid);

            bool IsOutEventRunningIn(ulong cid, out IChannelEvent @event)
                => this.Service.IsEventRunningInChannel(cid, out @event);

            bool IsEventOfTypeRunningIn<T>(ulong cid, out T @event) where T : class, IChannelEvent
                => this.Service.IsEventRunningInChannel<T>(cid, out @event);
        }

        [Test]
        public void RegisterEventInChannelTest()
        {
            Assert.IsNull(EventIn(MockData.Ids[0]));
            Assert.IsNull(EventIn(MockData.Ids[1]));
            Assert.IsNull(EventIn(MockData.Ids[2]));

            IChannelEvent war = new ChickenWar(null, null, null, null);
            IChannelEvent ttt = new TicTacToeGame(null, null, null, null);

            Assert.DoesNotThrow(() => this.Service.RegisterEventInChannel(war, MockData.Ids[0]));
            Assert.DoesNotThrow(() => this.Service.RegisterEventInChannel(ttt, MockData.Ids[1]));

            Assert.Throws<InvalidOperationException>(() => this.Service.RegisterEventInChannel(war, MockData.Ids[0]));
            Assert.Throws<InvalidOperationException>(() => this.Service.RegisterEventInChannel(ttt, MockData.Ids[0]));
            Assert.Throws<InvalidOperationException>(() => this.Service.RegisterEventInChannel(ttt, MockData.Ids[1]));
            Assert.Throws<InvalidOperationException>(() => this.Service.RegisterEventInChannel(war, MockData.Ids[1]));

            Assert.AreSame(EventIn(MockData.Ids[0]), war);
            Assert.AreSame(EventIn(MockData.Ids[1]), ttt);
            Assert.IsNull(EventIn(MockData.Ids[2]));


            IChannelEvent EventIn(ulong cid)
                => this.Service.GetEventInChannel(cid);
        }

        [Test]
        public void UnregisterEventInChannelTest()
        {
            Assert.IsNull(EventIn(MockData.Ids[0]));
            Assert.IsNull(EventIn(MockData.Ids[1]));
            Assert.IsNull(EventIn(MockData.Ids[2]));

            IChannelEvent war = new ChickenWar(null, null, null, null);
            IChannelEvent ttt = new TicTacToeGame(null, null, null, null);

            this.Service.RegisterEventInChannel(war, MockData.Ids[0]);
            this.Service.RegisterEventInChannel(ttt, MockData.Ids[1]);

            this.Service.UnregisterEventInChannel(MockData.Ids[0]);

            Assert.IsNull(EventIn(MockData.Ids[0]));
            Assert.AreSame(EventIn(MockData.Ids[1]), ttt);

            this.Service.UnregisterEventInChannel(MockData.Ids[1]);

            Assert.IsNull(EventIn(MockData.Ids[0]));
            Assert.IsNull(EventIn(MockData.Ids[1]));

            this.Service.UnregisterEventInChannel(MockData.Ids[0]);
            this.Service.UnregisterEventInChannel(MockData.Ids[1]);


            IChannelEvent EventIn(ulong cid)
                => this.Service.GetEventInChannel(cid);
        }
    }
}
