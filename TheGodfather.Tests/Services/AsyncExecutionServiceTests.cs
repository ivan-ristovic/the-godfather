using System.Threading;
using NUnit.Framework;

namespace TheGodfather.Tests.Services;

[TestFixture]
public sealed class AsyncExecutionServiceTests : ITheGodfatherServiceTest<AsyncExecutionService>
{
    public AsyncExecutionService Service { get; private set; } = null!;


    [SetUp]
    public void InitializeService() => this.Service = new AsyncExecutionService();


    [Test]
    public void VoidTests()
    {
        Box<int> x = 1;

        this.Service.Execute(WorkingVoid1Async(x));
        Assert.That(x.Value, Is.EqualTo(5));

        Assert.That(() => this.Service.Execute(FailingVoid1Async(x)), Throws.Exception);
        Assert.That(x.Value, Is.EqualTo(25));

        Assert.That(() => this.Service.Execute(FailingVoid2Async()), Throws.InvalidOperationException);


        static async Task WorkingVoid1Async(Box<int> value)
        {
            await Task.Yield();

            await Task.Delay(500);
            value.Value *= 5;
        }

        static async Task FailingVoid1Async(Box<int> value)
        {
            await Task.Yield();

            await Task.Delay(500);
            value.Value *= 5;

            await Task.Delay(500);
            throw new Exception("Test exception.");
#pragma warning disable 0162
            value.Value *= 5;
#pragma warning restore 0162
        }

        static async Task FailingVoid2Async()
        {
            await Task.Yield();

            await Task.Delay(500);

            await Task.Delay(500);
            throw new InvalidOperationException("Test exception.");
        }
    }

    [Test]
    public void RetTests()
    {
        Box<int> x = 1;

        Box<int>? y = this.Service.Execute(WorkingRet1Async(x));
        Assert.That(x.Value, Is.EqualTo(5));
        Assert.That(y.Value, Is.EqualTo(5));
        Assert.That(ReferenceEquals(x, y));

        Assert.That(() => y = this.Service.Execute(FailingRet1Async(x)), Throws.Exception);
        Assert.That(x.Value, Is.EqualTo(25));
        Assert.That(y.Value, Is.EqualTo(25));
        Assert.That(ReferenceEquals(x, y));

        Assert.That(() => y = this.Service.Execute(FailingRet2Async()), Throws.InvalidOperationException);


        static async Task<Box<int>> WorkingRet1Async(Box<int> value)
        {
            await Task.Yield();

            await Task.Delay(500);
            value.Value *= 5;

            return value;
        }

        static async Task<Box<int>> FailingRet1Async(Box<int> value)
        {
            await Task.Yield();

            await Task.Delay(500);
            value.Value *= 5;

            await Task.Delay(500);
            throw new Exception("Test exception.");
#pragma warning disable 0162
            value.Value *= 5;
            return value.Value;
#pragma warning restore 0162
        }

        static async Task<int> FailingRet2Async()
        {
            await Task.Yield();

            await Task.Delay(1000);
            throw new InvalidOperationException("Test exception.");
        }
    }

    [Test]
    public void TestCancellationAsync()
    {
        using (var cts = new CancellationTokenSource()) {
            _ = Task.Delay(100).ContinueWith(_ => cts.Cancel());
            Assert.That(() => this.Service.Execute(Task.Delay(500, cts.Token)),
                Throws.InstanceOf<TaskCanceledException>());
        }

        using (var cts = new CancellationTokenSource()) {
            _ = Task.Delay(100).ContinueWith(_ => cts.Cancel());
            Assert.That(() => this.Service.Execute(Task.Delay(-1, cts.Token)),
                Throws.InstanceOf<TaskCanceledException>());
        }

        Assume.That(true, "This line is reached");
    }


    private sealed class Box<T>
    {
        public static implicit operator Box<T>(T v) => new(v);

        public T Value { get; set; }


        public Box(T v)
        {
            this.Value = v;
        }
    }
}