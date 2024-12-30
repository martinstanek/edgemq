using System;
using System.Collections.Generic;
using System.Linq;

namespace EdgeMq.Service.Metrics;

internal sealed class LimitedSizeAddOnlyStack
{
    private readonly int _maxSize;
    private readonly LinkedList<Stamp> _stack = new();
    private readonly object _lock = new();

    public LimitedSizeAddOnlyStack(int maxSize)
    {
        _maxSize = maxSize;
    }

    public void Push(uint value, DateTime time)
    {
        lock (_lock)
        {
            _stack.AddFirst(new Stamp(value, time));

            if (_stack.Count > _maxSize)
            {
                _stack.RemoveLast();
            }
        }
    }

    public Stamp MostRecent()
    {
        lock (_lock)
        {
            if (_stack.Count > 0)
            {
                return _stack.First();
            }

            throw new InvalidOperationException();
        }
    }

    public Stamp Oldest()
    {
        lock (_lock)
        {
            if (_stack.Count > 0)
            {
                return _stack.Last();
            }

            throw new InvalidOperationException();
        }
    }

    public ulong Sum
    {
        get {
            lock (_lock)
            {
                return (ulong)_stack.Sum(s => (long)s.Value);
            }
        }
    }

    public int Count
    {
        get
        {
            lock (_lock)
            {
                return _stack.Count;
            }
        }
    }
}