using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using TheGodfather.Database;
using TheGodfather.Database.Models;

namespace TheGodfather.Tests.Services;

[TestFixture]
public sealed class DbAbstractionService2Tests : ITheGodfatherServiceTest<ConcreteService2>
{
    public ConcreteService2 Service { get; private set; } = null!;


    [SetUp]
    public void InitializeService()
    {
        this.Service = new ConcreteService2(TestDbProvider.Database);
    }


    [Test]
    public void GetTests()
    {
        TestDbProvider.AlterAndVerify(
            _ => { },
            _ => {
                IReadOnlyList<ulong> all = this.Service.GetIds();
                Assert.That(all, Is.Empty);
            }
        );

        TestDbProvider.SetupAlterAndVerify(
            db => this.AddMockData(db),
            _ => { },
            _ => {
                IReadOnlyList<ulong> all = this.Service.GetIds();
                Assert.That(all, Is.EqualTo(new[] { MockData.Ids[0], MockData.Ids[1] }));
            }
        );
    }

    [Test]
    public async Task ClearAsyncTests()
    {
        await TestDbProvider.SetupAlterAndVerifyAsync(
            db => {
                this.AddMockData(db);
                return Task.CompletedTask;
            },
            _ => this.Service.ClearAsync(),
            db => {
                IReadOnlyList<ulong> all = this.Service.GetIds();
                Assert.That(all, Is.Empty);
                Assert.That(db.PrivilegedUsers, Is.Empty);
                return Task.CompletedTask;
            }
        );

        await TestDbProvider.SetupAlterAndVerifyAsync(
            _ => Task.CompletedTask,
            _ => this.Service.ClearAsync(),
            db => {
                IReadOnlyList<ulong> all = this.Service.GetIds();
                Assert.That(all, Is.Empty);
                Assert.That(db.PrivilegedUsers, Is.Empty);
                return Task.CompletedTask;
            }
        );
    }

