using System;
using System.Threading;

namespace Clave.BackgroundUpdatable
{
    public class AtomicLazy<T>
    {
        private readonly Func<T> _factory;

        private T _value;

        private bool _initalized;

        private object _lock;

        public AtomicLazy(Func<T> factory)
        {
            _factory = factory;
        }

        public AtomicLazy(T value)
        {
            _value = value;
            _initalized = true;
        }

        public T Value => LazyInitializer.EnsureInitialized(ref _value, ref _initalized, ref _lock, _factory);

        public void Set(T value) => _value = value;
    }
}