using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using TheGodfather.Services;

namespace TheGodfatherTests.Services
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
            Assert.IsFalse(this.Service.IsResponsePending(MockData.Ids[0], MockData.Ids[0]));
            this.Service.AddPendingResponse(MockData.Ids[0], MockData.Ids[0]);
            this.Service.AddPendingResponse(MockData.Ids[0], MockData.Ids[0]);
            Assert.IsTrue(this.Service.IsResponsePending(MockData.Ids[0], MockData.Ids[0]));
            Assert.IsFalse(this.Service.IsResponsePending(MockData.Ids[0], MockData.Ids[1]));
            Assert.IsFalse(this.Service.IsResponsePending(MockData.Ids[1], MockData.Ids[0]));

            Assert.IsFalse(this.Service.RemovePendingResponse(MockData.Ids[1], MockData.Ids[1]));
            Assert.IsFalse(this.Service.RemovePendingResponse(MockData.Ids[1], MockData.Ids[0]));
            Assert.IsFalse(this.Service.RemovePendingResponse(MockData.Ids[1], MockData.Ids[0]));

            Assert.IsTrue(this.Service.RemovePendingResponse(MockData.Ids[0], MockData.Ids[0]));
            Assert.IsFalse(this.Service.IsResponsePending(MockData.Ids[0], MockData.Ids[0]));
            this.Service.AddPendingResponse(MockData.Ids[0], MockData.Ids[0]);
            Assert.IsTrue(this.Service.IsResponsePending(MockData.Ids[0], MockData.Ids[0]));

            this.Service.AddPendingResponse(MockData.Ids[0], MockData.Ids[1]);
            this.Service.AddPendingResponse(MockData.Ids[0], MockData.Ids[2]);
            this.Service.AddPendingResponse(MockData.Ids[0], MockData.Ids[3]);
            Assert.IsTrue(this.Service.IsResponsePending(MockData.Ids[0], MockData.Ids[1]));
            Assert.IsTrue(this.Service.IsResponsePending(MockData.Ids[0], MockData.Ids[2]));
            Assert.IsTrue(this.Service.IsResponsePending(MockData.Ids[0], MockData.Ids[3]));
            Assert.IsTrue(this.Service.RemovePendingResponse(MockData.Ids[0], MockData.Ids[1]));
            Assert.IsFalse(this.Service.RemovePendingResponse(MockData.Ids[0], MockData.Ids[1]));
            Assert.IsFalse(this.Service.IsResponsePending(MockData.Ids[0], MockData.Ids[1]));
            Assert.IsTrue(this.Service.IsResponsePending(MockData.Ids[0], MockData.Ids[2]));
            Assert.IsTrue(this.Service.IsResponsePending(MockData.Ids[0], MockData.Ids[3]));
            Assert.IsTrue(this.Service.RemovePendingResponse(MockData.Ids[0], MockData.Ids[2]));
            Assert.IsFalse(this.Service.IsResponsePending(MockData.Ids[0], MockData.Ids[1]));
            Assert.IsFalse(this.Service.IsResponsePending(MockData.Ids[0], MockData.Ids[2]));
            Assert.IsTrue(this.Service.IsResponsePending(MockData.Ids[0], MockData.Ids[3]));
            Assert.IsTrue(this.Service.RemovePendingResponse(MockData.Ids[0], MockData.Ids[3]));
            Assert.IsFalse(this.Service.IsResponsePending(MockData.Ids[0], MockData.Ids[1]));
            Assert.IsFalse(this.Service.IsResponsePending(MockData.Ids[0], MockData.Ids[2]));
            Assert.IsFalse(this.Service.IsResponsePending(MockData.Ids[0], MockData.Ids[3]));
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
                              Assert.IsTrue(this.Service.IsResponsePending(MockData.Ids[i], MockData.Ids[0]));
                              Assert.IsTrue(this.Service.RemovePendingResponse(MockData.Ids[i], MockData.Ids[0]));
                              Assert.IsFalse(this.Service.IsResponsePending(MockData.Ids[i], MockData.Ids[0]));
                              Assert.IsTrue(this.Service.IsResponsePending(MockData.Ids[i], MockData.Ids[1]));
                              Assert.IsTrue(this.Service.IsResponsePending(MockData.Ids[i], MockData.Ids[2]));
                              Assert.IsTrue(this.Service.RemovePendingResponse(MockData.Ids[i], MockData.Ids[1]));
                              Assert.IsTrue(this.Service.RemovePendingResponse(MockData.Ids[i], MockData.Ids[2]));
                              Assert.IsFalse(this.Service.IsResponsePending(MockData.Ids[i], MockData.Ids[1]));
                              Assert.IsFalse(this.Service.IsResponsePending(MockData.Ids[i], MockData.Ids[2]));
                          }))
            );
        }
    }
}