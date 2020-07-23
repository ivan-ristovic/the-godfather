using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using TheGodfather.Services;

namespace TheGodfather.Tests.Services
{
    [TestFixture]
    public sealed class InteractivityServiceTests : ITheGodfatherServiceTest<InteractivityService>
    {
        public InteractivityService Service { get; private set; }


        [SetUp]
        public void InitializeService()
        {
            this.Service = new InteractivityService();
        }


        [Test]
        public void Tests()
        {
            Assert.That(this.Service.IsResponsePending(MockData.Ids[0], MockData.Ids[0]), Is.False);
            this.Service.AddPendingResponse(MockData.Ids[0], MockData.Ids[0]);
            this.Service.AddPendingResponse(MockData.Ids[0], MockData.Ids[0]);
            Assert.That(this.Service.IsResponsePending(MockData.Ids[0], MockData.Ids[0]), Is.True);
            Assert.That(this.Service.IsResponsePending(MockData.Ids[0], MockData.Ids[1]), Is.False);
            Assert.That(this.Service.IsResponsePending(MockData.Ids[1], MockData.Ids[0]), Is.False);

            Assert.That(this.Service.RemovePendingResponse(MockData.Ids[1], MockData.Ids[1]), Is.False);
            Assert.That(this.Service.RemovePendingResponse(MockData.Ids[1], MockData.Ids[0]), Is.False);
            Assert.That(this.Service.RemovePendingResponse(MockData.Ids[1], MockData.Ids[0]), Is.False);

            Assert.That(this.Service.RemovePendingResponse(MockData.Ids[0], MockData.Ids[0]), Is.True);
            Assert.That(this.Service.IsResponsePending(MockData.Ids[0], MockData.Ids[0]), Is.False);
            this.Service.AddPendingResponse(MockData.Ids[0], MockData.Ids[0]);
            Assert.That(this.Service.IsResponsePending(MockData.Ids[0], MockData.Ids[0]), Is.True);

            this.Service.AddPendingResponse(MockData.Ids[0], MockData.Ids[1]);
            this.Service.AddPendingResponse(MockData.Ids[0], MockData.Ids[2]);
            this.Service.AddPendingResponse(MockData.Ids[0], MockData.Ids[3]);
            Assert.That(this.Service.IsResponsePending(MockData.Ids[0], MockData.Ids[1]), Is.True);
            Assert.That(this.Service.IsResponsePending(MockData.Ids[0], MockData.Ids[2]), Is.True);
            Assert.That(this.Service.IsResponsePending(MockData.Ids[0], MockData.Ids[3]), Is.True);
            Assert.That(this.Service.RemovePendingResponse(MockData.Ids[0], MockData.Ids[1]), Is.True);
            Assert.That(this.Service.RemovePendingResponse(MockData.Ids[0], MockData.Ids[1]), Is.False);
            Assert.That(this.Service.IsResponsePending(MockData.Ids[0], MockData.Ids[1]), Is.False);
            Assert.That(this.Service.IsResponsePending(MockData.Ids[0], MockData.Ids[2]), Is.True);
            Assert.That(this.Service.IsResponsePending(MockData.Ids[0], MockData.Ids[3]), Is.True);
            Assert.That(this.Service.RemovePendingResponse(MockData.Ids[0], MockData.Ids[2]), Is.True);
            Assert.That(this.Service.IsResponsePending(MockData.Ids[0], MockData.Ids[1]), Is.False);
            Assert.That(this.Service.IsResponsePending(MockData.Ids[0], MockData.Ids[2]), Is.False);
            Assert.That(this.Service.IsResponsePending(MockData.Ids[0], MockData.Ids[3]), Is.True);
            Assert.That(this.Service.RemovePendingResponse(MockData.Ids[0], MockData.Ids[3]), Is.True);
            Assert.That(this.Service.IsResponsePending(MockData.Ids[0], MockData.Ids[1]), Is.False);
            Assert.That(this.Service.IsResponsePending(MockData.Ids[0], MockData.Ids[2]), Is.False);
            Assert.That(this.Service.IsResponsePending(MockData.Ids[0], MockData.Ids[3]), Is.False);
        }

        [Test]
        public async Task ConcurrentTests()
        {
            await Task.WhenAll(
                Enumerable.Range(0, MockData.Ids.Count)
                          .Select(i => Task.Run(() => {
                              this.Service.AddPendingResponse(MockData.Ids[i], MockData.Ids[0]);
                              this.Service.AddPendingResponse(MockData.Ids[i], MockData.Ids[1]);
                              this.Service.AddPendingResponse(MockData.Ids[i], MockData.Ids[2]);
                              Assert.That(this.Service.IsResponsePending(MockData.Ids[i], MockData.Ids[0]), Is.True);
                              Assert.That(this.Service.RemovePendingResponse(MockData.Ids[i], MockData.Ids[0]), Is.True);
                              Assert.That(this.Service.IsResponsePending(MockData.Ids[i], MockData.Ids[0]), Is.False);
                              Assert.That(this.Service.IsResponsePending(MockData.Ids[i], MockData.Ids[1]), Is.True);
                              Assert.That(this.Service.IsResponsePending(MockData.Ids[i], MockData.Ids[2]), Is.True);
                              Assert.That(this.Service.RemovePendingResponse(MockData.Ids[i], MockData.Ids[1]), Is.True);
                              Assert.That(this.Service.RemovePendingResponse(MockData.Ids[i], MockData.Ids[2]), Is.True);
                              Assert.That(this.Service.IsResponsePending(MockData.Ids[i], MockData.Ids[1]), Is.False);
                              Assert.That(this.Service.IsResponsePending(MockData.Ids[i], MockData.Ids[2]), Is.False);
                          }))
            );
        }
    }
}