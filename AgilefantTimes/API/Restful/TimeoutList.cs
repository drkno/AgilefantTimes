using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace AgilefantTimes.API.Restful
{
    public class TimeoutList<T> : IList<T>, IDisposable
    {
        private class TimeoutListItem
        {
            public DateTime Accessed { get; private set; }
            private T _value;

            public T Value
            {
                get
                {
                    Accessed = DateTime.Now;
                    return _value;
                }
                set
                {
                    Accessed = DateTime.Now;
                    _value = value;
                }
            }

            public TimeoutListItem(T value)
            {
                Value = value;
            }
        }

        private readonly Timer _timer;
        private readonly List<TimeoutListItem> _items;
        private readonly TimeSpan _timeSpan;
        private readonly Dictionary<string, int> _indexMap;

        public TimeoutList(TimeSpan timeoutTimespan)
        {
            _indexMap = new Dictionary<string, int>();
            _items = new List<TimeoutListItem>();
            _timeSpan = timeoutTimespan;
            _timer = new Timer(timeoutTimespan.TotalMilliseconds);
            _timer.Elapsed += Elapsed_Event;
            _timer.AutoReset = true;
            _timer.Start();
        }

        private void Elapsed_Event(object sender, ElapsedEventArgs e)
        {
            var dt = DateTime.Now;
            var removedSubtractor = 0;
            for (var i = 0; i < _items.Count; i++)
            {
                if (dt - _items[i].Accessed > _timeSpan)
                {
                    _items.RemoveAt(i);
                    removedSubtractor++;
                    i--;
                }
                else
                {
                    var old = _indexMap.First(k => k.Value == i);
                    _indexMap[old.Key] = old.Value - removedSubtractor;
                }
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException("This decorator is not iterable.");
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(T item)
        {
            _items.Add(new TimeoutListItem(item));
        }

        public void Clear()
        {
            _items.Clear();
        }

        public bool Contains(T item)
        {
            return _items.Any(listItem => Equals(listItem.Value, item));
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException("This decorator cannot be copied.");
        }

        public bool Remove(T item)
        {
            for (var i = 0; i < _items.Count; i++)
            {
                if (!Equals(_items[i].Value, item)) continue;
                _items.RemoveAt(i);
                return true;
            }
            return false;
        }

        public int Count => _items.Count;
        public bool IsReadOnly => false;
        public int IndexOf(T item)
        {
            for (var i = 0; i < _items.Count; i++)
            {
                if (Equals(_items[i].Value, item))
                {
                    return i;
                }
            }
            return -1;
        }

        public void Insert(int index, T item)
        {
            _items.Insert(index, new TimeoutListItem(item));
        }

        public void RemoveAt(int index)
        {
            _items.RemoveAt(index);
            for (var i = index; i < _items.Count; i++)
            {
                var old = _indexMap.First(k => k.Value == i);
                _indexMap[old.Key] = old.Value - 1;
            }
        }

        public T this[int index]
        {
            get { return _items[index].Value; }
            set { _items[index].Value = value; }
        }
        
        public void Dispose()
        {
            _timer.Stop();
        }

        public int LookupId(string id)
        {
            if (!_indexMap.ContainsKey(id))
            {
                return -1;
            }
            return _indexMap[id];
        }

        public string AddAndGetId(T item)
        {
            Add(item);
            var id = Guid.NewGuid().ToString("N");
            _indexMap[id] = _items.Count - 1;
            return id;
        }

        public void RemoveId(string id)
        {
            var ind = LookupId(id);
            if (ind < 0)
            {
                return;
            }

            RemoveAt(ind);
            _indexMap.Remove(id);
        }

        public void RemoveIndex(int index)
        {
            var key = _indexMap.First(k => k.Value == index).Key;
            RemoveId(key);
        }
    }
}
