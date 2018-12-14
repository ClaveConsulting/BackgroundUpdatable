using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Clave.BackgroundUpdatable
{
    public class BackgroundUpdatable<T>
    {
        private readonly double _millisecondsBetweenUpdates;

        private readonly Func<Task<T>> _update;

        private readonly Stopwatch _lastAccessAt = Stopwatch.StartNew();

        private readonly AtomicBool _isUpdating = new AtomicBool();

        private readonly AtomicLazy<T> _value;

        public event BackgroundUpdateStartedHandler BackgroundUpdateStarted;

        public event BackgroundUpdateFailedHandler BackgroundUpdateFailed;

        public event BackgroundUpdateSucceededHandler BackgroundUpdateSucceeded;

        public BackgroundUpdatable(
            TimeSpan period,
            Func<Task<T>> update)
        {
            _value = new AtomicLazy<T>(Initialize);
            _millisecondsBetweenUpdates = period.TotalMilliseconds;
            _update = update;
        }

        public BackgroundUpdatable(
            T initialValue,
            TimeSpan period,
            Func<Task<T>> update)
        {
            _value = new AtomicLazy<T>(initialValue);
            _millisecondsBetweenUpdates = period.TotalMilliseconds;
            _update = update;
        }

        public T Value()
        {
            var value = _value.Value;

            if (_lastAccessAt.ElapsedMilliseconds <= _millisecondsBetweenUpdates)
            {
                return value;
            }

            if (_isUpdating.WasFalse())
            {
                try
                {
                    UpdateInBackgroundThread();
                }
                finally
                {
                    _lastAccessAt.Restart();
                }
            }

            return value;
        }

        public async Task Update()
        {
            if (_isUpdating.WasFalse())
            {
                try
                {
                    _value.Set(await _update().ConfigureAwait(false));
                }
                finally
                {
                    _lastAccessAt.Restart();
                    _isUpdating.SetFalse();
                }
            }
        }

        private void UpdateInBackgroundThread() => Task.Run(async () =>
        {
            try
            {
                BackgroundUpdateStarted?.Invoke();
                var value = await _update().ConfigureAwait(false);
                _value.Set(value);
                BackgroundUpdateSucceeded?.Invoke(value);
            }
            catch (Exception e)
            {
                BackgroundUpdateFailed?.Invoke(e);
            }
            finally
            {
                _isUpdating.SetFalse();
            }
        });

        private T Initialize()
        {
            _lastAccessAt.Restart();
            return Task.Run(() => _update()).ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }

}