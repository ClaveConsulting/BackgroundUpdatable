using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Clave.BackgroundUpdatable
{
    public class BackgroundUpdatableList<T> :
        BackgroundUpdatable<IReadOnlyList<T>>,
        IReadOnlyList<T>
    {
        public BackgroundUpdatableList(
            TimeSpan period,
            Func<Task<IReadOnlyList<T>>> update)
            : base(period, update)
        {
        }

        public BackgroundUpdatableList(
            IReadOnlyList<T> initialCollection,
            TimeSpan period,
            Func<Task<IReadOnlyList<T>>> update)
            : base(initialCollection, period, update)
        {
        }

        public IEnumerator<T> GetEnumerator() => Value().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int Count => Value().Count;

        public T this[int index] => Value()[index];
    }
}