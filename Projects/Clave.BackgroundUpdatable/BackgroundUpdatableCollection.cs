using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Clave.BackgroundUpdatable
{
    public class BackgroundUpdatableCollection<T> :
        BackgroundUpdatable<IReadOnlyCollection<T>>,
        IReadOnlyCollection<T>
    {
        public BackgroundUpdatableCollection(
            TimeSpan period,
            Func<Task<IReadOnlyCollection<T>>> update)
            : base(period, update)
        {
        }

        public BackgroundUpdatableCollection(
            IReadOnlyCollection<T> initialCollection,
            TimeSpan period,
            Func<Task<IReadOnlyCollection<T>>> update)
            : base(initialCollection, period, update)
        {
        }

        public IEnumerator<T> GetEnumerator() => Value().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int Count => Value().Count;
    }
}