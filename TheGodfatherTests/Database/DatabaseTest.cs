using System.Linq;
using NUnit.Framework;

namespace TheGodfatherTests.Database
{
    [TestFixture]
    public class DatabaseTest
    {
        [Test]
        public void Test()
        {
            TestDatabaseProvider.AlterAndVerify(
                alter: db => {
                    db.MessageCount.Add(new TheGodfather.Database.Entities.DatabaseMessageCount() {
                        UserId = 123,
                        MessageCount = 5,
                    });
                },
               verify: db => {
                   Assert.AreEqual(1, db.MessageCount.Count());
               },
               ensureSave: true
            );
        }
    }
}
