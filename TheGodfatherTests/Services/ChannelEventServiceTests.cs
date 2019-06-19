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
    public sealed class ChannelEventServiceTests : IServiceTest<ChannelEventService>
    {
        public ChannelEventService Service { get; private set; }

        private const ulong cid1 = 123456123456;
        private const ulong cid2 = 123456123457;
        private const ulong cid3 = 123456123458;


        [SetUp]
        public void InitializeService()
        {
            this.Service = new ChannelEventService();
        }


        [Test]
        public void GetEventInChannelTest()
        {
            Assert.IsNull(EventIn(cid1));
            Assert.IsNull(EventIn(cid2));
            Assert.IsNull(EventIn(cid3));

            IChannelEvent caro = new CaroGame(null, null, null, null);
            IChannelEvent holdem = new HoldemGame(null, null, 0);
            this.Service.RegisterEventInChannel(caro, cid1);
            this.Service.RegisterEventInChannel(holdem, cid2);

            Assert.AreSame(EventIn(cid1), caro);
            Assert.AreSame(EventIn(cid2), holdem);
            Assert.IsNull(EventIn(cid3));

            Assert.AreSame(EventOfTypeIn<CaroGame>(cid1), caro);
            Assert.IsNull(EventOfTypeIn<HoldemGame>(cid1));

            Assert.AreSame(EventOfTypeIn<HoldemGame>(cid2), holdem);
            Assert.IsNull(EventOfTypeIn<CaroGame>(cid2));


            IChannelEvent EventIn(ulong cid)
                => this.Service.GetEventInChannel(cid);

            T EventOfTypeIn<T>(ulong cid) where T : class, IChannelEvent
                => this.Service.GetEventInChannel<T>(cid);
        }

        [Test]
        public void IsEventRunningInChannelTest()
        {
            Assert.False(IsEventRunningIn(cid1));
            Assert.False(IsEventRunningIn(cid2));
            Assert.False(IsEventRunningIn(cid3));
            
            IChannelEvent caro = new CaroGame(null, null, null, null);
            IChannelEvent holdem = new HoldemGame(null, null, 0);

            this.Service.RegisterEventInChannel(caro, cid1);
            this.Service.RegisterEventInChannel(holdem, cid2);

            Assert.True(IsEventRunningIn(cid1));
            Assert.True(IsEventRunningIn(cid2));
            Assert.False(IsEventRunningIn(cid3));

            Assert.True(IsOutEventRunningIn(cid1, out IChannelEvent outEvent));
            Assert.AreSame(outEvent, caro);
            Assert.True(IsOutEventRunningIn(cid2, out outEvent));
            Assert.AreSame(outEvent, holdem);
            Assert.False(IsOutEventRunningIn(cid3, out outEvent));
            Assert.IsNull(outEvent);

            Assert.True(IsEventOfTypeRunningIn(cid1, out CaroGame outCaro));
            Assert.AreSame(outCaro, caro);
            Assert.False(IsEventOfTypeRunningIn(cid1, out HoldemGame outHoldem));
            Assert.IsNull(outHoldem);
            Assert.True(IsEventOfTypeRunningIn(cid2, out outHoldem));
            Assert.AreSame(outHoldem, holdem);
            Assert.False(IsEventOfTypeRunningIn(cid3, out IChannelEvent ret3));
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
            Assert.IsNull(EventIn(cid1));
            Assert.IsNull(EventIn(cid2));
            Assert.IsNull(EventIn(cid3));

            IChannelEvent war = new ChickenWar(null, null, null, null);
            IChannelEvent ttt = new TicTacToeGame(null, null, null, null);

            Assert.DoesNotThrow(() => this.Service.RegisterEventInChannel(war, cid1));
            Assert.DoesNotThrow(() => this.Service.RegisterEventInChannel(ttt, cid2));

            Assert.Throws<InvalidOperationException>(() => this.Service.RegisterEventInChannel(war, cid1));
            Assert.Throws<InvalidOperationException>(() => this.Service.RegisterEventInChannel(ttt, cid1));
            Assert.Throws<InvalidOperationException>(() => this.Service.RegisterEventInChannel(ttt, cid2));
            Assert.Throws<InvalidOperationException>(() => this.Service.RegisterEventInChannel(war, cid2));

            Assert.AreSame(EventIn(cid1), war);
            Assert.AreSame(EventIn(cid2), ttt);
            Assert.IsNull(EventIn(cid3));


            IChannelEvent EventIn(ulong cid)
                => this.Service.GetEventInChannel(cid);
        }

        [Test]
        public void UnregisterEventInChannelTest()
        {
            Assert.IsNull(EventIn(cid1));
            Assert.IsNull(EventIn(cid2));
            Assert.IsNull(EventIn(cid3));

            IChannelEvent war = new ChickenWar(null, null, null, null);
            IChannelEvent ttt = new TicTacToeGame(null, null, null, null);

            this.Service.RegisterEventInChannel(war, cid1);
            this.Service.RegisterEventInChannel(ttt, cid2);

            this.Service.UnregisterEventInChannel(cid1);

            Assert.IsNull(EventIn(cid1));
            Assert.AreSame(EventIn(cid2), ttt);

            this.Service.UnregisterEventInChannel(cid2);

            Assert.IsNull(EventIn(cid1));
            Assert.IsNull(EventIn(cid2));

            this.Service.UnregisterEventInChannel(cid1);
            this.Service.UnregisterEventInChannel(cid2);


            IChannelEvent EventIn(ulong cid)
                => this.Service.GetEventInChannel(cid);
        }
    }
}
