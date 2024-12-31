using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace EdgeMq.Infra.Metrics;

public sealed class LimitedSizeAddOnlyStampStack : LimitedSizeAddOnlyStack<Stamp>
{
    public LimitedSizeAddOnlyStampStack(int maxSize) : base(maxSize) { }

    public void Push(uint value, DateTime time)
    {
        base.Push(new Stamp(value, time));
    }

    public new Stamp MostRecent()
    {
        return base.MostRecent();
    }

    public new Stamp Oldest()
    {
        return base.Oldest();
    }

    public ulong Sum
    {
        get {
            lock (Lock)
            {
                return (ulong) Stack.Sum(s => (long)s.Value);
            }
        }
    }
}