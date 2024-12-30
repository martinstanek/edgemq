using System;

namespace EdgeMq.Service.Metrics;

public sealed class EventsPerInterval
{
    public const int MaxSecondsEvaluation = 60;

    private readonly LimitedSizeAddOnlyStack _stack = new(MaxSecondsEvaluation);

    public void AddEvents(uint count)
    {
        AddEvents(count, DateTime.Now);
    }

    public void AddEvents(uint count, DateTime time)
    {
        if (_stack.Count == 0)
        {
            _stack.Push(count, time);
            return;
        }

        var mostRecent = _stack.MostRecent();

        if (mostRecent.OldInSeconds < 1)
        {
            mostRecent.Increment(count);
            return;
        }

        _stack.Push(count, time);
    }

    public double CurrentEventsPerSecond()
    {
        if (_stack.Count == 0)
        {
            return 0;
        }

        if (_stack.Count == 1)
        {
            return _stack.MostRecent().Value;
        }

        var totalSeconds = _stack.Oldest().OldInSeconds;
        var events = _stack.Sum;

        if (events == 0)
        {
            return 0;
        }

        return events / totalSeconds;
    }

    public int Count => _stack.Count;
}