using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;

namespace Clave.BackgroundUpdatable.Tests
{
    public class AtomicBoolTests
    {
        [Test]
        public void TestWasFalse()
        {
            var boolean = new AtomicBool();

            if (boolean.WasFalse())
            {
                if (boolean.WasFalse())
                {
                    Assert.Fail("AtomicBool should be true until SetFalse is called");
                }

                boolean.SetFalse();

                if (boolean.WasFalse())
                {
                    Assert.Pass();
                }
                else
                {
                    Assert.Fail("AtomicBool should be reset by calling SetFalse");
                }
            }
            else
            {
                Assert.Fail("AtomicBool should be false when initialized");
            }
        }

        [Test]
        public async Task TestManyThreads()
        {
            var boolean = new AtomicBool();

            var threads = await Simultaneously.Run(100, boolean.WasFalse);

            threads.Where(x => x).ShouldHaveSingleItem();
        }
    }
}