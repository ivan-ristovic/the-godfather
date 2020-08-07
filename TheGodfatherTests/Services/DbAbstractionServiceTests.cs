using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using TheGodfather.Database;
using TheGodfather.Database.Models;
using TheGodfather.Services;

namespace TheGodfather.Tests.Services
{
    [TestFixture]
    public sealed class DbAbstractionServiceTests : ITheGodfatherServiceTest<ConcreteService2>
    {
        public ConcreteService2 Service { get; private set; } = null!;


        [SetUp]
        public void InitializeService()
        {
            this.Service = new ConcreteService2(TestDatabaseProvider.Database);
        }


        [Test]
        public void GetTests()
        {
            TestDatabaseProvider.AlterAndVerify(
                alter: _ => { },
                verify: db => {
                    IReadOnlyList<ulong> all = this.Service.Get();
                    Assert.That(all, Is.Empty);
                }
            );

            TestDatabaseProvider.SetupAlterAndVerify(
                setup: db => this.AddMockData(db),
                alter: _ => { },
                verify: db => {
                    IReadOnlyList<ulong> all = this.Service.Get();
                    Assert.That(all, Has.Exactly(2).Items);
                    Assert.That(all, Is.EqualTo(new[] { MockData.Ids[0], MockData.Ids[1] }));
                }
            );
        }

        [Test]
        public async Task ClearAsyncTests()
        {
            await TestDatabaseProvider.SetupAlterAndVerifyAsync(
                setup: db => {
                    this.AddMockData(db);
                    return Task.CompletedTask;
                },
                alter: db => this.Service.ClearAsync(),
                verify: db => {
                    IReadOnlyList<ulong> all = this.Service.Get();
                    Assert.That(all, Is.Empty);
                    Assert.That(db.PrivilegedUsers, Is.Empty);
                    return Task.CompletedTask;
                }
            );
        }

        [Test]
        public async Task ContainsAsyncTests()
        {
            await TestDatabaseProvider.AlterAndVerifyAsync(
                alter: _ => Task.CompletedTask,
                verify: async db => {
                    foreach (ulong id in MockData.Ids)
                        Assert.That(await this.Service.ContainsAsync(id), Is.False);
                }
            );

            await TestDatabaseProvider.SetupAlterAndVerifyAsync(
                setup: db => {
                    this.AddMockData(db);
                    return Task.CompletedTask;
                },
                alter: _ => Task.CompletedTask,
                verify: async db => {
                    Assert.That(await this.Service.ContainsAsync(MockData.Ids[0]));
                    Assert.That(await this.Service.ContainsAsync(MockData.Ids[1]));
                    foreach (ulong id in MockData.Ids.Skip(2))
                        Assert.That(await this.Service.ContainsAsync(id), Is.False);
                }
            );
        }

        [Test]
        public async Task AddAsyncTests()
        {
            await TestDatabaseProvider.AlterAndVerifyAsync(
                alter: _ => this.Service.AddAsync(MockData.Ids.Take(2)),
                verify: db => {
                    Assert.That(this.Service.Get(), Is.EquivalentTo(MockData.Ids.Take(2)));
                    Assert.That(db.PrivilegedUsers, Is.EquivalentTo(MockData.Ids.Take(2).Select(id => new PrivilegedUser { UserId = id })));
                    return Task.CompletedTask;
                }
            );

            await TestDatabaseProvider.SetupAlterAndVerifyAsync(
                setup: db => {
                    this.AddMockData(db);
                    return Task.CompletedTask;
                },
                alter: _ => this.Service.AddAsync(MockData.Ids[2]),
                verify: db => {
                    Assert.That(this.Service.Get(), Is.EquivalentTo(MockData.Ids.Take(3)));
                    Assert.That(db.PrivilegedUsers, Is.EquivalentTo(MockData.Ids.Take(3).Select(id => new PrivilegedUser { UserId = id })));
                    return Task.CompletedTask;
                }
            );

            await TestDatabaseProvider.SetupAlterAndVerifyAsync(
                setup: db => {
                    this.AddMockData(db);
                    return Task.CompletedTask;
                },
                alter: _ => this.Service.AddAsync(MockData.Ids[2], MockData.Ids[2], MockData.Ids[2]),
                verify: db => {
                    Assert.That(this.Service.Get(), Is.EquivalentTo(MockData.Ids.Take(3)));
                    Assert.That(db.PrivilegedUsers, Is.EquivalentTo(MockData.Ids.Take(3).Select(id => new PrivilegedUser { UserId = id })));
                    return Task.CompletedTask;
                }
            );

            await TestDatabaseProvider.SetupAlterAndVerifyAsync(
                setup: db => {
                    this.AddMockData(db);
                    return Task.CompletedTask;
                },
                alter: _ => this.Service.AddAsync(MockData.Ids[0], MockData.Ids[1]),
                verify: db => {
                    Assert.That(this.Service.Get(), Is.EquivalentTo(MockData.Ids.Take(2)));
                    Assert.That(db.PrivilegedUsers, Is.EquivalentTo(MockData.Ids.Take(2).Select(id => new PrivilegedUser { UserId = id })));
                    return Task.CompletedTask;
                }
            );

            await TestDatabaseProvider.AlterAndVerifyAsync(
                alter: async _ => {
                    await this.Service.AddAsync();
                    await this.Service.AddAsync(null!);
                    await this.Service.AddAsync(new ulong[] { });
                    await this.Service.AddAsync(Enumerable.Empty<ulong>());
                },
                verify: db => {
                    Assert.That(this.Service.Get(), Is.Empty);
                    Assert.That(db.PrivilegedUsers, Is.Empty);
                    return Task.CompletedTask;
                }
            );
        }

