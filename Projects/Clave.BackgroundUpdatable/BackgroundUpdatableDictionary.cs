using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Clave.BackgroundUpdatable
{
    public class BackgroundUpdatableDictionary<TKey, TValue> :
        BackgroundUpdatable<IReadOnlyDictionary<TKey, TValue>>,
        IReadOnlyDictionary<TKey, TValue>
    {
        public BackgroundUpdatableDictionary(
            TimeSpan period,
            Func<Task<IReadOnlyDictionary<TKey, TValue>>> update)
            : base(period, update)
        {
        }
        public BackgroundUpdatableDictionary(
            IReadOnlyDictionary<TKey, TValue> initialDictionary,
            TimeSpan period,
            Func<Task<IReadOnlyDictionary<TKey, TValue>>> update)
            : base(initialDictionary, period, update)
        {
        }

        public int Count => Value().Count;

        public IEnumerable<TKey> Keys => Value().Keys;

        public IEnumerable<TValue> Values => Value().Values;

        public TValue this[TKey key] => Value()[key];

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => Value().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => Value().GetEnumerator();

        public bool ContainsKey(TKey key) => Value().ContainsKey(key);

        public bool TryGetValue(TKey key, out TValue value) => Value().TryGetValue(key, out value);
    }
}