using System.Collections;

namespace Wra10Core2023.Util;

public class ThreadSafeSet<T> : IEnumerable<T>
{
    private HashSet<T> set = [];
    private object dataLock = new();

    public (bool success, T v) Peek()
    {
        lock (dataLock)
        {
            if (set.Count == 0) return (false, default);
            return (true, set.First());
        }
    }

    public bool TryAdd(T item)
    {
        lock (dataLock)
        {
            return set.Add(item);
        }
    }

    public bool TryRemove(T item)
    {
        lock (dataLock)
        {
            return set.Remove(item);
        }
    }

    public int Count
    {
        get
        {
            lock (dataLock)
            {
                return set.Count;
            }
        }
    }

    public bool Contains(T item)
    {
        lock (dataLock)
        {
            return set.Contains(item);
        }
    }

    public List<T> GetList()
    {
        lock (dataLock)
        {
            return set.ToList();
        }
    }

    public void Clear()
    {
        lock (dataLock)
        {
            set.Clear();
        }
    }

    public IEnumerator<T> GetEnumerator() => GetList().GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
