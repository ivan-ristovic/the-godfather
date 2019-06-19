using System.Linq;
using NUnit.Framework;
using TheGodfather.Database.Entities;
using TheGodfather.Services;
using TheGodfatherTests.Database;

namespace TheGodfatherTests.Services
{
    [TestFixture]
    public sealed class UserRanksServiceTests : IServiceTest<UserRanksService>
    {
        public UserRanksService Service { get; private set; }

        private const ulong uid1 = 123456123456;
        private const ulong uid2 = 123456123457;
        private const ulong uid3 = 123456123458;


        [SetUp]
        public void InitializeService()
        {
            this.Service = new UserRanksService();
        }


        [Test]
        public void CalculateRankForMessageCountTest()
        {
            Assert.AreEqual(0, this.Service.CalculateRankForMessageCount(0));
            Assert.AreEqual(0, this.Service.CalculateRankForMessageCount(1));
            Assert.AreEqual(0, this.Service.CalculateRankForMessageCount(9));
            Assert.AreEqual(1, this.Service.CalculateRankForMessageCount(10));
            Assert.AreEqual(1, this.Service.CalculateRankForMessageCount(20));
            Assert.AreEqual(1, this.Service.CalculateRankForMessageCount(39));
            Assert.AreEqual(2, this.Service.CalculateRankForMessageCount(40));
            Assert.AreEqual(2, this.Service.CalculateRankForMessageCount(60));
            Assert.AreEqual(2, this.Service.CalculateRankForMessageCount(89));
            Assert.AreEqual(3, this.Service.CalculateRankForMessageCount(90));
            Assert.AreEqual(3, this.Service.CalculateRankForMessageCount(101));
            Assert.AreEqual(3, this.Service.CalculateRankForMessageCount(159));
            Assert.AreEqual(4, this.Service.CalculateRankForMessageCount(160));
            Assert.AreEqual(5, this.Service.CalculateRankForMessageCount(250));
            Assert.AreEqual(94, this.Service.CalculateRankForMessageCount(88361));
        }

        [Test]
        public void GetAndIncrementMessageCountForUserTest()
        {
            Assert.AreEqual(0, this.Service.GetMessageCountForUser(uid1));
            Assert.AreEqual(0, this.Service.GetMessageCountForUser(uid2));
            Assert.AreEqual(0, this.Service.GetMessageCountForUser(uid3));

            Assert.AreEqual(0, this.Service.IncrementMessageCountForUser(uid1));

            Assert.AreEqual(1, this.Service.GetMessageCountForUser(uid1));
            Assert.AreEqual(0, this.Service.GetMessageCountForUser(uid2));
            Assert.AreEqual(0, this.Service.GetMessageCountForUser(uid3));

            for (int i = 0; i < 9; i++)
                Assert.AreEqual(0, this.Service.IncrementMessageCountForUser(uid2));
            Assert.AreEqual(1, this.Service.IncrementMessageCountForUser(uid2));
            Assert.AreEqual(0, this.Service.IncrementMessageCountForUser(uid2));

            Assert.AreEqual(1, this.Service.GetMessageCountForUser(uid1));
            Assert.AreEqual(11, this.Service.GetMessageCountForUser(uid2));
            Assert.AreEqual(0, this.Service.GetMessageCountForUser(uid3));
        }

        [Test]
        public void CalculateRankForUserTest()
        {
            Assert.AreEqual(0, this.Service.GetMessageCountForUser(uid1));
            Assert.AreEqual(0, this.Service.GetMessageCountForUser(uid2));
            Assert.AreEqual(0, this.Service.GetMessageCountForUser(uid3));

            for (int i = 0; i < 10; i++)
                this.Service.IncrementMessageCountForUser(uid1);

            Assert.AreEqual(1, this.Service.CalculateRankForUser(uid1));
            Assert.AreEqual(0, this.Service.CalculateRankForUser(uid2));
            Assert.AreEqual(0, this.Service.CalculateRankForUser(uid3));
        }