        [Test]
        public async Task RemoveAsyncTests()
        {
            await TestDatabaseProvider.SetupAlterAndVerifyAsync(
                setup: db => {
                    this.AddMockData(db);
                    return Task.CompletedTask;
                },
                alter: _ => this.Service.RemoveAsync(MockData.Ids.Take(2)),
                verify: db => {
                    Assert.That(this.Service.Get(), Is.Empty);
                    Assert.That(db.PrivilegedUsers, Is.Empty);
                    return Task.CompletedTask;
                }
            );

            await TestDatabaseProvider.SetupAlterAndVerifyAsync(
                setup: db => {
                    this.AddMockData(db);
                    return Task.CompletedTask;
                },
                alter: _ => this.Service.RemoveAsync(MockData.Ids[1]),
                verify: db => {
                    Assert.That(this.Service.Get().Single(), Is.EqualTo(MockData.Ids[0]));
                    Assert.That(db.PrivilegedUsers.Single().UserId, Is.EqualTo(MockData.Ids[0]));
                    return Task.CompletedTask;
                }
            );

            await TestDatabaseProvider.SetupAlterAndVerifyAsync(
                setup: db => {
                    this.AddMockData(db);
                    return Task.CompletedTask;
                },
                alter: _ => this.Service.RemoveAsync(MockData.Ids[1], MockData.Ids[1], MockData.Ids[1]),
                verify: db => {
                    Assert.That(this.Service.Get().Single(), Is.EqualTo(MockData.Ids[0]));
                    Assert.That(db.PrivilegedUsers.Single().UserId, Is.EqualTo(MockData.Ids[0]));
                    return Task.CompletedTask;
                }
            );

            await TestDatabaseProvider.SetupAlterAndVerifyAsync(
                setup: db => {
                    this.AddMockData(db);
                    return Task.CompletedTask;
                },
                alter: _ => this.Service.RemoveAsync(MockData.Ids[3], MockData.Ids[4]),
                verify: db => {
                    Assert.That(this.Service.Get(), Is.EquivalentTo(MockData.Ids.Take(2)));
                    Assert.That(db.PrivilegedUsers, Is.EquivalentTo(MockData.Ids.Take(2).Select(id => new PrivilegedUser { UserId = id })));
                    return Task.CompletedTask;
                }
            );

            await TestDatabaseProvider.SetupAlterAndVerifyAsync(
                setup: db => {
                    this.AddMockData(db);
                    return Task.CompletedTask;
                },
                alter: async _ => {
                    await this.Service.RemoveAsync();
                    await this.Service.RemoveAsync(null!);
                    await this.Service.RemoveAsync(new ulong[] { });
                    await this.Service.RemoveAsync(Enumerable.Empty<ulong>());
                },
                verify: db => {
                    Assert.That(this.Service.Get(), Is.EquivalentTo(MockData.Ids.Take(2)));
                    Assert.That(db.PrivilegedUsers, Is.EquivalentTo(MockData.Ids.Take(2).Select(id => new PrivilegedUser { UserId = id })));
                    return Task.CompletedTask;
                }
            );
        }


        private void AddMockData(TheGodfatherDbContext db)
        {
            db.PrivilegedUsers.Add(new PrivilegedUser { UserId = MockData.Ids[0] });
            db.PrivilegedUsers.Add(new PrivilegedUser { UserId = MockData.Ids[1] });
        }
    }
 
    public class ConcreteService2 : DbAbstractionServiceBase<PrivilegedUser, ulong>
    {
        public override bool IsDisabled => false;

        public ConcreteService2(DbContextBuilder dbb) : base(dbb) { }

        public override DbSet<PrivilegedUser> DbSetSelector(TheGodfatherDbContext db) => db.PrivilegedUsers;
        public override PrivilegedUser EntityFactory(ulong id) => new PrivilegedUser{ UserId = id };
        public override ulong EntityIdSelector(PrivilegedUser entity) => entity.UserId;
        public override object[] EntityPrimaryKeySelector(ulong id) => new object[] { (long)id };
    }
}
