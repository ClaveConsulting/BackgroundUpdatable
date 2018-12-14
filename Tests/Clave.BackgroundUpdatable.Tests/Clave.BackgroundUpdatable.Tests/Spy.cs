using System;
using System.Threading;
using System.Threading.Tasks;

namespace Clave.BackgroundUpdatable.Tests
{
    public class Spy
    {
        public static Spy1<T> On<T>(Func<T> func)
        {
            return new Spy1<T>(func);
        }

        public class Spy1<T>
        {
            private int _counter;

            private readonly Func<T> _func;

            public Spy1(Func<T> func)
            {
                _func = func;
            }

            public int Called => _counter;

            public Func<T> Func => () =>
            {
                Interlocked.Increment(ref _counter);
                return _func();
            };

            public void Reset()
            {
                Interlocked.Exchange(ref _counter, 0);
            }
        }
    }
}