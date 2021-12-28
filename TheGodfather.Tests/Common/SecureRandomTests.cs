using System.Collections.Generic;
using NUnit.Framework;

namespace TheGodfather.Tests.Common;

[TestFixture]
public class SecureRandomTests
{
    private SecureRandom rng = new();


    [SetUp]
    public void SetUp() => this.rng = new SecureRandom();


    [Test]
    public void NextBoolTests()
    {
        Assert.That(() => this.rng.NextBool(0), Throws.InstanceOf<ArgumentOutOfRangeException>());
        Assert.That(() => this.rng.NextBool(-1), Throws.InstanceOf<ArgumentOutOfRangeException>());
        Assert.That(() => this.rng.NextBool(), Throws.Nothing);

        bool[] values = {true, false};
        this.TryGenerateAll(values, () => this.rng.NextBool());
        this.TryGenerateAll(values, () => this.rng.NextBool());
        this.TryGenerateAll(values, () => this.rng.NextBool(2));
        this.TryGenerateAll(values, () => this.rng.NextBool(3));
    }

    [Test]
    public void GetBytesTests()
    {
        Assert.That(() => this.rng.GetBytes(0), Throws.InstanceOf<ArgumentOutOfRangeException>());
        Assert.That(() => this.rng.GetBytes(-1), Throws.InstanceOf<ArgumentOutOfRangeException>());
        Assert.That(() => this.rng.GetBytes(1), Throws.Nothing);

        for (int i = 1; i < 8; i++)
            Assert.That(this.rng.GetBytes(i).Length, Is.EqualTo(i));
    }

    [Test]
    public void NextTests()
    {
        Assert.That(() => this.rng.Next(-1), Throws.InstanceOf<ArgumentOutOfRangeException>());
        Assert.That(() => this.rng.Next(0, 0), Throws.InstanceOf<ArgumentOutOfRangeException>());
        Assert.That(() => this.rng.Next(1, 0), Throws.InstanceOf<ArgumentOutOfRangeException>());
        Assert.That(() => this.rng.Next(1), Throws.Nothing);
        Assert.That(() => this.rng.Next(1, 5), Throws.Nothing);
        Assert.That(() => this.rng.Next(-5, -3), Throws.Nothing);
        Assert.That(() => this.rng.Next(-5, 3), Throws.Nothing);

        this.TryGenerateAll(Enumerable.Range(0, 5), () => this.rng.Next(5));
        this.TryGenerateAll(Enumerable.Range(-5, 2), () => this.rng.Next(-5, -3));
        this.TryGenerateAll(Enumerable.Range(-3, 6), () => this.rng.Next(-3, 3));
    }


    private void TryGenerateAll<T>(IEnumerable<T> values, Func<T> unit, bool checkOutOfBounds = true)
    {
        int count = values.Count();
        var generated = new HashSet<T>(count);
        int maxIter = 1000;
        for (int i = 0; i < maxIter; i++) {
            T g = unit();
            if (checkOutOfBounds)
                Assert.That(values, Contains.Item(g),
                    $"Element generated which was not present in the original collection: {g}");
            generated.Add(g);
        }

        Assert.That(generated, Has.Count.EqualTo(count),
            $"Not all values generated ({generated.Count}/{count}) in {maxIter} iterations.");
    }
}