    [Test]
    public async Task ContainsAsyncTests()
    {
        await TestDbProvider.AlterAndVerifyAsync(
            _ => Task.CompletedTask,
            async _ => {
                foreach (ulong id in MockData.Ids)
                    Assert.That(await this.Service.ContainsAsync(id), Is.False);
            }
        );

        await TestDbProvider.SetupAlterAndVerifyAsync(
            db => {
                this.AddMockData(db);
                return Task.CompletedTask;
            },
            _ => Task.CompletedTask,
            async _ => {
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
        await TestDbProvider.AlterAndVerifyAsync(
            async _ => Assert.That(await this.Service.AddAsync(MockData.Ids.Take(2)), Is.EqualTo(2)),
            db => {
                Assert.That(this.Service.GetIds(), Is.EquivalentTo(MockData.Ids.Take(2)));
                Assert.That(db.PrivilegedUsers, Is.EquivalentTo(MockData.Ids.Take(2).Select(id => new PrivilegedUser { UserId = id })));
                return Task.CompletedTask;
            }
        );

        await TestDbProvider.SetupAlterAndVerifyAsync(
            db => {
                this.AddMockData(db);
                return Task.CompletedTask;
            },
            async _ => Assert.That(await this.Service.AddAsync(MockData.Ids[2]), Is.EqualTo(1)),
            db => {
                Assert.That(this.Service.GetIds(), Is.EquivalentTo(MockData.Ids.Take(3)));
                Assert.That(db.PrivilegedUsers, Is.EquivalentTo(MockData.Ids.Take(3).Select(id => new PrivilegedUser { UserId = id })));
                return Task.CompletedTask;
            }
        );

        await TestDbProvider.SetupAlterAndVerifyAsync(
            db => {
                this.AddMockData(db);
                return Task.CompletedTask;
            },
            async _ => Assert.That(await this.Service.AddAsync(MockData.Ids[2]), Is.EqualTo(1)),
            db => {
                Assert.That(this.Service.GetIds(), Is.EquivalentTo(MockData.Ids.Take(3)));
                Assert.That(db.PrivilegedUsers, Is.EquivalentTo(MockData.Ids.Take(3).Select(id => new PrivilegedUser { UserId = id })));
                return Task.CompletedTask;
            }
        );

        await TestDbProvider.SetupAlterAndVerifyAsync(
            db => {
                this.AddMockData(db);
                return Task.CompletedTask;
            },
            async _ => {
                Assert.That(await this.Service.AddAsync(MockData.Ids[0], MockData.Ids[0]), Is.EqualTo(0));
                Assert.That(await this.Service.AddAsync(MockData.Ids[0], MockData.Ids[1]), Is.EqualTo(0));
                Assert.That(await this.Service.AddAsync(MockData.Ids[0], MockData.Ids[1], MockData.Ids[2]), Is.EqualTo(1));
                Assert.That(await this.Service.AddAsync(MockData.Ids[0], MockData.Ids[1], MockData.Ids[2]), Is.EqualTo(0));
            },
            db => {
                Assert.That(this.Service.GetIds(), Is.EquivalentTo(MockData.Ids.Take(3)));
                Assert.That(db.PrivilegedUsers, Is.EquivalentTo(MockData.Ids.Take(3).Select(id => new PrivilegedUser { UserId = id })));
                return Task.CompletedTask;
            }
        );

        await TestDbProvider.AlterAndVerifyAsync(
            async _ => {
                Assert.That(await this.Service.AddAsync(Array.Empty<ulong>()), Is.Zero);
                Assert.That(await this.Service.AddAsync(Enumerable.Empty<ulong>()), Is.Zero);
            },
            db => {
                Assert.That(this.Service.GetIds(), Is.Empty);
                Assert.That(db.PrivilegedUsers, Is.Empty);
                return Task.CompletedTask;
            }
        );
    }

    [Test]
    public async Task RemoveAsyncTests()
    {
        await TestDbProvider.SetupAlterAndVerifyAsync(
            db => {
                this.AddMockData(db);
                return Task.CompletedTask;
            },
            async _ => Assert.That(await this.Service.RemoveAsync(MockData.Ids.Take(2)), Is.EqualTo(2)),
            db => {
                Assert.That(this.Service.GetIds(), Is.Empty);
                Assert.That(db.PrivilegedUsers, Is.Empty);
                return Task.CompletedTask;
            }
        );

        await TestDbProvider.SetupAlterAndVerifyAsync(
            db => {
                this.AddMockData(db);
                return Task.CompletedTask;
            },
            async _ => Assert.That(await this.Service.RemoveAsync(MockData.Ids[1]), Is.EqualTo(1)),
            db => {
                Assert.That(this.Service.GetIds().Single(), Is.EqualTo(MockData.Ids[0]));
                Assert.That(db.PrivilegedUsers.Single().UserId, Is.EqualTo(MockData.Ids[0]));
                return Task.CompletedTask;
            }
        );

        await TestDbProvider.SetupAlterAndVerifyAsync(
            db => {
                this.AddMockData(db);
                return Task.CompletedTask;
            },
            async _ => Assert.That(await this.Service.RemoveAsync(MockData.Ids[1], MockData.Ids[1], MockData.Ids[1]), Is.EqualTo(1)),
            db => {
                Assert.That(this.Service.GetIds().Single(), Is.EqualTo(MockData.Ids[0]));
                Assert.That(db.PrivilegedUsers.Single().UserId, Is.EqualTo(MockData.Ids[0]));
                return Task.CompletedTask;
            }
        );

        await TestDbProvider.SetupAlterAndVerifyAsync(
            db => {
                this.AddMockData(db);
                return Task.CompletedTask;
            },
            async _ => Assert.That(await this.Service.RemoveAsync(MockData.Ids[3], MockData.Ids[4]), Is.Zero),
            db => {
                Assert.That(this.Service.GetIds(), Is.EquivalentTo(MockData.Ids.Take(2)));
                Assert.That(db.PrivilegedUsers, Is.EquivalentTo(MockData.Ids.Take(2).Select(id => new PrivilegedUser { UserId = id })));
                return Task.CompletedTask;
            }
        );

        await TestDbProvider.SetupAlterAndVerifyAsync(
            db => {
                this.AddMockData(db);
                return Task.CompletedTask;
            },
            async _ => {
                Assert.That(await this.Service.RemoveAsync(Array.Empty<ulong>()), Is.Zero);
                Assert.That(await this.Service.RemoveAsync(Enumerable.Empty<ulong>()), Is.Zero);
            },
            db => {
                Assert.That(this.Service.GetIds(), Is.EquivalentTo(MockData.Ids.Take(2)));
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

[TestFixture]
public sealed class DbAbstractionService3Tests : ITheGodfatherServiceTest<ConcreteService3>
{
    public ConcreteService3 Service { get; private set; } = null!;


    [SetUp]
    public void InitializeService()
    {
        this.Service = new ConcreteService3(TestDbProvider.Database);
    }


    [Test]
    public void GetTests()
    {
        TestDbProvider.AlterAndVerify(
            _ => { },
            _ => {
                foreach (ulong id in MockData.Ids) {
                    IReadOnlyList<ulong> all = this.Service.GetIds(id);
                    Assert.That(all, Is.Empty);
                }
            }
        );

        TestDbProvider.SetupAlterAndVerify(
            db => this.AddMockData(db),
            _ => { },
            _ => {
                IReadOnlyList<ulong> gid0 = this.Service.GetIds(MockData.Ids[0]);
                Assert.That(gid0, Is.EquivalentTo(new[] { MockData.Ids[0], MockData.Ids[1] }));
                IReadOnlyList<ulong> gid1 = this.Service.GetIds(MockData.Ids[1]);
                Assert.That(gid1, Is.EquivalentTo(new[] { MockData.Ids[2] }));
                foreach (ulong id in MockData.Ids.Skip(2)) {
                    IReadOnlyList<ulong> gidx = this.Service.GetIds(id);
                    Assert.That(gidx, Is.Empty);
                }
            }
        );
    }

    [Test]
    public async Task ClearAsyncTests()
    {
        await TestDbProvider.SetupAlterAndVerifyAsync(
            db => {
                this.AddMockData(db);
                return Task.CompletedTask;
            },
            _ => this.Service.ClearAsync(MockData.Ids[0]),
            db => {
                IReadOnlyList<ulong> all = this.Service.GetIds(MockData.Ids[0]);
                Assert.That(all, Is.Empty);
                Assert.That(this.Service.GroupSelector(db.AutoRoles, MockData.Ids[0]), Is.Empty);
                IReadOnlyList<ulong> gid1 = this.Service.GetIds(MockData.Ids[1]);
                Assert.That(gid1, Is.EqualTo(new[] { MockData.Ids[2] }));
                Assert.That(this.GetGuildRoles(db, MockData.Ids[1]), Is.EquivalentTo(new[] { MockData.Ids[2] }));
                return Task.CompletedTask;
            }
        );

        int bef = 0;
        await TestDbProvider.SetupAlterAndVerifyAsync(
            db => {
                this.AddMockData(db);
                return Task.CompletedTask;
            },
            async db => {
                bef = db.AutoRoles.Count();
                await this.Service.ClearAsync(MockData.Ids[3]);
            },
            db => {
                Assert.That(db.AutoRoles, Has.Exactly(bef).Items);
                return Task.CompletedTask;
            }
        );

        await TestDbProvider.SetupAlterAndVerifyAsync(
            _ => Task.CompletedTask,
            _ => this.Service.ClearAsync(MockData.Ids[0]),
            _ => {
                foreach (ulong id in MockData.Ids) {
                    IReadOnlyList<ulong> all = this.Service.GetIds(id);
                    Assert.That(all, Is.Empty);
                }
                return Task.CompletedTask;
            }
        );
    }

    [Test]
    public async Task ContainsAsyncTests()
    {
        await TestDbProvider.AlterAndVerifyAsync(
            _ => Task.CompletedTask,
            async _ => {
                foreach (ulong gid in MockData.Ids)
                foreach (ulong rid in MockData.Ids)
                    Assert.That(await this.Service.ContainsAsync(gid, rid), Is.False);
            }
        );

        await TestDbProvider.SetupAlterAndVerifyAsync(
            db => {
                this.AddMockData(db);
                return Task.CompletedTask;
            },
            _ => Task.CompletedTask,
            async _ => {
                Assert.That(await this.Service.ContainsAsync(MockData.Ids[0], MockData.Ids[0]));
                Assert.That(await this.Service.ContainsAsync(MockData.Ids[0], MockData.Ids[1]));
                Assert.That(await this.Service.ContainsAsync(MockData.Ids[1], MockData.Ids[2]));
                Assert.That(await this.Service.ContainsAsync(MockData.Ids[0], MockData.Ids[2]), Is.False);
                Assert.That(await this.Service.ContainsAsync(MockData.Ids[0], MockData.Ids[3]), Is.False);
                Assert.That(await this.Service.ContainsAsync(MockData.Ids[1], MockData.Ids[0]), Is.False);
                Assert.That(await this.Service.ContainsAsync(MockData.Ids[1], MockData.Ids[1]), Is.False);
                Assert.That(await this.Service.ContainsAsync(MockData.Ids[1], MockData.Ids[3]), Is.False);
                foreach (ulong gid in MockData.Ids.Skip(2))
                foreach (ulong rid in MockData.Ids)
                    Assert.That(await this.Service.ContainsAsync(gid, rid), Is.False);
            }
        );
    }

    [Test]
    public async Task AddAsyncTests()
    {
        int bef = 0;

        await TestDbProvider.AlterAndVerifyAsync(
            async _ => Assert.That(await this.Service.AddAsync(MockData.Ids[0], MockData.Ids.Take(2)), Is.EqualTo(2)),
            db => {
                Assert.That(this.Service.GetIds(MockData.Ids[0]), Is.EquivalentTo(MockData.Ids.Take(2)));
                Assert.That(db.AutoRoles, Has.Exactly(2).Items);
                Assert.That(this.GetGuildRoles(db, MockData.Ids[0]), Is.EquivalentTo(MockData.Ids.Take(2)));
                foreach (ulong gid in MockData.Ids.Skip(1))
                    Assert.That(this.Service.GetIds(gid), Is.Empty);
                return Task.CompletedTask;
            }
        );

        await TestDbProvider.SetupAlterAndVerifyAsync(
            db => {
                this.AddMockData(db);
                return Task.CompletedTask;
            },
            async db => {
                bef = db.AutoRoles.Count();
                Assert.That(await this.Service.AddAsync(MockData.Ids[0], MockData.Ids[2]), Is.EqualTo(1));
            },
            db => {
                Assert.That(this.Service.GetIds(MockData.Ids[0]), Is.EquivalentTo(MockData.Ids.Take(3)));
                Assert.That(this.GetGuildRoles(db, MockData.Ids[0]), Is.EquivalentTo(MockData.Ids.Take(3)));
                Assert.That(db.AutoRoles, Has.Exactly(bef + 1).Items);
                return Task.CompletedTask;
            }
        );


        await TestDbProvider.SetupAlterAndVerifyAsync(
            db => {
                this.AddMockData(db);
                return Task.CompletedTask;
            },
            async db => {
                bef = db.AutoRoles.Count();
                Assert.That(await this.Service.AddAsync(MockData.Ids[2], MockData.Ids[1]), Is.EqualTo(1));
                Assert.That(await this.Service.AddAsync(MockData.Ids[2], MockData.Ids[1]), Is.EqualTo(0));
                Assert.That(await this.Service.AddAsync(MockData.Ids[3], MockData.Ids[1], MockData.Ids[1]), Is.EqualTo(1));
                Assert.That(await this.Service.AddAsync(MockData.Ids[0], MockData.Ids[0]), Is.EqualTo(0));
            },
            db => {
                Assert.That(this.Service.GetIds(MockData.Ids[0]), Is.EquivalentTo(MockData.Ids.Take(2)));
                Assert.That(this.Service.GetIds(MockData.Ids[1]).Single(), Is.EqualTo(MockData.Ids[2]));
                Assert.That(this.Service.GetIds(MockData.Ids[2]).Single(), Is.EqualTo(MockData.Ids[1]));
                Assert.That(this.Service.GetIds(MockData.Ids[3]).Single(), Is.EqualTo(MockData.Ids[1]));
                Assert.That(db.AutoRoles, Has.Exactly(bef + 2).Items);
                return Task.CompletedTask;
            }
        );

        await TestDbProvider.AlterAndVerifyAsync(
            async _ => {
                foreach (ulong gid in MockData.Ids) {
                    Assert.That(await this.Service.AddAsync(gid), Is.Zero);
                    Assert.That(await this.Service.AddAsync(gid, null!), Is.Zero);
                    Assert.That(await this.Service.AddAsync(gid, Array.Empty<ulong>()), Is.Zero);
                    Assert.That(await this.Service.AddAsync(gid, Enumerable.Empty<ulong>()), Is.Zero);
                }
            },
            db => {
                Assert.That(db.AutoRoles, Is.Empty);
                foreach (ulong gid in MockData.Ids)
                    Assert.That(this.Service.GetIds(gid), Is.Empty);
                return Task.CompletedTask;
            }
        );
    }

    [Test]
    public async Task RemoveAsyncTests()
    {
        int bef = 0;

        await TestDbProvider.SetupAlterAndVerifyAsync(
            db => {
                this.AddMockData(db);
                return Task.CompletedTask;
            },
            async _ => Assert.That(await this.Service.RemoveAsync(MockData.Ids[0], MockData.Ids.Take(2)), Is.EqualTo(2)),
            db => {
                Assert.That(this.Service.GetIds(MockData.Ids[0]), Is.Empty);
                Assert.That(this.Service.GetIds(MockData.Ids[1]).Single(), Is.EqualTo(MockData.Ids[2]));
                Assert.That(this.GetGuildRoles(db, MockData.Ids[0]), Is.Empty);
                Assert.That(this.GetGuildRoles(db, MockData.Ids[1]), Is.Not.Empty);
                return Task.CompletedTask;
            }
        );

        await TestDbProvider.SetupAlterAndVerifyAsync(
            db => {
                this.AddMockData(db);
                return Task.CompletedTask;
            },
            async _ => Assert.That(await this.Service.RemoveAsync(MockData.Ids[0], MockData.Ids[1]), Is.EqualTo(1)),
            db => {
                Assert.That(this.Service.GetIds(MockData.Ids[0]).Single(), Is.EqualTo(MockData.Ids[0]));
                Assert.That(this.GetGuildRoles(db, MockData.Ids[0]).Single(), Is.EqualTo(MockData.Ids[0]));
                return Task.CompletedTask;
            }
        );

        await TestDbProvider.SetupAlterAndVerifyAsync(
            db => {
                this.AddMockData(db);
                return Task.CompletedTask;
            },
            async _ => Assert.That(await this.Service.RemoveAsync(MockData.Ids[0], MockData.Ids[1], MockData.Ids[1], MockData.Ids[1]), Is.EqualTo(1)),
            db => {
                Assert.That(this.Service.GetIds(MockData.Ids[0]).Single(), Is.EqualTo(MockData.Ids[0]));
                Assert.That(this.GetGuildRoles(db, MockData.Ids[0]).Single(), Is.EqualTo(MockData.Ids[0]));
                return Task.CompletedTask;
            }
        );

        await TestDbProvider.SetupAlterAndVerifyAsync(
            db => {
                this.AddMockData(db);
                return Task.CompletedTask;
            },
            async db => {
                Assert.That(await this.Service.RemoveAsync(MockData.Ids[0], MockData.Ids[3], MockData.Ids[4]), Is.Zero);
                bef = db.AutoRoles.Count();
            },
            db => {
                Assert.That(this.GetGuildRoles(db, MockData.Ids[0]), Is.EquivalentTo(MockData.Ids.Take(2)));
                Assert.That(db.AutoRoles, Has.Exactly(bef).Items);
                return Task.CompletedTask;
            }
        );

        await TestDbProvider.SetupAlterAndVerifyAsync(
            db => {
                this.AddMockData(db);
                return Task.CompletedTask;
            },
            async db => {
                bef = db.AutoRoles.Count();
                foreach (ulong gid in MockData.Ids) {
                    Assert.That(await this.Service.RemoveAsync(gid), Is.Zero);
                    Assert.That(await this.Service.RemoveAsync(gid, null!), Is.Zero);
                    Assert.That(await this.Service.RemoveAsync(gid, Array.Empty<ulong>()), Is.Zero);
                    Assert.That(await this.Service.RemoveAsync(gid, Enumerable.Empty<ulong>()), Is.Zero);
                }
            },
            db => {
                Assert.That(this.Service.GetIds(MockData.Ids[0]), Is.EquivalentTo(MockData.Ids.Take(2)));
                Assert.That(db.AutoRoles, Has.Exactly(bef).Items);
                return Task.CompletedTask;
            }
        );
    }


    private void AddMockData(TheGodfatherDbContext db)
    {
        db.AutoRoles.Add(new AutoRole { GuildId = MockData.Ids[0], RoleId = MockData.Ids[0] });
        db.AutoRoles.Add(new AutoRole { GuildId = MockData.Ids[0], RoleId = MockData.Ids[1] });
        db.AutoRoles.Add(new AutoRole { GuildId = MockData.Ids[1], RoleId = MockData.Ids[2] });
    }

    private IEnumerable<ulong> GetGuildRoles(TheGodfatherDbContext db, ulong gid)
        => db.AutoRoles.AsQueryable().Where(ar => ar.GuildIdDb == (long)gid).AsEnumerable().Select(ar => ar.RoleId);
}

public class ConcreteService2 : DbAbstractionServiceBase<PrivilegedUser, ulong>
{
    public override bool IsDisabled => false;

    public ConcreteService2(DbContextBuilder dbb) : base(dbb) { }

    public override DbSet<PrivilegedUser> DbSetSelector(TheGodfatherDbContext db) => db.PrivilegedUsers;
    public override PrivilegedUser EntityFactory(ulong id) => new() { UserId = id };
    public override ulong EntityIdSelector(PrivilegedUser entity) => entity.UserId;
    public override object[] EntityPrimaryKeySelector(ulong id) => new object[] { (long)id };
}

public class ConcreteService3 : DbAbstractionServiceBase<AutoRole, ulong, ulong>
{
    public override bool IsDisabled => false;

    public ConcreteService3(DbContextBuilder dbb) : base(dbb) { }

    public override DbSet<AutoRole> DbSetSelector(TheGodfatherDbContext db) => db.AutoRoles;
    public override AutoRole EntityFactory(ulong gid, ulong id) => new() { GuildId = gid, RoleId = id };
    public override ulong EntityIdSelector(AutoRole entity) => entity.RoleId;
    public override ulong EntityGroupSelector(AutoRole entity) => entity.GuildId;
    public override object[] EntityPrimaryKeySelector(ulong gid, ulong id) => new object[] { (long)gid, (long)id };
    public override IQueryable<AutoRole> GroupSelector(IQueryable<AutoRole> entities, ulong gid)
        => entities.Where(e => e.GuildIdDb == (long)gid);
}