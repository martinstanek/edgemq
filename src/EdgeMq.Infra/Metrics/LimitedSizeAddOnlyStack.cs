using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace EdgeMq.Infra.Metrics;

public class LimitedSizeAddOnlyStack<T>
{
    protected readonly LinkedList<T> Stack = new();
    protected readonly Lock Lock = new();
    private readonly int _maxSize;

    public LimitedSizeAddOnlyStack(int maxSize)
    {
        _maxSize = maxSize;
    }

    public void Push(T item)
    {
        lock (Lock)
        {
            Stack.AddFirst(item);

            if (Stack.Count > _maxSize)
            {
                Stack.RemoveLast();
            }
        }
    }

    public T MostRecent()
    {
        lock (Lock)
        {
            if (Stack.Count > 0)
            {
                return Stack.First();
            }

            throw new InvalidOperationException();
        }
    }

    public T Oldest()
    {
        lock (Lock)
        {
            if (Stack.Count > 0)
            {
                return Stack.Last();
            }

            throw new InvalidOperationException();
        }
    }

    public IReadOnlyCollection<T> Items {
        get
        {
            lock (Lock)
            {
                return Stack;
            }
        }
    }

    public int Count
    {
        get
        {
            lock (Lock)
            {
                return Stack.Count;
            }
        }
    }
}