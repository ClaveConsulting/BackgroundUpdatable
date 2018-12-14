using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using Shouldly;

namespace Clave.BackgroundUpdatable.Tests
{
    using NUnit.Framework;

    [TestFixture]
    public class BackgroundUpdatableTests
    {
        [Test]
        public async Task TestWithInitialValue()
        {
            var updatable = new BackgroundUpdatable<string>(
                "initial value",
                TimeSpan.FromMinutes(1),
                () => Task.FromResult("updated value"));

            updatable.Value().ShouldBe("initial value");

            await Task.Delay(10);

            updatable.Value().ShouldBe("initial value");
        }

        [Test]
        public async Task TestWithoutInitialValue()
        {
            var updatable = new BackgroundUpdatable<string>(
                TimeSpan.FromMinutes(1),
                () => Task.FromResult("updated value"));

            updatable.Value().ShouldBe("updated value");

            await Task.Delay(10);

            updatable.Value().ShouldBe("updated value");
        }

        [Test]
        public async Task TestUpdatesAfterDelay()
        {
            var updatable = new BackgroundUpdatable<string>(
                "initial value",
                TimeSpan.FromMilliseconds(30),
                () => Task.FromResult("updated value"));

            updatable.Value().ShouldBe("initial value");

            await Task.Delay(50);

            updatable.Value().ShouldBe("initial value");

            await Task.Delay(10);

            updatable.Value().ShouldBe("updated value");
        }

        [Test]
        public async Task TestTriggersEventsDuringUpdateTests()
        {
            var backgroundUpdateStarted = Substitute.For<BackgroundUpdateStartedHandler>();
            var backgroundUpdateSucceeded = Substitute.For<BackgroundUpdateSucceededHandler>();

            var updatable = new BackgroundUpdatable<string>(
                "initial value",
                TimeSpan.FromMilliseconds(30),
                () => Task.FromResult("updated value"));

            updatable.BackgroundUpdateStarted += backgroundUpdateStarted;
            updatable.BackgroundUpdateSucceeded += backgroundUpdateSucceeded;

            updatable.Value().ShouldBe("initial value");

            await Task.Delay(50);

            updatable.Value().ShouldBe("initial value");

            await Task.Delay(50);

            backgroundUpdateStarted.Received(1)();
            backgroundUpdateSucceeded.Received(1)("updated value");
        }

        [Test]
        public async Task TestTriggersEventsWhenUpdateFailsTests()
        {
            var backgroundUpdateFailed = Substitute.For<BackgroundUpdateFailedHandler>();

            var updatable = new BackgroundUpdatable<string>(
                "initial value",
                TimeSpan.FromMilliseconds(30),
                () => Task.FromException<string>(new Exception("oh noes")));

            updatable.BackgroundUpdateFailed += backgroundUpdateFailed;

            updatable.Value().ShouldBe("initial value");

            await Task.Delay(50);

            updatable.Value().ShouldBe("initial value");

            await Task.Delay(50);

            backgroundUpdateFailed.Received(1)(Arg.Is<Exception>(e => e.Message == "oh noes"));
        }

        [Test]
        public async Task TestInitializesOnlyOnce()
        {
            var spy = Spy.On(() => Delay("updated value"));
            var updatable = new BackgroundUpdatable<string>(
                TimeSpan.FromMinutes(10),
                spy.Func);

            var results = await Simultaneously.Run(5, updatable.Value);

            results.ShouldAllBe(x => x == "updated value");

            spy.Called.ShouldBe(1);
        }

        [Test]
        public async Task TestUpdatesOnlyOnce()
        {
            var spy = Spy.On(() => Delay("updated value"));
            var updatable = new BackgroundUpdatable<string>(
                "initial value",
                TimeSpan.FromMilliseconds(100),
                spy.Func);

            updatable.Value();

            await Task.Delay(120);

            await Simultaneously.Run(50, updatable.Value);

            spy.Called.ShouldBe(1);
        }

        private static async Task<string> Delay(string value)
        {
            await Task.Delay(100);
            return value;
        }
    }
}