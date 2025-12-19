using System.Collections;
using System.Collections.Generic;

namespace AssetsTools.NET
{
    internal sealed class ConcurrentDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
#if NET35
        private readonly Dictionary<TKey, TValue> _dict = new Dictionary<TKey, TValue>();
        private readonly object _lock = new object();
#else
        private readonly System.Collections.Concurrent.ConcurrentDictionary<TKey, TValue> _dict = new System.Collections.Concurrent.ConcurrentDictionary<TKey, TValue>();
#endif

        public ICollection<TKey> Keys
        {
            get
            {
#if NET35
                lock (_lock)
                {
                    return new List<TKey>(_dict.Keys);
                }
#else
                return _dict.Keys;
#endif
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
#if NET35
                lock (_lock)
                {
                    return new List<TValue>(_dict.Values);
                }
#else
                return _dict.Values;
#endif
            }
        }

        public int Count
        {
            get
            {
#if NET35
                lock (_lock)
                {
                    return _dict.Count;
                }
#else
                return _dict.Count;
#endif
            }
        }

        public bool IsReadOnly => false;

        public bool ContainsKey(TKey key)
        {
#if NET35
            lock (_lock)
            {
                return _dict.ContainsKey(key);
            }
#else
            return _dict.ContainsKey(key);
#endif
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
#if NET35
            List<KeyValuePair<TKey, TValue>> snapshot;
            lock (_lock)
            {
                snapshot = new List<KeyValuePair<TKey, TValue>>(_dict);
            }
            return snapshot.GetEnumerator();
#else
            return _dict.GetEnumerator();
#endif
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Add(TKey key, TValue value)
        {
#if NET35
            lock (_lock)
            {
                _dict.Add(key, value);
            }
#else
            ((IDictionary<TKey, TValue>)_dict).Add(key, value);
#endif
        }

        public bool Remove(TKey key)
        {
#if NET35
            lock (_lock)
            {
                return _dict.Remove(key);
            }
#else
            return _dict.TryRemove(key, out _);
#endif
        }

        public bool TryRemove(TKey key, out TValue value)
        {
#if NET35
            lock (_lock)
            {
                if (_dict.TryGetValue(key, out value))
                {
                    _dict.Remove(key);
                    return true;
                }
                value = default;
                return false;
            }
#else
            return _dict.TryRemove(key, out value);
#endif
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
#if NET35
            lock (_lock)
            {
                return _dict.TryGetValue(key, out value);
            }
#else
            return _dict.TryGetValue(key, out value);
#endif
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
#if NET35
            lock (_lock)
            {
                _dict.Clear();
            }
#else
            _dict.Clear();
#endif
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
#if NET35
            lock (_lock)
            {
                return ((IDictionary<TKey, TValue>)_dict).Contains(item);
            }
#else
            return ((IDictionary<TKey, TValue>)_dict).Contains(item);
#endif
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
#if NET35
            lock (_lock)
            {
                ((IDictionary<TKey, TValue>)_dict).CopyTo(array, arrayIndex);
            }
#else
            ((IDictionary<TKey, TValue>)_dict).CopyTo(array, arrayIndex);
#endif
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
#if NET35
            lock (_lock)
            {
                return ((IDictionary<TKey, TValue>)_dict).Remove(item);
            }
#else
            return ((IDictionary<TKey, TValue>)_dict).Remove(item);
#endif
        }

        public TValue this[TKey key]
        {
            get
            {
#if NET35
                lock (_lock)
                {
                    return _dict[key];
                }
#else
                return _dict[key];
#endif
            }
            set
            {
#if NET35
                lock (_lock)
                {
                    _dict[key] = value;
                }
#else
                _dict[key] = value;
#endif
            }
        }
    }
}
