namespace Clave.BackgroundUpdatable
{
    using System.Threading;

    public class AtomicBool
    {
        private const int False = 0;

        private const int True = 1;

        private volatile int _state;

        public bool WasFalse() => Interlocked.CompareExchange(ref _state, True, False) == False;

        public void SetFalse() => Interlocked.Exchange(ref _state, False);
    }
}