        [Test]
        public void BlankDatabaseSyncTest()
        {
            this.Service.IncrementMessageCountForUser(uid1);
            this.Service.IncrementMessageCountForUser(uid1);
            for (int i = 0; i < 9; i++)
                this.Service.IncrementMessageCountForUser(uid2);
            this.Service.IncrementMessageCountForUser(uid3);

            TestDatabaseProvider.AlterAndVerify(
                alter: db => this.Service.Sync(db),
                verify: db => {
                    Assert.AreEqual(3, db.MessageCount.Count());
                    DatabaseMessageCount u1 = db.MessageCount.Find((long)uid1);
                    DatabaseMessageCount u2 = db.MessageCount.Find((long)uid2);
                    DatabaseMessageCount u3 = db.MessageCount.Find((long)uid3);
                    Assert.IsNull(db.MessageCount.Find(123451234512345));
                    Assert.IsNotNull(u1);
                    Assert.IsNotNull(u2);
                    Assert.IsNotNull(u3);
                    Assert.AreEqual(2, u1.MessageCount);
                    Assert.AreEqual(9, u2.MessageCount);
                    Assert.AreEqual(1, u3.MessageCount);
                }
            );
        }

        [Test]
        public void FilledDatabaseSyncTest()
        {
            this.Service.IncrementMessageCountForUser(uid2);
            this.Service.IncrementMessageCountForUser(uid3);

            TestDatabaseProvider.SetupAlterAndVerify(
                setup: db => {
                    var msgcount = new DatabaseMessageCount[] {
                        new DatabaseMessageCount() {
                            MessageCount = 5,
                            UserId = uid1
                        },
                        new DatabaseMessageCount() {
                            MessageCount = 5,
                            UserId = uid2
                        },
                    };
                    db.MessageCount.AddRange(msgcount);
                },
                alter: db => this.Service.Sync(db),
                verify: db => {
                    Assert.AreEqual(3, db.MessageCount.Count());
                    DatabaseMessageCount u1 = db.MessageCount.Find((long)uid1);
                    DatabaseMessageCount u2 = db.MessageCount.Find((long)uid2);
                    DatabaseMessageCount u3 = db.MessageCount.Find((long)uid3);
                    Assert.IsNull(db.MessageCount.Find(123451234512345));
                    Assert.IsNotNull(u1);
                    Assert.IsNotNull(u2);
                    Assert.IsNotNull(u3);
                    Assert.AreEqual(5, u1.MessageCount);
                    Assert.AreEqual(6, u2.MessageCount);
                    Assert.AreEqual(1, u3.MessageCount);
                }
            );
        }

        [Test]
        public void RepetitiveSyncTest()
        {
            this.Service.IncrementMessageCountForUser(uid2);
            this.Service.IncrementMessageCountForUser(uid3);

            TestDatabaseProvider.SetupAlterAndVerify(
                setup: db => {
                    var msgcount = new DatabaseMessageCount[] {
                        new DatabaseMessageCount() {
                            MessageCount = 5,
                            UserId = uid1
                        },
                        new DatabaseMessageCount() {
                            MessageCount = 5,
                            UserId = uid2
                        },
                    };
                    db.MessageCount.AddRange(msgcount);
                },
                alter: db => {
                    this.Service.Sync(db);
                    this.Service.IncrementMessageCountForUser(uid2);
                    this.Service.IncrementMessageCountForUser(uid3);
                    this.Service.Sync(db);
                    this.Service.IncrementMessageCountForUser(uid2);
                    this.Service.IncrementMessageCountForUser(uid3);
                    this.Service.Sync(db);
                    this.Service.Sync(db);
                    this.Service.Sync(db);
                },
                verify: db => {
                    Assert.AreEqual(3, db.MessageCount.Count());
                    DatabaseMessageCount u1 = db.MessageCount.Find((long)uid1);
                    DatabaseMessageCount u2 = db.MessageCount.Find((long)uid2);
                    DatabaseMessageCount u3 = db.MessageCount.Find((long)uid3);
                    Assert.IsNull(db.MessageCount.Find(123451234512345));
                    Assert.IsNotNull(u1);
                    Assert.IsNotNull(u2);
                    Assert.IsNotNull(u3);
                    Assert.AreEqual(5, u1.MessageCount);
                    Assert.AreEqual(8, u2.MessageCount);
                    Assert.AreEqual(3, u3.MessageCount);
                }
            );
        }
    }
}
