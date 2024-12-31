using System;

namespace EdgeMq.Infra.Metrics;

public sealed class Stamp
{
    public Stamp(ulong value, DateTime time)
    {
        Value = value;
        Time = time;
    }

    public void Increment(uint value)
    {
        Value += value;
    }

    public ulong Value { get; private set; }

    public DateTime Time { get; }

    public double OldInSeconds =>  DateTime.Now.Subtract(Time).TotalSeconds;
}